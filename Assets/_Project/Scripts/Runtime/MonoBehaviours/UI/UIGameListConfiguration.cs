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
    [DisallowMultipleComponent]
    public sealed class UIGameListConfiguration : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private LoopScrollRect _scrollRect;
        [SerializeField] private Button _cancelButton;

        private FilterableGameListVariable _gameListVariable;
        private UIGameListConfigurations _uiGameListConfigurations;
        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        private UIGameConfigurationCellCallback _currentCell;

        [Inject]
        public void Construct(UIGameListConfigurations uiGameListConfigurations,
                              FilterableGameListVariable gameListVariable,
                              FloatVariable animationDuration)
        {
            _uiGameListConfigurations = uiGameListConfigurations;
            _gameListVariable         = gameListVariable;
            _animationDuration        = animationDuration;
            _transform                = transform as RectTransform;
            _animationStartPosition   = -_transform.rect.width;
            _animationEndPosition     = 0f;

            _cancelButton.onClick.AddListener(() =>
            {
                Hide();
                _ = _uiGameListConfigurations.Show();
            });
        }

        private void OnDestroy() => _cancelButton.onClick.RemoveAllListeners();

        private void Update() => UI.HandleCellHighlighting(ref _currentCell);

        public void Show(string gameListName, GameConfiguration[] configurations)
        {
            if (_visible)
                return;

            _visible = true;

            gameObject.SetActive(true);

            _titleText.SetText(gameListName);

            _gameListVariable.GameListName = gameListName;
            _gameListVariable.Value        = configurations?.ToList();

            RefreshList(!(configurations is null) ? configurations.Length : 0);

            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic);
        }

        public void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            _gameListVariable.GameListName = null;
            _gameListVariable.Value        = null;

            _scrollRect.ClearCells();

            _ = _transform.DOKill();
            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .SetEase(Ease.InOutCubic)
                          .OnComplete(() =>
                          {
                              gameObject.SetActive(false);
                              _currentCell = null;
                          });
        }

        public void RemoveGame(GameConfiguration gameConfiguration)
        {
            if (gameConfiguration is null || _gameListVariable.Value is null)
                return;

            if (!_gameListVariable.Value.Contains(gameConfiguration))
                return;

            _currentCell = null;

            _ = _gameListVariable.Value.Remove(gameConfiguration);

            _scrollRect.totalCount = _gameListVariable.Value.Count;
            _scrollRect.RefreshCells();
        }

        private void RefreshList(int numItems)
        {
            _scrollRect.totalCount = numItems;
            _scrollRect.RefillCells();
        }
    }
}
