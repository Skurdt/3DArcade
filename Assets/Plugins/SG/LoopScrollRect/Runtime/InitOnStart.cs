using UnityEngine;

namespace SG
{
    [DisallowMultipleComponent, RequireComponent(typeof(LoopScrollRect))]
    public sealed class InitOnStart : MonoBehaviour
    {
        public int TotalCount = -1;

        void Start()
        {
            LoopScrollRect loopScrollRect = GetComponent<LoopScrollRect>();
            loopScrollRect.totalCount = TotalCount;
            loopScrollRect.RefillCells();
        }
    }
}
