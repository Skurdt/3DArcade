using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SG
{
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform)), AddComponentMenu("")]
    public abstract class LoopScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        //==========LoopScrollRect==========
        protected enum LoopScrollRectDirection
        {
            Vertical,
            Horizontal,
        }

        [Tooltip("Prefab Source")]
        public LoopScrollPrefabSource prefabSource;

        [Tooltip("Total count, negative means INFINITE mode")]
        public int totalCount;

        [NonSerialized, HideInInspector]
        public LoopScrollDataSource dataSource = LoopScrollSendIndexSource.Instance;

        public object[] ObjectsToFill
        {
            // wrapper for forward compatbility
            set
            {
                if (value != null)
                    dataSource = new LoopScrollArraySource<object>(value);
                else
                    dataSource = LoopScrollSendIndexSource.Instance;
            }
        }

        protected float ContentSpacing
        {
            get
            {
                if (_contentSpaceInit)
                    return _contentSpacing;

                _contentSpaceInit = true;
                _contentSpacing = 0f;
                if (Content != null)
                {
                    HorizontalOrVerticalLayoutGroup layoutGroup = Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if (layoutGroup != null)
                        _contentSpacing = layoutGroup.spacing;

                    _gridLayout = Content.GetComponent<GridLayoutGroup>();
                    if (_gridLayout != null)
                        _contentSpacing = Mathf.Abs(GetDimension(_gridLayout.spacing));
                }
                return _contentSpacing;
            }
        }

        protected int ContentConstraintCount
        {
            get
            {
                if (_contentConstraintCountInit)
                    return _contentConstraintCount;

                _contentConstraintCountInit = true;
                _contentConstraintCount = 1;
                if (Content != null)
                {
                    GridLayoutGroup layoutGroup = Content.GetComponent<GridLayoutGroup>();
                    if (layoutGroup != null)
                    {
                        if (layoutGroup.constraint == GridLayoutGroup.Constraint.Flexible)
                            Debug.LogWarning("[LoopScrollRect] Flexible not supported yet");
                        _contentConstraintCount = layoutGroup.constraintCount;
                    }
                }
                return _contentConstraintCount;
            }
        }

        protected int StartLine => Mathf.CeilToInt((float)_itemTypeStart / ContentConstraintCount);

        protected int CurrentLines => Mathf.CeilToInt((float)(_itemTypeEnd - _itemTypeStart) / ContentConstraintCount);

        protected int TotalLines => Mathf.CeilToInt((float)totalCount / ContentConstraintCount);

        [Tooltip("Reverse direction for dragging")]
        public bool reverseDirection = false;
        [Tooltip("Rubber scale for outside")]
        public float rubberScale = 1;

        protected float _threshold = 0;
        protected int _itemTypeStart = 0;
        protected int _itemTypeEnd = 0;
        protected LoopScrollRectDirection _direction = LoopScrollRectDirection.Horizontal;
        protected GridLayoutGroup _gridLayout = null;

        protected abstract float GetSize(RectTransform item);

        protected abstract float GetDimension(Vector2 vector);

        protected abstract Vector2 GetVector(float value);

        private bool _contentSpaceInit = false;
        private float _contentSpacing = 0;
        private bool _contentConstraintCountInit = false;
        private int _contentConstraintCount = 0;

        protected virtual bool UpdateItems(Bounds viewBounds, Bounds contentBounds) => false;
        //==========LoopScrollRect==========

        public enum MovementTypeEnum
        {
            Unrestricted, // Unrestricted movement -- can scroll forever
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped, // Restricted movement where it's not possible to go past the edges
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExpandViewport,
        }

        [Serializable]
        public sealed class ScrollRectEvent : UnityEvent<Vector2>
        {
        }

        [SerializeField]
        private RectTransform _content;
        public RectTransform Content { get => _content; set => _content = value; }

        [SerializeField]
        private bool _horizontal = true;
        public bool Horizontal { get => _horizontal; set => _horizontal = value; }

        [SerializeField]
        private bool _vertical = true;
        public bool Vertical { get => _vertical; set => _vertical = value; }

        [SerializeField]
        private MovementTypeEnum _movementType = MovementTypeEnum.Elastic;
        public MovementTypeEnum MovementType { get => _movementType; set => _movementType = value; }

        [SerializeField]
        private float _elasticity = 0.1f; // Only used for MovementType.Elastic
        public float Elasticity { get => _elasticity; set => _elasticity = value; }

        [SerializeField]
        private bool _inertia = true;
        public bool Inertia { get => _inertia; set => _inertia = value; }

        [SerializeField]
        private float _decelerationRate = 0.135f; // Only used when inertia is enabled
        public float DecelerationRate { get => _decelerationRate; set => _decelerationRate = value; }

        [SerializeField]
        private float _scrollSensitivity = 1.0f;
        public float ScrollSensitivity { get => _scrollSensitivity; set => _scrollSensitivity = value; }

        [SerializeField]
        private RectTransform _viewport;
        public RectTransform Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private Scrollbar _horizontalScrollbar;
        public Scrollbar HorizontalScrollbar
        {
            get => _horizontalScrollbar;
            set
            {
                if (_horizontalScrollbar)
                    _horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                _horizontalScrollbar = value;
                if (_horizontalScrollbar)
                    _horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private Scrollbar _verticalScrollbar;
        public Scrollbar VerticalScrollbar
        {
            get => _verticalScrollbar;
            set
            {
                if (_verticalScrollbar)
                    _verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                _verticalScrollbar = value;
                if (_verticalScrollbar)
                    _verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private ScrollbarVisibility _horizontalScrollbarVisibility;
        public ScrollbarVisibility HorizontalScrollbarVisibility
        {
            get => _horizontalScrollbarVisibility;
            set
            {
                _horizontalScrollbarVisibility = value;
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private ScrollbarVisibility _verticalScrollbarVisibility;
        public ScrollbarVisibility VerticalScrollbarVisibility
        {
            get => _verticalScrollbarVisibility;
            set
            {
                _verticalScrollbarVisibility = value;
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private float _horizontalScrollbarSpacing;
        public float HorizontalScrollbarSpacing
        {
            get => _horizontalScrollbarSpacing;
            set
            {
                _horizontalScrollbarSpacing = value;
                SetDirty();
            }
        }

        [SerializeField]
        private float _verticalScrollbarSpacing;
        public float VerticalScrollbarSpacing
        {
            get => _verticalScrollbarSpacing;
            set
            {
                _verticalScrollbarSpacing = value;
                SetDirty();
            }
        }

        [SerializeField]
        private ScrollRectEvent _onValueChanged = new ScrollRectEvent();
        public ScrollRectEvent OnValueChanged { get => _onValueChanged; set => _onValueChanged = value; }

        // The offset from handle position to mouse down position
        private Vector2 _pointerStartLocalCursor = Vector2.zero;
        private Vector2 _contentStartPosition = Vector2.zero;

        private RectTransform _viewRect;

        protected RectTransform ViewRect
        {
            get
            {
                if (_viewRect == null)
                    _viewRect = _viewport;
                if (_viewRect == null)
                    _viewRect = (RectTransform)transform;
                return _viewRect;
            }
        }

        private Bounds _contentBounds;
        private Bounds _viewBounds;

        private Vector2 _velocity;
        public Vector2 Velocity { get => _velocity; set => _velocity = value; }

        private bool _dragging;

        private Vector2 _prevPosition = Vector2.zero;
        private Bounds _prevContentBounds;
        private Bounds _prevViewBounds;
        [NonSerialized]
        private bool _hasRebuiltLayout = false;

        private bool _hSliderExpand;
        private bool _vSliderExpand;
        private float _hSliderHeight;
        private float _vSliderWidth;

        [NonSerialized]
        private RectTransform _rect;
        private RectTransform RectTransform
        {
            get
            {
                if (_rect == null)
                    _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }

        private RectTransform _horizontalScrollbarRect;
        private RectTransform _verticalScrollbarRect;

        private DrivenRectTransformTracker _tracker;

        protected LoopScrollRect() => flexibleWidth = -1;

        //==========LoopScrollRect==========
#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
            {
                float value = (reverseDirection ^ (_direction == LoopScrollRectDirection.Horizontal)) ? 0 : 1;
                Debug.Assert(Mathf.Abs(GetDimension(Content.pivot)) == value, this);
                Debug.Assert(Mathf.Abs(GetDimension(Content.anchorMin)) == value, this);
                Debug.Assert(Mathf.Abs(GetDimension(Content.anchorMax)) == value, this);
            }
        }
#endif
        public void ClearCells()
        {
            if (Application.isPlaying)
            {
                _itemTypeStart = 0;
                _itemTypeEnd = 0;
                totalCount = 0;
                ObjectsToFill = null;
                for (int i = Content.childCount - 1; i >= 0; i--)
                    prefabSource.ReturnObject(Content.GetChild(i));
            }
        }

        public void SrollToCell(int index, float speed)
        {
            if (totalCount >= 0 && (index < 0 || index >= totalCount))
            {
                Debug.LogErrorFormat("invalid index {0}", index);
                return;
            }
            StopAllCoroutines();
            if (speed <= 0)
            {
                RefillCells(index);
                return;
            }
            _ = StartCoroutine(ScrollToCellCoroutine(index, speed));
        }

        IEnumerator ScrollToCellCoroutine(int index, float speed)
        {
            bool needMoving = true;
            while (needMoving)
            {
                yield return null;
                if (!_dragging)
                {
                    float move;
                    if (index < _itemTypeStart)
                        move = -Time.deltaTime * speed;
                    else if (index >= _itemTypeEnd)
                        move = Time.deltaTime * speed;
                    else
                    {
                        _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
                        Bounds m_ItemBounds = GetBounds4Item(index);
                        float offset;
                        if (_direction == LoopScrollRectDirection.Vertical)
                            offset = reverseDirection ? (_viewBounds.min.y - m_ItemBounds.min.y) : (_viewBounds.max.y - m_ItemBounds.max.y);
                        else
                            offset = reverseDirection ? (m_ItemBounds.max.x - _viewBounds.max.x) : (m_ItemBounds.min.x - _viewBounds.min.x);
                        // check if we cannot move on
                        if (totalCount >= 0)
                        {
                            if (offset > 0 && _itemTypeEnd == totalCount && !reverseDirection)
                            {
                                m_ItemBounds = GetBounds4Item(totalCount - 1);
                                // reach bottom
                                if ((_direction == LoopScrollRectDirection.Vertical && m_ItemBounds.min.y > _viewBounds.min.y) ||
                                    (_direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.max.x < _viewBounds.max.x))
                                {
                                    break;
                                }
                            }
                            else if (offset < 0 && _itemTypeStart == 0 && reverseDirection)
                            {
                                m_ItemBounds = GetBounds4Item(0);
                                if ((_direction == LoopScrollRectDirection.Vertical && m_ItemBounds.max.y < _viewBounds.max.y) ||
                                    (_direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.min.x > _viewBounds.min.x))
                                {
                                    break;
                                }
                            }
                        }

                        float maxMove = Time.deltaTime * speed;
                        if (Mathf.Abs(offset) < maxMove)
                        {
                            needMoving = false;
                            move = offset;
                        }
                        else
                            move = Mathf.Sign(offset) * maxMove;
                    }
                    if (move != 0)
                    {
                        Vector2 offset = GetVector(move);
                        Content.anchoredPosition += offset;
                        _prevPosition += offset;
                        _contentStartPosition += offset;
                        UpdateBounds(true);
                    }
                }
            }
            StopMovement();
            UpdatePrevData();
        }

        public void RefreshCells()
        {
            if (Application.isPlaying && isActiveAndEnabled)
            {
                _itemTypeEnd = _itemTypeStart;
                // recycle items if we can
                for (int i = 0; i < Content.childCount; ++i)
                {
                    if (_itemTypeEnd < totalCount)
                    {
                        dataSource.ProvideData(Content.GetChild(i), _itemTypeEnd);
                        ++_itemTypeEnd;
                    }
                    else
                    {
                        prefabSource.ReturnObject(Content.GetChild(i));
                        --i;
                    }
                }
            }
        }

        public void RefillCellsFromEnd(int offset = 0, bool alignStart = false)
        {
            if (!Application.isPlaying || prefabSource == null)
                return;

            StopMovement();
            _itemTypeEnd = reverseDirection ? offset : totalCount - offset;
            _itemTypeStart = _itemTypeEnd;

            if (totalCount >= 0 && _itemTypeStart % ContentConstraintCount != 0)
                _itemTypeStart = _itemTypeStart / ContentConstraintCount * ContentConstraintCount;

            ReturnToTempPool(!reverseDirection, _content.childCount);

            float sizeToFill = Mathf.Abs(GetDimension(ViewRect.rect.size)), sizeFilled = 0;

            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtEnd() : NewItemAtStart();
                if (size <= 0)
                    break;
                sizeFilled += size;
            }

            // refill from start in case not full yet
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtStart() : NewItemAtEnd();
                if (size <= 0)
                    break;
                sizeFilled += size;
            }

            Vector2 pos = _content.anchoredPosition;
            float dist = alignStart ? 0 : Mathf.Max(0, sizeFilled - sizeToFill);
            if (reverseDirection)
                dist = -dist;
            if (_direction == LoopScrollRectDirection.Vertical)
                pos.y = dist;
            else
                pos.x = -dist;
            _content.anchoredPosition = pos;
            _contentStartPosition = pos;

            ClearTempPool();
            UpdateScrollbars(Vector2.zero);
        }

        public void RefillCells(int offset = 0, bool fillViewRect = false)
        {
            if (!Application.isPlaying || prefabSource == null)
                return;

            StopMovement();
            _itemTypeStart = reverseDirection ? totalCount - offset : offset;
            if (totalCount >= 0 && _itemTypeStart % ContentConstraintCount != 0)
                _itemTypeStart = _itemTypeStart / ContentConstraintCount * ContentConstraintCount;

            _itemTypeEnd = _itemTypeStart;

            // Don't `Canvas.ForceUpdateCanvases();` here, or it will new/delete cells to change itemTypeStart/End
            ReturnToTempPool(reverseDirection, _content.childCount);

            float sizeToFill = Mathf.Abs(GetDimension(ViewRect.rect.size)), sizeFilled = 0;
            // m_ViewBounds may be not ready when RefillCells on Start

            float itemSize = 0;

            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtStart() : NewItemAtEnd();
                if (size <= 0)
                    break;
                itemSize = size;
                sizeFilled += size;
            }

            // refill from start in case not full yet
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? NewItemAtEnd() : NewItemAtStart();
                if (size <= 0)
                    break;
                sizeFilled += size;
            }

            if (fillViewRect && itemSize > 0 && sizeFilled < sizeToFill)
            {
                int itemsToAddCount = (int)((sizeToFill - sizeFilled) / itemSize);        //calculate how many items can be added above the offset, so it still is visible in the view
                int newOffset = offset - itemsToAddCount;
                if (newOffset < 0)
                    newOffset = 0;
                if (newOffset != offset)
                    RefillCells(newOffset);                 //refill again, with the new offset value, and now with fillViewRect disabled.
            }

            Vector2 pos = _content.anchoredPosition;
            if (_direction == LoopScrollRectDirection.Vertical)
                pos.y = 0;
            else
                pos.x = 0;
            _content.anchoredPosition = pos;
            _contentStartPosition = pos;

            ClearTempPool();
            UpdateScrollbars(Vector2.zero);
        }

        protected float NewItemAtStart()
        {
            if (totalCount >= 0 && _itemTypeStart - ContentConstraintCount < 0)
                return 0;

            float size = 0;
            for (int i = 0; i < ContentConstraintCount; i++)
            {
                _itemTypeStart--;
                RectTransform newItem = GetFromTempPool(_itemTypeStart);
                newItem.SetSiblingIndex(_deletedItemTypeStart);
                size = Mathf.Max(GetSize(newItem), size);
            }
            _threshold = Mathf.Max(_threshold, size * 1.5f);

            if (!reverseDirection)
            {
                Vector2 offset = GetVector(size);
                Content.anchoredPosition += offset;
                _prevPosition += offset;
                _contentStartPosition += offset;
            }

            return size;
        }

        protected float DeleteItemAtStart()
        {
            // special case: when moving or dragging, we cannot simply delete start when we've reached the end
            if ((_dragging || _velocity != Vector2.zero) && totalCount >= 0 && _itemTypeEnd >= totalCount - ContentConstraintCount)
                return 0;

            int availableChilds = Content.childCount - _deletedItemTypeStart - _deletedItemTypeEnd;
            Debug.Assert(availableChilds >= 0);
            if (availableChilds == 0)
                return 0;

            float size = 0;
            for (int i = 0; i < ContentConstraintCount; i++)
            {
                RectTransform oldItem = Content.GetChild(_deletedItemTypeStart) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnToTempPool(true);
                availableChilds--;
                _itemTypeStart++;

                if (availableChilds == 0)
                    break;
            }

            if (!reverseDirection)
            {
                Vector2 offset = GetVector(size);
                Content.anchoredPosition -= offset;
                _prevPosition -= offset;
                _contentStartPosition -= offset;
            }
            return size;
        }

        protected float NewItemAtEnd()
        {
            if (totalCount >= 0 && _itemTypeEnd >= totalCount)
                return 0;

            float size = 0;
            // issue 4: fill lines to end first
            int availableChilds = Content.childCount - _deletedItemTypeStart - _deletedItemTypeEnd;
            int count = ContentConstraintCount - (availableChilds % ContentConstraintCount);
            for (int i = 0; i < count; i++)
            {
                RectTransform newItem = GetFromTempPool(_itemTypeEnd);
                newItem.SetSiblingIndex(Content.childCount - _deletedItemTypeEnd - 1);
                size = Mathf.Max(GetSize(newItem), size);
                _itemTypeEnd++;
                if (totalCount >= 0 && _itemTypeEnd >= totalCount)
                    break;
            }
            _threshold = Mathf.Max(_threshold, size * 1.5f);

            if (reverseDirection)
            {
                Vector2 offset = GetVector(size);
                Content.anchoredPosition -= offset;
                _prevPosition -= offset;
                _contentStartPosition -= offset;
            }

            return size;
        }

        protected float DeleteItemAtEnd()
        {
            if ((_dragging || _velocity != Vector2.zero) && totalCount >= 0 && _itemTypeStart < ContentConstraintCount)
                return 0;

            int availableChilds = Content.childCount - _deletedItemTypeStart - _deletedItemTypeEnd;
            Debug.Assert(availableChilds >= 0);
            if (availableChilds == 0)
                return 0;

            float size = 0;
            for (int i = 0; i < ContentConstraintCount; i++)
            {
                RectTransform oldItem = Content.GetChild(Content.childCount - _deletedItemTypeEnd - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnToTempPool(false);
                availableChilds--;
                _itemTypeEnd--;
                if (_itemTypeEnd % ContentConstraintCount == 0 || availableChilds == 0)
                    break;  //just delete the whole row
            }

            if (reverseDirection)
            {
                Vector2 offset = GetVector(size);
                Content.anchoredPosition += offset;
                _prevPosition += offset;
                _contentStartPosition += offset;
            }
            return size;
        }

        int _deletedItemTypeStart = 0;
        int _deletedItemTypeEnd   = 0;

        protected RectTransform GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem;
            if (_deletedItemTypeStart > 0)
            {
                --_deletedItemTypeStart;
                nextItem = Content.GetChild(0) as RectTransform;
                nextItem.SetSiblingIndex(itemIdx - _itemTypeStart + _deletedItemTypeStart);
            }
            else if (_deletedItemTypeEnd > 0)
            {
                --_deletedItemTypeEnd;
                nextItem = Content.GetChild(Content.childCount - 1) as RectTransform;
                nextItem.SetSiblingIndex(itemIdx - _itemTypeStart + _deletedItemTypeStart);
            }
            else
            {
                nextItem = prefabSource.GetObject().transform as RectTransform;
                nextItem.transform.SetParent(Content, false);
                nextItem.gameObject.SetActive(true);
            }
            dataSource.ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        protected void ReturnToTempPool(bool fromStart, int count = 1)
        {
            if (fromStart)
                _deletedItemTypeStart += count;
            else
                _deletedItemTypeEnd += count;
        }

        protected void ClearTempPool()
        {
            while (_deletedItemTypeStart > 0)
            {
                _deletedItemTypeStart--;
                prefabSource.ReturnObject(Content.GetChild(0));
            }

            while (_deletedItemTypeEnd > 0)
            {
                _deletedItemTypeEnd--;
                prefabSource.ReturnObject(Content.GetChild(Content.childCount - 1));
            }
        }
        //==========LoopScrollRect==========

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                _hasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            _horizontalScrollbarRect = _horizontalScrollbar == null ? null : _horizontalScrollbar.transform as RectTransform;
            _verticalScrollbarRect = _verticalScrollbar == null ? null : _verticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = ViewRect.parent == transform;
            bool hScrollbarIsChild = !_horizontalScrollbarRect || _horizontalScrollbarRect.parent == transform;
            bool vScrollbarIsChild = !_verticalScrollbarRect || _verticalScrollbarRect.parent == transform;
            bool allAreChildren = viewIsChild && hScrollbarIsChild && vScrollbarIsChild;

            _hSliderExpand = allAreChildren && _horizontalScrollbarRect && HorizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            _vSliderExpand = allAreChildren && _verticalScrollbarRect && VerticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            _hSliderHeight = _horizontalScrollbarRect == null ? 0 : _horizontalScrollbarRect.rect.height;
            _vSliderWidth = _verticalScrollbarRect == null ? 0 : _verticalScrollbarRect.rect.width;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_horizontalScrollbar)
                _horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (_verticalScrollbar)
                _verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (_horizontalScrollbar)
                _horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (_verticalScrollbar)
                _verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            _hasRebuiltLayout = false;
            _tracker.Clear();
            _velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            base.OnDisable();
        }

        public override bool IsActive() => base.IsActive() && _content != null;

        private void EnsureLayoutHasRebuilt()
        {
            if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public virtual void StopMovement() => _velocity = Vector2.zero;

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (Vertical && !Horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (Horizontal && !Vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            Vector2 position = _content.anchoredPosition;
            position += delta * _scrollSensitivity;
            if (_movementType == MovementTypeEnum.Clamped)
                position += CalculateOffset(position - _content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            _pointerStartLocalCursor = Vector2.zero;
            _ = RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out _pointerStartLocalCursor);
            _contentStartPosition = _content.anchoredPosition;
            _dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _dragging = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
                return;

            UpdateBounds();

            Vector2 pointerDelta = localCursor - _pointerStartLocalCursor;
            Vector2 position = _contentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - _content.anchoredPosition);
            position += offset;
            if (_movementType == MovementTypeEnum.Elastic)
            {
                //==========LoopScrollRect==========
                if (offset.x != 0)
                    position.x -= RubberDelta(offset.x, _viewBounds.size.x) * rubberScale;
                if (offset.y != 0)
                    position.y -= RubberDelta(offset.y, _viewBounds.size.y) * rubberScale;
                //==========LoopScrollRect==========
            }

            SetContentAnchoredPosition(position);
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!_horizontal)
                position.x = _content.anchoredPosition.x;
            if (!_vertical)
                position.y = _content.anchoredPosition.y;

            if ((position - _content.anchoredPosition).sqrMagnitude > 0.001f)
            {
                _content.anchoredPosition = position;
                UpdateBounds(true);
            }
        }

        protected virtual void LateUpdate()
        {
            if (!_content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateScrollbarVisibility();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (!_dragging && (offset != Vector2.zero || _velocity != Vector2.zero))
            {
                Vector2 position = _content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (_movementType == MovementTypeEnum.Elastic && offset[axis] != 0)
                    {
                        float speed = _velocity[axis];
                        position[axis] = Mathf.SmoothDamp(_content.anchoredPosition[axis], _content.anchoredPosition[axis] + offset[axis], ref speed, _elasticity, Mathf.Infinity, deltaTime);
                        _velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (_inertia)
                    {
                        _velocity[axis] *= Mathf.Pow(_decelerationRate, deltaTime);
                        if (Mathf.Abs(_velocity[axis]) < 1)
                            _velocity[axis] = 0;
                        position[axis] += _velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        _velocity[axis] = 0;
                    }
                }

                if (_velocity != Vector2.zero)
                {
                    if (_movementType == MovementTypeEnum.Clamped)
                    {
                        offset = CalculateOffset(position - _content.anchoredPosition);
                        position += offset;
                    }

                    SetContentAnchoredPosition(position);
                }
            }

            if (_dragging && _inertia)
            {
                Vector3 newVelocity = (_content.anchoredPosition - _prevPosition) / deltaTime;
                _velocity = Vector3.Lerp(_velocity, newVelocity, deltaTime * 10);
            }

            if (_viewBounds != _prevViewBounds || _contentBounds != _prevContentBounds || _content.anchoredPosition != _prevPosition)
            {
                UpdateScrollbars(offset);
                _onValueChanged.Invoke(NormalizedPosition);
                UpdatePrevData();
            }
        }

        private void UpdatePrevData()
        {
            if (_content == null)
                _prevPosition = Vector2.zero;
            else
                _prevPosition = _content.anchoredPosition;
            _prevViewBounds = _viewBounds;
            _prevContentBounds = _contentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (_horizontalScrollbar)
            {
                //==========LoopScrollRect==========
                if (_contentBounds.size.x > 0 && totalCount > 0)
                {
                    float elementSize = (_contentBounds.size.x - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                    float totalSize = (elementSize * TotalLines) + (ContentSpacing * (TotalLines - 1));
                    _horizontalScrollbar.size = Mathf.Clamp01((_viewBounds.size.x - Mathf.Abs(offset.x)) / totalSize);
                }
                //==========LoopScrollRect==========
                else
                    _horizontalScrollbar.size = 1;

                _horizontalScrollbar.value = HorizontalNormalizedPosition;
            }

            if (_verticalScrollbar)
            {
                //==========LoopScrollRect==========
                if (_contentBounds.size.y > 0 && totalCount > 0)
                {
                    float elementSize = (_contentBounds.size.y - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                    float totalSize = (elementSize * TotalLines) + (ContentSpacing * (TotalLines - 1));
                    _verticalScrollbar.size = Mathf.Clamp01((_viewBounds.size.y - Mathf.Abs(offset.y)) / totalSize);
                }
                //==========LoopScrollRect==========
                else
                    _verticalScrollbar.size = 1;

                _verticalScrollbar.value = VerticalNormalizedPosition;
            }
        }

        public Vector2 NormalizedPosition
        {
            get => new Vector2(HorizontalNormalizedPosition, VerticalNormalizedPosition);
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float HorizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if (totalCount > 0 && _itemTypeEnd > _itemTypeStart)
                {
                    float elementSize = (_contentBounds.size.x - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                    float totalSize = (elementSize * TotalLines) + (ContentSpacing * (TotalLines - 1));
                    float offset = _contentBounds.min.x - (elementSize * StartLine) - (ContentSpacing * StartLine);

                    if (totalSize <= _viewBounds.size.x)
                        return (_viewBounds.min.x > offset) ? 1 : 0;
                    return (_viewBounds.min.x - offset) / (totalSize - _viewBounds.size.x);
                }
                else
                    return 0.5f;
                //==========LoopScrollRect==========
            }
            set => SetNormalizedPosition(value, 0);
        }

        public float VerticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if (totalCount > 0 && _itemTypeEnd > _itemTypeStart)
                {
                    float elementSize = (_contentBounds.size.y - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                    float totalSize = (elementSize * TotalLines) + (ContentSpacing * (TotalLines - 1));
                    float offset = _contentBounds.max.y + (elementSize * StartLine) + (ContentSpacing * StartLine);

                    if (totalSize <= _viewBounds.size.y)
                        return (offset > _viewBounds.max.y) ? 1 : 0;
                    return (offset - _viewBounds.max.y) / (totalSize - _viewBounds.size.y);
                }
                else
                    return 0.5f;
                //==========LoopScrollRect==========
            }
            set => SetNormalizedPosition(value, 1);
        }

        private void SetHorizontalNormalizedPosition(float value) => SetNormalizedPosition(value, 0);

        private void SetVerticalNormalizedPosition(float value) => SetNormalizedPosition(value, 1);

        private void SetNormalizedPosition(float value, int axis)
        {
            //==========LoopScrollRect==========
            if (totalCount <= 0 || _itemTypeEnd <= _itemTypeStart)
                return;
            //==========LoopScrollRect==========

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            //==========LoopScrollRect==========
            Vector3 localPosition = _content.localPosition;
            float newLocalPosition = localPosition[axis];
            if (axis == 0)
            {
                float elementSize = (_contentBounds.size.x - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                float totalSize = (elementSize * TotalLines) + (ContentSpacing * (TotalLines - 1));
                float offset = _contentBounds.min.x - (elementSize * StartLine) - (ContentSpacing * StartLine);

                newLocalPosition += _viewBounds.min.x - (value * (totalSize - _viewBounds.size[axis])) - offset;
            }
            else if (axis == 1)
            {
                float elementSize = (_contentBounds.size.y - (ContentSpacing * (CurrentLines - 1))) / CurrentLines;
                float totalSize = (elementSize * TotalLines) + (ContentSpacing * (TotalLines - 1));
                float offset = _contentBounds.max.y + (elementSize * StartLine) + (ContentSpacing * StartLine);

                newLocalPosition -= offset - (value * (totalSize - _viewBounds.size.y)) - _viewBounds.max.y;
            }
            //==========LoopScrollRect==========

            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                _content.localPosition = localPosition;
                _velocity[axis] = 0;
                UpdateBounds(true);
            }
        }

        private static float RubberDelta(float overStretching, float viewSize) => (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);

        protected override void OnRectTransformDimensionsChange() => SetDirty();

        private bool HScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return _contentBounds.size.x > _viewBounds.size.x + 0.01f;
                return true;
            }
        }
        private bool VScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return _contentBounds.size.y > _viewBounds.size.y + 0.01f;
                return true;
            }
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual float minWidth => -1;
        public virtual float preferredWidth => -1;
        public virtual float flexibleWidth { get; private set; }

        public virtual float minHeight => -1;
        public virtual float preferredHeight => -1;
        public virtual float flexibleHeight => -1;

        public virtual int layoutPriority => -1;

        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();

            if (_hSliderExpand || _vSliderExpand)
            {
                _tracker.Add(this, ViewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                ViewRect.anchorMin = Vector2.zero;
                ViewRect.anchorMax = Vector2.one;
                ViewRect.sizeDelta = Vector2.zero;
                ViewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
                _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (_vSliderExpand && VScrollingNeeded)
            {
                ViewRect.sizeDelta = new Vector2(-(_vSliderWidth + _verticalScrollbarSpacing), ViewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
                _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (_hSliderExpand && HScrollingNeeded)
            {
                ViewRect.sizeDelta = new Vector2(ViewRect.sizeDelta.x, -(_hSliderHeight + _horizontalScrollbarSpacing));
                _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (_vSliderExpand && VScrollingNeeded && ViewRect.sizeDelta.x == 0 && ViewRect.sizeDelta.y < 0)
            {
                ViewRect.sizeDelta = new Vector2(-(_vSliderWidth + _verticalScrollbarSpacing), ViewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            _contentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            if (_verticalScrollbar && _verticalScrollbarVisibility != ScrollbarVisibility.Permanent && _verticalScrollbar.gameObject.activeSelf != VScrollingNeeded)
                _verticalScrollbar.gameObject.SetActive(VScrollingNeeded);

            if (_horizontalScrollbar && _horizontalScrollbarVisibility != ScrollbarVisibility.Permanent && _horizontalScrollbar.gameObject.activeSelf != HScrollingNeeded)
                _horizontalScrollbar.gameObject.SetActive(HScrollingNeeded);
        }

        void UpdateScrollbarLayout()
        {
            if (_vSliderExpand && _horizontalScrollbar)
            {
                _tracker.Add(this, _horizontalScrollbarRect,
                              DrivenTransformProperties.AnchorMinX |
                              DrivenTransformProperties.AnchorMaxX |
                              DrivenTransformProperties.SizeDeltaX |
                              DrivenTransformProperties.AnchoredPositionX);
                _horizontalScrollbarRect.anchorMin = new Vector2(0, _horizontalScrollbarRect.anchorMin.y);
                _horizontalScrollbarRect.anchorMax = new Vector2(1, _horizontalScrollbarRect.anchorMax.y);
                _horizontalScrollbarRect.anchoredPosition = new Vector2(0, _horizontalScrollbarRect.anchoredPosition.y);
                if (VScrollingNeeded)
                    _horizontalScrollbarRect.sizeDelta = new Vector2(-(_vSliderWidth + _verticalScrollbarSpacing), _horizontalScrollbarRect.sizeDelta.y);
                else
                    _horizontalScrollbarRect.sizeDelta = new Vector2(0, _horizontalScrollbarRect.sizeDelta.y);
            }

            if (_hSliderExpand && _verticalScrollbar)
            {
                _tracker.Add(this, _verticalScrollbarRect,
                              DrivenTransformProperties.AnchorMinY |
                              DrivenTransformProperties.AnchorMaxY |
                              DrivenTransformProperties.SizeDeltaY |
                              DrivenTransformProperties.AnchoredPositionY);
                _verticalScrollbarRect.anchorMin = new Vector2(_verticalScrollbarRect.anchorMin.x, 0);
                _verticalScrollbarRect.anchorMax = new Vector2(_verticalScrollbarRect.anchorMax.x, 1);
                _verticalScrollbarRect.anchoredPosition = new Vector2(_verticalScrollbarRect.anchoredPosition.x, 0);
                if (HScrollingNeeded)
                    _verticalScrollbarRect.sizeDelta = new Vector2(_verticalScrollbarRect.sizeDelta.x, -(_hSliderHeight + _horizontalScrollbarSpacing));
                else
                    _verticalScrollbarRect.sizeDelta = new Vector2(_verticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        private void UpdateBounds(bool updateItems = false)
        {
            _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            _contentBounds = GetBounds();

            if (_content == null)
                return;

            // ============LoopScrollRect============
            // Don't do this in Rebuild
            if (Application.isPlaying && updateItems && UpdateItems(_viewBounds, _contentBounds))
            {
                Canvas.ForceUpdateCanvases();
                _contentBounds = GetBounds();
            }
            // ============LoopScrollRect============

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = _contentBounds.size;
            Vector3 contentPos = _contentBounds.center;
            Vector3 excess = _viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (_content.pivot.x - 0.5f);
                contentSize.x = _viewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (_content.pivot.y - 0.5f);
                contentSize.y = _viewBounds.size.y;
            }

            _contentBounds.size = contentSize;
            _contentBounds.center = contentPos;
        }

        private readonly Vector3[] _corners = new Vector3[4];

        private Bounds GetBounds()
        {
            if (_content == null)
                return new Bounds();

            Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            Matrix4x4 toLocal = ViewRect.worldToLocalMatrix;
            _content.GetWorldCorners(_corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(_corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            Bounds bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Bounds GetBounds4Item(int index)
        {
            if (_content == null)
                return new Bounds();

            Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            Matrix4x4 toLocal = ViewRect.worldToLocalMatrix;
            int offset = index - _itemTypeStart;
            if (offset < 0 || offset >= _content.childCount)
                return new Bounds();
            RectTransform rt = _content.GetChild(offset) as RectTransform;
            if (rt == null)
                return new Bounds();
            rt.GetWorldCorners(_corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(_corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            Bounds bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (_movementType == MovementTypeEnum.Unrestricted)
                return offset;

            if (_movementType == MovementTypeEnum.Clamped)
            {
                if (totalCount < 0)
                    return offset;
                if (GetDimension(delta) < 0 && _itemTypeStart > 0)
                    return offset;
                if (GetDimension(delta) > 0 && _itemTypeEnd < totalCount)
                    return offset;
            }

            Vector2 min = _contentBounds.min;
            Vector2 max = _contentBounds.max;

            if (_horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > _viewBounds.min.x)
                    offset.x = _viewBounds.min.x - min.x;
                else if (max.x < _viewBounds.max.x)
                    offset.x = _viewBounds.max.x - max.x;
            }

            if (_vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < _viewBounds.max.y)
                    offset.y = _viewBounds.max.y - max.y;
                else if (min.y > _viewBounds.min.y)
                    offset.y = _viewBounds.min.y - min.y;
            }

            return offset;
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate() => SetDirtyCaching();
#endif
    }
}
