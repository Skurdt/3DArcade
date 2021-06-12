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
using Zenject;

namespace Arcade
{
    public sealed class UIInstaller : MonoInstaller<UIInstaller>
    {
        [SerializeField] private UIContext _uiContext;
        [SerializeField] private FileExplorer _fileExplorer;
        [SerializeField] private AvailableModels _availableModels;
        [SerializeField] private GeneralConfigurationVariable _generalConfigurationVariable;
        [SerializeField] private EmulatorsDatabase _emulatorsDatabase;
        [SerializeField] private PlatformsDatabase _platformsDatabase;
        [SerializeField] private GamesDatabase _gamesDatabase;
        [SerializeField] private ArcadesDatabase _arcadesDatabase;
        [SerializeField] private ArcadeStandardFpsNormalState _arcadeStandardFpsNormalState;
        [SerializeField] private ArcadeStandardFpsEditContentState _arcadeStandardFpsEditContentState;
        [SerializeField] private ArcadeControllerVariable _arcadeControllerVariable;
        [SerializeField] private FloatVariable _animationDuration;
        [SerializeField] private FilterableArcadeListVariable _filterableArcadeListVariable;
        [SerializeField] private FilterableGameListVariable _filterableGameListVariable;
        [SerializeField] private MasterListGenerator _masterListGenerator;

        [SerializeField] private UIListButton _listButtonPrefab;

        [SerializeField] private UISelectionText _uiSelectionText;
        [SerializeField] private UILoading _uiLoading;
        [SerializeField] private UINormal _uiNormal;
        [SerializeField] private UIEditPositions _uiEditPositions;
        [SerializeField] private UIEditContent _uiEditContent;
        [SerializeField] private UINormalMenuButton _uiNormalMenuButton;
        [SerializeField] private UINormalMenu _uiNormalMenu;
        [SerializeField] private UIGeneralConfiguration _uiGeneralConfiguration;
        [SerializeField] private UIEmulatorConfigurations _uiEmulatorConfigurations;
        [SerializeField] private UIEmulatorConfiguration _uiEmulatorConfiguration;
        [SerializeField] private UIPlatformConfigurations _uiPlatformConfigurations;
        [SerializeField] private UIPlatformConfiguration _uiPlatformConfiguration;
        [SerializeField] private UIGameListConfigurations _uiGameListConfigurations;
        [SerializeField] private UIGameListConfiguration _uiGameListConfiguration;
        [SerializeField] private UINewGameList _uiNewGameList;
        [SerializeField] private UIInfo _uiInfo;

        public override void InstallBindings()
        {
            _ = Container.Bind<UIContext>().FromScriptableObject(_uiContext).AsSingle();
            _ = Container.Bind<FileExplorer>().FromScriptableObject(_fileExplorer).AsSingle();
            _ = Container.Bind<AvailableModels>().FromScriptableObject(_availableModels).AsSingle();
            _ = Container.Bind<GeneralConfigurationVariable>().FromScriptableObject(_generalConfigurationVariable).AsSingle();
            _ = Container.Bind<EmulatorsDatabase>().FromScriptableObject(_emulatorsDatabase).AsSingle();
            _ = Container.Bind<PlatformsDatabase>().FromScriptableObject(_platformsDatabase).AsSingle();
            _ = Container.Bind<GamesDatabase>().FromScriptableObject(_gamesDatabase).AsSingle();
            _ = Container.Bind<ArcadesDatabase>().FromScriptableObject(_arcadesDatabase).AsSingle();
            _ = Container.Bind<ArcadeStandardFpsNormalState>().FromScriptableObject(_arcadeStandardFpsNormalState).AsSingle();
            _ = Container.Bind<ArcadeStandardFpsEditContentState>().FromScriptableObject(_arcadeStandardFpsEditContentState).AsSingle();
            _ = Container.Bind<ArcadeControllerVariable>().FromScriptableObject(_arcadeControllerVariable).AsSingle();
            _ = Container.Bind<FloatVariable>().FromScriptableObject(_animationDuration).AsSingle();
            _ = Container.Bind<FilterableArcadeListVariable>().FromScriptableObject(_filterableArcadeListVariable).AsSingle();
            _ = Container.Bind<FilterableGameListVariable>().FromScriptableObject(_filterableGameListVariable).AsSingle();
            _ = Container.Bind<MasterListGenerator>().FromScriptableObject(_masterListGenerator).AsSingle();

            _ = Container.BindInstance(_listButtonPrefab).AsSingle();

            _ = Container.BindInstance(_uiSelectionText).AsSingle();
            _ = Container.BindInstance(_uiLoading).AsSingle();
            _ = Container.BindInstance(_uiNormal).AsSingle();
            _ = Container.BindInstance(_uiEditPositions).AsSingle();
            _ = Container.BindInstance(_uiEditContent).AsSingle();
            _ = Container.BindInstance(_uiNormalMenuButton).AsSingle();
            _ = Container.BindInstance(_uiNormalMenu).AsSingle();
            _ = Container.BindInstance(_uiGeneralConfiguration).AsSingle();
            _ = Container.BindInstance(_uiEmulatorConfigurations).AsSingle();
            _ = Container.BindInstance(_uiEmulatorConfiguration).AsSingle();
            _ = Container.BindInstance(_uiPlatformConfigurations).AsSingle();
            _ = Container.BindInstance(_uiPlatformConfiguration).AsSingle();
            _ = Container.BindInstance(_uiGameListConfigurations).AsSingle();
            _ = Container.BindInstance(_uiGameListConfiguration).AsSingle();
            _ = Container.BindInstance(_uiNewGameList).AsSingle();
            _ = Container.BindInstance(_uiInfo).AsSingle();
        }
    }
}
