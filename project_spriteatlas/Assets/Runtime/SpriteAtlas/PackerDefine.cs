using System;
using UnityEngine;

namespace SpriteAtlas
{
	[Serializable]
	internal class TextureAsset
    {
        public int x;
		public int y;
		public int width;
		public int height;
		public string name;
    }

    internal class TextureAtlas
    {
        public Texture2D Texture;
        public RectanglePacker Packer;
        public bool IsDirty;
    }

    internal class TextureToPack
    {
        public string name;
        public string file;
        public Texture2D texture;

        public TextureToPack(string file, string id)
        {
            this.file = file;
            this.name = id;
        }
        public TextureToPack(Texture2D texture, string id)
        {
            this.texture = texture;
            this.name = id;
        }
    }
}