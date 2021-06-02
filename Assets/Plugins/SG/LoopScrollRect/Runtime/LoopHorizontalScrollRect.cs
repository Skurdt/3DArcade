using UnityEngine;
using UnityEngine.UI;

namespace SG
{
    [DisallowMultipleComponent, AddComponentMenu("UI/Loop Horizontal Scroll Rect", 50)]
    public sealed class LoopHorizontalScrollRect : LoopScrollRect
    {
        protected override float GetSize(RectTransform item)
        {
            float size = ContentSpacing;
            if (_gridLayout != null)
                size += _gridLayout.cellSize.x;
            else
                size += LayoutUtility.GetPreferredWidth(item);
            return size;
        }

        protected override float GetDimension(Vector2 vector) => -vector.x;

        protected override Vector2 GetVector(float value) => new Vector2(-value, 0f);

        protected override void Awake()
        {
            _direction = LoopScrollRectDirection.Horizontal;
            base.Awake();

            GridLayoutGroup layout = Content.GetComponent<GridLayoutGroup>();
            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedRowCount)
                Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
        }

        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            // special case: handling move several page in one frame
            if (viewBounds.max.x < contentBounds.min.x && _itemTypeEnd > _itemTypeStart)
            {
                float currentSize = contentBounds.size.x;
                float elementSize = (currentSize - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                ReturnToTempPool(false, _itemTypeEnd - _itemTypeStart);
                _itemTypeEnd = _itemTypeStart;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.x - viewBounds.max.x) / (elementSize + ContentSpacing));
                if (totalCount >= 0 && _itemTypeStart - (offsetCount * ContentConstraintCount) < 0)
                    offsetCount = Mathf.FloorToInt((float)_itemTypeStart / ContentConstraintCount);

                _itemTypeStart -= offsetCount * ContentConstraintCount;
                if (totalCount >= 0)
                    _itemTypeStart = Mathf.Max(_itemTypeStart, 0);

                _itemTypeEnd = _itemTypeStart;

                float offset              = offsetCount * (elementSize + ContentSpacing);
                Content.anchoredPosition -= new Vector2(offset + (reverseDirection ? currentSize : 0f), 0f);
                contentBounds.center     -= new Vector3(offset + (currentSize / 2f), 0f, 0f);
                contentBounds.size        = Vector3.zero;

                changed = true;
            }

            if (viewBounds.min.x > contentBounds.max.x && _itemTypeEnd > _itemTypeStart)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, totalCount - (_itemTypeEnd - _itemTypeStart));
                    maxItemTypeStart = maxItemTypeStart / ContentConstraintCount * ContentConstraintCount;
                }

                float currentSize = contentBounds.size.x;
                float elementSize = (currentSize - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                ReturnToTempPool(true, _itemTypeEnd - _itemTypeStart);
                // TODO: fix with contentConstraint?
                _itemTypeStart = _itemTypeEnd;

                int offsetCount = Mathf.FloorToInt((viewBounds.min.x - contentBounds.max.x) / (elementSize + ContentSpacing));
                if (maxItemTypeStart >= 0 && _itemTypeStart + (offsetCount * ContentConstraintCount) > maxItemTypeStart)
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - _itemTypeStart) / ContentConstraintCount);

                _itemTypeStart += offsetCount * ContentConstraintCount;
                if (totalCount >= 0)
                    _itemTypeStart = Mathf.Max(_itemTypeStart, 0);

                _itemTypeEnd = _itemTypeStart;

                float offset              = offsetCount * (elementSize + ContentSpacing);
                Content.anchoredPosition += new Vector2(offset + (reverseDirection ? 0f : currentSize), 0f);
                contentBounds.center     += new Vector3(offset + (currentSize / 2f), 0f, 0f);
                contentBounds.size        = Vector3.zero;

                changed = true;
            }

            if (viewBounds.max.x < contentBounds.max.x - _threshold)
            {
                float size      = DeleteItemAtEnd();
                float totalSize = size;
                while (size > 0f && viewBounds.max.x < contentBounds.max.x - _threshold - totalSize)
                {
                    size       = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0f)
                    changed = true;
            }

            if (viewBounds.min.x > contentBounds.min.x + _threshold)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0f && viewBounds.min.x > contentBounds.min.x + _threshold + totalSize)
                {
                    size       = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0f)
                    changed = true;
            }

            if (viewBounds.max.x > contentBounds.max.x)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0f && viewBounds.max.x > contentBounds.max.x + totalSize)
                {
                    size       = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0f)
                    changed = true;
            }

            if (viewBounds.min.x < contentBounds.min.x)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x < contentBounds.min.x - totalSize)
                {
                    size       = NewItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (changed)
                ClearTempPool();

            return changed;
        }
    }
}
