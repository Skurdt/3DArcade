using UnityEngine;

namespace SG
{
    public sealed class LoopScrollArraySource<T> : LoopScrollDataSource
    {
        private readonly T[] _objectsToFill;

        public LoopScrollArraySource(T[] objectsToFill) => _objectsToFill = objectsToFill;

        public override void ProvideData(Transform transform, int idx) => transform.SendMessage("ScrollCellContent", _objectsToFill[idx]);
    }
}
