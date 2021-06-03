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
using UnityEngine.EventSystems;

namespace Arcade
{
    public sealed class ArcadeEditModeManualMoveState : ArcadeEditModeState
    {
        [SerializeField] private float _movementSpeedMultiplier = 0.8f;
        [SerializeField] private float _rotationSpeedMultiplier = 0.8f;

        [System.NonSerialized] private Vector2 _aimPosition;
        [System.NonSerialized] private float _aimRotation;

        public override void OnEnter()
        {
            Debug.Log($">> <color=green>Entered</color> {GetType().Name}");

            Context.ArcadeContext.InteractionControllers.EditModeEditPositionsController.ResetData();
        }

        public override void OnExit() => Debug.Log($">> <color=orange>Exited</color> {GetType().Name}");

        public override void OnUpdate(float dt)
        {
            ArcadeContext arcadeContext = Context.ArcadeContext;
            InputActions inputActions   = arcadeContext.InputActions;

            if (inputActions.Global.Quit.triggered)
            {
                arcadeContext.ArcadeController.RestoreModelPositions();
                arcadeContext.TransitionToPrevious();
                return;
            }

            if (inputActions.FpsNormal.EditPositions.triggered)
            {
                arcadeContext.SaveCurrentArcade(true);
                arcadeContext.TransitionToPrevious();
                return;
            }

            InteractionControllers interactionControllers                 = arcadeContext.InteractionControllers;
            EditModeEditPositionsInteractionController editModeController = interactionControllers.EditModeEditPositionsController;

            editModeController.UpdateCurrentTarget(arcadeContext.Player.Value.Camera);

            ModelConfigurationComponent currentTarget = editModeController.InteractionData.Current;
            if (currentTarget == null || !currentTarget.MoveCabMovable)
            {
                _aimPosition = Vector2.zero;
                _aimRotation = 0f;
                return;
            }

            Vector2 positionInput = inputActions.FpsEditPositions.Move.ReadValue<Vector2>();
            float rotationInput   = inputActions.FpsEditPositions.Rotate.ReadValue<float>();
            _aimPosition          = positionInput * _movementSpeedMultiplier;
            _aimRotation          = rotationInput * _rotationSpeedMultiplier;

            bool mouseIsOverUI = EventSystem.current.IsPointerOverGameObject();
            if (mouseIsOverUI || !currentTarget.MoveCabGrabbable)
                return;

            if (inputActions.FpsEditPositions.Grab.triggered)
            {
                Context.TransitionTo<ArcadeEditModeAutoMoveState>();
                return;
            }
        }

        public override void OnFixedUpdate(float dt)
            => Context.ArcadeContext.InteractionControllers.EditModeEditPositionsController.ManualMoveAndRotate(_aimPosition, _aimRotation);
    }
}
