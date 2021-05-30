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

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UICanvasController : MonoBehaviour
    {
        [SerializeField] private UILoading _loading;
        [SerializeField] private UINormal _normal;
        [SerializeField] private UIEditPositions _editPositions;
        [SerializeField] private UIEditContent _editContent;

        public void SetSceneLoadingUIVisibility(bool visible) => _loading.SetVisibility(visible);

        public void SetNormalUIVisibility(bool visible) => _normal.SetVisibility(visible);

        public void SetEditPositionsUIVisibility(bool visible) => _editPositions.SetVisibility(visible);

        public void SetEditContentUIVisibility(bool visible) => _editContent.SetVisibility(visible);

        public void Hide()
        {
            SetSceneLoadingUIVisibility(false);
            SetNormalUIVisibility(false);
            SetEditPositionsUIVisibility(false);
            SetEditContentUIVisibility(false);
        }
    }
}
