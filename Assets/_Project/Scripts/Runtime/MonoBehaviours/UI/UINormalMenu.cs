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
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UINormalMenu : MonoBehaviour
    {
        [SerializeField] private FloatVariable _animationDuration;
        [SerializeField] private Button _menuButton;

        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;

        private void Awake()
        {
            _transform              = transform as RectTransform;
            _animationStartPosition = _transform.anchoredPosition.x;
            _animationEndPosition   = _animationStartPosition + _transform.rect.width;
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
            _menuButton.interactable = false;
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                          .OnComplete(() => _menuButton.interactable = true);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            _menuButton.interactable = false;
            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .OnComplete(() =>
                          {
                              _menuButton.interactable = true;
                              gameObject.SetActive(false);
                          });
        }
    }
}
