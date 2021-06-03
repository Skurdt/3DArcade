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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent, RequireComponent(typeof(Image), typeof(Button))]
    public sealed class UIMenuToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite _openSprite;
        [SerializeField] private Sprite _closeSprite;
        [SerializeField] private UINormalMenu _normalMenu;

        private Button _button;
        private Image _buttonImage;

        private void Awake()
        {
            _button      = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
        }

        private void Start()
        {
            Hide();

            _button.onClick.AddListener(() =>
            {
                SwapSprite();
                _normalMenu.SetVisibility(_buttonImage.sprite == _openSprite);
            });
        }

        private void OnDisable() => Hide();

        private void OnDestroy() => _button.onClick.RemoveAllListeners();

        public void Show()
        {
            _buttonImage.sprite = _openSprite;
            _buttonImage.color  = Color.white;
        }

        public void Hide()
        {
            _buttonImage.sprite = _openSprite;
            _buttonImage.color  = Color.clear;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_buttonImage.sprite == _closeSprite)
                return;

            _buttonImage.color = Color.white;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_buttonImage.sprite == _closeSprite)
                return;

            _buttonImage.color = Color.clear;
        }

        private void SwapSprite() => _buttonImage.sprite = _buttonImage.sprite == _openSprite ? _closeSprite : _openSprite;
    }
}
