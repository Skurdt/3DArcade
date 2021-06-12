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

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcade
{
    public sealed class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string _header;
        [SerializeField, TextArea] private string _content;

        private static WaitForSecondsRealtime _waitForSecondsRealtime;

        private Coroutine _delayedCall;

        private void Awake()
        {
            if (_waitForSecondsRealtime == null)
                _waitForSecondsRealtime = new WaitForSecondsRealtime(0.8f);
        }

        public void OnPointerEnter(PointerEventData eventData) => _delayedCall = StartCoroutine(DelayedShow());

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine(_delayedCall);
            UITooltipSystem.Hide();
        }

        private IEnumerator DelayedShow()
        {
            yield return _waitForSecondsRealtime;
            UITooltipSystem.Show(_content, _header);
        }
    }
}
