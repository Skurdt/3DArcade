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
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
        [SerializeField] private float _verticalOffset = 0.05f;
        [SerializeField] private LayerMask _worldLayerMask;

        [field: System.NonSerialized] public GameEntity ReferenceTarget { get; private set; }

        private readonly List<GameEntity> _addedItems   = new List<GameEntity>();
        private readonly List<GameEntity> _removedItems = new List<GameEntity>();

        [System.NonSerialized] private Transform _player;
        [System.NonSerialized] private Camera _camera;
        [System.NonSerialized] private GameObject _spawnIndicator;
        [System.NonSerialized] private Bounds _spawnIndicatorBounds;

        [System.NonSerialized] private bool _drawSpawnIndicator = true;
        [System.NonSerialized] private bool _spawning;
        [System.NonSerialized] private float _rotationOffset;

        public void Initialize(Camera camera)
        {
            _spawnIndicator = Instantiate(_spawnIndicatorPrefab);

            _arcadeContext.Databases.Initialize();

            _addedItems.Clear();
            _removedItems.Clear();
            _rotationOffset = 0f;

            _player = _arcadeContext.Player.ActiveTransform;
            _camera = _arcadeContext.Player.Camera;

            MeshRenderer[] spawnIndicatorRenderers = _spawnIndicator.GetComponentsInChildren<MeshRenderer>(true);
            _spawnIndicatorBounds = new Bounds(spawnIndicatorRenderers[0].bounds.center, spawnIndicatorRenderers[0].bounds.size);
            foreach (MeshRenderer renderer in spawnIndicatorRenderers)
                _spawnIndicatorBounds.Encapsulate(renderer.bounds);
            _spawnIndicatorBounds.Expand(new Vector3(0.25f, 0f, 0.25f));

            UpdateCurrentTarget(camera);
        }

        public void DeInitialize()
        {
            if (_spawnIndicator != null)
                Destroy(_spawnIndicator);
        }

        public override void UpdateCurrentTarget(Camera camera)
        {
            GameEntity target = Raycaster.GetCurrentTarget(camera, new Vector2(0f, -Screen.height * 0.25f));
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
                    DrawSpawnIndicator();

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

                foreach (Collider collider in item.Colliders)
                    collider.enabled = true;
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
            Vector3 spawnPosition                 = CurrentTarget.transform.position + new Vector3(0f, _verticalOffset, 0f);
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

            Vector3 spawnPosition    = _spawnIndicator.transform.position + new Vector3(0f, _verticalOffset, 0f);
            Quaternion spawnRotation = Quaternion.LookRotation(-_spawnIndicator.transform.forward);

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

        private void DrawSpawnIndicator()
        {
            bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();

            bool useMousePosition = !(Mouse.current is null) && Cursor.lockState != CursorLockMode.Locked && !pointerOverUI;
            Vector2 rayPosition   = useMousePosition ? Mouse.current.position.ReadValue(): new Vector2(Screen.width * 0.5f, Screen.height * 0.25f);
            Ray ray               = _camera.ScreenPointToRay(rayPosition);

            if (!Physics.Raycast(ray, out RaycastHit hitInfo, math.INFINITY, _worldLayerMask))
                return;

            Transform transform = _spawnIndicator.transform;
            Vector3 position    = hitInfo.point;
            Vector3 normal      = hitInfo.normal;
            float dot           = Vector3.Dot(Vector3.up, normal);

            if (!pointerOverUI)
            {
                InputAction rotateAction = _arcadeContext.InputActions.FpsEditPositions.Rotate;
                if (!(rotateAction.activeControl is null) && !(rotateAction.activeControl.device is Mouse))
                    _rotationOffset += rotateAction.ReadValue<float>() * Time.deltaTime * 100f;
                else
                    _rotationOffset += rotateAction.ReadValue<float>();
            }

            // Floor
            if (dot > 0.05f)
            {
                transform.position      = Vector3.Lerp(transform.position, position, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal)
                                        * Quaternion.LookRotation(_player.forward)
                                        * Quaternion.AngleAxis(transform.localRotation.y, Vector3.up)
                                        * Quaternion.AngleAxis(_rotationOffset, Vector3.up);
                return;
            }

            // Ceiling
            if (dot < -0.05f)
            {
                transform.position      = Vector3.Lerp(transform.position, position, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.FromToRotation(Vector3.up, -normal)
                                        * Quaternion.LookRotation(_player.forward)
                                        * Quaternion.AngleAxis(transform.localRotation.y, Vector3.up)
                                        * Quaternion.AngleAxis(_rotationOffset, Vector3.up);
                return;
            }

            _rotationOffset = 0f;

            // Vertical surface
            Vector3 positionOffset  = normal * Mathf.Max(_spawnIndicatorBounds.extents.x + 0.05f, _spawnIndicatorBounds.extents.z + 0.05f);
            Vector3 newPosition     = new Vector3(position.x, transform.position.y, position.z) + positionOffset;
            transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
            transform.localRotation = Quaternion.LookRotation(-normal);
        }
    }
}
