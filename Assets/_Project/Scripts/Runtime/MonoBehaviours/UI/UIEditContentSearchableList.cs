/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using DG.Tweening;
using SG;
using TMPro;
using UnityEngine;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public abstract class UIEditContentSearchableList<TType, TListType> : MonoBehaviour
        where TListType : FilterableListVariable<TType>
    {
        [SerializeField] private ArcadeState _arcadeState;
        [SerializeField] protected TMP_InputField _searchInputField;

        public abstract int Count { get; }

        protected Databases _databases;
        protected TListType _filterableList;
        protected LoopScrollRect _scrollRect;
        protected bool _visible;

        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;

        [Inject]
        public void Construct(Databases databases, TListType filterableList, FloatVariable animationDuration)
        {
            _databases              = databases;
            _filterableList         = filterableList;
            _animationDuration      = animationDuration;
            _scrollRect             = GetComponentInChildren<LoopScrollRect>(true);
            _transform              = transform as RectTransform;
            _animationStartPosition = _transform.rect.width;
            _animationEndPosition   = 0f;

            _searchInputField.onSelect.AddListener((str) => _arcadeState.DisableInput());
            _searchInputField.onDeselect.AddListener((str) => _arcadeState.EnableInput());
            _searchInputField.onValueChanged.AddListener((str) => Search(str));
        }

        private void OnDestroy()
        {
            _searchInputField.onSelect.RemoveAllListeners();
            _searchInputField.onDeselect.RemoveAllListeners();
            _searchInputField.onValueChanged.RemoveAllListeners();
        }

        public void Init()
        {
            _searchInputField.DeactivateInputField(true);
            _searchInputField.SetTextWithoutNotify("");

            gameObject.SetActive(true);
            OnInit();
        }

        public void DeInit()
        {
            OnDeinit();

            _searchInputField.DeactivateInputField(true);
            _searchInputField.SetTextWithoutNotify("");
            _filterableList.Value.Clear();
            _filterableList.Filtered.Clear();
            _scrollRect.totalCount = 0;
            _scrollRect.RefillCells();

            Hide();
        }

        public void Show()
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);

            _searchInputField.DeactivateInputField(true);
            _searchInputField.SetTextWithoutNotify("");
            OnShow();

            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic);
        }

        public void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            _= _transform.DOKill();
            _= _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                         .SetEase(Ease.InOutCubic)
                         .OnComplete(() =>
                         {
                             _searchInputField.DeactivateInputField(true);
                             _searchInputField.SetTextWithoutNotify("");
                             OnHide();
                         });
        }

        protected abstract void OnShow();

        protected abstract void OnHide();

        protected abstract void OnInit();

        protected virtual void OnDeinit()
        {
        }

        protected abstract void Search(string lookUp);
    }
}
