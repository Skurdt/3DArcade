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
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIModelConfiguration: MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField] private Databases _databases;
        [SerializeField] private AvailableModels _availableModels;
        [SerializeField] private ArcadeState _arcadeState;
        [SerializeField] private EditContentInteractions _interactions;

        [Header("Scene References")]
        [SerializeField] private UISelectionText _selectionText;
        [SerializeField] private UIEditContentSearchableGameList _gamesList;
        [SerializeField] private UIEditContentSearchableArcadeList _arcadesList;

        [Header("Object References")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _idInputField;
        [SerializeField] private TMP_Dropdown _interactionTypeDropdown;
        [SerializeField] private GameObject _platformObject;
        [SerializeField] private TMP_Dropdown _platformDropdown;
        [SerializeField] private Toggle _grabbableToggle;
        [SerializeField] private Toggle _editMovableToggle;
        [SerializeField] private Toggle _editGrabbableToggle;
        [SerializeField] private TMP_Dropdown _modelDropdown;
        [SerializeField] private GameObject _emulatorObject;
        [SerializeField] private TMP_Dropdown _emulatorDropdown;
        [SerializeField] private Button _actionBarApplyButton;
        [SerializeField] private Button _actionBarAddButton;
        [SerializeField] private Button _actionBarRemoveButton;

        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        [Inject]
        public void Construct(FloatVariable animationDuration)
        {
            _animationDuration      = animationDuration;
            _transform              = transform as RectTransform;
            _animationStartPosition = -_transform.rect.width;
            _animationEndPosition   = 0f;

            _idInputField.onSelect.AddListener((str) => _arcadeState.DisableInput());
            _idInputField.onDeselect.AddListener((str) => _arcadeState.EnableInput());

            _interactionTypeDropdown.AddOptions(new List<string>
            {
                $"{InteractionType.Default}",
                $"{InteractionType.FpsArcadeConfiguration}",
                $"{InteractionType.CylArcadeConfiguration}"
            });
            _interactionTypeDropdown.onValueChanged.AddListener((index) =>
            {
                _idInputField.DeactivateInputField(true);
                _idInputField.SetTextWithoutNotify("");
                _platformDropdown.SetValueWithoutNotify(0);
                _emulatorDropdown.SetValueWithoutNotify(0);
                SetConfigurationMode(index);
            });

            _platformDropdown.onValueChanged.AddListener((index) =>
            {
                _idInputField.DeactivateInputField(true);
                _idInputField.SetTextWithoutNotify("");
                _gamesList.Refresh(index);
            });
        }

        private void OnDestroy()
        {
            _idInputField.onSelect.RemoveAllListeners();
            _idInputField.onDeselect.RemoveAllListeners();
            _interactionTypeDropdown.onValueChanged.RemoveAllListeners();
            _interactionTypeDropdown.ClearOptions();
            _platformDropdown.onValueChanged.RemoveAllListeners();
        }

        public Tween SetVisibility(bool visible) => visible ? Show() : Hide();

        public void TargetChangedCallback(ModelConfigurationComponent modelConfigurationComponent)
        {
            if (modelConfigurationComponent != null)
            {
                SetButtonsState(true);
                SetConfigurationMode(modelConfigurationComponent.Configuration.InteractionType == InteractionType.Default ? 0 : 1);
                SetUIValues(modelConfigurationComponent.Configuration);
                return;
            }

            SetButtonsState(false);

            if (_interactions.ReferenceTarget != null)
            {
                SetConfigurationMode(_interactions.ReferenceTarget.Configuration.InteractionType == InteractionType.Default ? 0 : 1);
                SetUIValues(_interactions.ReferenceTarget.Configuration);
                return;
            }

            SetConfigurationMode(0);
            _interactionTypeDropdown.SetValueWithoutNotify(0);
        }

        public void ApplyChangesOrAddModel(string text)
        {
            _idInputField.SetTextWithoutNotify(text);
            _interactions.ApplyChangesOrAddModel();
        }

        public void UpdateModelConfigurationValues(ModelConfiguration modelConfiguration)
        {
            _idInputField.DeactivateInputField();

            modelConfiguration.Id              = _idInputField.text;
            modelConfiguration.InteractionType = _interactionTypeDropdown.value switch
            {
                0 => InteractionType.Default,
                1 => InteractionType.FpsArcadeConfiguration,
                2 => InteractionType.CylArcadeConfiguration,
                _ => throw new NotImplementedException(),
            };

            modelConfiguration.Platform           =  _platformDropdown.options[_platformDropdown.value].text;
            modelConfiguration.Grabbable          = _grabbableToggle.isOn;
            modelConfiguration.MoveCabMovable     = _editMovableToggle.isOn;
            modelConfiguration.MoveCabGrabbable   = _editGrabbableToggle.isOn;
            modelConfiguration.Overrides.Model    = _modelDropdown.options[_modelDropdown.value].text;
            modelConfiguration.Overrides.Emulator = _emulatorDropdown.options[_emulatorDropdown.value].text;
        }

        private Tween Show()
        {
            if (_visible)
                return null;

            _visible = true;

            gameObject.SetActive(true);

            _selectionText.Disable();

            _databases.Arcades.Initialize();
            _databases.Games.Initialize();

            _databases.Platforms.Initialize();
            _platformDropdown.AddOptions(new List<string> { "" }.Concat(_databases.Platforms.Names).ToList());

            _availableModels.Refresh();
            _modelDropdown.AddOptions(new List<string> { "" }.Concat(_availableModels.GameModels).ToList());

            _databases.Emulators.Initialize();
            _emulatorDropdown.AddOptions(new List<string> { "" }.Concat(_databases.Emulators.Names).ToList());

            _gamesList.Init();
            _arcadesList.Init();

            ModelConfigurationComponent currentTarget = _interactions.CurrentTarget;
            if (currentTarget == null)
            {
                SetButtonsState(false);
                SetConfigurationMode(0);
                ClearUIValues();
            }
            else
            {
                SetButtonsState(true);
                ModelConfiguration currentConfiguration = currentTarget.Configuration;
                SetConfigurationMode(currentConfiguration.InteractionType == InteractionType.Default ? 0 : 1);
                SetUIValues(currentConfiguration);
            }

            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic);
        }

        private Tween Hide()
        {
            if (!_visible)
                return null;

            _visible = false;

            _gamesList.DeInit();
            _arcadesList.DeInit();

            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic)
                             .OnComplete(() =>
                             {
                                 gameObject.SetActive(false);
                                 ClearUIValues();
                                 _interactionTypeDropdown.SetValueWithoutNotify(0);
                                 _platformDropdown.ClearOptions();
                                 _modelDropdown.ClearOptions();
                                 _emulatorDropdown.ClearOptions();
                             });
        }

        private void SetButtonsState(bool applyRemove)
        {
            _actionBarApplyButton.gameObject.SetActive(applyRemove);
            _actionBarRemoveButton.interactable = applyRemove;
            _actionBarAddButton.gameObject.SetActive(!applyRemove);
        }

        private void SetConfigurationMode(int index)
        {
            bool isForGames = index == 0;

            _platformObject.SetActive(isForGames);
            _emulatorObject.SetActive(isForGames);

            if (isForGames)
            {
                _arcadesList.Hide();
                _gamesList.Refresh(_platformDropdown.value);
            }
            else
            {
                _arcadesList.Show();
                _gamesList.Hide();
            }
        }

        private void SetUIValues(ModelConfiguration modelConfiguration)
        {
            _title.SetText(modelConfiguration.GetDescription());

            _idInputField.DeactivateInputField(true);
            _idInputField.SetTextWithoutNotify(modelConfiguration.Id);

            int interactionTypeIndex = modelConfiguration.InteractionType switch
            {
                InteractionType.Default                => 0,
                InteractionType.FpsArcadeConfiguration => 1,
                InteractionType.CylArcadeConfiguration => 2,
                _ => throw new NotImplementedException(),
            };
            _interactionTypeDropdown.SetValueWithoutNotify(interactionTypeIndex);

            SetConfigurationMode(interactionTypeIndex);

            _grabbableToggle.SetIsOnWithoutNotify(modelConfiguration.Grabbable);
            _editMovableToggle.SetIsOnWithoutNotify(modelConfiguration.MoveCabMovable);
            _editGrabbableToggle.SetIsOnWithoutNotify(modelConfiguration.MoveCabGrabbable);

            int modelIndex = !string.IsNullOrEmpty(modelConfiguration.Overrides.Model)
                           ? _modelDropdown.options.FindIndex(x => x.text == modelConfiguration.Overrides.Model)
                           : 0;
            _modelDropdown.SetValueWithoutNotify(modelIndex);

            bool isForGames = interactionTypeIndex == 0;
            if (isForGames)
            {
                _arcadesList.Hide();
                int platformIndex = !(modelConfiguration.PlatformConfiguration is null)
                                    ? _platformDropdown.options.FindIndex(x => x.text == modelConfiguration.PlatformConfiguration.Id)
                                    : 0;
                int emulatorIndex = !string.IsNullOrEmpty(modelConfiguration.Overrides.Emulator)
                                  ? _emulatorDropdown.options.FindIndex(x => x.text == modelConfiguration.Overrides.Emulator)
                                  : 0;
                _platformDropdown.SetValueWithoutNotify(platformIndex);
                _emulatorDropdown.SetValueWithoutNotify(emulatorIndex);
                _gamesList.Refresh(platformIndex);
                return;
            }

            _gamesList.Hide();
            _platformDropdown.SetValueWithoutNotify(0);
            _emulatorDropdown.SetValueWithoutNotify(0);

            if (_arcadesList.Count > 0)
                _arcadesList.Show();
            else
                _arcadesList.Hide();
        }

        private void ClearUIValues()
        {
            _idInputField.DeactivateInputField(true);
            _idInputField.SetTextWithoutNotify("");
            _interactionTypeDropdown.SetValueWithoutNotify(0);
            _platformDropdown.SetValueWithoutNotify(0);
            _grabbableToggle.SetIsOnWithoutNotify(true);
            _editMovableToggle.SetIsOnWithoutNotify(true);
            _editGrabbableToggle.SetIsOnWithoutNotify(true);
            _modelDropdown.SetValueWithoutNotify(0);
            _emulatorDropdown.SetValueWithoutNotify(0);
        }
    }
}
