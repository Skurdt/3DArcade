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
using UnityEngine.UI;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public abstract class UIConfiguration<TDatabase, TConfiguration, TUIConfigurations> : MonoBehaviour, IUIConfiguration<TConfiguration>
        where TDatabase : Database<TConfiguration>
        where TConfiguration : DatabaseEntry
        where TUIConfigurations : IUIVisibility
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }
        [SerializeField] protected TMP_InputField _descriptionInputField;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;

        protected FileExplorer _fileExplorer;
        protected TDatabase _database;
        protected TUIConfigurations _uiConfigurations;
        protected TConfiguration _configuration;
        protected bool _initialized;

        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        [Inject]
        public void Construct(FileExplorer fileExplorer,
                              TDatabase database,
                              TUIConfigurations uiConfigurations,
                              FloatVariable animationDuration)
        {
            _fileExplorer           = fileExplorer;
            _database               = database;
            _uiConfigurations       = uiConfigurations;
            _animationDuration      = animationDuration;
            _transform              = transform as RectTransform;
            _animationStartPosition = -_transform.rect.width;
            _animationEndPosition   = 0f;

            _saveButton.onClick.AddListener(() =>
            {
                SaveAndHide();
                _ = _uiConfigurations.Show();
            });

            _cancelButton.onClick.AddListener(() =>
            {
                Hide();
                _ = _uiConfigurations.Show();
            });
        }

        private void OnDestroy()
        {
            _saveButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
        }

        public void Show(TConfiguration configuration)
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);

            _configuration = configuration;
            _descriptionInputField.DeactivateInputField(true);
            _descriptionInputField.SetTextWithoutNotify(configuration.Description);
            _descriptionInputField.caretPosition = 0;
            SetUIValues();
            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic);
        }

        private void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            _descriptionInputField.DeactivateInputField(true);
            _descriptionInputField.SetTextWithoutNotify("");
            _descriptionInputField.caretPosition = 0;
            ClearUIValues();
            _configuration = null;
            _initialized   = false;
            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic)
                          .OnComplete(() => gameObject.SetActive(false));
        }

        private void SaveAndHide()
        {
            GetUIValues();
            _ = _database.Save(_configuration);
            Hide();
        }

        protected abstract void GetUIValues();

        protected abstract void SetUIValues();

        protected abstract void ClearUIValues();
    }
}
