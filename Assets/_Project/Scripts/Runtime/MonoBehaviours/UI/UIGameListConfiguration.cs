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
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGameListConfiguration : MonoBehaviour
    {
        [SerializeField] private GameListVariable _gameListVariable;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private LoopScrollRect _scrollRect;
        [SerializeField] private FloatVariable _animationDuration;

        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;

        private UIGameConfigurationCellCallback _currentCell;

        private void Awake()
        {
            _transform              = transform as RectTransform;
            _animationStartPosition = -_transform.rect.width;
            _animationEndPosition   = 0f;
        }

        private void Update()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (raycastResults.Count == 0)
                return;

            UIGameConfigurationCellCallback cell = raycastResults[0].gameObject.GetComponentInParent<UIGameConfigurationCellCallback>();
            if (_currentCell == cell)
                return;

            if (_currentCell != null)
                _currentCell.StopHighlight();

            _currentCell = cell;

            if (_currentCell != null)
                _currentCell.StartHighlight();
        }

        public void Show(string gameListName, GameConfiguration[] configurations)
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);

            _titleText.SetText(gameListName);

            _gameListVariable.GameListName = gameListName;
            _gameListVariable.Value        = configurations?.ToList();

            RefreshList(configurations.Length);

            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            _gameListVariable.GameListName = null;
            _gameListVariable.Value        = null;

            _scrollRect.ClearCells();

            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .OnComplete(() => gameObject.SetActive(false));
        }

        public void RemoveGame(GameConfiguration gameConfiguration)
        {
            if (gameConfiguration is null || _gameListVariable.Value is null)
                return;

            if (!_gameListVariable.Value.Contains(gameConfiguration))
                return;

            _currentCell = null;

            //int index = _gameListVariable.Value.IndexOf(gameConfiguration);
            _ = _gameListVariable.Value.Remove(gameConfiguration);

            _scrollRect.RefreshCells();

            //RefreshList(_gameListVariable.Value.Count);

            //if (index >= _scrollRect.totalCount)
            //    --index;
            //_scrollRect.SrollToCell(index, 0f);
        }

        private void RefreshList(int numItems)
        {
            _scrollRect.totalCount = numItems;
            _scrollRect.RefillCells();
        }
    }
}
