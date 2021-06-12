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
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UINormalMenu : MonoBehaviour
    {
        private FloatVariable _animationDuration;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        private Button[] _buttons;

        [Inject]
        public void Construct(FloatVariable animationDuration)
        {
            _animationDuration      = animationDuration;
            _animationStartPosition = -85f;
            _animationEndPosition   = 85f;

            _buttons = GetComponentsInChildren<Button>();
        }

        public void Toggle()
        {
            if (_visible)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);
            for (int i = _buttons.Length - 1; i >= 0; --i)
            {
                RectTransform buttonTransform = _buttons[i].transform as RectTransform;
                _ = buttonTransform.DOKill();
                _ = buttonTransform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                                   .SetEase(Ease.InOutCubic)
                                   .SetDelay(0.05f * (_buttons.Length - 1 - i));
            }
        }

        public void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            for (int i = _buttons.Length - 1; i >= 0; --i)
            {
                RectTransform buttonTransform = _buttons[i].transform as RectTransform;
                _ = buttonTransform.DOKill();
                _ = buttonTransform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                                   .SetEase(Ease.InOutCubic)
                                   .SetDelay(0.05f * i);
            }
        }
    }
}
