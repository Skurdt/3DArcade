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
    [DisallowMultipleComponent, RequireComponent(typeof(Renderer))]
    public sealed class DynamicArtworkComponent : MonoBehaviour
    {
        [SerializeField] private bool _useRandomDelay                  = true;
        [SerializeField] private bool _changeRandomDelayAfterTimerEnds = true;

        public bool UseRandomDelay
        {
            get => _useRandomDelay;
            set
            {
                _useRandomDelay    = value;
                _imageCyclingDelay = Random.Range(DEFAULT_MIN_DELAY, DEFAULT_MAX_DELAY);
            }
        }

        public bool ChangeRandomDelayAfterTimerEnds
        {
            get => _changeRandomDelayAfterTimerEnds;
            set => _changeRandomDelayAfterTimerEnds = value;
        }

        private const float DEFAULT_MIN_DELAY = 0.4f;
        private const float DEFAULT_MAX_DELAY = 1.6f;

        private bool VideoIsPlaying => _videoPlayer != null && _videoPlayer.enabled && _videoPlayer.isPlaying;

        private Texture[] _imageCyclingTextures;
        private float _imageCyclingDelay = DEFAULT_MIN_DELAY;
        private float _emissionIntensity = 1f;

        private int _imageCyclingIndex   = 0;
        private float _imageCyclingTimer = 0f;

        private Renderer _renderer;
        private VideoPlayer _videoPlayer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        private void OnEnable() => ArtworkController.OnVideoPlayerAdded += VideoPlayerAddedCallback;

        private void OnDisable() => ArtworkController.OnVideoPlayerAdded -= VideoPlayerAddedCallback;

        private void Update()
        {
            if (VideoIsPlaying)
            {
                _imageCyclingTimer = _imageCyclingDelay;
                return;
            }

            if ((_imageCyclingTimer += Time.deltaTime) >= _imageCyclingDelay)
            {
                SwapTexture();

                if (_useRandomDelay && _changeRandomDelayAfterTimerEnds)
                    RandomizeDelay();

                _imageCyclingTimer = 0f;
            }
        }

        public void Construct(Texture[] textures, float emissionIntensity)
        {
            _emissionIntensity    = emissionIntensity;
            _imageCyclingTextures = textures;
            _imageCyclingIndex    = 0;
            _imageCyclingTimer    = 0f;

             if (_useRandomDelay)
                RandomizeDelay();

           SwapTexture();
        }

        private void SwapTexture()
        {
            if (!_renderer.enabled)
                return;

            _imageCyclingIndex++;
            _imageCyclingIndex %= _imageCyclingTextures.Length;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(block);
            if (_renderer.material.IsKeywordEnabled(ArtworkController.ShaderEmissionKeyword))
            {
                block.SetColor(ArtworkController.ShaderBaseColorId, Color.black);
                block.SetColor(ArtworkController.ShaderEmissionColorId, Color.white * _emissionIntensity);
                block.SetTexture(ArtworkController.ShaderEmissionMapId, _imageCyclingTextures[_imageCyclingIndex]);
            }
            else
            {
                block.SetColor(ArtworkController.ShaderBaseColorId, Color.white);
                block.SetTexture(ArtworkController.ShaderBaseMapId, _imageCyclingTextures[_imageCyclingIndex]);
            }
            _renderer.SetPropertyBlock(block);
        }

        private void RandomizeDelay() => _imageCyclingDelay = Random.Range(DEFAULT_MIN_DELAY, DEFAULT_MAX_DELAY);

        private void VideoPlayerAddedCallback() => _videoPlayer = GetComponent<VideoPlayer>();
    }
}
