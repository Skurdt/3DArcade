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

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcade.UnityEditor
{
    internal sealed class ArcadeManager
    {
        public static ArcadeManager Instance => _instance ?? new ArcadeManager();

        public readonly ArcadeContext ArcadeContext;

        private static ArcadeManager _instance;
        private static GameObject _dummyGamePrefab;
        private static GameObject _dummyPropPrefab;

        private ArcadeController _arcadeController;

        public ArcadeManager()
        {
            _instance = this;

            string dataPath = SystemUtils.GetDataPath();
            IVirtualFileSystem vfs = new VirtualFileSystem().MountFile("general_cfg", $"{dataPath}/3darcade~/Configuration/GeneralConfiguration.xml")
                                                            .MountDirectory("emulator_cfgs", $"{dataPath}/3darcade~/Configuration/Emulators")
                                                            .MountDirectory("platform_cfgs", $"{dataPath}/3darcade~/Configuration/Platforms")
                                                            .MountDirectory("arcade_cfgs", $"{dataPath}/3darcade~/Configuration/Arcades")
                                                            .MountDirectory("gamelist_cfgs", $"{dataPath}/3darcade~/Configuration/Gamelists")
                                                            .MountDirectory("medias", $"{dataPath}/3darcade~/Media");

            Player player = Object.FindObjectOfType<Player>();
            if (player == null)
                return;

            player.Construct(new PlayerContext(player));

            GeneralConfiguration generalConfiguration = new GeneralConfiguration(vfs);
            generalConfiguration.Initialize();

            Databases databases = new Databases(new EmulatorDatabase(vfs), new PlatformDatabase(vfs), new ArcadeDatabase(vfs));
            databases.Initialize();

            ArcadeSceneAddressesProvider arcadeProvider = new ArcadeSceneAddressesProvider();
            GamePrefabAddressesProvider gameProvider    = new GamePrefabAddressesProvider();
            PropPrefabAddressesProvider propProvider    = new PropPrefabAddressesProvider();
            AddressesProviders addressesProviders       = new AddressesProviders(arcadeProvider, gameProvider, propProvider);

            EntitiesSceneCreator entitiesSceneCreator = new EntitiesSceneCreator();
            EntitiesScene entitiesScene               = new EntitiesScene(entitiesSceneCreator);
            ArcadeSceneLoader arcadeSceneLoader       = new ArcadeSceneLoader();
            ArcadeScene arcadeScene                   = new ArcadeScene(arcadeSceneLoader);
            Scenes scenes                             = new Scenes(entitiesScene, arcadeScene);

            ArcadeContext = new ArcadeContext(null, player, generalConfiguration, databases, scenes, addressesProviders, null, null, null);
        }

        public void LoadArcade(string name, ArcadeType arcadeType)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (ArcadeContext == null)
                return;

            if (!ArcadeContext.Databases.Arcades.Get(name, out ArcadeConfiguration arcadeConfiguration))
                return;

            UE_Utilities.CloseAllScenes();

            ArcadeContext.Player.TransitionTo<PlayerDisabledState>();
            _arcadeController = null;

            switch (arcadeType)
            {
                case ArcadeType.Fps:
                    _arcadeController = new FpsArcadeController(ArcadeContext);
                    break;
                case ArcadeType.Cyl:
                {
                    _arcadeController = arcadeConfiguration.CylArcadeProperties.WheelVariant switch
                    {
                        //WheelVariant.CameraInsideHorizontal  => new CylArcadeControllerWheel3DCameraInsideHorizontal(ArcadeContext),
                        //WheelVariant.CameraOutsideHorizontal => new CylArcadeControllerWheel3DCameraOutsideHorizontal(ArcadeContext),
                        //WheelVariant.CameraInsideVertical    => new CylArcadeControllerWheel3DCameraInsideVertical(ArcadeContext),
                        //WheelVariant.CameraOutsideVertical   => new CylArcadeControllerWheel3DCameraOutsideVertical(ArcadeContext),
                        //WheelVariant.LineVertical            => new CylArcadeControllerLineVertical(ArcadeContext),
                        CylArcadeWheelVariant.LineCustom     => new CylArcadeControllerLine(ArcadeContext),
                        _                                    => new CylArcadeControllerLineHorizontal(ArcadeContext)
                    };
                }
                break;
            }

            if (_arcadeController == null)
                return;

            SetCurrentArcadeStateInEditorPrefs(arcadeConfiguration.Id, arcadeType);
            _arcadeController.StartArcade(arcadeConfiguration, arcadeType);
        }

        public void ReloadCurrentArcade()
        {
            string loadedArcadeId       = EditorPrefs.GetString("LoadedArcadeId", null);
            ArcadeType loadedArcadeType = (ArcadeType)EditorPrefs.GetInt("LoadedArcadeType", 0);
            LoadArcade(loadedArcadeId, loadedArcadeType);
        }

        public void SaveCurrentArcade()
        {
            if (!ArcadeContext.SaveCurrentArcade())
                return;

            SaveCurrentArcadeStateInEditorPrefs();
            ReloadCurrentArcade();
        }

        public static void SaveCurrentArcadeStateInEditorPrefs()
        {
            ClearCurrentArcadeStateFromEditorPrefs();

            if (!EntitiesScene.TryGetArcadeConfiguration(out ArcadeConfigurationComponent arcadeConfigurationComponent, false))
                return;

            SetCurrentArcadeStateInEditorPrefs(arcadeConfigurationComponent.Id, arcadeConfigurationComponent.ArcadeType);
        }

        public static void ClearCurrentArcadeStateFromEditorPrefs()
        {
            EditorPrefs.SetString("LoadedArcadeId", null);
            EditorPrefs.SetInt("LoadedArcadeType", 0);
        }

        public static void SetCurrentArcadeStateInEditorPrefs(string arcadeId, ArcadeType arcadeType)
        {
            EditorPrefs.SetString("LoadedArcadeId", arcadeId);
            EditorPrefs.SetInt("LoadedArcadeType", (int)arcadeType);
        }

        public static void SpawnGame()
        {
            if (!ValidatePrefabStatus("pfDummyCabModel", ref _dummyGamePrefab))
                return;

            if (!EntitiesScene.TryGetGamesNode(out Transform gamesNode))
                return;

            SpawnEntity(gamesNode, _dummyGamePrefab);
        }

        public static void SpawnProp()
        {
            if (!ValidatePrefabStatus("pfDummyPropModel", ref _dummyPropPrefab))
                return;

            if (!EntitiesScene.TryGetPropsNode(out Transform gamesNode))
                return;

            SpawnEntity(gamesNode, _dummyPropPrefab);
        }

        private static bool ValidatePrefabStatus(string prefabName, ref GameObject outPrefab)
        {
            if (string.IsNullOrEmpty(prefabName))
                return false;

            if (outPrefab == null)
                outPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/{prefabName}.prefab");
            return outPrefab != null;
        }

        private static void SpawnEntity(Transform parent, GameObject prefab)
        {
            if (parent == null || prefab == null)
                return;

            GameObject gameObject = Object.Instantiate(prefab, parent);
            gameObject.name       = "set_proper_id";

            ModelConfiguration modelConfiguration = new ModelConfiguration
            {
                Id          = "id",
                Description = "Description"
            };
            gameObject.AddComponent<ModelConfigurationComponent>()
                      .SetModelConfiguration(modelConfiguration);

            Scene entititesScene = SceneManager.GetSceneByName(EntitiesScene.ARCADE_SETUP_SCENE_NAME);
            _ = EditorSceneManager.MarkSceneDirty(entititesScene);

            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }

        //public void SaveArcade(ArcadeConfigurationComponent arcadeConfiguration)
        //{
            //_playerCylControls.gameObject.SetActive(true);
            //_playerCylControls.gameObject.SetActive(false);

            //Camera camera                          = _playerFpsControls.Camera;
            //CinemachineVirtualCamera virtualCamera = _playerFpsControls.VirtualCamera;
            //CinemachineTransposer transposer       = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            //CameraSettings fpsCameraSettings = new CameraSettings
            //{
            //    Position      = _playerFpsControls.transform.position,
            //    Rotation      = MathUtils.CorrectEulerAngles(camera.transform.eulerAngles),
            //    Height        = transposer.m_FollowOffset.y,
            //    Orthographic  = camera.orthographic,
            //    FieldOfView   = virtualCamera.m_Lens.FieldOfView,
            //    AspectRatio   = virtualCamera.m_Lens.OrthographicSize,
            //    NearClipPlane = virtualCamera.m_Lens.NearClipPlane,
            //    FarClipPlane  = virtualCamera.m_Lens.FarClipPlane,
            //    ViewportRect  = camera.rect
            //};

            //camera        = _playerCylControls.Camera;
            //virtualCamera = _playerCylControls.VirtualCamera;
            //transposer    = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            //CameraSettings cylCameraSettings = new CameraSettings
            //{
            //    Position      = _playerCylControls.transform.position,
            //    Rotation      = MathUtils.CorrectEulerAngles(camera.transform.eulerAngles),
            //    Height        = transposer.m_FollowOffset.y,
            //    Orthographic  = camera.orthographic,
            //    FieldOfView   = virtualCamera.m_Lens.FieldOfView,
            //    AspectRatio   = virtualCamera.m_Lens.OrthographicSize,
            //    NearClipPlane = virtualCamera.m_Lens.NearClipPlane,
            //    FarClipPlane  = virtualCamera.m_Lens.FarClipPlane,
            //    ViewportRect  = camera.rect
            //};

            //_ = arcadeConfiguration.Save(_arcadeDatabase, fpsCameraSettings, cylCameraSettings, !_playerCylControls.gameObject.activeInHierarchy);
        //}
    }
}
