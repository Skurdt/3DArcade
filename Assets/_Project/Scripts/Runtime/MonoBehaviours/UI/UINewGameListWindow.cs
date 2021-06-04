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
using SG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UINewGameListWindow : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }

        [SerializeField] private GamesDatabase _gamesDatabase;
        [SerializeField] private MasterListGenerator _masterListGenerator;
        [SerializeField] private GameListVariable _gameListVariable;
        [SerializeField] private TMP_Dropdown _generateFromDropdown;
        [SerializeField] private Button _generateButton;
        [SerializeField] private LoopScrollRect _scrollRect;
        [SerializeField] private TMP_InputField _idInputField;
        [SerializeField] private Button _saveButton;
        [SerializeField] private FloatVariable _animationDuration;

        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;

        private int _lastGeneratorIndex;

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

            _generateFromDropdown.value = _lastGeneratorIndex;
            SetGenerator(_lastGeneratorIndex);

            RefreshList(null);
            _idInputField.text = "";

            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            RefreshList(null);
            _idInputField.text = "";

            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .OnComplete(() => gameObject.SetActive(false));
        }

        public void SetGenerator(int index)
        {
            IGameConfigurationListGenerator generator = index switch
            {
                0 => new ListFromFolderGenerator(),
                2 => new ListFromMameXmlGenerator(),
                _ => null
            };

            if (generator is null)
            {
                _lastGeneratorIndex          = 0;
                _generateButton.interactable = false;
                RefreshList(null);
            }
            else
            {
                _lastGeneratorIndex          = index;
                _generateButton.interactable = true;
            }

            _masterListGenerator.SetGenerator(generator);
        }

        public void SetSaveButtonState(string value) => _saveButton.interactable = !string.IsNullOrEmpty(value)
                                                                                && _gameListVariable.Value.Length > 0;

        public void AddListToDatabase()
        {
            if (string.IsNullOrEmpty(_idInputField.text) || _gameListVariable.Value.Length == 0)
                return;

            _gamesDatabase.AddGameList(_idInputField.text);
            _gamesDatabase.AddGames(_idInputField.text, _gameListVariable.Value);
        }

        private void RefreshList(GameConfiguration[] gameConfigurations)
        {
            _gameListVariable.Value = gameConfigurations;
            _scrollRect.totalCount  = !(gameConfigurations is null) ? gameConfigurations.Length : 0;
            _scrollRect.RefillCells();
        }
    }
}
