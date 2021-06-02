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
using UnityEngine;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIInfo : MonoBehaviour
    {
        [SerializeField] private RectTransform _leftPanel;
        [SerializeField] private RectTransform _rightPanel;
        [SerializeField] private RectTransform _topPanel;
        [SerializeField] private RectTransform _bottomPanel;
        [SerializeField] private RectTransform _closeButton;
        [SerializeField] private FloatVariable _animationDuration;

        private RectTransform _transform;

        private void Awake() => _transform = transform as RectTransform;

        public void Show()
        {
            gameObject.SetActive(true);

            Rect transformRect = _transform.rect;
            float width        = transformRect.width / 3f;
            float height       = transformRect.height;

            _leftPanel.sizeDelta        = new Vector2(width, height);
            _leftPanel.anchoredPosition = new Vector2(-width, 0f);

            _rightPanel.sizeDelta        = new Vector2(width, height);
            _rightPanel.anchoredPosition = new Vector2(width, 0f);

            height /= 2f;

            _topPanel.sizeDelta        = new Vector2(width, height);
            _topPanel.anchoredPosition = new Vector2(0f, height);

            _bottomPanel.sizeDelta        = new Vector2(width, height);
            _bottomPanel.anchoredPosition = new Vector2(0f, -height);

            float animationDuration = _animationDuration.Value;
            _ = DOTween.Sequence().Join(_leftPanel.DOAnchorPosX(0f, animationDuration))
                                  .Join(_rightPanel.DOAnchorPosX(0f, animationDuration))
                                  .Join(_topPanel.DOAnchorPosY(0f, animationDuration))
                                  .Join(_bottomPanel.DOAnchorPosY(0f, animationDuration));
        }

        public void Hide()
        {
            float animationDuration = _animationDuration.Value;
            _ = DOTween.Sequence().Join(_leftPanel.DOAnchorPosX(-_leftPanel.rect.width, animationDuration))
                                  .Join(_rightPanel.DOAnchorPosX(_rightPanel.rect.width, animationDuration))
                                  .Join(_topPanel.DOAnchorPosY(_topPanel.rect.height, animationDuration))
                                  .Join(_bottomPanel.DOAnchorPosY(-_bottomPanel.rect.height, animationDuration))
                                  .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
