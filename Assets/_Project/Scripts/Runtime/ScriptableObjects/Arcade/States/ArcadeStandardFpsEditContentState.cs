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
    public sealed class ArcadeStandardFpsEditContentState : ArcadeState
    {
        public override void OnEnter()
        {
            Debug.Log($"> <color=green>Entered</color> {GetType().Name}");

            Context.VideoPlayerController.Value.StopAllVideos();

            SetupInput();

            Context.Interactions.Reset();

            Context.Interactions.EditContent.Initialize(Context.Player.Camera);

            Context.OnArcadeStateChanged.Raise(this);
        }

        public override void OnExit()
        {
            Debug.Log($"> <color=orange>Exited</color> {GetType().Name}");

            Context.Interactions.EditContent.DeInitialize();
        }

        public override void OnUpdate(float dt)
        {
            InputActions inputActions = Context.InputActions;

            if (inputActions.Global.ToggleCursor.triggered)
                HandleCursorToggle();

            EditContentInteractions editContent = Context.Interactions.EditContent;

            editContent.UpdateCurrentTarget(Context.Player.Camera);

            if (inputActions.FpsEditContent.ApplyOrAdd.triggered)
                editContent.ApplyChangesOrAddModel();

            if (inputActions.FpsEditContent.Remove.triggered)
                editContent.RemoveModel();

            if (inputActions.Global.Quit.triggered)
            {
                editContent.DestroyAddedItems();
                editContent.RestoreRemovedItems();
                Context.TransitionToPrevious();
                return;
            }

            if (inputActions.FpsNormal.EditContent.triggered)
            {
                editContent.DestroyRemovedItems();
                Context.SaveCurrentArcade(true);
                Context.TransitionToPrevious();
                return;
            }
        }

        public override void EnableInput()
        {
            Context.InputActions.Global.Enable();
            Context.InputActions.Global.Reload.Disable();
            Context.InputActions.Global.Restart.Disable();
            Context.InputActions.FpsNormal.Enable();
            if (Cursor.lockState != CursorLockMode.Locked)
                Context.InputActions.FpsNormal.Look.Disable();
            Context.InputActions.FpsEditContent.Enable();
        }

        private void SetupInput()
        {
            DisableInput();
            EnableInput();
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
