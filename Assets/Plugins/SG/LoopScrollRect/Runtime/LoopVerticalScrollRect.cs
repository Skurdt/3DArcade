using UnityEngine;
using UnityEngine.UI;

namespace SG
{
    [DisallowMultipleComponent, AddComponentMenu("UI/Loop Vertical Scroll Rect", 51)]
    public sealed class LoopVerticalScrollRect : LoopScrollRect
    {
        protected override float GetSize(RectTransform item)
        {
            float size = ContentSpacing;
            if (_gridLayout != null)
                size += _gridLayout.cellSize.y;
            else
                size += LayoutUtility.GetPreferredHeight(item);
            return size;
        }

        protected override float GetDimension(Vector2 vector) => vector.y;

        protected override Vector2 GetVector(float value) => new Vector2(0, value);

        protected override void Awake()
        {
            _direction = LoopScrollRectDirection.Vertical;
            base.Awake();

            GridLayoutGroup layout = Content.GetComponent<GridLayoutGroup>();
            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
                Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
        }

        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            // special case: handling move several page in one frame
            if (viewBounds.max.y < contentBounds.min.y && _itemTypeEnd > _itemTypeStart)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                    maxItemTypeStart = Mathf.Max(0, totalCount - (_itemTypeEnd - _itemTypeStart));

                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                ReturnToTempPool(true, _itemTypeEnd - _itemTypeStart);
                _itemTypeStart = _itemTypeEnd;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.y - viewBounds.max.y) / (elementSize + ContentSpacing));
                if (maxItemTypeStart >= 0 && _itemTypeStart + (offsetCount * ContentConstraintCount) > maxItemTypeStart)
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - _itemTypeStart) / ContentConstraintCount);

                _itemTypeStart += offsetCount * ContentConstraintCount;
                if (totalCount >= 0)
                    _itemTypeStart = Mathf.Max(_itemTypeStart, 0);

                _itemTypeEnd = _itemTypeStart;

                float offset = offsetCount * (elementSize + ContentSpacing);
                Content.anchoredPosition -= new Vector2(0, offset + (reverseDirection ? 0 : currentSize));
                contentBounds.center -= new Vector3(0, offset + (currentSize / 2), 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.min.y > contentBounds.max.y && _itemTypeEnd > _itemTypeStart)
            {
                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                ReturnToTempPool(false, _itemTypeEnd - _itemTypeStart);
                _itemTypeEnd = _itemTypeStart;

                int offsetCount = Mathf.FloorToInt((viewBounds.min.y - contentBounds.max.y) / (elementSize + ContentSpacing));
                if (totalCount >= 0 && _itemTypeStart - (offsetCount * ContentConstraintCount) < 0)
                    offsetCount = Mathf.FloorToInt((float)_itemTypeStart / ContentConstraintCount);

                _itemTypeStart -= offsetCount * ContentConstraintCount;
                if (totalCount >= 0)
                    _itemTypeStart = Mathf.Max(_itemTypeStart, 0);

                _itemTypeEnd = _itemTypeStart;

                float offset = offsetCount * (elementSize + ContentSpacing);
                Content.anchoredPosition += new Vector2(0, offset + (reverseDirection ? currentSize : 0));
                contentBounds.center += new Vector3(0, offset + (currentSize / 2), 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.min.y > contentBounds.min.y + _threshold)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y > contentBounds.min.y + _threshold + totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.y < contentBounds.max.y - _threshold)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y < contentBounds.max.y - _threshold - totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.y < contentBounds.min.y)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y < contentBounds.min.y - totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.y > contentBounds.max.y)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y > contentBounds.max.y + totalSize)
                {
                    size = NewItemAtStart();
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
