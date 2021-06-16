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
using SK.Utilities.Unity;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Interaction/EditContent", fileName = "EditContentInteractions")]
    public sealed class EditContentInteractions : InteractionsBase
    {
        [SerializeField] private ArcadeContext _arcadeContext;
        [SerializeField] private ArcadeControllerVariable _arcadeController;
        [SerializeField] private ModelConfigurationEvent _onRequestUpdatedModelConfiguration;
        [SerializeField] private GameObject _spawnIndicatorPrefab;
        [SerializeField] private Material _dissolveMaterial;

        [SerializeField, Layer] private int _hightlightLayer;
        [SerializeField] private float _spawnDistance  = 2f;
        [SerializeField] private float _verticalOffset = 0.05f;
        [SerializeField] private LayerMask _worldLayerMask;

        [field: System.NonSerialized] public ModelConfigurationComponent ReferenceTarget { get; private set; }

        private readonly List<ModelConfigurationComponent> _addedItems   = new List<ModelConfigurationComponent>();
        private readonly List<ModelConfigurationComponent> _removedItems = new List<ModelConfigurationComponent>();

        [System.NonSerialized] private Transform _player;
        [System.NonSerialized] private Camera _camera;
        [System.NonSerialized] private int _dissolvePropertyId;
        [System.NonSerialized] private GameObject _spawnIndicator;

        [System.NonSerialized] private bool _drawSpawnIndicator = true;
        [System.NonSerialized] private bool _spawning;

        public void Initialize(Camera camera)
        {
            _dissolvePropertyId = Shader.PropertyToID("_Dissolve");
            _spawnIndicator     = Instantiate(_spawnIndicatorPrefab);

            _arcadeContext.Databases.Initialize();

            _addedItems.Clear();
            _removedItems.Clear();

            _player = _arcadeContext.Player.ActiveTransform;
            _camera = _arcadeContext.Player.Camera;

            UpdateCurrentTarget(camera);
        }

        public void DeInitialize()
        {
            if (_spawnIndicator != null)
                Destroy(_spawnIndicator);
        }

        public override void UpdateCurrentTarget(Camera camera)
        {
            ModelConfigurationComponent target = Raycaster.GetCurrentTarget(camera);
            if (target == null)
            {
                if (!_spawning)
                    EnableSpawnIndicator();

                if (ReferenceTarget != null && ReferenceTarget != CurrentTarget)
                {
                    ReferenceTarget.RestoreLayerToOriginal();
                    ReferenceTarget.RemoveOutline();
                }

                Reset();

                if (_drawSpawnIndicator)
                {
                    Vector3 playerPosition   = _player.position;
                    Vector3 playerDirection  = _player.forward;
                    Vector3 spawnPosition    = playerPosition + (playerDirection * _spawnDistance);

                    Ray ray = _camera.ScreenPointToRay(_camera.WorldToScreenPoint(spawnPosition));
                    if (Physics.Raycast(ray, out RaycastHit _, math.INFINITY, _worldLayerMask))
                    {
                        Quaternion spawnRotation = Quaternion.LookRotation(playerDirection);
                        _spawnIndicator.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
                    }
                }
                return;
            }

            DisableSpawnIndicator();

            if (target == CurrentTarget)
                return;

            if (CurrentTarget != null && target != CurrentTarget)
            {
                CurrentTarget.RestoreLayerToOriginal();
                CurrentTarget.RemoveOutline();
            }

            if (ReferenceTarget != null && target != ReferenceTarget)
            {
                ReferenceTarget.RestoreLayerToOriginal();
                ReferenceTarget.RemoveOutline();
            }

            target.SetLayer(_hightlightLayer);
            target.AddOutline(_outlineColor);

            CurrentTarget = ReferenceTarget = target;

            _onCurrentTargetChanged.Raise(target);
        }

        public void ApplyChangesOrAddModel()
        {
            if (CurrentTarget != null)
            {
                CurrentTarget.RemoveOutline();
                ApplyChanges();
            }
            else
                AddModel();
        }

        public void ApplyChanges() => ApplyChangesAsync().Forget();

        public void AddModel() => SpawnGameAsync().Forget();

        public void RemoveModel() => RemoveGameAsync().Forget();

        public void DestroyAddedItems()
        {
            foreach (ModelConfigurationComponent item in _addedItems)
                Destroy(item.gameObject);
            _addedItems.Clear();
        }

        public void DestroyRemovedItems()
        {
            foreach (ModelConfigurationComponent item in _removedItems)
                Destroy(item.gameObject);
            _removedItems.Clear();
        }

        public void RestoreRemovedItems()
        {
            foreach (ModelConfigurationComponent item in _removedItems)
            {
                item.gameObject.SetActive(true);

                item.Collider.enabled      = true;
                item.Rigidbody.isKinematic = false;

                DynamicArtworkComponent[] dynamicArtworkComponents = item.GetComponentsInChildren<DynamicArtworkComponent>();
                foreach (DynamicArtworkComponent dynamicArtworkComponent in dynamicArtworkComponents)
                    dynamicArtworkComponent.enabled = true;
            }
            _removedItems.Clear();
        }

        private async UniTaskVoid ApplyChangesAsync()
        {
            if (_drawSpawnIndicator || CurrentTarget == null)
                return;

            ModelConfiguration modelConfiguration = ClassUtils.DeepCopy(CurrentTarget.Configuration);
            Vector3 spawnPosition                 = CurrentTarget.transform.position + (Vector3.up * _verticalOffset);
            Quaternion spawnRotation              = CurrentTarget.transform.localRotation;

            _onRequestUpdatedModelConfiguration.Raise(modelConfiguration);
            if (string.IsNullOrEmpty(modelConfiguration.Id))
                return;

            _spawning = true;
            DisableSpawnIndicator();

            if (_addedItems.Contains(CurrentTarget))
            {
                _ = _addedItems.Remove(CurrentTarget);
                await DissolveObject(CurrentTarget, true);
            }
            else
            {
                _removedItems.Add(CurrentTarget);
                await DissolveObject(CurrentTarget, false);
            }

            ModelConfigurationComponent modelConfigurationComponent = await _arcadeController.Value.ModelSpawner.SpawnGameAsync(modelConfiguration, spawnPosition, spawnRotation, true);
            _addedItems.Add(modelConfigurationComponent);

            _spawning = false;
        }

        private async UniTaskVoid SpawnGameAsync()
        {
            if (!_drawSpawnIndicator || CurrentTarget != null)
                return;

            _spawning = true;
            DisableSpawnIndicator();

            ModelConfiguration modelConfiguration = new ModelConfiguration();

            Vector3 playerPosition   = _player.position;
            Vector3 playerDirection  = _player.forward;
            Vector3 spawnPosition    = playerPosition + (Vector3.up * _verticalOffset) + (playerDirection * _spawnDistance);
            Quaternion spawnRotation = Quaternion.LookRotation(-playerDirection);

            _onRequestUpdatedModelConfiguration.Raise(modelConfiguration);
            if (string.IsNullOrEmpty(modelConfiguration.Id))
            {
                _spawning = false;
                EnableSpawnIndicator();
                return;
            }

            ModelConfigurationComponent modelConfigurationComponent = await _arcadeController.Value.ModelSpawner.SpawnGameAsync(modelConfiguration, spawnPosition, spawnRotation, true);
            _addedItems.Add(modelConfigurationComponent);

            _spawning = false;
            EnableSpawnIndicator();
        }

        private async UniTask RemoveGameAsync()
        {
            if (_drawSpawnIndicator || CurrentTarget == null)
                return;

            _spawning = true;
            DisableSpawnIndicator();

            CurrentTarget.RemoveOutline();

            if (_addedItems.Contains(CurrentTarget))
            {
                _ = _addedItems.Remove(CurrentTarget);
                await DissolveObject(CurrentTarget, true);
            }
            else
            {
                _removedItems.Add(CurrentTarget);
                await DissolveObject(CurrentTarget, false);
            }

            _spawning = false;
            EnableSpawnIndicator();
        }

        private async UniTask DissolveObject(ModelConfigurationComponent modelConfigurationComponent, bool destroy)
        {
            GameObject obj = modelConfigurationComponent.gameObject;

            DynamicArtworkComponent[] dynamicArtworkComponents = obj.GetComponentsInChildren<DynamicArtworkComponent>();
            foreach (DynamicArtworkComponent dynamicArtworkComponent in dynamicArtworkComponents)
                dynamicArtworkComponent.enabled = false;

            modelConfigurationComponent.Rigidbody.isKinematic = true;
            modelConfigurationComponent.Collider.enabled      = false;

            modelConfigurationComponent.SaveMaterials();
            modelConfigurationComponent.SetMaterials(_dissolveMaterial);

            float dissolveValue = 0f;
            while (dissolveValue < 1f)
            {
                modelConfigurationComponent.SetMaterialsValue(_dissolvePropertyId, dissolveValue);
                dissolveValue += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (destroy)
                Destroy(obj);
            else
                obj.SetActive(false);

            modelConfigurationComponent.RestoreMaterials();
        }

        private void EnableSpawnIndicator()
        {
            _spawnIndicator.SetActive(true);
            _drawSpawnIndicator = true;
        }

        private void DisableSpawnIndicator()
        {
            _spawnIndicator.SetActive(false);
            _drawSpawnIndicator = false;
        }
    }
}
