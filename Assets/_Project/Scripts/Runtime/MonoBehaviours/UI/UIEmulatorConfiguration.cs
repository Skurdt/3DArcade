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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    public sealed class UIEmulatorConfiguration : UIConfiguration<EmulatorsDatabase, EmulatorConfiguration, UIEmulatorConfigurations>
    {
        [SerializeField] private GameObject[] _externalSetupObjects;
        [SerializeField] private GameObject _coreObject;

        [SerializeField] private TMP_Dropdown _interactionTypeDropdown;
        [SerializeField] private Button _directoryButton;
        [SerializeField] private TMP_InputField _directoryInputField;
        [SerializeField] private Button _workingDirectoryButton;
        [SerializeField] private TMP_InputField _workingDirectoryInputField;
        [SerializeField] private Button _executableButton;
        [SerializeField] private TMP_InputField _executableInputField;
        [SerializeField] private TMP_Dropdown _coreDropdown;
        [SerializeField] private TMP_InputField _argumentsInputField;
        [SerializeField] private TMP_InputField _supportedExtensionsInputField;
        [SerializeField] private UIDirectories _gamesDirectories;

        protected override void GetUIValues()
        {
            _configuration.Description         = _descriptionInputField.text;
            _configuration.InteractionType     = (InteractionType)_interactionTypeDropdown.value + 2;
            _configuration.Directory           = _directoryInputField.text;
            _configuration.WorkingDirectory    = _workingDirectoryInputField.text;
            _configuration.Executable          = _configuration.InteractionType == InteractionType.LibretroCore
                                               ? _coreDropdown.options[_coreDropdown.value].text
                                               : _executableInputField.text;
            _configuration.Arguments           = _argumentsInputField.text;
            _configuration.SupportedExtensions = _supportedExtensionsInputField.text;
            _configuration.GamesDirectories    = _gamesDirectories.GetValues();
        }

        protected override void SetUIValues()
        {
            if (!_initialized)
            {
                _interactionTypeDropdown.ClearOptions();
                _interactionTypeDropdown.AddOptions(new List<string>
                {
                    $"{InteractionType.LibretroCore}",
                    $"{InteractionType.ExternalApplication}"
                });
                _interactionTypeDropdown.onValueChanged.RemoveAllListeners();
                _interactionTypeDropdown.onValueChanged.AddListener((index) => SetSetupMode((InteractionType)index + 2));

                _coreDropdown.ClearOptions();
                _coreDropdown.AddOptions(new List<string> { "" });
                if (FileSystemUtils.TryGetFiles(FileSystemUtils.PathCombine(SystemUtils.GetDataPath(), "libretro~/cores"), "*.dll", false, out string[] fileNames))
                    _coreDropdown.AddOptions(fileNames.Select(x => Path.GetFileNameWithoutExtension(x).Replace("_libretro", "")).ToList());

                _initialized = true;
            }

            _interactionTypeDropdown.SetValueWithoutNotify((int)_configuration.InteractionType - 2);
            SetSetupMode(_configuration.InteractionType);
        }

        protected override void ClearUIValues()
        {
            _directoryButton.onClick.RemoveAllListeners();
            _directoryInputField.DeactivateInputField(true);
            _directoryInputField.SetTextWithoutNotify("");

            _workingDirectoryButton.onClick.RemoveAllListeners();
            _workingDirectoryInputField.DeactivateInputField(true);
            _workingDirectoryInputField.SetTextWithoutNotify("");

            _executableButton.onClick.RemoveAllListeners();
            _executableInputField.DeactivateInputField(true);
            _executableInputField.SetTextWithoutNotify("");

            _argumentsInputField.DeactivateInputField(true);
            _argumentsInputField.SetTextWithoutNotify("");

            _supportedExtensionsInputField.DeactivateInputField(true);
            _supportedExtensionsInputField.SetTextWithoutNotify("");

            _gamesDirectories.Clear();
        }

        private void SetSetupMode(InteractionType interactionType)
        {
            ClearUIValues();

            if (interactionType == InteractionType.LibretroCore)
                SwitchToInternalSetup();
            else
                SwitchToExternalSetup();

            _gamesDirectories.Init(_fileExplorer, _configuration.GamesDirectories);
        }

        private void SwitchToInternalSetup()
        {
            foreach (GameObject externalSetupObject in _externalSetupObjects)
                externalSetupObject.SetActive(false);

            _coreDropdown.SetValueWithoutNotify(_coreDropdown.options.FindIndex(x => x.text == _configuration.Executable));

            _coreObject.SetActive(true);
        }

        private void SwitchToExternalSetup()
        {
            _coreObject.SetActive(false);

            _directoryButton.onClick.AddListener(()
                => _fileExplorer.OpenDirectoryDialog(paths =>
                {
                    if (paths is null || paths.Length == 0)
                        return;
                    _directoryInputField.SetTextWithoutNotify(FileSystemUtils.GetRelativePath(paths[0]));
                    _directoryInputField.caretPosition = 0;
                }));
            _directoryInputField.SetTextWithoutNotify(_configuration.Directory);
            _directoryInputField.caretPosition = 0;

            _workingDirectoryButton.onClick.AddListener(()
                => _fileExplorer.OpenDirectoryDialog(paths =>
                {
                    if (paths is null || paths.Length == 0)
                        return;
                    _workingDirectoryInputField.SetTextWithoutNotify(FileSystemUtils.GetRelativePath(paths[0]));
                    _workingDirectoryInputField.caretPosition = 0;
                }));
            _workingDirectoryInputField.text = _configuration.WorkingDirectory;

            _executableButton.onClick.AddListener(()
                => _fileExplorer.OpenFileDialog("Select executable", paths =>
                {
                    if (paths is null || paths.Length == 0)
                        return;

                    if (string.IsNullOrEmpty(_directoryInputField.text))
                    {
                        _directoryInputField.SetTextWithoutNotify(Path.GetDirectoryName(paths[0]));
                        _directoryInputField.caretPosition = 0;
                    }

                    _executableInputField.SetTextWithoutNotify(Path.GetFileName(paths[0]));
                    _executableInputField.caretPosition = 0;

                }));
            _executableInputField.SetTextWithoutNotify(_configuration.Executable);
            _executableInputField.caretPosition = 0;

            _argumentsInputField.SetTextWithoutNotify(_configuration.Arguments);
            _argumentsInputField.caretPosition = 0;

            _supportedExtensionsInputField.SetTextWithoutNotify(_configuration.SupportedExtensions);
            _supportedExtensionsInputField.caretPosition = 0;

            foreach (GameObject externalSetupObject in _externalSetupObjects)
                externalSetupObject.SetActive(true);
        }
    }
}
