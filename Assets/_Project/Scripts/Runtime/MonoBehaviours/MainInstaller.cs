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

using Zenject;

namespace Arcade
{
    public sealed class MainInstaller : MonoInstaller<MainInstaller>
    {
        public override void InstallBindings()
        {
            _ = Container.Bind<InputActions>().AsSingle().NonLazy();

            _ = Container.Bind<Player>().FromComponentInHierarchy(true).AsSingle();
            _ = Container.Bind<PlayerContext>().AsSingle().NonLazy();

            _ = Container.Bind<IUIController>().To<UIController>().FromComponentInHierarchy(true).AsSingle();

            _ = Container.Bind<IVirtualFileSystem>().To<VirtualFileSystem>().AsSingle().NonLazy();
            _ = Container.Bind<GeneralConfiguration>().AsSingle().NonLazy();
            _ = Container.Bind<MultiFileDatabase<EmulatorConfiguration>>().To<EmulatorDatabase>().AsSingle().NonLazy();
            _ = Container.Bind<MultiFileDatabase<PlatformConfiguration>>().To<PlatformDatabase>().AsSingle().NonLazy();
            _ = Container.Bind<MultiFileDatabase<ArcadeConfiguration>>().To<ArcadeDatabase>().AsSingle().NonLazy();

            _ = Container.Bind<ModelMatcher>().AsSingle().NonLazy();

            _ = Container.Bind<SceneContext>().AsSingle().NonLazy();
        }
    }
}
