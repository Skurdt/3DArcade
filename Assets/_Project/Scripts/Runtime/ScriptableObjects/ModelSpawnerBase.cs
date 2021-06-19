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
using System.Collections.Generic;
using UnityEngine;

namespace Arcade
{
    public abstract class ModelSpawnerBase : ScriptableObject
    {
        [SerializeField] private ArcadeContext _arcadeContext;
        [SerializeField] private ArcadeConfigurationVariable _arcadeConfiguration;
        [SerializeField] private Scenes _scenes;
        [SerializeField] private ArcadeControllerVariable _arcadeController;
        [SerializeField] private GeneralConfigurationVariable _generalConfiguration;
        [SerializeField] private Databases _databases;
        [SerializeField] private Material _dissolveMaterial;

        public async UniTask<GameEntity[]> SpawnGamesAsync()
        {
            GameEntityConfiguration[] configurations = _arcadeConfiguration.Value.Games;
            if (configurations is null || configurations.Length == 0)
                return null;

            List<GameEntity> result = new List<GameEntity>();
            foreach (GameEntityConfiguration configuration in configurations)
            {
                configuration.AssignConfigurations(_databases);
                AssetAddresses addressesToTry = _arcadeContext.AssetAddressesProviders.Game.GetAddressesToTry(configuration);
                GameEntity entity = await SpawnModelAsync<GameEntityConfiguration, GameEntity>(configuration, _scenes.Entities.GamesNodeTransform, EntitiesScene.GamesLayer, _arcadeController.Value.GameModelsSpawnAtPositionWithRotation, addressesToTry);
                result.Add(entity);
            }
            return result.ToArray();
        }

        public async UniTask<PropEntity[]> SpawPropsAsync()
        {
            ArcadeConfiguration arcadeConfiguration  = _arcadeConfiguration.Value;
            PropEntityConfiguration[] configurations = arcadeConfiguration.ArcadeType switch
            {
                ArcadeType.Fps => arcadeConfiguration.FpsArcadeProperties.Props,
                ArcadeType.Cyl => arcadeConfiguration.CylArcadeProperties.Props,
                _              => throw new System.NotImplementedException($"Unhandled switch case for ArcadeType: {arcadeConfiguration.ArcadeType}"),
            };
            if (configurations is null || configurations.Length == 0)
                return null;

            List<PropEntity> result = new List<PropEntity>();
            foreach (PropEntityConfiguration configuration in configurations)
            {
                AssetAddresses addressesToTry = _arcadeContext.AssetAddressesProviders.Prop.GetAddressesToTry(configuration);
                PropEntity entity = await SpawnModelAsync<PropEntityConfiguration, PropEntity>(configuration, _scenes.Entities.PropsNodeTransform, EntitiesScene.PropsLayer, true, addressesToTry);
                result.Add(entity);
            }
            return result.ToArray();
        }

        public async UniTask<GameEntity> SpawnGameAsync(GameEntityConfiguration configuration, Vector3 position, Quaternion rotation)
        {
            configuration.AssignConfigurations(_databases);
            AssetAddresses addressesToTry = _arcadeContext.AssetAddressesProviders.Game.GetAddressesToTry(configuration);
            return await SpawnModelAsync<GameEntityConfiguration, GameEntity>(configuration, position, rotation, _scenes.Entities.GamesNodeTransform, EntitiesScene.GamesLayer, addressesToTry);
        }

        public async UniTask<PropEntity> SpawnPropAsync(PropEntityConfiguration configuration, Vector3 position, Quaternion rotation)
        {
            AssetAddresses addressesToTry = _arcadeContext.AssetAddressesProviders.Prop.GetAddressesToTry(configuration);
            return await SpawnModelAsync<PropEntityConfiguration, PropEntity>(configuration, position, rotation, _scenes.Entities.PropsNodeTransform, EntitiesScene.PropsLayer, addressesToTry);
        }

        protected abstract UniTask<GameObject> SpawnAsync(AssetAddresses addressesToTry, Vector3 position, Quaternion orientation, Transform parent);

        private async UniTask<TResult> SpawnModelAsync<TParam, TResult>(TParam configuration,
                                                                        Transform parent,
                                                                        int layer,
                                                                        bool spawnAtPositionWithRotation,
                                                                        AssetAddresses addressesToTry)
            where TParam : EntityConfigurationBase
            where TResult : EntityBase<TParam>
        {
            Vector3 position;
            Quaternion rotation;

            if (spawnAtPositionWithRotation)
            {
                position = configuration.Position;
                rotation = Quaternion.Euler(configuration.Rotation);
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }

            return await SpawnModelAsync<TParam, TResult>(configuration, position, rotation, parent, layer, addressesToTry);
        }

        private async UniTask<TResult> SpawnModelAsync<TParam, TResult>(TParam modelConfiguration,
                                                                        Vector3 position,
                                                                        Quaternion rotation,
                                                                        Transform parent,
                                                                        int layer,
                                                                        AssetAddresses addressesToTry)
            where TParam : EntityConfigurationBase
            where TResult : EntityBase<TParam>
        {
            GameObject go = await SpawnAsync(addressesToTry, position, rotation, parent);
            if (go == null)
                return null;

            TResult component = go.AddComponent<TResult>();
            component.InitialSetup(modelConfiguration, layer, _generalConfiguration.Value.EnableVR);
            go.SetActive(false);
            return component;
        }
    }
}
