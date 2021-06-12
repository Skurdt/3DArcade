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
using UnityEngine.InputSystem.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UITooltipSystem : MonoBehaviour
    {
        [SerializeField] private UITooltip _tooltip;
        [SerializeField] private InputSystemUIInputModule _inputModule;

        private static UITooltipSystem _instance;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _instance    = this;
            _canvasGroup = _instance.GetComponent<CanvasGroup>();
        }

        public static void Show(string content, string header)
        {
            _instance._tooltip.gameObject.SetActive(true);
            _instance._tooltip.SetText(content, header);
            _ = _instance._canvasGroup.DOKill();
            _ = _instance._canvasGroup.DOFade(1f, 0.4f)
                                      .SetEase(Ease.InOutCubic)
                                      .SetDelay(0.5f);
        }

        public static void Hide()
        {
            _ = _instance._canvasGroup.DOKill();
            _ = _instance._canvasGroup.DOFade(0f, 0.1f)
                                      .SetEase(Ease.InOutCubic)
                                      .OnComplete(() =>
                                      {
                                         _instance._tooltip.gameObject.SetActive(false);
                                         _instance._tooltip.SetText("", "");
                                      });
        }

        public static Vector2 GetMousePosition() => _instance != null ? _instance._inputModule.point.action.ReadValue<Vector2>() : Vector2.zero;
    }
}
