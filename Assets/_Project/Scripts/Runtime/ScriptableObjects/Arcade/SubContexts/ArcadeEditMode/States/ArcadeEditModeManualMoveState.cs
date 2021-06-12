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
        [SerializeField] private float _rotationScale = 240;

        [System.NonSerialized] private Vector2 _aimPosition;
        [System.NonSerialized] private float _aimRotation;

        public override void OnEnter()
        {
            Debug.Log($">> <color=green>Entered</color> {GetType().Name}");

            Context.Interactions.EditPositions.Reset();
        }

        public override void OnExit() => Debug.Log($">> <color=orange>Exited</color> {GetType().Name}");

        public override void OnUpdate(float dt)
        {
            InputActions inputActions = Context.ArcadeContext.InputActions;

            if (inputActions.Global.Quit.triggered)
            {
                Context.ArcadeController.Value.RestoreModelPositions();
                Context.ArcadeContext.TransitionToPrevious();
                return;
            }

            if (inputActions.FpsNormal.EditPositions.triggered)
            {
                Context.ArcadeContext.SaveCurrentArcade(true);
                Context.ArcadeContext.TransitionToPrevious();
                return;
            }

            EditPositionsInteractions editPositions = Context.Interactions.EditPositions;
            editPositions.UpdateCurrentTarget(Context.ArcadeContext.Player.Camera);
            ModelConfigurationComponent currentTarget = editPositions.CurrentTarget;
            if (currentTarget == null || !currentTarget.MoveCabMovable)
            {
                _aimPosition = Vector2.zero;
                _aimRotation = 0f;
                return;
            }

            Vector2 positionInput = inputActions.FpsEditPositions.Move.ReadValue<Vector2>();
            _aimPosition = dt * positionInput;

            float rotationInput = inputActions.FpsEditPositions.Rotate.ReadValue<float>() * _rotationScale;
            _aimRotation = dt * rotationInput;

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
            => Context.Interactions.EditPositions.ManualMoveAndRotate(_aimPosition, _aimRotation);
    }
}
