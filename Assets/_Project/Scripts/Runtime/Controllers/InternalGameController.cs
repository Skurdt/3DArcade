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

        public InternalGameController(Player player, [Inject(Id = "libretro")] Material screenMaterial)
        {
            _player        = player;
            ScreenMaterial = screenMaterial;
        }

        public bool StartGame(ScreenNodeTag screenNodeTag, GameEntityConfiguration configuration)
        {
            StopGame();

            if (screenNodeTag == null || configuration is null)
                return false;

            EmulatorConfiguration emulator = configuration.EmulatorConfiguration;
            if (emulator is null)
                return false;

            _screenNode = screenNodeTag;

            LibretroScreenNode screenNode = screenNodeTag.gameObject.AddComponentIfNotFound<LibretroScreenNode>();
            _libretroBridge = new LibretroBridge(screenNode, _player.ActiveTransform);

            string coreName = !string.IsNullOrEmpty(emulator.Executable) ? emulator.Executable : emulator.Id;
            foreach (string gameDirectory in emulator.GamesDirectories)
            {
                try
                {
                    _libretroBridge.Start(coreName, gameDirectory, configuration.Id);
                    //_libretroBridge.InputEnabled = true;
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    _libretroBridge.Stop();
                    continue;
                }
            }

            StopGame();

            return false;
        }

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
