using UnityEngine;

namespace SG
{
    [System.Serializable]
    public sealed class LoopScrollPrefabSource
    {
        public string prefabName;
        public int poolSize = 5;

        private bool _inited = false;

        public GameObject GetObject()
        {
            if (!_inited)
            {
                ResourceManager.Instance.InitPool(prefabName, poolSize);
                _inited = true;
            }
            return ResourceManager.Instance.GetObjectFromPool(prefabName);
        }

        public void ReturnObject(Transform go)
        {
            go.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            ResourceManager.Instance.ReturnObjectToPool(go.gameObject);
        }
    }
}
