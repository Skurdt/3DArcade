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
using UnityEngine.Video;

namespace Arcade
{
    public abstract class ArcadeInternalGameState : ArcadeState
    {
        [System.NonSerialized] private ScreenNodeTag _screenNode;
        [System.NonSerialized] private Renderer _screenRenderer;
        [System.NonSerialized] private Material _savedMaterial;
        [System.NonSerialized] private DynamicArtworkComponent _dynamicArtworkComponent;

        public sealed override void OnEnter()
        {
            Debug.Log($">> <color=green>Entered</color> {GetType().Name}");

            ModelConfigurationComponent currentTarget = Context.Interactions.Normal.CurrentTarget;
            if (currentTarget == null)
            {
                Context.TransitionToPrevious();
                return;
            }

            ScreenNodeTag screenNodeTag = currentTarget.GetComponentInChildren<ScreenNodeTag>(true);
            if (screenNodeTag == null)
            {
                Context.TransitionToPrevious();
                return;
            }

            if (screenNodeTag.TryGetComponent(out VideoPlayer videoPlayer))
                Context.VideoPlayerController.Value.StopVideo(videoPlayer);

            if (!Context.GameControllers.Internal.StartGame(screenNodeTag, currentTarget.Configuration))
            {
                Context.TransitionToPrevious();
                return;
            }

            _screenNode = screenNodeTag;

            _dynamicArtworkComponent = _screenNode.GetComponent<DynamicArtworkComponent>();
            if (_dynamicArtworkComponent != null)
                _dynamicArtworkComponent.enabled = false;

            _screenRenderer = _screenNode.GetComponent<Renderer>();
            _savedMaterial  = _screenRenderer.material;
            _screenRenderer.material = Context.GameControllers.Internal.ScreenMaterial;

            Context.InputActions.Disable();
            Context.InputActions.Global.Quit.Enable();

            OnStateEnter();
        }

        public sealed override void OnExit()
        {
            Debug.Log($">> <color=orange>Exited</color> {GetType().Name}");

            OnStateExit();

            Context.GameControllers.Internal.StopGame();

            if (_screenRenderer != null)
                _screenRenderer.material = _savedMaterial;

            if (_dynamicArtworkComponent != null)
                _dynamicArtworkComponent.enabled = true;
        }

        public sealed override void OnUpdate(float dt)
        {
            if (Context.InputActions.Global.Quit.triggered)
            {
                Context.TransitionToPrevious();
                return;
            }
        }

        protected virtual void OnStateEnter()
        {
        }

        protected virtual void OnStateExit()
        {
        }
    }
}
