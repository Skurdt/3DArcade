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

using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIModelConfiguration: MonoBehaviour
    {
        [SerializeField] private FloatVariable _animationDuration;
        [SerializeField] private Databases _databases;
        [SerializeField] private AvailableModels _availableModels;

        [SerializeField] private Button _applyChangesButton;
        [SerializeField] private Button _addModelButton;
        [SerializeField] private Button _removeModelButton;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _idInputField;
        [SerializeField] private TMP_Dropdown _interactionTypeDropdown;
        [SerializeField] private TMP_Dropdown _platformDropdown;
        [SerializeField] private Toggle _grabbableToggle;
        [SerializeField] private Toggle _editMovableToggle;
        [SerializeField] private Toggle _editGrabbableToggle;
        [SerializeField] private TMP_Dropdown _modelOverrideDropdown;
        [SerializeField] private TMP_Dropdown _emulatorOverrideDropdown;

        private RectTransform _transform;
        private float _startPositionX;
        private float _endPositionX;

        private void Awake()
        {
            _transform      = transform as RectTransform;
            _startPositionX = _transform.anchoredPosition.x;
            _endPositionX   = 0f;
        }

        private void Start()
        {
            _interactionTypeDropdown.ClearOptions();
            _interactionTypeDropdown.AddOptions(System.Enum.GetNames(typeof(InteractionType)).ToList());
        }

        public void SetVisibility(bool visible)
        {
            _idInputField.DeactivateInputField();

            if (visible)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            _databases.Initialize();

            _platformDropdown.ClearOptions();
            _platformDropdown.AddOptions(new List<string> { "" }.Concat(_databases.Platforms.Names).ToList());

            _availableModels.Refresh();
            _modelOverrideDropdown.ClearOptions();
            _modelOverrideDropdown.AddOptions(_availableModels.GameModels);

            _emulatorOverrideDropdown.ClearOptions();
            _emulatorOverrideDropdown.AddOptions(new List<string> { "" }.Concat(_databases.Emulators.Names).ToList());

            _ = _transform.DOAnchorPosX(_endPositionX, _animationDuration.Value);
        }

        public void Hide()
        {
            ResetUIData();
            _ = _transform.DOAnchorPosX(_startPositionX, _animationDuration.Value);
        }

        public void SetUIData(ModelConfigurationComponent modelConfigurationComponent)
        {
            if (modelConfigurationComponent == null)
                return;

            ModelConfiguration modelConfiguration = modelConfigurationComponent.Configuration;

            _title.text = modelConfiguration.GetDescription();

            _idInputField.text = modelConfiguration.Id;
            _idInputField.DeactivateInputField();

            _interactionTypeDropdown.value  = (int)modelConfiguration.InteractionType;
            _platformDropdown.value         = !(modelConfiguration.PlatformConfiguration is null)
                                            ? _platformDropdown.options.FindIndex(x => x.text == modelConfiguration.PlatformConfiguration.Id)
                                            : 0;
            _grabbableToggle.isOn           = modelConfiguration.Grabbable;
            _editMovableToggle.isOn         = modelConfiguration.MoveCabMovable;
            _editGrabbableToggle.isOn       = modelConfiguration.MoveCabGrabbable;
            _modelOverrideDropdown.value    = !string.IsNullOrEmpty(modelConfiguration.Overrides.Model)
                                            ? _modelOverrideDropdown.options.FindIndex(x => x.text == modelConfiguration.Overrides.Model)
                                            : 0;
            _emulatorOverrideDropdown.value = !string.IsNullOrEmpty(modelConfiguration.Overrides.Emulator)
                                            ? _emulatorOverrideDropdown.options.FindIndex(x => x.text == modelConfiguration.Overrides.Emulator)
                                            : 0;
        }

        public void SetupButtonsStates(ModelConfigurationComponent modelConfigurationComponent)
        {
            bool state = modelConfigurationComponent != null;

            _applyChangesButton.interactable = state;
            _removeModelButton.interactable  = state;
            _addModelButton.interactable     = !state;
        }

        public void UpdateModelConfigurationValues(ModelConfiguration modelConfiguration)
        {
            modelConfiguration.Id = _idInputField.text;
            _idInputField.DeactivateInputField();

            modelConfiguration.InteractionType    = (InteractionType)_interactionTypeDropdown.value;
            modelConfiguration.Platform           = _platformDropdown.options[_platformDropdown.value].text;
            modelConfiguration.Grabbable          = _grabbableToggle.isOn;
            modelConfiguration.MoveCabMovable     = _editMovableToggle.isOn;
            modelConfiguration.MoveCabGrabbable   = _editGrabbableToggle.isOn;
            modelConfiguration.Overrides.Model    = _modelOverrideDropdown.options[_modelOverrideDropdown.value].text;
            modelConfiguration.Overrides.Emulator = _emulatorOverrideDropdown.options[_emulatorOverrideDropdown.value].text;
        }

        private void ResetUIData()
        {
            _idInputField.text = null;
            _idInputField.DeactivateInputField();

            _interactionTypeDropdown.value  = 0;
            _platformDropdown.value         = 0;
            _grabbableToggle.isOn           = false;
            _editMovableToggle.isOn         = false;
            _editGrabbableToggle.isOn       = false;
            _modelOverrideDropdown.value    = 0;
            _emulatorOverrideDropdown.value = 0;
        }
    }
}
