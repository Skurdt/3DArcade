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

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Interaction/Data", fileName = "InteractionData")]
    public sealed class InteractionData : ScriptableObject
    {
        [field: System.NonSerialized] public ModelConfigurationComponent Current { get; private set; }

        [SerializeField] private ModelConfigurationComponentEvent _targetChangedEvent;

        public void HoverEnteredCallback(HoverEnterEventArgs args)
            => Set(args.interactable.GetComponent<ModelConfigurationComponent>(), int.MinValue);

        public void HoverExitedCallback(HoverExitEventArgs _)
            => Reset();

        public void Set(ModelConfigurationComponent target, int layer)
        {
            if (Current == target)
                return;

            if (target != null && layer >= 0)
                target.SetLayer(layer);

            Current = target;
            _targetChangedEvent.Raise(target);
        }

        public void Reset(bool raiseEvent = true)
        {
            if (Current == null)
            {
                if (raiseEvent)
                    _targetChangedEvent.Raise(null);
                return;
            }

            Current.RestoreLayerToOriginal();
            if (Current.TryGetComponent(out QuickOutline quickOutlineComponent))
                Destroy(quickOutlineComponent);
            Current = null;

            if (raiseEvent)
                _targetChangedEvent.Raise(null);
        }
    }
}
