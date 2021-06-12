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
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIHelpPanel : MonoBehaviour
    {
        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;
        private bool _cachedVisibility;

        [Inject]
        public void Construct(FloatVariable animationDuration)
        {
            _animationDuration      = animationDuration;
            _transform              = transform! as RectTransform;
            _animationStartPosition = _transform.rect.width;
            _animationEndPosition   = 0f;
        }

        public Tween SetVisibility(bool visible) => _cachedVisibility && visible ? Show() : Hide();

        public Tween Toggle()
        {
            Tween result      = _visible ? Hide() : Show();
            _cachedVisibility = !_cachedVisibility;
            return result;
        }

        private Tween Show()
        {
            if (_visible)
                return null;

            _visible = true;

            gameObject.SetActive(true);

            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic);
        }

        private Tween Hide()
        {
            if (!_visible)
                return null;

            _visible = false;

            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic)
                             .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
