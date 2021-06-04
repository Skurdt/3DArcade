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
using SK.Utilities.Unity.StateMachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/StateMachine/ArcadeContext", fileName = "ArcadeContext")]
    public sealed class ArcadeContext : Context<ArcadeState>
    {
        [field: SerializeField] public Databases Databases { get; private set; }
        [field: SerializeField] public Scenes Scenes { get; private set; }
        [field: SerializeField] public PlayerVariable Player { get; private set; }
        [field: SerializeField] public InteractionControllers InteractionControllers { get; private set; }
        [field: SerializeField] public GeneralConfigurationVariable GeneralConfiguration { get; private set; }
        [field: SerializeField] public ArcadeConfigurationVariable ArcadeConfiguration { get; private set; }
        [field: SerializeField] public ModelSpawnerBase ModelSpawner { get; private set; }
        [field: SerializeField] public ArcadeControllerVariable ArcadeController { get; private set; }
        [field: SerializeField] public VideoPlayerControllerVariable VideoPlayerController { get; private set; }
        [field: SerializeField] public ArcadeStateEvent ArcadeStateChangeEvent { get; private set; }

        [field: System.NonSerialized] public InputActions InputActions { get; private set; }
        [field: System.NonSerialized] public AssetAddressesProviders AssetAddressesProviders { get; private set; }
        [field: System.NonSerialized] public NodeControllers NodeControllers { get; private set; }
        [field: System.NonSerialized] public GameControllers GameControllers { get; private set; }

        [Inject]
        public void Construct(InputActions inputActions, Player player, AssetAddressesProviders assetAddressesProviders, NodeControllers nodeControllers = null, GameControllers gameControllers = null)
        {
            InputActions            = inputActions;
            AssetAddressesProviders = assetAddressesProviders;
            NodeControllers         = nodeControllers;
            GameControllers         = gameControllers;
            Player.Value            = player;
        }

        protected override void OnContextStart() => Restart();

        public void ReloadCurrentArcade()
            => StartArcade(ArcadeConfiguration.Value.Id, ArcadeConfiguration.Value.ArcadeType, ArcadeConfiguration.Value.ArcadeMode).Forget();

        public void Restart()
        {
            GeneralConfiguration.Initialize();
            StartArcade(GeneralConfiguration.Value.StartingArcade, GeneralConfiguration.Value.StartingArcadeType, ArcadeMode.Normal).Forget();
        }

        public async UniTaskVoid StartArcade(string id, ArcadeType arcadeType, ArcadeMode arcadeMode)
        {
            if (string.IsNullOrEmpty(id))
                return;

            Databases.Initialize();
            if (!Databases.Arcades.TryGet(id, out ArcadeConfiguration arcadeConfiguration))
                return;

            InputActions?.Enable();

            GeneralConfiguration.Initialize();

            arcadeConfiguration.ArcadeType = arcadeType;
            arcadeConfiguration.ArcadeMode = arcadeMode;

            ArcadeConfiguration.Value = arcadeConfiguration;

            ArcadeController.Value = null;

            VideoPlayerController.Value?.StopAllVideos();
            VideoPlayerController.Value = null;

            Player.Value.TransitionTo<PlayerDisabledState>();

            switch (arcadeConfiguration.ArcadeType)
            {
                case ArcadeType.Fps:
                {
                    VideoPlayerController.Value = new FpsArcadeVideoPlayerController(Player.Value, LayerMask.GetMask("Arcade/GameModels", "Arcade/PropModels", "Arcade/Selection"));
                    ArcadeController.Value      = new FpsArcadeController(this);
                }
                break;
                case ArcadeType.Cyl:
                {
                    ArcadeController.Value = arcadeConfiguration.CylArcadeProperties.WheelVariant switch
                    {
                        //WheelVariant.CameraInsideHorizontal  => new CylArcadeControllerWheel3DCameraInsideHorizontal(ArcadeContext),
                        //WheelVariant.CameraOutsideHorizontal => new CylArcadeControllerWheel3DCameraOutsideHorizontal(ArcadeContext),
                        //WheelVariant.CameraInsideVertical    => new CylArcadeControllerWheel3DCameraInsideVertical(ArcadeContext),
                        //WheelVariant.CameraOutsideVertical   => new CylArcadeControllerWheel3DCameraOutsideVertical(ArcadeContext),
                        CylArcadeWheelVariant.LineHorizontal => new CylArcadeControllerLineHorizontal(this),
                        //WheelVariant.LineVertical            => new CylArcadeControllerLineVertical(ArcadeContext),
                        CylArcadeWheelVariant.LineCustom     => new CylArcadeControllerLine(this),
                        _                                    => new CylArcadeControllerLineHorizontal(this)
                    };
                }
                break;
            }

            if (ArcadeController.Value is null)
                return;

            if (Application.isPlaying)
            {
                if (GeneralConfiguration.Value.EnableVR)
                {
                    List<InputDevice> devices = new List<InputDevice>();
                    InputDevices.GetDevices(devices);
                    if (devices.Count == 0)
                        GeneralConfiguration.Value.EnableVR = false;
                }

                if (GeneralConfiguration.Value.EnableVR)
                    TransitionTo<ArcadeVirtualRealityLoadingState>();
                else
                    TransitionTo<ArcadeStandardLoadingState>();
            }

            Scenes.Entities.Initialize(arcadeConfiguration);

            AssetAddresses addressesToTry = AssetAddressesProviders.Arcade.GetAddressesToTry(arcadeConfiguration);

            bool success = await Scenes.Arcade.Load(addressesToTry);
            if (!success)
                return;

            await ArcadeController.Value.Initialize(ModelSpawner);
        }

        // TODO: save all arcade properties
        public void SaveCurrentArcade(bool modelsOnly = false)
        {
            if (!EntitiesScene.TryGetArcadeConfiguration(out ArcadeConfigurationComponent arcadeConfigurationComponent))
                return;

            ArcadeConfiguration arcadeConfiguration = arcadeConfigurationComponent.GetArcadeConfigurationWithUpdatedEntries();
            foreach (ModelConfiguration game in arcadeConfiguration.Games)
            {
                game.Position.RoundValues();
                game.Rotation.RoundValues();
                game.Scale.RoundValues();
            }

            if (!modelsOnly)
            {
                // TODO: implement
            }

            if (!Databases.Arcades.Save(arcadeConfiguration))
                return;

            ArcadeController.Value.UpdateLists();
            _ = Databases.Arcades.LoadAll();
        }
    }
}
