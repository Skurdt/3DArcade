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

using SK.Utilities.Unity;
using UnityEngine;

namespace Arcade
{
    public sealed class ArcadeVirtualRealityFpsNormalState : ArcadeState
    {
        public override void OnEnter()
        {
            Debug.Log($"> <color=green>Entered</color> {GetType().Name}");
            Context.ArcadeStateChangeEvent.Raise(this);

            Context.InputActions.Disable();
            Context.InputActions.Global.Enable();
        }

        public override void OnExit() => Debug.Log($"> <color=orange>Exited</color> {GetType().Name}");

        public override void OnUpdate(float dt)
        {
            Context.VideoPlayerController.Value.UpdateVideosState();

            if (Context.InputActions.Global.Quit.triggered)
            {
                ApplicationUtils.ExitApp();
                return;
            }

            if (Context.InputActions.Global.Reload.triggered)
            {
                Context.ReloadCurrentArcade();
                return;
            }

            if (Context.InputActions.Global.Restart.triggered)
            {
                Context.Restart();
                return;
            }
        }
    }
}
