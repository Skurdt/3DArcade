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
    public sealed class ArcadeVirtualRealityLoadState : ArcadeState
    {
        public ArcadeVirtualRealityLoadState(ArcadeContext context)
        : base(context)
        {
        }

        public override void OnEnter()
        {
            Debug.Log($"> <color=green>Entered</color> {GetType().Name}");

            _context.UIManager.TransitionTo<UIVirtualRealitySceneLoadingState>();
            _context.UIManager.InitStatusBar($"Loading arcade: {_context.ArcadeConfiguration}...");
        }

        public override void OnExit()
        {
            Debug.Log($"> <color=orange>Exited</color> {GetType().Name}");

            _context.UIManager.TransitionTo<UIDisabledState>();
        }

        public override void OnUpdate(float dt)
        {
            if (!_context.Scenes.Arcade.Loaded)
            {
                float percentComplete = _context.Scenes.Arcade.LoadingPercentCompleted;
                _context.UIManager.UpdateStatusBar(percentComplete);
                return;
            }

            switch (_context.ArcadeConfiguration.ArcadeType)
            {
                case ArcadeType.Fps:
                    _context.TransitionTo<ArcadeVirtualRealityFpsState>();
                    break;
                case ArcadeType.Cyl:
                    _context.TransitionTo<ArcadeVirtualRealityCylState>();
                    break;
                default:
                    break;
            }
        }
    }
}
