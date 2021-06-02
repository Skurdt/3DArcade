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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public abstract class UIConfigurations<TDatabase, TConfiguration, TUIConfiguration> : MonoBehaviour
        where TDatabase : Database<TConfiguration>
        where TConfiguration : DatabaseEntry, new()
        where TUIConfiguration : UIConfiguration<TDatabase, TConfiguration>
    {
        [SerializeField] private TDatabase _database;
        [SerializeField] private Button _addButton;
        [SerializeField] private TMP_InputField _addInputField;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private UIListButton _listButtonPrefab;
        [SerializeField] private TUIConfiguration _uiConfiguration;
        [SerializeField] private FloatVariable _animationDuration;

        private readonly List<UIListButton> _buttons = new List<UIListButton>();

        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;

        private void Awake()
        {
            _transform              = transform as RectTransform;
            _animationStartPosition = -_transform.rect.width;
            _animationEndPosition   = 0f;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            _addButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(_addInputField.text))
                    return;

                string text = _addInputField.text;
                TConfiguration cfg = new TConfiguration { Id = text, Description = text };
                if (_database.Add(cfg) is null)
                    return;

                _addInputField.text = null;
                InitializeList();
            });

            InitializeList();
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value);
        }

        public void Hide()
        {
            _addButton.onClick.RemoveAllListeners();
            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .OnComplete(() => gameObject.SetActive(false));
        }

        private void InitializeList()
        {
            foreach (UIListButton button in _buttons)
                Destroy(button.gameObject);
            _buttons.Clear();

            _database.Initialize();

            TConfiguration[] configurations = _database.Values;
            foreach (TConfiguration configuration in configurations)
            {
                UIListButton buttonObject = Instantiate(_listButtonPrefab, _listContent);
                _buttons.Add(buttonObject);

                buttonObject.name = configuration.Id;

                buttonObject.SelectButtonText.SetText(configuration.Description);

                buttonObject.SelectButton.onClick.RemoveAllListeners();
                buttonObject.SelectButton.onClick.AddListener(() =>
                {
                    Hide();
                    _uiConfiguration.TitleText.SetText(configuration.Id);
                    _uiConfiguration.Show(configuration);
                });

                buttonObject.DeleteButton.onClick.RemoveAllListeners();
                buttonObject.DeleteButton.onClick.AddListener(() =>
                {
                    _ = _database.Delete(configuration.Id);
                    InitializeList();
                });
            }
        }
    }
}
