﻿/* MIT License

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
        [SerializeField] private float _spawnDistance  = 2f;
        [SerializeField] private float _verticalOffset = 0.4f;

        private readonly List<GameObject> _addedItems   = new List<GameObject>();
        private readonly List<GameObject> _removedItems = new List<GameObject>();

        [System.NonSerialized] private ModelConfigurationComponent _realTarget;

        public void Initialize()
        {
            _arcadeContext.Databases.Initialize();

            _addedItems.Clear();
            _removedItems.Clear();
        }

        public override void UpdateCurrentTarget(Camera camera)
        {
            ModelConfigurationComponent target = _raycaster.GetCurrentTarget(camera);
            if (target == null && InteractionData.Current != null)
                InteractionData.Current.RestoreLayerToOriginal();

            if (target != _realTarget)
            {
                if (_realTarget != null)
                    _realTarget.RestoreLayerToOriginal();
                _realTarget = target;

                _onRealTargetChange.Raise(_realTarget);
            }

            if (_realTarget == null)
                return;

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
                ObjectUtils.DestroyObject(targetObject);
                return;
            }

            _removedItems.Add(targetObject);
            targetObject.SetActive(false);
        }

        public void DestroyAddedItems()
        {
            foreach (GameObject item in _addedItems)
                ObjectUtils.DestroyObject(item);
            _addedItems.Clear();
        }

        public void DestroyRemovedItems()
        {
            foreach (GameObject item in _removedItems)
                ObjectUtils.DestroyObject(item);
            _removedItems.Clear();
        }

        public void RestoreRemovedItems()
        {
            foreach (GameObject item in _removedItems)
                item.SetActive(true);
            _removedItems.Clear();
        }

        private async UniTaskVoid ApplyChangesAsync()
        {
            if (_realTarget == null)
                return;

            ModelConfiguration modelConfiguration = ClassUtils.DeepCopy(_realTarget.Configuration);
            Vector3 spawnPosition                 = _realTarget.transform.position;
            Quaternion spawnRotation              = _realTarget.transform.localRotation;

            _requestUpdatedModelConfigurationValues.Raise(modelConfiguration);

            RemoveModel();

            GameObject spawnedGame = await _arcadeContext.ArcadeController.Value.ModelSpawner.SpawnGameAsync(modelConfiguration, spawnPosition, spawnRotation);
            _addedItems.Add(spawnedGame);
        }

        private async UniTaskVoid SpawnGameAsync()
        {
            if (_realTarget != null)
                return;

            ModelConfiguration modelConfiguration = new ModelConfiguration();

            Transform playerTransform = _arcadeContext.Player.Value.ActiveTransform;
            Vector3 playerPosition    = playerTransform.position;
            Vector3 playerDirection   = playerTransform.forward;
            Vector3 spawnPosition     = playerPosition + (Vector3.up * _verticalOffset) + (playerDirection * _spawnDistance);
            Quaternion spawnRotation  = Quaternion.LookRotation(-playerDirection);

            _requestUpdatedModelConfigurationValues.Raise(modelConfiguration);

            GameObject spawnedGame = await _arcadeContext.ArcadeController.Value.ModelSpawner.SpawnGameAsync(modelConfiguration, spawnPosition, spawnRotation);
            _addedItems.Add(spawnedGame);
        }
    }
}
