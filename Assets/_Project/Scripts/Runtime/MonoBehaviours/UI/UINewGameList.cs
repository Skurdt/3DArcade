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
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Arcade
{
    public enum MasterListSource
    {
        Folder,
        Mame2003PlusXml,
        Mame0221CompatibleExe
    }

    [DisallowMultipleComponent]
    public sealed class UINewGameList : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }

        [SerializeField] private TMP_Dropdown _generateFromDropdown;
        [SerializeField] private TMP_InputField _extensionsInputField;
        [SerializeField] private Button _generateButton;
        [SerializeField] private LoopScrollRect _scrollRect;
        [SerializeField] private TMP_InputField _idInputField;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _closeButton;

        private GamesDatabase _gamesDatabase;
        private MasterListGenerator _masterListGenerator;
        private FilterableGameListVariable _gameListVariable;
        private UIGameListConfigurations _uiGameListConfigurations;
        private GameConfigurationsEvent _gamesGeneratedEvent;
        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private int _lastGeneratorIndex;
        private UINewGameConfigurationCellCallback _currentCell;
        private bool _fromExeEventRaised;
        private bool _visible;

        [Inject]
        public void Construct(GamesDatabase gamesDatabase,
                              MasterListGenerator masterListGenerator,
                              FilterableGameListVariable gameListVariable,
                              UIGameListConfigurations uiGameListConfigurations,
                              GameConfigurationsEvent gamesGeneratedEvent,
                              FloatVariable animationDuration)
        {
            _gamesDatabase            = gamesDatabase;
            _masterListGenerator      = masterListGenerator;
            _gameListVariable         = gameListVariable;
            _uiGameListConfigurations = uiGameListConfigurations;
            _gamesGeneratedEvent      = gamesGeneratedEvent;
            _animationDuration        = animationDuration;
            _transform                = transform as RectTransform;
            _animationStartPosition   = -_transform.rect.width;
            _animationEndPosition     = 0f;

            _generateFromDropdown.onValueChanged.AddListener((index) => SetGenerator(index));

            _generateButton.onClick.AddListener(()
                => _masterListGenerator.Generate(_extensionsInputField.text.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)));

            _idInputField.onValueChanged.AddListener((str) => SetSaveButtonState(str));

            _saveButton.onClick.AddListener(() =>
            {
                AddListToDatabase();
                Hide();
                _ = _uiGameListConfigurations.Show();
            });

            _closeButton.onClick.AddListener(() =>
            {
                Hide();
                _ = _uiGameListConfigurations.Show();
            });
        }

        private void OnDestroy()
        {
            _generateFromDropdown.onValueChanged.RemoveAllListeners();
            _idInputField.onValueChanged.RemoveAllListeners();
            _generateButton.onClick.RemoveAllListeners();
            _saveButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            UI.HandleCellHighlighting(ref _currentCell);

            if (_fromExeEventRaised)
                return;

            if (!(_masterListGenerator.Generator is ListFromMame0221CompatibleExeGenerator fromExeGenerator))
                return;

            if (fromExeGenerator.Games is null)
                return;

            _gamesGeneratedEvent.Raise(fromExeGenerator.Games);
            _fromExeEventRaised = true;
        }

        public void Show()
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);

            _generateFromDropdown.ClearOptions();
            _generateFromDropdown.AddOptions(System.Enum.GetNames(typeof(MasterListSource)).ToList());
            _generateFromDropdown.value = _lastGeneratorIndex;
            SetGenerator(_lastGeneratorIndex);

            RefreshList(null);
            _idInputField.text = "";

            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic);
        }

        public void RefreshList(GameConfiguration[] gameConfigurations)
        {
            _gameListVariable.Value = gameConfigurations?.ToList();
            _scrollRect.totalCount  = !(gameConfigurations is null) ? gameConfigurations.Length : 0;
            _scrollRect.RefillCells();
        }

        public void RemoveGameFromList(GameConfiguration gameConfiguration)
        {
            if (_gameListVariable.Value is null || !_gameListVariable.Value.Contains(gameConfiguration))
                return;

            _ = _gameListVariable.Value.Remove(gameConfiguration);
            _scrollRect.totalCount  = _gameListVariable.Value.Count;
            _scrollRect.RefreshCells();
        }

        private void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            RefreshList(null);
            _idInputField.text = "";

            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic)
                          .OnComplete(() => gameObject.SetActive(false));
        }

        private void SetGenerator(int index)
        {
            IGameConfigurationListGenerator generator = (MasterListSource)index switch
            {
                MasterListSource.Folder                => new ListFromFolderGenerator(_gamesGeneratedEvent),
                MasterListSource.Mame2003PlusXml       => new ListFromMame2003PlusXmlGenerator(_gamesGeneratedEvent),
                MasterListSource.Mame0221CompatibleExe => new ListFromMame0221CompatibleExeGenerator(),
                _                                      => null
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

                if (generator is ListFromMame0221CompatibleExeGenerator)
                    _fromExeEventRaised = false;
            }

            _masterListGenerator.SetGenerator(generator);
        }

        private void SetSaveButtonState(string value) => _saveButton.interactable = !string.IsNullOrEmpty(value)
                                                                                && _gameListVariable.Value.Count > 0;

        private void AddListToDatabase()
        {
            if (string.IsNullOrEmpty(_idInputField.text) || _gameListVariable.Value.Count == 0)
                return;

            _gamesDatabase.AddGameList(_idInputField.text);
            _gamesDatabase.AddGames(_idInputField.text, _gameListVariable.Value);
        }
    }
}
