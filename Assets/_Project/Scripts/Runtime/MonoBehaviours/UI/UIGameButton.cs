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

using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGameButton : MonoBehaviour, ICell
    {
        [SerializeField] private EditModeEditContentInteractionController _interactionController;

        private Button _button;
        private TMP_Text _buttonText;

        private void Awake()
        {
            _button     = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TMP_Text>();
        }

        public void ConfigureCell(TMP_InputField idInputField, GameConfiguration gameConfiguration)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                idInputField.text = gameConfiguration.Name;
                idInputField.DeactivateInputField();
                _interactionController.ApplyChangesOrAddModel();
            });
            _buttonText.SetText(gameConfiguration.ToString());
        }
    }
}
