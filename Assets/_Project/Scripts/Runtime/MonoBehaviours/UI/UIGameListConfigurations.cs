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
    public sealed class UIGameListConfigurations : MonoBehaviour
    {
        [SerializeField] private GamesDatabase _database;
        [SerializeField] private Button _addButton;
        [SerializeField] private TMP_InputField _addInputField;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private UIListButton _listButtonPrefab;
        [SerializeField] private UIGameListConfiguration _uiConfiguration;
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
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);

            _addButton.onClick.AddListener(() =>
            {
                string listName = _addInputField.text;
                if (string.IsNullOrEmpty(listName))
                    return;

                _database.AddGameList(listName);

                _addInputField.SetTextWithoutNotify(null);
                _addInputField.DeactivateInputField(true);
                InitializeList();
            });

            InitializeList();
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

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

            IEnumerable<string> gameLists = _database.GetGameLists();
            foreach (string gameList in gameLists)
            {
                UIListButton buttonObject = Instantiate(_listButtonPrefab, _listContent);
                _buttons.Add(buttonObject);

                buttonObject.name = gameList;

                buttonObject.SelectButtonText.SetText(gameList);

                buttonObject.SelectButton.onClick.AddListener(() =>
                {
                    Hide();
                    _uiConfiguration.TitleText.SetText(gameList);
                    _uiConfiguration.Show(_database.GetGames(gameList));
                });

                buttonObject.DeleteButton.onClick.AddListener(() =>
                {
                    _database.RemoveGameList(gameList);
                    InitializeList();
                });
            }
        }
    }
}
