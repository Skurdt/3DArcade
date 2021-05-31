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
using System.Linq;
using TMPro;
using UnityEngine;

namespace Arcade
{
    public sealed class UIPlatformConfiguration : UIConfiguration<PlatformsDatabase, PlatformConfiguration>
    {
        [SerializeField] private EmulatorsDatabase _emulatorsDatabase;
        [SerializeField] private GamesDatabase _gamesDatabase;
        [SerializeField] private AvailableModels _availableModels;

        [SerializeField] private TMP_Dropdown _masterListDropdown;
        [SerializeField] private TMP_Dropdown _emulatorDropdown;
        [SerializeField] private TMP_Dropdown _modelDropdown;
        [SerializeField] private UIDirectories _marqueeImagesDirectories;
        [SerializeField] private UIDirectories _marqueeVideosDirectories;
        [SerializeField] private UIDirectories _screenSnapsDirectories;
        [SerializeField] private UIDirectories _screenTitlesDirectories;
        [SerializeField] private UIDirectories _screenVideosDirectories;
        [SerializeField] private UIDirectories _genericImagesDirectories;
        [SerializeField] private UIDirectories _genericVideosDirectories;
        [SerializeField] private UIDirectories _infoDirectories;

        protected override void SetUIValues()
        {
            _gamesDatabase.Initialize();
            _masterListDropdown.ClearOptions();
            _masterListDropdown.AddOptions(new List<string> { "" }.Concat(_gamesDatabase.GetGameLists()).ToList());
            _masterListDropdown.value = _masterListDropdown.options.FindIndex(x => x.text == _configuration.MasterList);

            _emulatorsDatabase.Initialize();
            _emulatorDropdown.ClearOptions();
            _emulatorDropdown.AddOptions(new List<string> { "" }.Concat(_emulatorsDatabase.Names).ToList());
            _emulatorDropdown.value = _emulatorDropdown.options.FindIndex(x => x.text == _configuration.Emulator);

            _availableModels.Refresh();
            _modelDropdown.ClearOptions();
            _modelDropdown.AddOptions(_availableModels.GameModels);
            _modelDropdown.value = _modelDropdown.options.FindIndex(x => x.text == _configuration.Model);

            _marqueeImagesDirectories.Init(_fileExplorer, _configuration.MarqueeImagesDirectories);
            _marqueeVideosDirectories.Init(_fileExplorer, _configuration.MarqueeVideosDirectories);
            _screenSnapsDirectories.Init(_fileExplorer, _configuration.ScreenSnapsDirectories);
            _screenTitlesDirectories.Init(_fileExplorer, _configuration.ScreenTitlesDirectories);
            _screenVideosDirectories.Init(_fileExplorer, _configuration.ScreenVideosDirectories);
            _genericImagesDirectories.Init(_fileExplorer, _configuration.GenericImagesDirectories);
            _genericVideosDirectories.Init(_fileExplorer, _configuration.GenericVideosDirectories);
            _infoDirectories.Init(_fileExplorer, _configuration.InfoDirectories);
        }

        protected override void GetUIValues()
        {
            _configuration.Description              = _descriptionInputField.text;
            _configuration.MasterList               = _masterListDropdown.options[_masterListDropdown.value].text;
            _configuration.Emulator                 = _emulatorDropdown.options[_emulatorDropdown.value].text;
            _configuration.Model                    = _modelDropdown.options[_modelDropdown.value].text;
            _configuration.MarqueeImagesDirectories = _marqueeImagesDirectories.GetValues();
            _configuration.MarqueeVideosDirectories = _marqueeVideosDirectories.GetValues();
            _configuration.ScreenSnapsDirectories   = _screenSnapsDirectories.GetValues();
            _configuration.ScreenTitlesDirectories  = _screenTitlesDirectories.GetValues();
            _configuration.ScreenVideosDirectories  = _screenVideosDirectories.GetValues();
            _configuration.GenericImagesDirectories = _genericImagesDirectories.GetValues();
            _configuration.GenericVideosDirectories = _genericVideosDirectories.GetValues();
            _configuration.InfoDirectories          = _infoDirectories.GetValues();
        }

        protected override void ClearUIValues()
        {
            _masterListDropdown.value = 0;
            _masterListDropdown.ClearOptions();

            _emulatorDropdown.value = 0;
            _emulatorDropdown.ClearOptions();

            _modelDropdown.value = 0;
            _modelDropdown.ClearOptions();

            _marqueeImagesDirectories.Clear();
            _marqueeVideosDirectories.Clear();
            _screenSnapsDirectories.Clear();
            _screenTitlesDirectories.Clear();
            _screenVideosDirectories.Clear();
            _genericImagesDirectories.Clear();
            _genericVideosDirectories.Clear();
            _infoDirectories.Clear();
        }
    }
}
