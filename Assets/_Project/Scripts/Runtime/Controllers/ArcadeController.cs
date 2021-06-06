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

namespace Arcade
{
    public abstract class ArcadeController
    {
        public ModelSpawnerBase ModelSpawner { get; private set; }
        public bool Loaded { get; private set; } = false;

        public abstract float AudioMinDistance { get; protected set; }
        public abstract float AudioMaxDistance { get; protected set; }
        public abstract AnimationCurve VolumeCurve { get; protected set; }
        public abstract CameraSettings CameraSettings { get; }

        public abstract RenderSettings RenderSettings { get; }
        public abstract bool GameModelsSpawnAtPositionWithRotation { get; }

        protected readonly ArcadeContext _arcadeContext;

        private ModelConfigurationComponent[] _gameModels;
        private ModelConfigurationComponent[] _propModels;

        public ArcadeController(ArcadeContext arcadeContext) => _arcadeContext = arcadeContext;

        public async UniTask Initialize(ModelSpawnerBase modelSpawner)
        {
            Loaded = false;

            ModelSpawner = modelSpawner;

            SetupPlayer();
            _arcadeContext.Player.SaveTransformState();

            _gameModels = await ModelSpawner.SpawnGamesAsync(false);
            _propModels = await ModelSpawner.SpawPropsAsync(false);

            ReflectionProbe[] probes = Object.FindObjectsOfType<ReflectionProbe>();
            foreach (ReflectionProbe probe in probes)
                _ = probe.RenderProbe();

            Loaded = true;
        }

        public void UpdateLists()
        {
            _gameModels = _arcadeContext.Scenes.Entities.GamesNodeTransform.GetComponentsInChildren<ModelConfigurationComponent>(false);
            _propModels = _arcadeContext.Scenes.Entities.PropsNodeTransform.GetComponentsInChildren<ModelConfigurationComponent>(false);
        }

        public void SaveTransformStates()
        {
            SaveTransformState(_gameModels);
            SaveTransformState(_propModels);
        }

        public void RestoreModelPositions()
        {
            RestoreTransformState(_gameModels);
            RestoreTransformState(_propModels);
        }

        protected abstract void SetupPlayer();

        private static void SaveTransformState(ModelConfigurationComponent[] models)
        {
            if (models is null)
                return;

            foreach (ModelConfigurationComponent model in models)
                model.SaveTransformState();
        }

        private static void RestoreTransformState(ModelConfigurationComponent[] models)
        {
            if (models is null)
                return;

            foreach (ModelConfigurationComponent model in models)
                model.RestoreTransformState();
        }
    }
}
