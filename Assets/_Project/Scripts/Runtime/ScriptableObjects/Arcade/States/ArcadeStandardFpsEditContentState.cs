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

            Context.InteractionControllers.ResetControllers();

            Context.InteractionControllers.EditModeEditContentController.Initialize();

            Context.ArcadeStateChangeEvent.Raise(this);
        }

        public override void OnExit() => Debug.Log($"> <color=orange>Exited</color> {GetType().Name}");

        public override void OnUpdate(float dt)
        {
            InputActions inputActions = Context.InputActions;

            if (inputActions.Global.ToggleCursor.triggered)
                HandleCursorToggle();

            InteractionControllers interactionControllers               = Context.InteractionControllers;
            EditModeEditContentInteractionController editModeController = interactionControllers.EditModeEditContentController;

            editModeController.UpdateCurrentTarget(Context.Player.Value.Camera);

            if (inputActions.FpsEditContent.ApplyOrAdd.triggered)
                editModeController.ApplyChangesOrAddModel();

            if (inputActions.FpsEditContent.Remove.triggered)
                editModeController.RemoveModel();

            if (inputActions.Global.Quit.triggered)
            {
                editModeController.DestroyAddedItems();
                editModeController.RestoreRemovedItems();
                Context.TransitionToPrevious();
                return;
            }

            if (inputActions.FpsNormal.EditContent.triggered)
            {
                editModeController.DestroyRemovedItems();
                Context.SaveCurrentArcade(true);
                Context.TransitionToPrevious();
                return;
            }
        }

        public void EnableInput()
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
