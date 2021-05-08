﻿/* MIT License

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
using UnityEngine.InputSystem;

namespace Arcade
{
    public sealed class ArcadeEditModeGrabState : ArcadeEditModeState
    {
        public ArcadeEditModeGrabState(ArcadeEditModeContext context)
        : base(context)
        {
        }

        public override void OnEnter()
        {
            Debug.Log($">> <color=green>Entered</color> {GetType().Name}");

            _context.InteractionController.InteractionData.InitGrabMode();

            _context.UITransitionEvent.Raise(typeof(UIDisabledState));
        }

        public override void OnExit()
        {
            Debug.Log($">> <color=orange>Exited</color> {GetType().Name}");

            _context.InteractionController.InteractionData.RestoreValues();
        }

        public override void OnUpdate(float dt)
        {
            bool useMousePosition = Mouse.current != null && Cursor.lockState != CursorLockMode.Locked;
            Vector2 rayPosition   = useMousePosition ? Mouse.current.position.ReadValue() : _context.InteractionController.InteractionData.ScreenPoint;
            Ray ray               = _context.InteractionRaycaster.Camera.ScreenPointToRay(rayPosition);
            _context.InteractionController.AutoMoveAndRotate(ray, _context.Player.ActiveTransform.forward, _context.InteractionRaycaster.RaycastMaxDistance, _context.InteractionRaycaster.WorldRaycastLayerMask);

            if (_context.InputActions.FpsMoveCab.Grab.triggered)
                _context.TransitionToPrevious();
        }
    }
}
