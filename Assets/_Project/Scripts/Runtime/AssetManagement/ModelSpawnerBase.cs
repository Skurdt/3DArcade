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

using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Arcade
{
    public abstract class ModelSpawnerBase : ScriptableObject
    {
        [SerializeField] private ArcadeContext _arcadeContext;

        public async UniTask<ModelConfigurationComponent[]> SpawnGamesAsync(bool dissolveEffect)
        {
            ArcadeConfiguration arcadeConfiguration  = _arcadeContext.ArcadeConfiguration.Value;
            ModelConfiguration[] modelConfigurations = arcadeConfiguration.Games;
            if (modelConfigurations is null || modelConfigurations.Length == 0)
                return null;

            List<ModelConfigurationComponent> result = new List<ModelConfigurationComponent>();
            foreach (ModelConfiguration modelConfiguration in modelConfigurations)
            {
                GameObject go = await SpawnGameAsync(modelConfiguration, _arcadeContext.Scenes.Entities.GamesNodeTransform, dissolveEffect);
                result.Add(go.GetComponent<ModelConfigurationComponent>());
            }
            return result.ToArray();
        }

        public async UniTask<ModelConfigurationComponent[]> SpawPropsAsync(bool dissolveEffect)
        {
            ArcadeConfiguration arcadeConfiguration  = _arcadeContext.ArcadeConfiguration.Value;
            ModelConfiguration[] modelConfigurations = arcadeConfiguration.ArcadeType switch
            {
                ArcadeType.Fps => arcadeConfiguration.FpsArcadeProperties.Props,
                ArcadeType.Cyl => arcadeConfiguration.CylArcadeProperties.Props,
                _ => throw new System.NotImplementedException($"Unhandled switch case for ArcadeType: {arcadeConfiguration.ArcadeType}"),
            };
            if (modelConfigurations is null || modelConfigurations.Length == 0)
                return null;

            return await SpawnPropsAsync(modelConfigurations, dissolveEffect);
        }

        public async UniTask<GameObject> SpawnGameAsync(ModelConfiguration modelConfiguration, Vector3 position, Quaternion rotation, bool dissolveEffect)
        {
            AssetAddresses addressesToTry = ProcessGameConfigurationAndGetAddressesToTry(modelConfiguration);
            return await SpawnModelAsync(modelConfiguration, position, rotation, _arcadeContext.Scenes.Entities.GamesNodeTransform, EntitiesScene.GamesLayer, addressesToTry, dissolveEffect, true);
        }

        protected abstract UniTask<GameObject> SpawnAsync(AssetAddresses addressesToTry, Vector3 position, Quaternion orientation, Transform parent, bool dissolveEffect);

        private async UniTask<GameObject> SpawnGameAsync(ModelConfiguration modelConfiguration, Transform parent, bool dissolveEffect)
        {
            AssetAddresses addressesToTry = ProcessGameConfigurationAndGetAddressesToTry(modelConfiguration);
            return await SpawnModelAsync(modelConfiguration, parent, EntitiesScene.GamesLayer, _arcadeContext.ArcadeController.Value.GameModelsSpawnAtPositionWithRotation, addressesToTry, dissolveEffect, true);
        }

        private async UniTask<ModelConfigurationComponent[]> SpawnPropsAsync(ModelConfiguration[] modelConfigurations, bool dissolveEffect)
        {
            List<ModelConfigurationComponent> result = new List<ModelConfigurationComponent>();
            foreach (ModelConfiguration modelConfiguration in modelConfigurations)
            {
                GameObject go = await SpawnPropAsync(modelConfiguration, _arcadeContext.Scenes.Entities.PropsNodeTransform, dissolveEffect);
                result.Add(go.GetComponent<ModelConfigurationComponent>());
            }
            return result.ToArray();
        }

        private async UniTask<GameObject> SpawnPropAsync(ModelConfiguration modelConfiguration, Transform parent, bool dissolveEffect)
        {
            AssetAddresses addressesToTry = _arcadeContext.AssetAddressesProviders.Prop.GetAddressesToTry(modelConfiguration);
            return await SpawnModelAsync(modelConfiguration, parent, EntitiesScene.PropsLayer, true, addressesToTry, dissolveEffect, false);
        }

        private async UniTask<GameObject> SpawnModelAsync(ModelConfiguration modelConfiguration, Transform parent, int layer, bool spawnAtPositionWithRotation, AssetAddresses addressesToTry, bool dissolveEffect, bool applyArtworks)
        {
            Vector3 position;
            Quaternion rotation;

            if (spawnAtPositionWithRotation)
            {
                position = modelConfiguration.Position;
                rotation = Quaternion.Euler(modelConfiguration.Rotation);
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }

            return await SpawnModelAsync(modelConfiguration, position, rotation, parent, layer, addressesToTry, dissolveEffect, applyArtworks);
        }

        private async UniTask<GameObject> SpawnModelAsync(ModelConfiguration modelConfiguration, Vector3 position, Quaternion rotation, Transform parent, int layer, AssetAddresses addressesToTry, bool dissolveEffect, bool applyArtworks)
        {
            GameObject go = await SpawnAsync(addressesToTry, position, rotation, parent, dissolveEffect);
            if (go == null)
                return null;

            go.AddComponent<ModelConfigurationComponent>()
              .InitialSetup(modelConfiguration, layer);

            if (_arcadeContext.GeneralConfiguration.Value.EnableVR)
                _ = go.AddComponent<XRSimpleInteractable>();
            else
            {
                CinemachineNewVirtualCamera vCam = go.GetComponentInChildren<CinemachineNewVirtualCamera>();
                if (vCam != null)
                    vCam.Priority = 0;
            }

            if (applyArtworks)
                ApplyArtworksAsync(go, modelConfiguration).Forget();

            return go;
        }

        private AssetAddresses ProcessGameConfigurationAndGetAddressesToTry(ModelConfiguration modelConfiguration)
        {
            GameConfiguration game = null;
            if (_arcadeContext.Databases.Platforms.TryGet(modelConfiguration.Platform, out PlatformConfiguration platform))
            {
                string[] returnFields = new string[]
                {
                    "Description",
                    "CloneOf",
                    "RomOf",
                    "Genre",
                    "Year",
                    "Manufacturer",
                    "ScreenType",
                    "ScreenRotation",
                    "Mature",
                    "Playable",
                    "IsBios",
                    "IsDevice",
                    "IsMechanical",
                    "Available",
                };
                string[] searchFields = new string[] { "Name" };
                _ = _arcadeContext.Databases.Games.TryGet(platform.MasterList, modelConfiguration.Id, returnFields, searchFields, out game);
            }

            modelConfiguration.PlatformConfiguration = platform;
            modelConfiguration.GameConfiguration     = game;

            AssetAddresses addressesToTry = _arcadeContext.AssetAddressesProviders.Game.GetAddressesToTry(modelConfiguration);
            return addressesToTry;
        }

        private async UniTaskVoid ApplyArtworksAsync(GameObject gameObject, ModelConfiguration modelConfiguration)
        {
            // Look for artworks only in play mode / runtime
            if (!Application.isPlaying)
                return;

            ArcadeController arcadeController = _arcadeContext.ArcadeController.Value;
            NodeControllers nodeControllers   = _arcadeContext.NodeControllers;

            await nodeControllers.Marquee.Setup(arcadeController, gameObject, modelConfiguration, arcadeController.RenderSettings.MarqueeIntensity);

            float screenIntensity = GetScreenIntensity(modelConfiguration.GameConfiguration);
            await nodeControllers.Screen.Setup(arcadeController, gameObject, modelConfiguration, screenIntensity);

            await nodeControllers.Generic.Setup(arcadeController, gameObject, modelConfiguration, 1f);
        }

        private float GetScreenIntensity(GameConfiguration game)
        {
            if (game is null)
                return 1f;

            RenderSettings renderSettings = _arcadeContext.ArcadeController.Value.RenderSettings;

            return game.ScreenType switch
            {
                GameScreenType.Default => 1f,
                GameScreenType.Lcd     => renderSettings.ScreenLcdIntensity,
                GameScreenType.Raster  => renderSettings.ScreenRasterIntensity,
                GameScreenType.Svg     => renderSettings.ScreenSvgIntensity,
                GameScreenType.Vector  => renderSettings.ScreenVectorIntenstity,
                _                      => throw new System.NotImplementedException($"Unhandled switch case for GameScreenType: {game.ScreenType}")
            };
        }
    }
}
