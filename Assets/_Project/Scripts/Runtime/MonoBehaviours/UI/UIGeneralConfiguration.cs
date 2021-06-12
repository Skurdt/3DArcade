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
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGeneralConfiguration : MonoBehaviour, IUIVisibility
    {
        [SerializeField] private TMP_Dropdown _startingArcadeDropdown;
        [SerializeField] private TMP_Dropdown _startingArcadeTypeDropdown;
        [SerializeField] private Toggle _mouseLookReverseToggle;
        [SerializeField] private Toggle _enableVRToggle;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;

        private ArcadesDatabase _arcadesDatabase;
        private GeneralConfigurationVariable _generalConfigurationVariable;
        private ArcadeStandardFpsNormalState _arcadeStandardFpsNormalState;
        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        [Inject]
        public void Construct(ArcadesDatabase arcadesDatabase,
                              GeneralConfigurationVariable generalConfigurationVariable,
                              ArcadeStandardFpsNormalState arcadeStandardFpsNormalState,
                              FloatVariable animationDuration)
        {
            _arcadesDatabase              = arcadesDatabase;
            _generalConfigurationVariable = generalConfigurationVariable;
            _arcadeStandardFpsNormalState = arcadeStandardFpsNormalState;
            _animationDuration            = animationDuration;
            _transform                    = transform! as RectTransform;
            _animationStartPosition       = -_transform.rect.width;
            _animationEndPosition         = 0f;

            _saveButton.onClick.AddListener(() =>
            {
                Save();
                _ = Hide();
                _arcadeStandardFpsNormalState.EnableInput();
            });
            _cancelButton.onClick.AddListener(() =>
            {
                _ = Hide();
                _arcadeStandardFpsNormalState.EnableInput();
            });
        }

        private void OnDestroy()
        {
            _saveButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
        }

        public Tween SetVisibility(bool visible) => visible ? Show() : Hide();

        public Tween Show()
        {
            if (_visible)
                return null;

            _visible = true;

            gameObject.SetActive(true);

            _generalConfigurationVariable.Initialize();
            _arcadesDatabase.Initialize();

            GeneralConfiguration generalConfiguration = _generalConfigurationVariable.Value;

            _startingArcadeDropdown.ClearOptions();
            _startingArcadeDropdown.AddOptions(new List<string> { "" }.Concat(_arcadesDatabase.Names).ToList());
            _startingArcadeDropdown.value = _startingArcadeDropdown.options.FindIndex(x => x.text == generalConfiguration.StartingArcade);

            _startingArcadeTypeDropdown.ClearOptions();
            _startingArcadeTypeDropdown.AddOptions(System.Enum.GetNames(typeof(ArcadeType)).ToList());
            _startingArcadeTypeDropdown.value = (int)generalConfiguration.StartingArcadeType;

            _mouseLookReverseToggle.isOn = generalConfiguration.MouseLookReverse;
            _enableVRToggle.isOn         = generalConfiguration.EnableVR;

            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic);
        }

        public Tween Hide()
        {
            if (!_visible)
                return null;

            _visible = false;

            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic)
                             .OnComplete(() => gameObject.SetActive(false));
        }

        private void Save()
        {
            _generalConfigurationVariable.Value.StartingArcade     = _startingArcadeDropdown.options[_startingArcadeDropdown.value].text;
            _generalConfigurationVariable.Value.StartingArcadeType = (ArcadeType)_startingArcadeTypeDropdown.value;
            _generalConfigurationVariable.Value.MouseLookReverse   = _mouseLookReverseToggle.isOn;
            _generalConfigurationVariable.Value.EnableVR           = _enableVRToggle.isOn;

            if (_generalConfigurationVariable.Value.Save())
                _generalConfigurationVariable.Initialize();
        }
    }
}
