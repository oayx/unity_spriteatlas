using SpriteAtlas;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace YX
{
    public class TestAssetPacker : MonoBehaviour
    {
        private AssetPacker _assetPacker;
        private List<string> _allFiles;
        private List<Texture2D> _unpackerTextures;
        private List<Texture2D> _packerTextures;

        private bool _isLoading = false;

        private void Start()
        {
            _allFiles = new List<string>(Directory.GetFiles(Application.dataPath + "/Demo/Art/Icons", "*.png"));

            _assetPacker = GetComponent<AssetPacker>();
            _assetPacker.OnCompleted.AddListener(OnCompleted);
        }

        private void OnGUI()
        {
            if(GUILayout.Button("文件夹打图集"))
            {
                StartCoroutine(PackerAllFiles());
            }
            if (GUILayout.Button("预加载资源"))
            {
                StartCoroutine(PreLoader());
            }
            if (GUILayout.Button("全部打图集"))
            {
                StartCoroutine(PackerAllTextures());
            }
            if (GUILayout.Button("随机打图集(50-100)"))
            {
                StartCoroutine(PackerRandomCount(50, 100));
            }
            if (GUILayout.Button("随机打图集(1-10)"))
            {
                StartCoroutine(PackerRandomCount(1, 10));
            }
            if (GUILayout.Button("随机移除(1-10)"))
            {
                RemoveRandomCount(1, 10);
            }
        }

        private IEnumerator PreLoader()
        {
            if (_unpackerTextures != null)
                yield break;

            _isLoading = true;
            _unpackerTextures = new List<Texture2D>();
            _packerTextures = new List<Texture2D>();
            foreach (var file in _allFiles)
            {
                WWW loader = new WWW("file:///" + file);
                yield return loader;
                var texture = loader.texture;
                texture.name = Path.GetFileNameWithoutExtension(file);
                _unpackerTextures.Add(texture);
            }
            Debug.Log("加载完毕");
            _isLoading = false;
        }
        private IEnumerator PackerAllFiles()
        {
            _assetPacker.AddTextures(_allFiles.ToArray());
            StartCoroutine(_assetPacker.Packer());
            yield return null;
        }
        private IEnumerator PackerAllTextures()
        {
            if (_isLoading)
            {
                Debug.LogError("等待图集加载完成");
                yield break;
            }

            foreach (var texture in _unpackerTextures)
            {
                _assetPacker.AddTexture(texture);
            }
            StartCoroutine(_assetPacker.Packer());
            _packerTextures.AddRange(_unpackerTextures);
            _unpackerTextures.Clear();
            yield return null;
        }
        private IEnumerator PackerRandomCount(int min, int max)
        {
            if (_isLoading)
            {
                Debug.LogError("等待图集加载完成");
                yield break;
            }

            if (_unpackerTextures == null)
            {
                StartCoroutine(PreLoader());
            }
            int count = RandRange(min, max);
            while (count > 0)
            {
                if (_unpackerTextures.Count == 0)
                    break;
                int index = RandRange(0, _unpackerTextures.Count);
                _assetPacker.AddTexture(_unpackerTextures[index]);
                _packerTextures.Add(_unpackerTextures[index]);
                _unpackerTextures.RemoveAt(index);
                count--;
            }

            StartCoroutine(_assetPacker.Packer());
            yield return null;
        }
        private void RemoveRandomCount(int min, int max)
        {
            if (_isLoading)
            {
                Debug.LogError("等待图集加载完成");
                return;
            }

            int count = RandRange(min, max);
            while (count > 0)
            {
                if (_packerTextures.Count == 0)
                    break;
                int index = RandRange(0, _packerTextures.Count);
                _assetPacker.RemoveTexture(_packerTextures[index]);
                _unpackerTextures.Add(_packerTextures[index]);
                _packerTextures.RemoveAt(index);
                count--;
            }
        }

        private void OnCompleted()
        {
            var textures = _assetPacker.GetTextures();
            for(int i = 0; i < textures.Count && i < transform.childCount; ++i)
            {
                var image = transform.GetChild(i).GetComponent<RawImage>();
                image.texture = textures[i];
            }
        }
        private int RandRange(int param1, int param2)
        {
            int loc = param1 + (int)(Random.Range(0f, 1f) * (param2 - param1));
            loc = Mathf.Clamp(loc, param1, param2 - 1);
            return loc;
        }
    }
}