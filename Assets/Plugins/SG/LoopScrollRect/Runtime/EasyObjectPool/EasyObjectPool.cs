/*
 * Unless otherwise licensed, this file cannot be copied or redistributed in any format without the explicit consent of the author.
 * (c) Preet Kamal Singh Minhas, http://marchingbytes.com
 * contact@marchingbytes.com
 */
// modified version by Kanglai Qian
using UnityEngine;
using System.Collections.Generic;

namespace SG
{
    public enum PoolInflationType
    {
        INCREMENT,
        DOUBLE
    }

    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class PoolObject : MonoBehaviour
    {
        public string PoolName;
        public bool IsPooled;
    }

    public sealed class Pool
    {
        private readonly Stack<PoolObject> _availableObjStack = new Stack<PoolObject>();
        private readonly string _poolName;
        private readonly PoolInflationType _inflationType;
        private readonly GameObject _rootObj;

        private int _objectsInUse = 0;

        public Pool(string poolName, GameObject poolObjectPrefab, GameObject rootPoolObj, int initialCount, PoolInflationType type)
        {
            if (poolObjectPrefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[ObjPoolManager] null pool object prefab !");
#endif
                return;
            }

            _poolName      = poolName;
            _inflationType = type;
            _rootObj       = new GameObject($"{poolName} Pool");
            _rootObj.transform.SetParent(rootPoolObj.transform, false);

            // In case the origin one is Destroyed, we should keep at least one
            GameObject go = Object.Instantiate(poolObjectPrefab);
            PoolObject po = go.GetComponent<PoolObject>();
            if (po == null)
                po = go.AddComponent<PoolObject>();
            po.PoolName = poolName;

            AddObjectToPool(po);
            PopulatePool(Mathf.Max(initialCount, 1));
        }

        //o(1)
        private void AddObjectToPool(PoolObject po)
        {
            po.gameObject.SetActive(false);
            po.gameObject.name = _poolName;
            _availableObjStack.Push(po);
            po.IsPooled = true;
            po.gameObject.transform.SetParent(_rootObj.transform, false);
        }

        private void PopulatePool(int initialCount)
        {
            for (int index = 0; index < initialCount; ++index)
            {
                PoolObject po = Object.Instantiate(_availableObjStack.Peek());
                AddObjectToPool(po);
            }
        }

        //o(1)
        public GameObject NextAvailableObject(bool autoActive)
        {
            PoolObject po = null;
            if (_availableObjStack.Count > 1)
                po = _availableObjStack.Pop();
            else
            {
                int increaseSize = _inflationType switch
                {
                    PoolInflationType.INCREMENT => 1,
                    PoolInflationType.DOUBLE    => _availableObjStack.Count + Mathf.Max(_objectsInUse, 0),
                    _                           => 0,
                };
#if UNITY_EDITOR
                Debug.Log(string.Format("Growing pool {0}: {1} populated", _poolName, increaseSize));
#endif
                if (increaseSize > 0)
                {
                    PopulatePool(increaseSize);
                    po = _availableObjStack.Pop();
                }
            }

            GameObject result = null;
            if (po != null)
            {
                ++_objectsInUse;
                po.IsPooled = false;
                result = po.gameObject;
                if (autoActive)
                    result.SetActive(true);
            }

            return result;
        }

        //o(1)
        public void ReturnObjectToPool(PoolObject po)
        {
            if (_poolName.Equals(po.PoolName))
            {
                --_objectsInUse;
                /* we could have used availableObjStack.Contains(po) to check if this object is in pool.
                 * While that would have been more robust, it would have made this method O(n)
                 */
                if (!po.IsPooled)
                    AddObjectToPool(po);
#if UNITY_EDITOR
                else
                    Debug.LogWarning(po.gameObject.name + " is already in pool. Why are you trying to return it again? Check usage.");
#endif
            }
            else
                Debug.LogError(string.Format("Trying to add object to incorrect pool {0} {1}", po.PoolName, _poolName));
        }
    }
}
