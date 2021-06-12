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
    public abstract class UIEditBase : MonoBehaviour
    {
        protected UISelectionText _selectionText;

        private UITopBar _topBar;
        private UIHelpPanel _helpPanel;
        private UIActionBar _actionbar;
        private bool _visible;

        [Inject]
        public void Construct(UISelectionText selectionText)
        {
            _selectionText = selectionText;
            _topBar        = GetComponentInChildren<UITopBar>(true);
            _helpPanel     = GetComponentInChildren<UIHelpPanel>(true);
            _actionbar     = GetComponentInChildren<UIActionBar>(true);
            OnConstruct();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                Show();
            else
                Hide();
        }

        protected virtual void OnConstruct()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        private void Show()
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);
            _ = _topBar.SetVisibility(true);
            if (_helpPanel != null)
                _ = _helpPanel.SetVisibility(true);
            _ = _actionbar.SetVisibility(true);
            OnShow();
        }

        private void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            OnHide();
            _ = _topBar.SetVisibility(false);
            if (_helpPanel != null)
                _ = _helpPanel.SetVisibility(false);
            _ = _actionbar.SetVisibility(false)
                          ?.OnComplete(() => gameObject.SetActive(false));
        }
    }
}
