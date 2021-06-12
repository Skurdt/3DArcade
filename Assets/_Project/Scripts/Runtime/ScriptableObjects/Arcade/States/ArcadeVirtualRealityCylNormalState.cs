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
    public sealed class ArcadeVirtualRealityCylNormalState : ArcadeState
    {
        //private float _timer        = 0f;
        //private float _acceleration = 1f;

        public override void OnEnter()
        {
            Debug.Log($"> <color=green>Entered</color> {GetType().Name}");
            Context.OnArcadeStateChanged.Raise(this);

            Context.InputActions.CylNormal.Enable();
            //if (!_context.Main.PlayerCylControls.MouseLookEnabled)
            //    _context.Main.PlayerCylControls.CylArcadeActions.Look.Disable();

            //_context.CurrentModelConfiguration = null;

            //_context.CurrentPlayerControls = _context.Main.PlayerCylControls;

            //switch (_context.CurrentSceneConfiguration.CylArcadeProperties.WheelVariant)
            //{
            //    case WheelVariant.CameraInsideHorizontal:
            //    case WheelVariant.CameraOutsideHorizontal:
            //    case WheelVariant.LineHorizontal:
            //    {
            //        _navigationInput = _context.Main.PlayerCylControls.CylArcadeActions.NavigationLeftRight;
            //        _context.Main.PlayerCylControls.SetupForHorizontalWheel();
            //    }
            //    break;
            //    case WheelVariant.CameraInsideVertical:
            //    case WheelVariant.CameraOutsideVertical:
            //    case WheelVariant.LineVertical:
            //    {
            //        _navigationInput = _context.Main.PlayerCylControls.CylArcadeActions.NavigationUpDown;
            //        _context.Main.PlayerCylControls.SetupForVerticalWheel();
            //    }
            //    break;
            //    case WheelVariant.LineCustom:
            //    {
            //        if (_context.CurrentSceneConfiguration.CylArcadeProperties.HorizontalNavigation)
            //        {
            //            _navigationInput = _context.Main.PlayerCylControls.CylArcadeActions.NavigationLeftRight;
            //            _context.Main.PlayerCylControls.SetupForHorizontalWheel();
            //        }
            //        else
            //        {
            //            _navigationInput = _context.Main.PlayerCylControls.CylArcadeActions.NavigationUpDown;
            //            _context.Main.PlayerCylControls.SetupForVerticalWheel();
            //        }
            //    }
            //    break;
            //}
        }

        public override void OnExit()
        {
            Debug.Log($"> <color=orange>Exited</color> {GetType().Name}");

            Context.InputActions.CylNormal.Disable();
        }

        public override void OnUpdate(float dt)
        {
            if (Context.InputActions.Global.Quit.triggered)
                ApplicationUtils.ExitApp();

            //    HandleNavigation(dt);

            //    if (_context.CurrentModelConfiguration != _context.ArcadeController.CurrentGame)
            //        UpdateCurrentInteractable();

            //    if (Cursor.lockState == CursorLockMode.Locked && _context.Main.PlayerCylControls.CylArcadeActions.Interact.triggered)
            //    {
            //        VideoPlayerController.StopVideo(_context.CurrentModelConfiguration);
            //        HandleInteraction();
            //    }
        }

        //private void UpdateCurrentInteractable()
        //{
        //    VideoPlayerController.StopVideo(_context.CurrentModelConfiguration);
        //    InteractionController.FindInteractable(ref _context.CurrentModelConfiguration, _context.ArcadeController);
        //    VideoPlayerController.PlayVideo(_context.CurrentModelConfiguration);
        //}

        //private void HandleNavigation(float dt)
        //{
        //    if (_navigationInput.phase != InputActionPhase.Started)
        //        return;

        //    float direction = _navigationInput.ReadValue<float>();

        //    if (_navigationInput.triggered)
        //    {
        //        _timer        = 0f;
        //        _acceleration = 1f;

        //        if (direction > 0f)
        //        {
        //            if (_context.CurrentSceneConfiguration.CylArcadeProperties.InverseNavigation)
        //                _context.ArcadeController.NavigateBackward(dt);
        //            else
        //                _context.ArcadeController.NavigateForward(dt);
        //        }
        //        else if (direction < 0f)
        //        {
        //            if (_context.CurrentSceneConfiguration.CylArcadeProperties.InverseNavigation)
        //                _context.ArcadeController.NavigateForward(dt);
        //            else
        //                _context.ArcadeController.NavigateBackward(dt);
        //        }
        //    }
        //    else if ((_timer += _acceleration * dt) > 1.0f)
        //    {
        //        _acceleration += 0.5f;
        //        _acceleration  = Mathf.Clamp(_acceleration, 1f, 20f);

        //        if (direction > 0f)
        //        {
        //            if (_context.CurrentSceneConfiguration.CylArcadeProperties.InverseNavigation)
        //                _context.ArcadeController.NavigateBackward(_acceleration * dt);
        //            else
        //                _context.ArcadeController.NavigateForward(_acceleration * dt);
        //        }
        //        else if (direction < 0f)
        //        {
        //            if (_context.CurrentSceneConfiguration.CylArcadeProperties.InverseNavigation)
        //                _context.ArcadeController.NavigateForward(_acceleration * dt);
        //            else
        //                _context.ArcadeController.NavigateBackward(_acceleration * dt);
        //        }

        //        _timer = 0f;
        //    }
        //}

        //private void HandleInteraction()
        //{
        //    if (_context.CurrentModelConfiguration == null)
        //        return;

        //    //if (_context.CurrentModelConfiguration.Grabbable)
        //    //{
        //    //    _context.TransitionTo<ArcadeGrabState>();
        //    //}
        //    //else
        //    {
        //        switch (_context.CurrentModelConfiguration.InteractionType)
        //        {
        //            case InteractionType.GameInternal:
        //                _context.TransitionTo<SceneInternalGameState>();
        //                break;
        //            case InteractionType.GameExternal:
        //                _context.TransitionTo<SceneExternalGameState>();
        //                break;
        //            case InteractionType.FpsArcadeConfiguration:
        //                _context.SetAndStartCurrentArcadeConfiguration(_context.CurrentModelConfiguration.Id, SceneType.Fps);
        //                break;
        //            case InteractionType.CylArcadeConfiguration:
        //                _context.SetAndStartCurrentArcadeConfiguration(_context.CurrentModelConfiguration.Id, SceneType.Cyl);
        //                break;
        //            case InteractionType.FpsMenuConfiguration:
        //            case InteractionType.CylMenuConfiguration:
        //            case InteractionType.URL:
        //            case InteractionType.Undefined:
        //            case InteractionType.Inherited:
        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
