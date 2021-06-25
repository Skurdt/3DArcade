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

using SK.Utilities.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Arcade
{
    [System.Serializable, DisallowMultipleComponent, SelectionBase]
    public abstract class EntityBase<T> : MonoBehaviour, ITransformState, IEntity
        where T : EntityConfigurationBase
    {
        private sealed class PhysicsState
        {
            public Dictionary<Collider, bool> ColliderTriggerMap = new Dictionary<Collider, bool>();
            public CollisionDetectionMode CollisionDetectionMode = CollisionDetectionMode.Discrete;
            public RigidbodyInterpolation Interpolation = RigidbodyInterpolation.None;
            public bool IsKinematic = false;
        }

        public T Configuration { get; set; }
        public Bounds Bounds { get; internal set; }
        public Collider[] Colliders { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public bool MoveCabMovable => Configuration.MoveCabMovable && !(Colliders is null) && Colliders.Length > 0 && Rigidbody != null;
        public bool MoveCabGrabbable => Configuration.MoveCabGrabbable && !(Colliders is null) && Colliders.Length > 0 && Rigidbody != null;

        private readonly Dictionary<Renderer, Material> _savedMaterials = new Dictionary<Renderer, Material>();
        private MeshRenderer[] _renderers;

        private int _originalLayer;
        private int _previousLayer;
        private TransformState _savedTransformState;
        private PhysicsState _savedPhysicsState;

        private void Awake()
        {
            Colliders  = GetComponentsInChildren<Collider>(true);
            Rigidbody  = GetComponent<Rigidbody>();
            _renderers = GetComponentsInChildren<MeshRenderer>(true);
            InitBounds();
        }

        private void InitBounds()
        {
            if (_renderers is null || _renderers.Length == 0)
                return;

            Bounds = new Bounds(_renderers[0].bounds.center, _renderers[0].bounds.size);
            foreach (Renderer renderer in _renderers)
                Bounds.Encapsulate(renderer.bounds);
        }

        public void InitialSetup(T configuration, int layer, bool vr)
        {
            Configuration = configuration;
            _originalLayer = layer;

            gameObject.name = configuration.Id;
            SetLayer(layer);

            transform.localScale = configuration.Scale;

            if (vr)
                _ = gameObject.AddComponent<XRSimpleInteractable>();

            OnInitialSetup(vr);
        }

        public T GetConfigurationWithUpdatedTransforms()
        {
            Configuration.Position = transform.localPosition;
            Configuration.Rotation = MathUtils.ClampEulerAngles(transform.localEulerAngles);
            Configuration.Scale = transform.localScale;
            return Configuration;
        }

        public void SetLayer(int layer)
        {
            if (gameObject.layer != layer)
                gameObject.SetLayerRecursively(layer);
        }

        public void RestoreLayerToOriginal() => SetLayer(_originalLayer);

        public void SaveMaterials()
        {
            _savedMaterials.Clear();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                _savedMaterials[renderer] = renderer.material;
        }

        public void SetMaterials(Material material)
        {
            for (int i = 0; i < _renderers.Length; ++i)
            {
                Renderer renderer = _renderers[i];

                Color color = renderer.materials[0].GetColor(ArtworksController.ShaderBaseColorId);
                Texture texture = renderer.materials[0].GetTexture(ArtworksController.ShaderBaseMapId);
                if (texture == null)
                {
                    texture = renderer.materials[0].GetTexture(ArtworksController.ShaderEmissionMapId);
                    if (texture == null)
                    {
                        MaterialPropertyBlock blockRead = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(blockRead);
                        texture = blockRead.GetTexture(ArtworksController.ShaderEmissionMapId);
                    }
                }

                renderer.material = material;

                MaterialPropertyBlock blockWrite = new MaterialPropertyBlock();
                blockWrite.SetColor(ArtworksController.ShaderBaseColorId, color);
                if (texture != null)
                    blockWrite.SetTexture(ArtworksController.ShaderBaseMapId, texture);
                renderer.SetPropertyBlock(blockWrite);
            }
        }

        public void SetMaterialsValue(int id, float value)
        {
            foreach (MeshRenderer renderer in _renderers)
            {
                MaterialPropertyBlock blockRead = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(blockRead);
                blockRead.SetFloat(id, value);
                renderer.SetPropertyBlock(blockRead);
            }
        }

        public void RestoreMaterials()
        {
            foreach (KeyValuePair<Renderer, Material> kvPair in _savedMaterials)
                kvPair.Key.material = kvPair.Value;
        }

        public void AddOutline(Color color)
        {
            if (GetComponent<QuickOutline>() == null)
            {
                QuickOutline outlineComponent = gameObject.AddComponent<QuickOutline>();
                outlineComponent.OutlineColor = color;
                outlineComponent.OutlineWidth = 6f;
            }
        }

        public void RemoveOutline()
        {
            if (gameObject.TryGetComponent(out QuickOutline quickOutlineComponent))
                Destroy(quickOutlineComponent);
        }

        public bool InitAutoMove(int grabLayer)
        {
            if (Colliders is null || Colliders.Length == 0 || Rigidbody == null)
                return false;

            SavePhysicsState();

            _previousLayer = gameObject.layer;
            SetLayer(grabLayer);

            foreach (Collider collider in Colliders)
                collider.isTrigger = true;

            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            Rigidbody.isKinematic = true;

            return true;
        }

        public void DeInitAutoMove()
        {
            RestorePhysicsState();
            RestoreLayerToPrevious();
        }

        public void SaveTransformState() => _savedTransformState = new TransformState
        {
            Position = transform.position,
            Rotation = transform.localEulerAngles,
            Scale = transform.localScale
        };

        public void RestoreTransformState()
        {
            if (_savedTransformState is null)
                return;

            transform.position = _savedTransformState.Position;
            transform.localEulerAngles = _savedTransformState.Rotation;
            transform.localScale = _savedTransformState.Scale;

            _savedTransformState = null;
        }

        protected abstract void OnInitialSetup(bool vr);

        private void RestoreLayerToPrevious() => SetLayer(_previousLayer);

        private void SavePhysicsState()
        {
            Dictionary<Collider, bool> colliderTriggerMap = new Dictionary<Collider, bool>();
            foreach (Collider collider in Colliders)
                colliderTriggerMap.Add(collider, collider.isTrigger);

            _savedPhysicsState = new PhysicsState
            {
                ColliderTriggerMap     = colliderTriggerMap,
                CollisionDetectionMode = Rigidbody.collisionDetectionMode,
                Interpolation          = Rigidbody.interpolation,
                IsKinematic            = Rigidbody.isKinematic
            };
        }

        private void RestorePhysicsState()
        {
            if (_savedPhysicsState is null)
                return;

            if (!(Colliders is null) && Colliders.Length > 0)
                foreach (Collider collider in Colliders)
                    if (_savedPhysicsState.ColliderTriggerMap.TryGetValue(collider, out bool wasTrigger))
                        collider.isTrigger = wasTrigger;

            if (Rigidbody != null)
            {
                Rigidbody.collisionDetectionMode = _savedPhysicsState.CollisionDetectionMode;
                Rigidbody.interpolation          = _savedPhysicsState.Interpolation;
                Rigidbody.isKinematic            = _savedPhysicsState.IsKinematic;
            }

            _savedPhysicsState = null;
        }
    }
}
