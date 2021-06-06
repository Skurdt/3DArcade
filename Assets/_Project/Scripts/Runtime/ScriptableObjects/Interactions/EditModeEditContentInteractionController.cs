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
    [CreateAssetMenu(menuName = "3DArcade/Interaction/EditModeEditContent", fileName = "EditModeEditContentInteractionController")]
    public sealed class EditModeEditContentInteractionController : InteractionController
    {
        [SerializeField] private ArcadeContext _arcadeContext;
        [SerializeField] private ModelConfigurationComponentEvent _onRealTargetChange;
        [SerializeField] private ModelConfigurationEvent _requestUpdatedModelConfigurationValues;
        [SerializeField, Layer] private int _hightlightLayer;
        [SerializeField] private Color _outlineColor;
        [SerializeField] private float _spawnDistance  = 2f;
        [SerializeField] private float _verticalOffset = 0.4f;
        [SerializeField] private LayerMask _worldLayerMask;
        [SerializeField] private Mesh _spawnPositionMesh;
        [SerializeField] private Material _spawnPositionMaterial;
        [SerializeField] private Material _dissolveMaterial;

        private readonly List<GameObject> _addedItems   = new List<GameObject>();
        private readonly List<GameObject> _removedItems = new List<GameObject>();

        [System.NonSerialized] private ModelConfigurationComponent _realTarget = null;
        [System.NonSerialized] private bool _drawDummyMesh                     = true;

        public void Initialize(Camera camera)
        {
            _arcadeContext.Databases.Initialize();

            _addedItems.Clear();
            _removedItems.Clear();

            UpdateCurrentTarget(camera);
        }

        public override void UpdateCurrentTarget(Camera camera)
        {
            ModelConfigurationComponent target = _raycaster.GetCurrentTarget(camera);
            if (target == null && InteractionData.Current != null)
                InteractionData.Current.RestoreLayerToOriginal();

            if (target != _realTarget)
            {
                if (_realTarget != null)
                {
                    if (_realTarget.gameObject.TryGetComponent(out QuickOutline quickOutlineComponent))
                        Destroy(quickOutlineComponent);
                    _realTarget.RestoreLayerToOriginal();
                }

                _realTarget = target;

                if (_realTarget != null)
                {
                    QuickOutline outlineComponent = target.gameObject.AddComponentIfNotFound<QuickOutline>();
                    outlineComponent.OutlineColor = _outlineColor;
                    outlineComponent.OutlineWidth = 6f;
                }

                _onRealTargetChange.Raise(_realTarget);
            }

            if (_realTarget == null && _drawDummyMesh)
            {
                Player player             = _arcadeContext.Player.Value;
                Transform playerTransform = player.ActiveTransform;
                Vector3 playerPosition    = playerTransform.position;
                Vector3 playerDirection   = playerTransform.forward;
                Vector3 spawnPosition     = playerPosition + (playerDirection * _spawnDistance);
                Quaternion spawnRotation  = Quaternion.LookRotation(-playerDirection);

                Ray ray = player.Camera.ScreenPointToRay(player.Camera.WorldToScreenPoint(spawnPosition));
                if (Physics.Raycast(ray, out RaycastHit hitInfo, math.INFINITY, _worldLayerMask))
                    Graphics.DrawMesh(_spawnPositionMesh, hitInfo.point, spawnRotation, _spawnPositionMaterial, 0);
                return;
            }

            InteractionData.Set(_realTarget, _hightlightLayer);
        }

        public void ApplyChanges() => ApplyChangesAsync().Forget();

        public void AddModel() => SpawnGameAsync().Forget();

        public void ApplyChangesOrAddModel()
        {
            if (_realTarget != null)
                ApplyChanges();
            else
                AddModel();
        }

        public void RemoveModel()
        {
            if (_realTarget == null)
                return;

            GameObject targetObject = _realTarget.gameObject;

            if (_addedItems.Contains(targetObject))
            {
                _ = _addedItems.Remove(targetObject);
                DissolveObject(targetObject, true).Forget();
                return;
            }

            _removedItems.Add(targetObject);
            DissolveObject(targetObject, false).Forget();
        }

        public void DestroyAddedItems()
        {
            foreach (GameObject item in _addedItems)
                Destroy(item);
            _addedItems.Clear();
        }

        public void DestroyRemovedItems()
        {
            foreach (GameObject item in _removedItems)
                Destroy(item);
            _removedItems.Clear();
        }

        public void RestoreRemovedItems()
        {
            foreach (GameObject item in _removedItems)
            {
                item.SetActive(true);
                if (item.TryGetComponent(out Collider collider))
                    collider.enabled = true;
                if (item.TryGetComponent(out Rigidbody rigidbody))
                    rigidbody.isKinematic = false;
                DynamicArtworkComponent[] dynamicArtworkComponents = item.GetComponentsInChildren<DynamicArtworkComponent>();
                foreach (DynamicArtworkComponent dynamicArtworkComponent in dynamicArtworkComponents)
                    dynamicArtworkComponent.enabled = false;
            }
            _removedItems.Clear();
        }

        private async UniTaskVoid ApplyChangesAsync()
        {
            if (_realTarget == null)
                return;

            _drawDummyMesh = false;

            ModelConfiguration modelConfiguration = ClassUtils.DeepCopy(_realTarget.Configuration);
            Vector3 spawnPosition                 = _realTarget.transform.position;
            Quaternion spawnRotation              = _realTarget.transform.localRotation;

            _requestUpdatedModelConfigurationValues.Raise(modelConfiguration);

            GameObject targetObject = _realTarget.gameObject;
            if (_addedItems.Contains(targetObject))
            {
                _ = _addedItems.Remove(targetObject);
                await DissolveObject(targetObject, true, false);
            }
            else
            {
                _removedItems.Add(targetObject);
                await DissolveObject(targetObject, false, false);
            }

            GameObject spawnedGame = await _arcadeContext.ArcadeController.Value.ModelSpawner.SpawnGameAsync(modelConfiguration, spawnPosition, spawnRotation, true);
            _addedItems.Add(spawnedGame);

            _drawDummyMesh = true;
        }

        private async UniTaskVoid SpawnGameAsync()
        {
            if (!_drawDummyMesh || _realTarget != null)
                return;

            _drawDummyMesh = false;

            ModelConfiguration modelConfiguration = new ModelConfiguration();

            Transform playerTransform = _arcadeContext.Player.Value.ActiveTransform;
            Vector3 playerPosition    = playerTransform.position;
            Vector3 playerDirection   = playerTransform.forward;
            Vector3 spawnPosition     = playerPosition + (Vector3.up * _verticalOffset) + (playerDirection * _spawnDistance);
            Quaternion spawnRotation  = Quaternion.LookRotation(-playerDirection);

            _requestUpdatedModelConfigurationValues.Raise(modelConfiguration);

            GameObject spawnedGame = await _arcadeContext.ArcadeController.Value.ModelSpawner.SpawnGameAsync(modelConfiguration, spawnPosition, spawnRotation, true);
            _addedItems.Add(spawnedGame);

            _drawDummyMesh = true;
        }

        private async UniTask DissolveObject(GameObject obj, bool destroy, bool reEnableDummy = true)
        {
            _drawDummyMesh = false;

            DynamicArtworkComponent[] dynamicArtworkComponents = obj.GetComponentsInChildren<DynamicArtworkComponent>();
            foreach (DynamicArtworkComponent dynamicArtworkComponent in dynamicArtworkComponents)
                dynamicArtworkComponent.enabled = false;
            if (obj.TryGetComponent(out Rigidbody rigidbody))
                rigidbody.isKinematic = true;
            if (obj.TryGetComponent(out Collider collider))
                collider.enabled = false;

            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
                renderer.material = _dissolveMaterial;

            float dissolve = 0f;
            while (dissolve < 1f)
            {
                foreach (MeshRenderer renderer in renderers)
                    renderer.material.SetFloat("_Dissolve", dissolve);
                dissolve += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (destroy)
                Destroy(obj);
            else
                obj.SetActive(false);

            if (reEnableDummy)
                _drawDummyMesh = true;
        }
    }
}
