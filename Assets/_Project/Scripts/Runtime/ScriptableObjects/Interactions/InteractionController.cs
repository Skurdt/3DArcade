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

using Unity.Mathematics;
using UnityEngine;

namespace Arcade
{
    public abstract class InteractionController<T> : ScriptableObject
        where T : InteractionData
    {
        [SerializeField] protected T _interactionData;
        [SerializeField] private LayerMask _raycastMask;
        [SerializeField] private float _raycastMaxDistance = math.INFINITY;
        [SerializeField] private InteractionDataEvent _targetChangedEvent;

        public T InteractionData => _interactionData;
        public Camera Camera => _camera;

        [System.NonSerialized] private Camera _camera;

        public void Construct(Camera camera) => _camera = camera;

        public void UpdateCurrentTarget(bool resetCurrent = true)
        {
            Ray ray = GetRay();
            if (!Physics.Raycast(ray, out RaycastHit hitInfo, _raycastMaxDistance, _raycastMask) || !hitInfo.transform.TryGetComponent(out ModelConfigurationComponent modelConfigurationComponent))
            {
                if (resetCurrent)
                {
                    _interactionData.Reset();
                    _targetChangedEvent.Raise(_interactionData);
                }
                return;
            }

            if (_interactionData.Current == modelConfigurationComponent)
                return;

            _interactionData.Set(modelConfigurationComponent);

            _targetChangedEvent.Raise(_interactionData);

            return;
        }

        public void Reset() => _interactionData.Reset();

        protected abstract Ray GetRay();
    }
}
