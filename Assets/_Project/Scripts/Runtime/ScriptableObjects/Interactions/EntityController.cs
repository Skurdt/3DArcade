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
    [CreateAssetMenu(menuName = "3DArcade/EntityController", fileName = "EntityController")]
    public sealed class EntityController : ScriptableObject
    {
        [SerializeField] private Material _dissolveMaterial;
        [SerializeField] private float _dissolveRate = 4f;

        private static readonly int _dissolvePropertyId = Shader.PropertyToID("_Dissolve");

        public async UniTask ShowObject(IEntity entity)
        {
            GameObject obj = entity.gameObject;
            obj.SetActive(true);

            entity.SaveMaterials();
            entity.SetMaterials(_dissolveMaterial);

            float dissolveValue = 1f;
            while (dissolveValue > 0f)
            {
                entity.SetMaterialsValue(_dissolvePropertyId, dissolveValue);
                dissolveValue -= Time.deltaTime * _dissolveRate;
                await UniTask.Yield();
            }

            entity.RestoreMaterials();

            DynamicArtworkComponent[] dynamicArtworkComponents = obj.GetComponentsInChildren<DynamicArtworkComponent>();
            foreach (DynamicArtworkComponent dynamicArtworkComponent in dynamicArtworkComponents)
                dynamicArtworkComponent.enabled = true;

            foreach (Collider collider in entity.Colliders)
                collider.enabled = true;
            entity.Rigidbody.isKinematic = false;
        }

        public async UniTask HideObject(IEntity entity, bool destroy)
        {
            GameObject obj = entity.gameObject;

            DynamicArtworkComponent[] dynamicArtworkComponents = obj.GetComponentsInChildren<DynamicArtworkComponent>();
            foreach (DynamicArtworkComponent dynamicArtworkComponent in dynamicArtworkComponents)
                dynamicArtworkComponent.enabled = false;

            entity.Rigidbody.isKinematic = true;
            foreach (Collider collider in entity.Colliders)
                collider.enabled = false;

            entity.SaveMaterials();
            entity.SetMaterials(_dissolveMaterial);

            float dissolveValue = 0f;
            while (dissolveValue < 1f)
            {
                entity.SetMaterialsValue(_dissolvePropertyId, dissolveValue);
                dissolveValue += Time.deltaTime * _dissolveRate;
                await UniTask.Yield();
            }

            if (destroy)
                Destroy(obj);
            else
                obj.SetActive(false);

            entity.RestoreMaterials();
        }
    }
}
