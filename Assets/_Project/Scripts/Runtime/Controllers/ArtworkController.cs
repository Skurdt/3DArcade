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
    public sealed class ArtworkController
    {
        //public event System.Action OnVideoPlayerAdded;

        public string DefaultMediaDirectory { get; private set; }

        private readonly IVirtualFileSystem _virtualFileSystem;
        private readonly AssetCache<Texture> _textureCache;
        //private readonly AssetCache<string> _videoCache;
        private readonly IArtworkDirectoriesResolver _directoriesResolver;

        public ArtworkController(IVirtualFileSystem virtualFileSystem, AssetCache<Texture> textureCache, IArtworkDirectoriesResolver directoriesResolver)
        {
            _virtualFileSystem   = virtualFileSystem;
            _textureCache        = textureCache;
            //_videoCache          = videoCache;
            _directoriesResolver = directoriesResolver;
        }

        public void Initialize() => DefaultMediaDirectory ??= _virtualFileSystem.GetDirectory("medias");

        public void SetupImages(IArtworkDirectoryNamesProvider directoryNamesProvider, ModelConfiguration modelConfiguration, string[] fileNamesToTry, Renderer[] renderers, float emissionIntensity)
        {
            if (modelConfiguration == null || fileNamesToTry == null || renderers == null)
                return;

            ArtworkDirectories gameDirectories     = directoryNamesProvider.GetModelImageDirectories(modelConfiguration);
            ArtworkDirectories platformDirectories = directoryNamesProvider.GetPlatformImageDirectories(modelConfiguration.PlatformConfiguration);
            ArtworkDirectories defaultDirectories  = directoryNamesProvider.DefaultImageDirectories;
            string[] directories                   = _directoriesResolver.GetDirectoriesToTry(gameDirectories, platformDirectories, defaultDirectories);

            Texture[] textures = _textureCache.LoadMultiple(directories, fileNamesToTry);
            if (textures == null || textures.Length == 0)
            {
                if (directoryNamesProvider is GenericArtworkDirectoryNamesProvider)
                    SetRandomColors(renderers);
                return;
            }

            for (int i = 0; i < renderers.Length; ++i)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", Color.black);
                block.SetColor("_EmissionColor", Color.white * emissionIntensity);
                block.SetTexture("_EmissionMap", textures[0]);
                renderers[i].SetPropertyBlock(block);

                // Only setup magic pixels for the first Marquee Node found
                //if (i == 0 && directoryNamesProvider is MarqueeArtworkDirectoryNamesProvider)
                //    SetupMagicPixels(renderers[i]);
            }

            //if (textures.Length == 1)
            //    SetupStaticImage(renderers, textures[0]);
            //else if (textures.Length > 1)
            //    SetupImageCycling(renderers, textures);
        }

        private void SetRandomColors(Renderer[] renderers)
        {
            Color color = Random.ColorHSV();

            foreach (Renderer renderer in renderers)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(block);
            }
        }

        //private static void SetupMagicPixels(Renderer sourceRenderer)
        //{
        //    Transform parentTransform = sourceRenderer.transform.parent;
        //    if (parentTransform == null)
        //        return;

        //    IEnumerable<Renderer> renderers = parentTransform.GetComponentsInChildren<Renderer>()
        //                                                     .Where(r => r.GetComponent<NodeTag>() == null
        //                                                              && sourceRenderer.sharedMaterial.name.StartsWith(r.sharedMaterial.name));

        //    Color color;
        //    Texture texture;

        //    bool sourceIsEmissive = sourceRenderer.IsEmissive();

        //    if (sourceIsEmissive)
        //    {
        //        color = sourceRenderer.material.GetEmissionColor();
        //        texture = sourceRenderer.material.GetEmissionTexture();
        //    }
        //    else
        //    {
        //        color = sourceRenderer.material.GetBaseColor();
        //        texture = sourceRenderer.material.GetBaseTexture();
        //    }

        //    if (texture == null)
        //        return;

        //    MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        //    foreach (Renderer renderer in renderers)
        //    {
        //        renderer.GetPropertyBlock(materialPropertyBlock);

        //        if (renderer.IsEmissive())
        //        {
        //            materialPropertyBlock.SetColor(MaterialUtils.SHADER_EMISSIVE_COLOR_ID, color);
        //            materialPropertyBlock.SetTexture(MaterialUtils.SHADER_EMISSIVE_TEXTURE_ID, texture);
        //        }
        //        else
        //        {
        //            color = sourceIsEmissive ? Color.white : color;
        //            materialPropertyBlock.SetColor(MaterialUtils.SHADER_BASE_COLOR_ID, color);
        //            materialPropertyBlock.SetTexture(MaterialUtils.SHADER_BASE_TEXTURE_ID, texture);
        //        }

        //        for (int i = 0; i < renderer.materials.Length; ++i)
        //            renderer.SetPropertyBlock(materialPropertyBlock, i);
        //    }
        //}

        //public static void SetupVideos(IEnumerable<string> directories, IEnumerable<string> namesToTry, Renderer[] renderers, float audioMinDistance, float audioMaxDistance, AnimationCurve volumeCurve)
        //{
        //    string videopath = _videoCache.Load(directories, namesToTry);
        //    if (string.IsNullOrEmpty(videopath))
        //        return;

        //    foreach (Renderer renderer in renderers)
        //    {
        //        AudioSource audioSource  = renderer.gameObject.AddComponentIfNotFound<AudioSource>();
        //        audioSource.playOnAwake  = false;
        //        audioSource.dopplerLevel = 0f;
        //        audioSource.spatialBlend = 1f;
        //        audioSource.minDistance  = audioMinDistance;
        //        audioSource.maxDistance  = audioMaxDistance;
        //        audioSource.volume       = 1f;
        //        audioSource.rolloffMode  = AudioRolloffMode.Custom;
        //        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);

        //        VideoPlayer videoPlayer               = renderer.gameObject.AddComponentIfNotFound<VideoPlayer>();
        //        videoPlayer.errorReceived            -= OnVideoPlayerErrorReceived;
        //        videoPlayer.errorReceived            += OnVideoPlayerErrorReceived;
        //        videoPlayer.playOnAwake               = false;
        //        videoPlayer.waitForFirstFrame         = true;
        //        videoPlayer.isLooping                 = true;
        //        videoPlayer.skipOnDrop                = true;
        //        videoPlayer.source                    = VideoSource.Url;
        //        videoPlayer.url                       = videopath;
        //        videoPlayer.renderMode                = VideoRenderMode.MaterialOverride;
        //        videoPlayer.targetMaterialProperty    = MaterialUtils.SHADER_EMISSIVE_TEXTURE_NAME;
        //        videoPlayer.audioOutputMode           = VideoAudioOutputMode.AudioSource;
        //        videoPlayer.controlledAudioTrackCount = 1;
        //        videoPlayer.Stop();

        //        OnVideoPlayerAdded?.Invoke();
        //    }
        //}

        //private static void SetupImageCycling(Renderer[] renderers, Texture[] textures)
        //{
        //    foreach (Renderer renderer in renderers)
        //    {
        //        DynamicArtworkComponent dynamicArtworkComponent = renderer.gameObject.AddComponentIfNotFound<DynamicArtworkComponent>();
        //        dynamicArtworkComponent.Construct(textures);
        //    }
        //}

        //private static void SetupStaticImage(Renderer[] renderers, Texture texture)
        //{
        //    foreach (Renderer renderer in renderers)
        //        renderer.material.SetEmissionTexture(texture);
        //}

        //private static void OnVideoPlayerErrorReceived(VideoPlayer _, string message)
        //    => Debug.LogError($"OnVideoPlayerErrorReceived: {message}");
    }
}
