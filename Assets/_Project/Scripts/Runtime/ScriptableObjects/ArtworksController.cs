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

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/ArtworksController", fileName = "ArtworksController")]
    public sealed class ArtworksController : ScriptableObject
    {
        public static event System.Action OnVideoPlayerAdded;

        public const string SHADER_EMISSION_KEYWORD      = "_EMISSION";
        public static readonly int ShaderBaseColorId     = Shader.PropertyToID("_BaseColor");
        public static readonly int ShaderBaseMapId       = Shader.PropertyToID("_BaseMap");
        public static readonly int ShaderEmissionColorId = Shader.PropertyToID("_EmissionColor");
        public static readonly int ShaderEmissionMapId   = Shader.PropertyToID("_EmissionMap");

        public static string DefaultMediaDirectory { get; private set; }

        private static readonly string[] _imageExtensions = new string[] { "png", "jpg", "jpeg" };
        private static readonly string[] _videoExtensions = new string[] { "mp4", "mkv", "avi" };

        [SerializeField] private VirtualFileSystem _virtualFileSystem;
        [SerializeField] private ArcadeContext _arcadeContext;

        public async UniTask SetupArtworksAsync(GameEntity entity)
        {
            if (entity == null || entity.Configuration is null)
                return;

            GameObject gameObject                 = entity.gameObject;
            GameEntityConfiguration configuration = entity.Configuration;

            if (DefaultMediaDirectory is null)
            {
                if (!_virtualFileSystem.TryGetDirectory("medias", out string defaultMediaDirectory))
                    throw new System.Exception("Directory 'medias' not mapped in VirtualFileSystem.");
                DefaultMediaDirectory = defaultMediaDirectory;
            }

            ArcadeController arcadeController = _arcadeContext.ArcadeController.Value;
            NodeControllers nodeControllers   = _arcadeContext.NodeControllers;

            float marqueeIntensity = arcadeController.RenderSettings.MarqueeIntensity;
            float screenIntensity  = configuration is GameEntityConfiguration gameEntityConfiguration ? GetScreenIntensity(gameEntityConfiguration) : 1f;
            float genericIntensity = 1f;

            UniTask marqueeSetupTask = SetupNodeAsync(nodeControllers.Marquee, arcadeController, gameObject, configuration, (float)marqueeIntensity);
            UniTask screenSetupTask  = SetupNodeAsync(nodeControllers.Screen, arcadeController, gameObject, configuration, screenIntensity);
            UniTask genericNodeTask  = SetupNodeAsync(nodeControllers.Generic, arcadeController, gameObject, configuration, genericIntensity);

            await UniTask.WhenAll(marqueeSetupTask, screenSetupTask, genericNodeTask);

            float GetScreenIntensity(GameEntityConfiguration gameEntityConfiguration)
            {
                if (gameEntityConfiguration.GameConfiguration is null)
                    return 1f;

                RenderSettings renderSettings = arcadeController.RenderSettings;

                return gameEntityConfiguration.GameConfiguration.ScreenType switch
                {
                    GameScreenType.Default => 1f,
                    GameScreenType.Lcd => renderSettings.ScreenLcdIntensity,
                    GameScreenType.Raster => renderSettings.ScreenRasterIntensity,
                    GameScreenType.Svg => renderSettings.ScreenSvgIntensity,
                    GameScreenType.Vector => renderSettings.ScreenVectorIntenstity,
                    _ => throw new System.NotImplementedException($"Unhandled switch case for GameScreenType: {gameEntityConfiguration.GameConfiguration.ScreenType}")
                };
            }
        }

        private async UniTask SetupNodeAsync<T>(NodeController<T> nodeController, ArcadeController arcadeController, GameObject gameObject, GameEntityConfiguration configuration, float emissionIntensity)
            where T : NodeTag
        {
            Renderer[] renderers = nodeController.GetNodeRenderers(gameObject);
            if (renderers is null || renderers.Length == 0)
                return;

            string[] namesToTry = _arcadeContext.FileNamesProvider.GetNamesToTry(configuration);
            if (namesToTry is null)
                return;

            await SetupImagesAsync(nodeController, configuration, namesToTry, renderers, emissionIntensity);
            await SetupVideosAsync(nodeController, configuration, namesToTry, renderers, arcadeController.AudioMinDistance, arcadeController.AudioMaxDistance, arcadeController.VolumeCurve);
        }

        private async UniTask SetupImagesAsync<T>(NodeController<T> nodeController, GameEntityConfiguration configuration, string[] fileNamesToTry, Renderer[] renderers, float emissionIntensity)
            where T : NodeTag
        {
            string[] gameDirectories     = nodeController.DirectoryNamesProvider.GetModelImageDirectories(configuration);
            string[] platformDirectories = nodeController.DirectoryNamesProvider.GetPlatformImageDirectories(configuration.PlatformConfiguration);
            Directories directories      = new Directories(gameDirectories, platformDirectories);

            Files files = new Files(directories, fileNamesToTry, _imageExtensions);

            if (files.Count == 0)
            {
                directories = new Directories(nodeController.DirectoryNamesProvider.DefaultImageDirectories);
                files       = new Files(directories, fileNamesToTry, _imageExtensions);
            }

            if (files.Count == 0)
                return;

            Texture[] textures = await _arcadeContext.TextureCache.LoadMultipleAsync(files);
            if (textures is null || textures.Length == 0)
            {
                if (nodeController.DirectoryNamesProvider is GenericArtworkDirectoriesProvider)
                    SetRandomColors(renderers);
                return;
            }

            SetupDynamicArtworkComponents(renderers, textures, emissionIntensity);
        }

        private async UniTask SetupVideosAsync<T>(NodeController<T> nodeController, GameEntityConfiguration configuration, string[] fileNamesToTry, Renderer[] renderers, float audioMinDistance, float audioMaxDistance, AnimationCurve volumeCurve)
            where T : NodeTag
        {
            if (configuration is null || fileNamesToTry is null || renderers is null)
                return;

            string[] gameDirectories     = nodeController.DirectoryNamesProvider.GetModelVideoDirectories(configuration);
            string[] platformDirectories = nodeController.DirectoryNamesProvider.GetPlatformVideoDirectories(configuration.PlatformConfiguration);
            Directories directories      = new Directories(gameDirectories, platformDirectories);

            Files files = new Files(directories, fileNamesToTry, _videoExtensions);

            if (files.Count == 0)
            {
                directories = new Directories(nodeController.DirectoryNamesProvider.DefaultVideoDirectories);
                files       = new Files(directories, fileNamesToTry, _videoExtensions);
            }

            if (files.Count == 0)
                return;

            foreach (Renderer renderer in renderers)
            {
                if (!renderer.gameObject.TryGetComponent(out AudioSource audioSource))
                    audioSource = renderer.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake  = false;
                audioSource.dopplerLevel = 0f;
                audioSource.spatialBlend = 1f;
                audioSource.minDistance  = audioMinDistance;
                audioSource.maxDistance  = audioMaxDistance;
                audioSource.volume       = 1f;
                audioSource.rolloffMode  = AudioRolloffMode.Custom;
                audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);

                if (!renderer.gameObject.TryGetComponent(out VideoPlayer videoPlayer))
                    videoPlayer = renderer.gameObject.AddComponent<VideoPlayer>();
                videoPlayer.errorReceived            -= OnVideoPlayerErrorReceived;
                videoPlayer.errorReceived            += OnVideoPlayerErrorReceived;
                videoPlayer.playOnAwake               = false;
                videoPlayer.waitForFirstFrame         = true;
                videoPlayer.isLooping                 = true;
                videoPlayer.skipOnDrop                = true;
                videoPlayer.source                    = VideoSource.Url;
                videoPlayer.url                       = files[0];
                videoPlayer.renderMode                = VideoRenderMode.MaterialOverride;
                videoPlayer.targetMaterialProperty    = "_EmissionMap";
                videoPlayer.audioOutputMode           = VideoAudioOutputMode.AudioSource;
                videoPlayer.controlledAudioTrackCount = 1;
                videoPlayer.Stop();

                OnVideoPlayerAdded?.Invoke();

                await UniTask.Yield();
            }
        }

        private static void SetRandomColors(Renderer[] renderers)
        {
            Color color = new Color(Random.value, Random.value, Random.value);

            foreach (Renderer renderer in renderers)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor(ShaderBaseColorId, color);
                renderer.SetPropertyBlock(block);
            }
        }

        private static void SetupDynamicArtworkComponents(Renderer[] renderers, Texture[] textures, float emissionIntensity)
        {
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.gameObject.TryGetComponent(out DynamicArtworkComponent dynamicArtworkComponent))
                    dynamicArtworkComponent = renderer.gameObject.AddComponent<DynamicArtworkComponent>();
                dynamicArtworkComponent.Construct(textures, emissionIntensity);
            }
        }

        private static void OnVideoPlayerErrorReceived(VideoPlayer videoPlayer, string message)
            => Debug.LogError($"OnVideoPlayerErrorReceived: {message}", videoPlayer);
    }
}
