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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public abstract class UIConfigurations<TDatabase, TConfiguration, TUIConfiguration> : MonoBehaviour
        where TDatabase : MultiFileDatabase<TConfiguration>
        where TConfiguration : DatabaseEntry, new()
        where TUIConfiguration : UIConfiguration<TDatabase, TConfiguration>
    {
        [SerializeField] private TDatabase _database;
        [SerializeField] private Button _addButton;
        [SerializeField] private TMP_InputField _addInputField;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private UIListButton _listButtonPrefab;
        [SerializeField] private TUIConfiguration _uiConfiguration;

        private readonly List<UIListButton> _buttons = new List<UIListButton>();

        private RectTransform _transform;

        private void Awake() => _transform = transform as RectTransform;

        public void Show()
        {
            _addButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(_addInputField.text))
                    return;

                string text = _addInputField.text;
                TConfiguration cfg = new TConfiguration { Id = text, Description = text };
                if (_database.Add(cfg) == null)
                    return;

                _addInputField.text = null;
                InitializeList();
            });

            InitializeList();
            _ = _transform.DOAnchorPosX(0f, 0.3f);
        }

        public void Hide()
        {
            _addButton.onClick.RemoveAllListeners();
            _ = _transform.DOAnchorPosX(-500f, 0.3f);
        }

        private void InitializeList()
        {
            foreach (UIListButton button in _buttons)
                Destroy(button.gameObject);
            _buttons.Clear();

            _database.Initialize();

            TConfiguration[] configurations = _database.GetValues();
            foreach (TConfiguration configuration in configurations)
            {
                UIListButton buttonObject = Instantiate(_listButtonPrefab, _listContent);
                _buttons.Add(buttonObject);

                buttonObject.name = configuration.Id;

                buttonObject.SelectButtonText.SetText(configuration.Description);

                buttonObject.SelectButton.onClick.AddListener(() =>
                {
                    Hide();
                    _uiConfiguration.TitleText.SetText(configuration.ToString());
                    _uiConfiguration.Show(configuration);
                });

                buttonObject.DeleteButton.onClick.AddListener(() =>
                {
                    _ = _database.Delete(configuration.Id);
                    InitializeList();
                });
            }
        }
    }
}
