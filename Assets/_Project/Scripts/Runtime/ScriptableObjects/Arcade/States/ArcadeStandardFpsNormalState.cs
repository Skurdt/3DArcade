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
    public sealed class ArcadeStandardFpsNormalState : ArcadeState
    {
        public override void OnEnter()
        {
            Debug.Log($"> <color=green>Entered</color> {GetType().Name}");
            Context.OnArcadeStateChanged.Raise(this);

            Context.InputActions.Disable();
            Context.InputActions.Global.Enable();
            Context.InputActions.FpsNormal.Enable();
            if (Cursor.lockState != CursorLockMode.Locked)
                Context.InputActions.FpsNormal.Look.Disable();

            Context.Interactions.Reset();
        }

        public override void OnExit() => Debug.Log($"> <color=orange>Exited</color> {GetType().Name}");

        public override void OnUpdate(float dt)
        {
            if (Context.InputActions.Global.ToggleCursor.triggered)
                HandleCursorToggle();

            Context.VideoPlayerController.Value.UpdateVideosState();

            Context.Interactions.Normal.UpdateCurrentTarget(Context.Player.Camera);

            if (Cursor.lockState == CursorLockMode.Locked && Context.InputActions.FpsNormal.Interact.triggered)
            {
                Context.Interactions.Normal.HandleInteraction();
                return;
            }

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

            if (Context.InputActions.FpsNormal.EditPositions.triggered)
            {
                Context.TransitionTo<ArcadeStandardFpsEditPositionsState>();
                return;
            }

            if (Context.InputActions.FpsNormal.EditContent.triggered)
            {
                Context.TransitionTo<ArcadeStandardFpsEditContentState>();
                return;
            }
        }

        public override void EnableInput()
        {
            Context.InputActions.Global.Enable();
            Context.InputActions.FpsNormal.Enable();
            if (Cursor.lockState != CursorLockMode.Locked)
                Context.InputActions.FpsNormal.Look.Disable();
        }

        private void HandleCursorToggle()
        {
            CursorUtils.ToggleMouseCursor();
            if (Cursor.lockState == CursorLockMode.Locked)
                Context.InputActions.FpsNormal.Look.Enable();
            else
                Context.InputActions.FpsNormal.Look.Disable();
        }
    }
}
