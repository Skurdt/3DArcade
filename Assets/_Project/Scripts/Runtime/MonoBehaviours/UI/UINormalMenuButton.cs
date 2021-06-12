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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent, RequireComponent(typeof(Image), typeof(Button))]
    public sealed class UINormalMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private UINormalMenu _normalMenu;
        private FloatVariable _animationDuration;
        private Image _image;
        private Button _button;
        private Color _hiddenColor = new Color(1f, 1f, 1f, 0f);

        [Inject]
        public void Construct(UINormalMenu normalMenu, FloatVariable animationDuration)
        {
            _normalMenu        = normalMenu;
            _animationDuration = animationDuration;
            _image             = GetComponent<Image>();
            _button            = GetComponent<Button>();

            _button.onClick.AddListener(() => _normalMenu.Toggle());
        }

        private void OnDestroy() => _button.onClick.RemoveAllListeners();

        public void OnPointerEnter(PointerEventData eventData) => _image.DOFade(1f, _animationDuration.Value).SetEase(Ease.OutCubic);

        public void OnPointerExit(PointerEventData eventData) => _image.DOFade(0f, _animationDuration.Value).SetEase(Ease.OutCubic);

        public void Enable()
        {
            _image.color = _hiddenColor;
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            _image.color = _hiddenColor;
        }
    }
}
