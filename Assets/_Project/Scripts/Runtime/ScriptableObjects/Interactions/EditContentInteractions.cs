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
        [SerializeField] private GameEntityConfigurationEvent _onRequestUpdatedModelConfiguration;
        [SerializeField] private GameObject _spawnIndicatorPrefab;
        [SerializeField] private EntityController _entityController;
        [SerializeField] private ArtworksController _artworksController;

        [SerializeField, Layer] private int _hightlightLayer;
        [SerializeField] private float _spawnDistance  = 2f;
        [SerializeField] private float _verticalOffset = 0.05f;
        [SerializeField] private LayerMask _worldLayerMask;

        [field: System.NonSerialized] public GameEntity ReferenceTarget { get; private set; }

        private readonly List<GameEntity> _addedItems   = new List<GameEntity>();
        private readonly List<GameEntity> _removedItems = new List<GameEntity>();

        [System.NonSerialized] private Transform _player;
        [System.NonSerialized] private Camera _camera;
        [System.NonSerialized] private GameObject _spawnIndicator;

        [System.NonSerialized] private bool _drawSpawnIndicator = true;
        [System.NonSerialized] private bool _spawning;

        public void Initialize(Camera camera)
        {
            _spawnIndicator = Instantiate(_spawnIndicatorPrefab);

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
            GameEntity target = Raycaster.GetCurrentTarget(camera);
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
                    Vector3 playerPosition  = _player.position;
                    Vector3 playerDirection = _player.forward;
                    Vector3 spawnPosition   = playerPosition + (playerDirection * _spawnDistance);

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

            _onCurrentGameTargetChange.Raise(target);
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
            foreach (GameEntity item in _addedItems)
                Destroy(item.gameObject);
            _addedItems.Clear();
        }

        public void DestroyRemovedItems()
        {
            foreach (GameEntity item in _removedItems)
                Destroy(item.gameObject);
            _removedItems.Clear();
        }

        public void RestoreRemovedItems()
        {
            foreach (GameEntity item in _removedItems)
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

            GameEntityConfiguration configuration = ClassUtils.DeepCopy(CurrentTarget.Configuration);
            Vector3 spawnPosition                 = CurrentTarget.transform.position + (Vector3.up * _verticalOffset);
            Quaternion spawnRotation              = CurrentTarget.transform.localRotation;

            _onRequestUpdatedModelConfiguration.Raise(configuration);
            if (string.IsNullOrEmpty(configuration.Id))
                return;

            _spawning = true;
            DisableSpawnIndicator();

            if (_addedItems.Contains(CurrentTarget))
            {
                _ = _addedItems.Remove(CurrentTarget);
                await _entityController.HideObject(CurrentTarget, true);
            }
            else
            {
                _removedItems.Add(CurrentTarget);
                await _entityController.HideObject(CurrentTarget, false);
            }

            GameEntity entity = await _arcadeController.Value.ModelSpawner.SpawnGameAsync(configuration, spawnPosition, spawnRotation);
            entity.Configuration = configuration;
            _addedItems.Add(entity);
            await _entityController.ShowObject(entity);
            _artworksController.SetupArtworksAsync(entity).Forget();

            _spawning = false;
        }

        private async UniTaskVoid SpawnGameAsync()
        {
            if (!_drawSpawnIndicator || CurrentTarget != null)
                return;

            _spawning = true;
            DisableSpawnIndicator();

            GameEntityConfiguration configuration = new GameEntityConfiguration();

            Vector3 playerPosition   = _player.position;
            Vector3 playerDirection  = _player.forward;
            Vector3 spawnPosition    = playerPosition + (Vector3.up * _verticalOffset) + (playerDirection * _spawnDistance);
            Quaternion spawnRotation = Quaternion.LookRotation(-playerDirection);

            _onRequestUpdatedModelConfiguration.Raise(configuration);
            if (string.IsNullOrEmpty(configuration.Id))
            {
                _spawning = false;
                EnableSpawnIndicator();
                return;
            }

            GameEntity entity = await _arcadeController.Value.ModelSpawner.SpawnGameAsync(configuration, spawnPosition, spawnRotation);
            entity.Configuration = configuration;
            _addedItems.Add(entity);
            await _entityController.ShowObject(entity);
            _artworksController.SetupArtworksAsync(entity).Forget();

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
                await _entityController.HideObject(CurrentTarget, true);
            }
            else
            {
                _removedItems.Add(CurrentTarget);
                await _entityController.HideObject(CurrentTarget, false);
            }

            _spawning = false;
            EnableSpawnIndicator();
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
