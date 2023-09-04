using SpriteAtlas;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace YX
{
    public class BagAssetPacker : MonoBehaviour
    {
        private AssetPacker _assetPacker;
        private List<string> _allFiles;

        private void Start()
        {
            _allFiles = new List<string>(Directory.GetFiles(Application.dataPath + "/Demo/Art/Icons", "*.png"));

            _assetPacker = GetComponent<AssetPacker>();
            _assetPacker.OnCompleted.AddListener(OnCompleted);
            _assetPacker.AddTextures(_allFiles.ToArray());
            StartCoroutine(_assetPacker.Packer());
        }

        private void OnCompleted()
        {
            int count = 1;
            var sprites = _assetPacker.GetSprites("item");
            foreach(Transform child in transform)
            {
                child.GetComponent<Image>().sprite = RandRange(sprites);
                child.Find("Text").GetComponent<Text>().text = (count++).ToString();
            }
        }
        private T RandRange<T>(List<T> arr)
        {
            if (arr.Count == 0)
            {
                Debug.LogError("容器数量为空");
                return default(T);
            }
            T loc = arr[RandRange(0, arr.Count)];
            return loc;
        }
        private int RandRange(int param1, int param2)
        {
            int loc = param1 + (int)(Random.Range(0f, 1f) * (param2 - param1));
            loc = Mathf.Clamp(loc, param1, param2 - 1);
            return loc;
        }
    }
}