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
    public sealed class ArcadeVirtualRealityExternalGameState : ArcadeExternalGameState
    {
        [System.NonSerialized] private ScreenNodeTag _screenNode;
        [System.NonSerialized] private Material _savedMaterial;

        protected override void OnEnterState()
        {
            Debug.Log($">> <color=green>Entered</color> {GetType().Name}");

            _screenNode = Context.Interactions.Normal.CurrentTarget.GetComponentInChildren<ScreenNodeTag>();
            if (_screenNode == null)
            {
                Context.TransitionToPrevious();
                return;
            }

            if (_screenNode.TryGetComponent(out VideoPlayer videoPlayer))
                Context.VideoPlayerController.Value.StopVideo(videoPlayer);

            if (_screenNode.TryGetComponent(out DynamicArtworkComponent dynamicArtworkComponent))
                dynamicArtworkComponent.enabled = false;

            Renderer renderer = _screenNode.GetComponent<Renderer>();
            _savedMaterial    = renderer.material;
            renderer.material = Context.GameControllers.External.ScreenMaterial;

            uDesktopDuplication.Texture uddTexture = _screenNode.gameObject.AddComponent<uDesktopDuplication.Texture>();
            uddTexture.InvertY = true;
        }

        protected override void OnExitState()
        {
            Debug.Log($">> <color=orange>Exited</color> {GetType().Name}");

            if (_screenNode == null)
                return;

            if (_screenNode.TryGetComponent(out uDesktopDuplication.Texture uddTexture))
                Destroy(uddTexture);

            Renderer renderer = _screenNode.GetComponent<Renderer>();
            renderer.material = _savedMaterial;

            if (_screenNode.TryGetComponent(out DynamicArtworkComponent dynamicArtworkComponent))
                dynamicArtworkComponent.enabled = true;
        }
    }
}
