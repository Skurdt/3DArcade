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

using SK.Utilities;
using System.IO;
using UnityEngine;
using Zenject;

namespace Arcade
{
    public sealed class ExternalGameController
    {
        public readonly Material ScreenMaterial;

        public event System.Action<OSUtils.ProcessStartedData, EmulatorConfiguration, string> OnGameStarted;
        public event System.Action<OSUtils.ProcessExitedData, EmulatorConfiguration, string> OnGameExited;

        private OSUtils.ProcessLauncher _processLauncher;

        public ExternalGameController([Inject(Id = "udd")] Material screenMaterial) => ScreenMaterial = screenMaterial;

        public bool StartGame(EmulatorConfiguration emulator, string gameName, bool persistent = false)
        {
            _processLauncher ??= new OSUtils.ProcessLauncher(Debug.Log, Debug.LogWarning, Debug.LogError);

            if (string.IsNullOrEmpty(gameName))
            {
                Debug.LogError("[ExternalGameController.StartGame] game is null or empty.");
                return false;
            }

            if (emulator is null)
            {
                Debug.LogError("[ExternalGameController.StartGame] emulator is null.");
                return false;
            }

            string extension = GetFileExtension(emulator, gameName);

            OSUtils.ProcessCommand command = new OSUtils.ProcessCommand
            {
                Name             = emulator.Description,
                Id               = emulator.Id,
                Path             = Path.Combine(emulator.Directory, emulator.Executable),
                WorkingDirectory = emulator.WorkingDirectory,
                Extension        = extension,
                CommandLine      = emulator.Arguments,
                CommandId        = gameName
            };

            return _processLauncher.StartProcess(command, persistent, processStartedCallback, processExitedCallback);

            void processStartedCallback(OSUtils.ProcessStartedData processStartedData) => OnGameStarted?.Invoke(processStartedData, emulator, gameName);
            void processExitedCallback(OSUtils.ProcessExitedData processExitedData) => OnGameExited?.Invoke(processExitedData, emulator, gameName);
        }

        public void StopCurrent() => _processLauncher.StopCurrentProcess();

        public void StopAll() => _processLauncher.StopAllProcesses();

        private static string GetFileExtension(EmulatorConfiguration emulator, string gameName)
        {
            if (string.IsNullOrEmpty(emulator.SupportedExtensions) || emulator.GamesDirectories is null)
                return null;
            return ProcessExtensions(emulator.SupportedExtensions.Split(';'), emulator.GamesDirectories, gameName);
        }

        private static string ProcessExtensions(string[] extensions, string[] directories, string gameName)
        {
            foreach (string extension in extensions)
                if (ProcessFile(directories, gameName, extension))
                    return extension;
            return null;
        }

        private static bool ProcessFile(string[] directories, string gameName, string extension)
        {
            foreach (string directory in directories)
                if (File.Exists($"{directory}/{gameName}.{extension}"))
                    return true;
            return false;
        }
    }
}
