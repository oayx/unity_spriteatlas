using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SpriteAtlas
{

    public class AssetPacker : MonoBehaviour 
	{
		public UnityEvent OnCompleted;
        /// <summary>
        /// 贴图大小
        /// </summary>
        public int TextureWidth = 1024;
        public int TextureHeight = 1024;
        /// <summary>
        /// 单个大小
        /// </summary>
        public int CellWidth = 64;
        public int CellHeight = 64;
        /// <summary>
        /// 空隙
        /// </summary>
        public int Padding = 1;
        public float PixelsPerUnit = 100.0f;
        /// <summary>
        /// 格式
        /// </summary>
        public TextureFormat Format = TextureFormat.ARGB32;

        private List<TextureAtlas> _textureAtlas = new List<TextureAtlas>();
        private List<TextureToPack> _items = new List<TextureToPack>();
        private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        private void Start()
        {
            CreateNewAtlas();
        }
        private void Destroy()
        {
            Dispose();
        }
        public void Dispose()
        {
            OnCompleted = null;
            foreach(var atlas in _textureAtlas)
                Destroy(atlas.Texture);
            foreach (var asset in _sprites)
                Destroy(asset.Value);
            _textureAtlas.Clear();
            _sprites.Clear();
            _items.Clear();
        }


        public bool AddTexture(string file, string name = null)
        {
            name = !string.IsNullOrEmpty(name) ? name : Path.GetFileNameWithoutExtension(file);
            if (_sprites.ContainsKey(name))
            {
                Debug.LogWarning("已经存在sprite:" + file);
                return false;
            }
            _items.Add(new TextureToPack(file, name));
            return true;
        }
        public bool AddTexture(Texture2D texture, string name = null)
        {
            if(string.IsNullOrEmpty(name))
                name = texture.name;
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("需要设置唯一name");
                return false;
            }
            if (_sprites.ContainsKey(name))
            {
                Debug.LogWarning("已经存在sprite:" + name);
                return false;
            }
            _items.Add(new TextureToPack(texture, name));
            return true;
        }

        public void AddTextures(string[] files)
        {
            foreach (string file in files)
				AddTexture(file);
        }
        public void AddTextures(Texture2D[] textures)
        {
            foreach (var texture in textures)
                AddTexture(texture, texture.name);
        }
        /// <summary>
        /// 单独的移除不会触发图集重新生成，只是设置了脏标记
        /// </summary>
        public void RemoveTexture(Texture2D texture)
        {
            string name = texture.name;
            for (int i = 0; i < _items.Count; ++i)
            {
                if (_items[i].texture == texture)
                {
                    name = _items[i].name;
                    _items.RemoveAt(i);
                    break;
                }
            }
            if(!string.IsNullOrEmpty(name))
            {
                RemoveSprite(name);
            }
        }
        /// <summary>
        /// 单独的移除不会触发图集重新生成，只是设置了脏标记
        /// </summary>
        public void RemoveTextureById(string name)
        {
            for (int i = 0; i < _items.Count; ++i)
            {
                if (_items[i].name == name)
                {
                    _items.RemoveAt(i);
                    break;
                }
            }
            if (!string.IsNullOrEmpty(name))
            {
                RemoveSprite(name);
            }
        }
        /// <summary>
        /// 单独的移除不会触发图集重新生成，只是设置了脏标记
        /// </summary>
        public void RemoveTextureByName(string file)
        {
            string name = "";
            for (int i = 0; i < _items.Count; ++i)
            {
                if (_items[i].file == file)
                {
                    name = _items[i].name;
                    _items.RemoveAt(i);
                    break;
                }
            }
            if (!string.IsNullOrEmpty(name))
            {
                RemoveSprite(name);
            }
        }
        /// <summary>
        /// 打图集入口
        /// </summary>
        public IEnumerator Packer() 
		{
			List<Texture2D> textures = new List<Texture2D>();
			List<string> textureNames = new List<string>();
			foreach (TextureToPack item in _items) 
			{
				if(item.texture == null)
                {
                    WWW loader = new WWW("file:///" + item.file);
                    yield return loader;
                    if (string.IsNullOrEmpty(loader.error))
                    {
                        textures.Add(loader.texture);
                        textureNames.Add(item.name);
                    }
                    else
                        Debug.LogError("加载资源失败：" + item.file);
                }
				else
				{
                    textures.Add(item.texture);
                    textureNames.Add(item.name);
                }
			}

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (textureNames.Count > 0)
            {
                var textureAssets = new List<TextureAsset>();
                var textureAtlas = GetFreeAtlas(textureNames.Count);
				for (int i = textureNames.Count - 1; i >= 0; --i)
				{
					if (!textureAtlas.Packer.TryAddRectangle(textureNames[i], out var rect))
						break;//如果满了就返回

                    textureAtlas.Texture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[i].GetPixels32());

                    TextureAsset textureAsset = new TextureAsset();
                    textureAsset.x = rect.x;
                    textureAsset.y = rect.y;
                    textureAsset.width = rect.width;
                    textureAsset.height = rect.height;
                    textureAsset.name = textureNames[i];
                    textureAssets.Add(textureAsset);

                    textures.Remove(textures[i]);
                    textureNames.Remove(textureNames[i]);
                }
                textureAtlas.Texture.Apply();

                foreach (TextureAsset textureAsset in textureAssets)
                {
                    _sprites.Add(textureAsset.name, Sprite.Create(textureAtlas.Texture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, PixelsPerUnit, 0, SpriteMeshType.FullRect));
                }
            }
            sw.Stop();
            Debug.LogFormat("打图集时长:{0}ms", sw.ElapsedMilliseconds);

            _items.Clear();
            OnCompleted.Invoke();
		}
        /// <summary>
        /// 找到一个空隙的packer，最好能有count空隙格子
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private TextureAtlas GetFreeAtlas(int count)
        {
            //优先找有足够多的
            for(int i = 0; i < _textureAtlas.Count; ++i)
            {
                if (_textureAtlas[i].Packer.GetFreeCount() >= count)
                    return _textureAtlas[i];
            }
            //只要有空闲，就返回
            for (int i = 0; i < _textureAtlas.Count; ++i)
            {
                if (_textureAtlas[i].Packer.GetFreeCount() > 0)
                    return _textureAtlas[i];
            }
            //创建新的
            return CreateNewAtlas();
        }
        private TextureAtlas CreateNewAtlas()
        {
            Texture2D texture = new Texture2D(TextureWidth, TextureHeight, Format, false);
            byte[] by = new byte[TextureWidth * TextureHeight * 4];
            texture.SetPixelData(by, 0);

            RectanglePacker packer = new RectanglePacker(texture.width, texture.height, CellWidth, CellHeight, Padding);

            TextureAtlas textureAtlas = new TextureAtlas();
            textureAtlas.Texture = texture;
            textureAtlas.Packer = packer;
            _textureAtlas.Add(textureAtlas);
            return textureAtlas;
        }
        private void RemoveSprite(string name)
        {
            if (_sprites.TryGetValue(name, out var sprite))
            {
                for (int i = 0; i < _textureAtlas.Count; ++i)
                {
                    if (_textureAtlas[i].Packer.RemoveRectangle(name))
                        break;
                }
                GameObject.Destroy(sprite);
                _sprites.Remove(name);
            }
        }
        #region 取sprite
        public List<Texture2D> GetTextures()
        {
            List<Texture2D> list = new List<Texture2D>();
            for (int i = 0; i < _textureAtlas.Count; ++i)
            {
                list.Add(_textureAtlas[i].Texture);
            }
            return list;
        }
        public Sprite GetSprite(string name)
        {
			if(_sprites.TryGetValue (name, out var sprite))
			    return sprite;
            return null;
		}
        public Sprite[] GetSprites()
        {
            return _sprites.Values.ToArray();
        }
        public List<Sprite> GetSprites(string prefix)
        {
            List<string> spriteNames = new List<string>();
			foreach (var asset in _sprites)
				if (asset.Key.StartsWith(prefix))
					spriteNames.Add(asset.Key);

			List<Sprite> sprites = new List<Sprite>();
			Sprite sprite;
			for (int i = 0; i < spriteNames.Count; ++i)
            {
                if(_sprites.TryGetValue(spriteNames[i], out sprite))
				    sprites.Add(sprite);
			}

			return sprites;
		}
        #endregion
    }
}
