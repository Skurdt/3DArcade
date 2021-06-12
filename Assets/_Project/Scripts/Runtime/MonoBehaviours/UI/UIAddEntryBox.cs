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

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIAddEntryBox : MonoBehaviour
    {
        [field: SerializeField] public TMP_InputField DescriptionInputField { get; private set; }
        [field: SerializeField] public TMP_InputField IdInputField { get; private set; }
        [field: SerializeField] public Button AddButton { get; private set; }

        [SerializeField] private TMP_Text _errorText;

        private bool _idManuallySet;

        [Inject]
        public void Construct()
        {
            DescriptionInputField.onValueChanged.AddListener((str) =>
            {
                bool hasDescription = str.Length > 0;
                if (!hasDescription)
                    _errorText.SetText("Description must be set.");

                if (!_idManuallySet)
                    IdInputField.SetTextWithoutNotify(str.ToLowerInvariant().Replace(' ', '_').Replace(":", ""));

                SetButtonState();
            });

            IdInputField.onValueChanged.AddListener((str) =>
            {
                bool hasDescription = DescriptionInputField.text.Length > 0;
                bool hasId          = str.Length > 0;

                _idManuallySet = hasId;

                if (!hasId && hasDescription)
                    _errorText.SetText("ID must be set.");

                if (hasId)
                    IdInputField.text = str.ToLowerInvariant().Replace(' ', '_').Replace(":", "");

                SetButtonState();
            });
        }

        public void ResetFields()
        {
            DescriptionInputField.SetTextWithoutNotify("");
            IdInputField.SetTextWithoutNotify("");
            _idManuallySet = false;
        }

        private void SetButtonState()
        {
            bool hasDescription = DescriptionInputField.text.Length > 0;
            bool hasId          = IdInputField.text.Length > 0;
            bool valid          = hasDescription && hasId;

            AddButton.interactable = valid;
            if (valid)
                _errorText.Clear();
        }
    }
}
