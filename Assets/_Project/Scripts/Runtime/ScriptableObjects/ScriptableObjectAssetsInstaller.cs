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
    [CreateAssetMenu(menuName = "3DArcade/ScriptableObjectAssetsInstaller", fileName = "ScriptableObjectAssetsInstaller")]
    public sealed class ScriptableObjectAssetsInstaller : ScriptableObjectInstaller<ScriptableObjectAssetsInstaller>
    {
        [SerializeField] private Material _libretroScreenMaterial;
        [SerializeField] private Material _uddScreenMaterial;

        [SerializeField] private VirtualFileSystem _virtualFileSystem;
        [SerializeField] private Databases _databases;
        [SerializeField] private ArcadeConfigurationVariable _arcadeConfiguration;
        [SerializeField] private GameConfigurationsEvent _onGamesGenerated;
        [SerializeField] private EditContentInteractions _editContent;
        [SerializeField] private GameControllers _gameControllers;
        [SerializeField] private ArcadeContext _arcadeContext;

        public override void InstallBindings()
        {
            _ = Container.Bind<InputActions>().AsSingle().NonLazy();
            _ = Container.Bind<IAssetAddressesProvider<ArcadeConfiguration>>().To<ArcadeSceneAddressesProvider>().AsSingle().NonLazy();
            _ = Container.Bind<IAssetAddressesProvider<ModelConfiguration>>().WithId("game").To<GamePrefabAddressesProvider>().AsSingle().NonLazy();
            _ = Container.Bind<IAssetAddressesProvider<ModelConfiguration>>().WithId("prop").To<PropPrefabAddressesProvider>().AsSingle().NonLazy();
            _ = Container.Bind<AssetAddressesProviders>().AsSingle().NonLazy();
            _ = Container.Bind<IArtworkFileNamesProvider>().To<ArtworkFileNamesProvider>().AsSingle().NonLazy();
            _ = Container.Bind<IArtworkDirectoriesProvider>().To<MarqueeArtworkDirectoriesProvider>().AsSingle().WhenInjectedInto<NodeController<MarqueeNodeTag>>().NonLazy();
            _ = Container.Bind<IArtworkDirectoriesProvider>().To<ScreenArtworkDirectoriesProvider>().AsSingle().WhenInjectedInto<NodeController<ScreenNodeTag>>().NonLazy();
            _ = Container.Bind<IArtworkDirectoriesProvider>().To<GenericArtworkDirectoriesProvider>().AsSingle().WhenInjectedInto<NodeController<GenericNodeTag>>().NonLazy();
            _ = Container.Bind<NodeController<MarqueeNodeTag>>().AsSingle().NonLazy();
            _ = Container.Bind<NodeController<ScreenNodeTag>>().AsSingle().NonLazy();
            _ = Container.Bind<NodeController<GenericNodeTag>>().AsSingle().NonLazy();
            _ = Container.Bind<NodeControllers>().AsSingle().NonLazy();
            _ = Container.Bind<AssetCache<Texture>>().To<TextureCache>().AsSingle().NonLazy();
            _ = Container.Bind<ArtworkController>().AsSingle().NonLazy();
            _ = Container.Bind<ExternalGameController>().AsSingle().NonLazy();
            _ = Container.Bind<InternalGameController>().AsSingle().NonLazy();

            _ = Container.Bind<Material>().WithId("libretro").FromInstance(_libretroScreenMaterial).AsCached();
            _ = Container.Bind<Material>().WithId("udd").FromInstance(_uddScreenMaterial).AsCached();

            _ = Container.Bind<VirtualFileSystem>().FromScriptableObject(_virtualFileSystem).AsSingle();
            _ = Container.Bind<Databases>().FromScriptableObject(_databases).AsSingle();
            _ = Container.Bind<ArcadeConfigurationVariable>().FromScriptableObject(_arcadeConfiguration).AsSingle();
            _ = Container.Bind<GameConfigurationsEvent>().FromScriptableObject(_onGamesGenerated).AsSingle();
            _ = Container.Bind<EditContentInteractions>().FromScriptableObject(_editContent).AsSingle();
            _ = Container.Bind<GameControllers>().FromScriptableObject(_gameControllers).AsSingle();
            _ = Container.Bind<ArcadeContext>().FromScriptableObject(_arcadeContext).AsSingle();
        }
    }
}
