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

using System.Linq;
using UnityEngine;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class ArcadeConfigurationComponent : MonoBehaviour
    {
        [SerializeField] private ArcadeConfiguration _arcadeConfiguration;

        public ArcadeConfiguration Configuration => _arcadeConfiguration;

        public ArcadeConfiguration GetArcadeConfigurationWithUpdatedEntries()
        {
            GamesNodeTag gamesRoot = FindObjectOfType<GamesNodeTag>();
            if (gamesRoot != null)
            {
                GameEntity[] components = gamesRoot.GetComponentsInChildren<GameEntity>(false);
                _arcadeConfiguration.Games = components.Select(x => x.GetConfigurationWithUpdatedTransforms()).ToArray();
            }

            PropsNodeTag propsRoot = FindObjectOfType<PropsNodeTag>();
            if (propsRoot != null)
            {
                PropEntity[] components = propsRoot.GetComponentsInChildren<PropEntity>();
                if (_arcadeConfiguration.ArcadeType == ArcadeType.Fps)
                    _arcadeConfiguration.FpsArcadeProperties.Props = components.Select(x => x.GetConfigurationWithUpdatedTransforms())
                                                                               .ToArray();
                else if (_arcadeConfiguration.ArcadeType == ArcadeType.Cyl)
                    _arcadeConfiguration.CylArcadeProperties.Props = components.Select(x => x.GetConfigurationWithUpdatedTransforms())
                                                                               .ToArray();
            }

            return _arcadeConfiguration;
        }

        public void SetArcadeConfiguration(ArcadeConfiguration arcadeConfiguration) => _arcadeConfiguration = arcadeConfiguration;

        //public bool Save(MultiFileDatabase<ArcadeConfiguration> arcadeDatabase, CameraSettings fpsCameraSettings, CameraSettings cylCameraSettings, bool saveGameTransforms)
        //{
        //    GetParentNodes(out Transform games, out Transform props);

        //    ArcadeConfiguration cfg = new ArcadeConfiguration
        //    {
        //        Id                  = Id,
        //        Description         = Description,
        //        FpsArcadeProperties = FpsArcadeProperties,
        //        CylArcadeProperties = CylArcadeProperties,
        //        Games               = saveGameTransforms ? GetModelConfigurations(games) : arcadeDatabase.Get(Id).Games,
        //        Props               = GetModelConfigurations(props)
        //    };

        //    if (fpsCameraSettings != null)
        //        cfg.FpsArcadeProperties.CameraSettings = fpsCameraSettings;

        //    if (cylCameraSettings != null)
        //        cfg.CylArcadeProperties.CameraSettings = cylCameraSettings;

        //    return arcadeDatabase.Save(cfg);
        //}

        //public bool SaveModelsOnly(MultiFileDatabase<ArcadeConfiguration> arcadeDatabase, ArcadeConfiguration cfg)
        //{
        //    GetGamesAndProps(out cfg.Games, out cfg.Props);
        //    return arcadeDatabase.Save(cfg);
        //}

        //public void SetGamesAndPropsTransforms(ArcadeConfiguration cfg) => SetGamesAndPropsTransforms(cfg.Games, cfg.Props);

        //public void SetGamesAndPropsTransforms(ModelConfiguration[] games, ModelConfiguration[] props)
        //{
        //    GetChildNodes(out Transform tGames, out Transform tProps);
        //    SetModelTransforms(tGames, games);
        //    SetModelTransforms(tProps, props);
        //}

        //private void GetGamesAndProps(out ModelConfiguration[] outGames, out ModelConfiguration[] outProps)
        //{
        //    GetParentNodes(out Transform gamesNode, out Transform propsNode);
        //    outGames = gamesNode != null ? GetModelConfigurations(gamesNode) : null;
        //    outProps = propsNode != null ? GetModelConfigurations(propsNode) : null;
        //}

        //private void GetParentNodes(out Transform outGamesNode, out Transform outPropsNode)
        //{
        //    GamesNodeTag gamesNode = FindObjectOfType<GamesNodeTag>();
        //    outGamesNode = gamesNode != null ? gamesNode.transform : null;

        //    PropsNodeTag propsNode = FindObjectOfType<PropsNodeTag>();
        //    outPropsNode = propsNode != null ? propsNode.transform : null;
        //}

        //private static ModelConfiguration[] GetModelConfigurations(Transform node)
        //{
        //    ModelConfigurationComponent[] cfgComponents = node.GetComponentsInChildren<ModelConfigurationComponent>();
        //    return cfgComponents.Select(x => x.GetModelConfiguration()).ToArray();;
        //}

        //private static void SetModelTransforms(Transform node, ModelConfiguration[] modelConfigurations)
        //{
        //    for (int i = 0; i < node.childCount; ++i)
        //    {
        //        Transform child = node.GetChild(i);
        //        ModelConfiguration modelConfiguration = modelConfigurations[i];
        //        child.SetPositionAndRotation(modelConfiguration.Position, Quaternion.Euler(modelConfiguration.Rotation));
        //        child.localScale = modelConfiguration.Scale;
        //    }
        //}
    }
}
