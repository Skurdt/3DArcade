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

using SK.Libretro.Unity;
using SK.Utilities.Unity;
using UnityEngine;
using Zenject;

namespace Arcade
{
    public sealed class InternalGameController
    {
        public readonly Material ScreenMaterial;

        private readonly Player _player;

        private LibretroBridge _libretroBridge;
        private ScreenNodeTag _screenNode;

        public InternalGameController(Player player, [Inject(Id = "LibretroScreenMaterial")] Material screenMaterial)
        {
            _player        = player;
            ScreenMaterial = screenMaterial;
        }

        public bool StartGame(ScreenNodeTag screenNodeTag, ModelConfiguration modelConfiguration)
        {
            ResetFields();

            if (screenNodeTag == null)
                return false;

            if (modelConfiguration == null)
                return false;

            EmulatorConfiguration emulator = modelConfiguration.EmulatorConfiguration;
            if (emulator == null)
                return false;

            _screenNode = screenNodeTag;

            LibretroScreenNode screenNode = screenNodeTag.gameObject.AddComponentIfNotFound<LibretroScreenNode>();
            _libretroBridge = new LibretroBridge(screenNode, _player.ActiveTransform);

            foreach (string gameDirectory in emulator.GamesDirectories)
            {
                if (!_libretroBridge.Start(emulator.Id, gameDirectory, modelConfiguration.Id))
                {
                    _libretroBridge.Stop();
                    continue;
                }

                _libretroBridge.InputEnabled = true;

                return true;
            }

            StopGame();

            return false;
        }

        public void UpdateGame() => _libretroBridge?.Update();

        public void StopGame()
        {
            _libretroBridge?.Stop();
            _libretroBridge = null;

            if (_screenNode != null && _screenNode.gameObject.TryGetComponent(out LibretroScreenNode libretroScreenNode))
                Object.Destroy(libretroScreenNode);

            ResetFields();
        }

        private void ResetFields()
        {
            _libretroBridge = null;
            _screenNode     = null;
        }
    }
}
