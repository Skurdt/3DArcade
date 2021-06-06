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

namespace Arcade
{
    public enum MasterListSource
    {
        Folder,
        Mame2003PlusXML,
        Mame0221Compatible,
        NoIntroDAT
    }

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
        private bool _fromExeEventRaised;

        private void Awake()
        {
            _transform              = transform as RectTransform;
            _animationStartPosition = -_transform.rect.width;
            _animationEndPosition   = 0f;
        }

        private void Update()
        {
            if (_fromExeEventRaised)
                return;

            if (!(_masterListGenerator.Generator is ListFromMame0221CompatibleGenerator fromExeGenerator))
                return;

            if (fromExeGenerator.Games is null)
                return;

            _masterListGenerator.GameConfigurationsEvent.Raise(fromExeGenerator.Games);
            _fromExeEventRaised = true;
        }

        public void Show()
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);

            _generateFromDropdown.ClearOptions();
            _generateFromDropdown.AddOptions(System.Enum.GetNames(typeof(MasterListSource)).ToList());
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
            IGameConfigurationListGenerator generator = (MasterListSource)index switch
            {
                MasterListSource.Folder             => new ListFromFolderGenerator(),
                MasterListSource.Mame2003PlusXML    => new ListFromMame2003PlusXmlGenerator(),
                MasterListSource.Mame0221Compatible => new ListFromMame0221CompatibleGenerator(),
                _                                   => null
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

                if (generator is ListFromMame0221CompatibleGenerator)
                    _fromExeEventRaised = false;
            }

            _masterListGenerator.SetGenerator(generator);
        }

        public void SetSaveButtonState(string value) => _saveButton.interactable = !string.IsNullOrEmpty(value)
                                                                                && _gameListVariable.Value.Count > 0;

        public void AddListToDatabase()
        {
            if (string.IsNullOrEmpty(_idInputField.text) || _gameListVariable.Value.Count == 0)
                return;

            _gamesDatabase.AddGameList(_idInputField.text);
            _gamesDatabase.AddGames(_idInputField.text, _gameListVariable.Value);
        }

        private void RefreshList(GameConfiguration[] gameConfigurations)
        {
            _gameListVariable.Value = gameConfigurations?.ToList();
            _scrollRect.totalCount  = !(gameConfigurations is null) ? gameConfigurations.Length : 0;
            _scrollRect.RefillCells();
        }
    }
}
