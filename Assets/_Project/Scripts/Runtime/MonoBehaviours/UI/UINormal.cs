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

using UnityEngine;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UINormal : MonoBehaviour
    {
        private UINormalMenuButton _menuButton;
        private UISelectionText _selectionText;
        private bool _visible;

        [Inject]
        public void Construct(UINormalMenuButton menuButton, UISelectionText selectionText)
        {
            _menuButton    = menuButton;
            _selectionText = selectionText;
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);
            _menuButton.Enable();
            _selectionText.ClearText();
            _selectionText.Enable();
        }

        private void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            gameObject.SetActive(false);
            _selectionText.ClearText();
            _selectionText.Disable();
            _menuButton.Disable();
        }
    }
}
