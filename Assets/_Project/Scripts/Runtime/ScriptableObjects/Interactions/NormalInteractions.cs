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
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Interaction/Normal", fileName = "NormalInteractions")]
    public sealed class NormalInteractions : InteractionsBase
    {
        [SerializeField] private ArcadeContext _arcadeContext;
        [SerializeField, Layer] private int _selectionlayer;

        public void HoverEnteredCallback(HoverEnterEventArgs args)
            => Set(args.interactable.GetComponent<ModelConfigurationComponent>());

        public void HoverExitedCallback(HoverExitEventArgs _)
            => Reset();

        public override void UpdateCurrentTarget(Camera camera)
        {
            ModelConfigurationComponent target = Raycaster.GetCurrentTarget(camera);
            Set(target);
        }

        public void HandleInteraction()
        {
            if (CurrentTarget == null)
                return;

            ModelConfiguration modelConfiguration = CurrentTarget.Configuration;
            InteractionType interactionType       = modelConfiguration.InteractionType;

            switch (interactionType)
            {
                case InteractionType.Default:
                case InteractionType.LibretroCore:
                case InteractionType.ExternalApplication:
                    HandleEmulatorInteraction(modelConfiguration);
                    break;
                case InteractionType.FpsArcadeConfiguration:
                case InteractionType.CylArcadeConfiguration:
                    HandleArcadeTransition(modelConfiguration);
                    break;
                case InteractionType.None:
                    break;
                default:
                    throw new System.Exception($"Unhandled switch case for InteractionType: {modelConfiguration.InteractionType}");
            }
        }

        private void Set(ModelConfigurationComponent target)
        {
            if (CurrentTarget == target)
                return;

            if (CurrentTarget != null)
                CurrentTarget.RestoreLayerToOriginal();

            CurrentTarget = target;

            if (CurrentTarget != null)
                CurrentTarget.SetLayer(_selectionlayer);

            _onCurrentTargetChanged.Raise(CurrentTarget);
        }

        private void HandleEmulatorInteraction(ModelConfiguration modelConfiguration)
        {
            bool foundPlatform         = _arcadeContext.Databases.Platforms.TryGet(modelConfiguration.Platform, out PlatformConfiguration platform);
            bool foundEmulatorOverride = _arcadeContext.Databases.Emulators.TryGet(modelConfiguration.Overrides.Emulator, out EmulatorConfiguration emulator);
            if (foundPlatform && !foundEmulatorOverride)
                _ = _arcadeContext.Databases.Emulators.TryGet(platform.Emulator, out emulator);

            modelConfiguration.EmulatorConfiguration = emulator;
            if (modelConfiguration.EmulatorConfiguration is null)
                return;

            InteractionType interactionType = modelConfiguration.EmulatorConfiguration.InteractionType;

            switch (interactionType)
            {
                case InteractionType.LibretroCore:
                {
                    if (_arcadeContext.GeneralConfiguration.Value.EnableVR)
                        _arcadeContext.TransitionTo<ArcadeVirtualRealityInternalGameState>();
                    else
                        _arcadeContext.TransitionTo<ArcadeStandardInternalGameState>();
                }
                break;
                case InteractionType.ExternalApplication:
                {
                    if (_arcadeContext.GeneralConfiguration.Value.EnableVR)
                        _arcadeContext.TransitionTo<ArcadeVirtualRealityExternalGameState>();
                    else
                        _arcadeContext.TransitionTo<ArcadeStandardExternalGameState>();
                }
                break;
                case InteractionType.FpsArcadeConfiguration:
                case InteractionType.CylArcadeConfiguration:
                    HandleArcadeTransition(modelConfiguration);
                    break;
                case InteractionType.Default:
                case InteractionType.None:
                    Debug.LogError("This message should never appear!");
                    break;
                default:
                    throw new System.Exception($"Unhandled switch case for InteractionType: {interactionType}");
            }
        }

        private void HandleArcadeTransition(ModelConfiguration modelConfiguration)
        {
            InteractionType interactionType = modelConfiguration.InteractionType;

            switch (interactionType)
            {
                case InteractionType.FpsArcadeConfiguration:
                    _arcadeContext.StartArcade(modelConfiguration.Id, ArcadeType.Fps).Forget();
                    break;
                case InteractionType.CylArcadeConfiguration:
                    _arcadeContext.StartArcade(modelConfiguration.Id, ArcadeType.Cyl).Forget();
                    break;
                case InteractionType.Default:
                case InteractionType.None:
                case InteractionType.LibretroCore:
                case InteractionType.ExternalApplication:
                    Debug.LogError("This message should never appear!");
                    break;
                default:
                    throw new System.Exception($"Unhandled switch case for InteractionType: {interactionType}");
            }
        }
    }
}
