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
using TMPro;
using UnityEngine;

namespace Arcade
{
    [DisallowMultipleComponent]
    public abstract class UIConfiguration<TDatabase, TConfiguration> : MonoBehaviour
        where TDatabase : Database<TConfiguration>
        where TConfiguration : DatabaseEntry
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }

        [SerializeField] protected FileExplorer _fileExplorer;
        [SerializeField] protected TMP_InputField _descriptionInputField;
        [SerializeField] protected TDatabase _database;

        protected TConfiguration _configuration;

        private RectTransform _transform;

        private void Awake() => _transform = transform as RectTransform;

        public void Show(TConfiguration configuration)
        {
            _configuration = configuration;
            _descriptionInputField.SetTextWithoutNotify(configuration.Description);
            SetUIValues();
            _ = _transform.DOAnchorPosX(0f, 0.3f);
        }

        public void SaveAndHide()
        {
            GetUIValues();
            _ = _database.Save(_configuration);
            Hide();
        }

        public void Hide()
        {
            _descriptionInputField.SetTextWithoutNotify("");
            ClearUIValues();
            _configuration = null;
            _ = _transform.DOAnchorPosX(-500f, 0.3f);
        }

        protected abstract void SetUIValues();

        protected abstract void GetUIValues();

        protected abstract void ClearUIValues();
    }
}
