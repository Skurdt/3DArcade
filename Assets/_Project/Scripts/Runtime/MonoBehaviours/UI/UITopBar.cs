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
    public sealed class UITopBar : MonoBehaviour
    {
        private Button _helpButton;
        private UIHelpPanel _helpPanel;

        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        [Inject]
        public void Construct(FloatVariable animationDuration)
        {
            _animationDuration      = animationDuration;
            _transform              = transform! as RectTransform;
            _animationStartPosition = _transform.rect.height;
            _animationEndPosition   = 0f;

            _helpButton = GetComponentInChildren<Button>(true);
            _helpPanel  = transform.parent.GetComponentInChildren<UIHelpPanel>(true);

            if (_helpButton != null)
                _helpButton.onClick.AddListener(() =>
                {
                    if (_helpPanel != null)
                        _ = _helpPanel.Toggle();
                });
        }

        private void OnDestroy()
        {
            if (_helpButton != null)
                _helpButton.onClick.RemoveAllListeners();
        }

        public Tween SetVisibility(bool visible) => visible ? Show() : Hide();

        private Tween Show()
        {
            if (_visible)
                return null;

            _visible = true;

            gameObject.SetActive(true);

            _ = _transform.DOKill();
            return _transform.DOAnchorPosY(_animationEndPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic);
        }

        private Tween Hide()
        {
            if (!_visible)
                return null;

            _visible = false;

            _ = _transform.DOKill();
            return _transform.DOAnchorPosY(_animationStartPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic)
                             .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
