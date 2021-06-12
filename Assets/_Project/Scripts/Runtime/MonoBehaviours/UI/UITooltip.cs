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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Image))]
    public sealed class UITooltip : MonoBehaviour
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _content;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private int _characterWrapLimit;

        private RectTransform _transform;

        private void Awake() => _transform = transform as RectTransform;

        private void Update()
        {
            if (Application.isEditor)
                AdjustLayout();

            if (!Application.isPlaying)
                return;

            Vector2 position = UITooltipSystem.GetMousePosition();

            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            _transform.pivot   = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        public void SetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
                _header.gameObject.SetActive(false);
            else
            {
                _header.gameObject.SetActive(true);
                _header.SetText(header);
            }

            _content.SetText(content);

            AdjustLayout();
        }

        private void AdjustLayout()
        {
            int headerLenght  = _header.text.Length;
            int contentLenght = _content.text.Length;
            _layoutElement.enabled = headerLenght > _characterWrapLimit || contentLenght > _characterWrapLimit;
        }
    }
}
