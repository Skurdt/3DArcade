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

using SK.Utilities.Unity.StateMachine;
using UnityEngine;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/StateMachine/UIContext", fileName = "UIContext")]
    public sealed class UIContext : Context<UIState>
    {
        [field: System.NonSerialized] public UICanvasController StandardUI { get; private set; }
        [field: System.NonSerialized] public UICanvasController VirtualRealityUI { get; private set; }
        [field: System.NonSerialized] public bool ValidStandardUI { get; private set; }
        [field: System.NonSerialized] public bool ValidVirtualRealityUI { get; private set; }

        public void Initialize(UICanvasController standardUI,
                               UICanvasController virtualRealityUI)
        {
            StandardUI       = standardUI;
            VirtualRealityUI = virtualRealityUI;

            ValidStandardUI       = standardUI != null;
            ValidVirtualRealityUI = virtualRealityUI != null;
        }
    }
}
