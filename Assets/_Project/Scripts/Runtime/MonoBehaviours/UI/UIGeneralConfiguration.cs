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

using DG.Tweening;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGeneralConfiguration : MonoBehaviour
    {
        [SerializeField] private GeneralConfigurationVariable _generalConfiguration;
        [SerializeField] private ArcadesDatabase _arcadeDatabase;
        [SerializeField] private TMP_Dropdown _startingArcadeDropdown;
        [SerializeField] private TMP_Dropdown _startingArcadeTypeDropdown;
        [SerializeField] private Toggle _mouseLookReverseToggle;
        [SerializeField] private Toggle _enableVRToggle;

        private RectTransform _transform;

        private void Awake() => _transform = transform as RectTransform;

        public void Show()
        {
            _generalConfiguration.Initialize();
            _arcadeDatabase.Initialize();

            GeneralConfiguration generalConfiguration = _generalConfiguration.Value;

            _startingArcadeDropdown.ClearOptions();
            _startingArcadeDropdown.AddOptions(_arcadeDatabase.GetNames());
            _startingArcadeDropdown.value = _startingArcadeDropdown.options.FindIndex(x => x.text == generalConfiguration.StartingArcade);

            _startingArcadeTypeDropdown.ClearOptions();
            _startingArcadeTypeDropdown.AddOptions(System.Enum.GetNames(typeof(ArcadeType)).ToList());
            _startingArcadeTypeDropdown.value = (int)generalConfiguration.StartingArcadeType;

            _mouseLookReverseToggle.isOn = generalConfiguration.MouseLookReverse;
            _enableVRToggle.isOn         = generalConfiguration.EnableVR;

            _ = _transform.DOAnchorPosX(0f, 0.3f)
                          .OnStart(() => gameObject.SetActive(true));
        }

        public void Hide() => _transform.DOAnchorPosX(-500f, 0.3f)
                                        .OnComplete(() => gameObject.SetActive(false));

        public void Save()
        {
            _generalConfiguration.Value = new GeneralConfiguration
            {
                StartingArcade     = _startingArcadeDropdown.options[_startingArcadeDropdown.value].text,
                StartingArcadeType = (ArcadeType)_startingArcadeDropdown.value,
                MouseLookReverse   = _mouseLookReverseToggle.isOn,
                EnableVR           = _enableVRToggle.isOn
            };

            if (_generalConfiguration.Value.Save())
                _generalConfiguration.Initialize();
        }
    }
}
