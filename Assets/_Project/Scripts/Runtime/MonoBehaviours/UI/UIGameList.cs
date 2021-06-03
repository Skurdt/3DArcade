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

using Cysharp.Threading.Tasks;
using DG.Tweening;
using SG;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGameList : MonoBehaviour
    {
        [SerializeField] private Databases _databases;
        [SerializeField] private GameListVariable _gameListVariable;
        [SerializeField] private FloatVariable _animationDuration;
        [SerializeField] private TMP_InputField _searchInputField;
        [SerializeField] private LoopScrollRect _scrollRect;

        private readonly Dictionary<int, GameConfiguration[]> _allGames = new Dictionary<int, GameConfiguration[]>();

        private RectTransform _transform;
        private float _startPositionX;
        private float _endPositionX;

        private int _platformIndex;

        private void Awake()
        {
            _transform      = transform as RectTransform;
            _startPositionX = _transform.rect.width;
            _endPositionX   = 0f;
        }

        public void SetVisibility(bool visible)
        {
            _searchInputField.SetTextWithoutNotify("");
            _gameListVariable.FilteredList.Clear();
            if (visible)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);

            _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
            _scrollRect.RefillCells();

            _databases.Games.Initialize();
            _ = _transform.DOAnchorPosX(_endPositionX, _animationDuration.Value);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            ResetLists();
            _ = _transform.DOAnchorPosX(_startPositionX, _animationDuration.Value)
                          .OnComplete(() => gameObject.SetActive(false));
        }

        public void Refresh(int index)
        {
            if (index == 0)
            {
                _searchInputField.SetTextWithoutNotify("");
                _gameListVariable.FilteredList.Clear();
                _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
                _scrollRect.RefillCells();
                return;
            }

            _platformIndex = index - 1;
            if (!_allGames.ContainsKey(_platformIndex))
            {
                RefreshAsync().Forget();
                return;
            }

            if (_allGames[_platformIndex] is null)
            {
                _searchInputField.SetTextWithoutNotify("");
                _gameListVariable.FilteredList.Clear();
            }
            else
                _gameListVariable.FilteredList = _allGames[_platformIndex].ToList();

            _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
            _scrollRect.RefillCells();
        }

        public void Search(string lookUp)
        {
            if (!_allGames.ContainsKey(_platformIndex))
            {
                _gameListVariable.FilteredList.Clear();
                _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
                _scrollRect.RefillCells();
                return;
            }

            if (string.IsNullOrEmpty(lookUp))
            {
                _gameListVariable.FilteredList = _allGames[_platformIndex].ToList();
                _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
                _scrollRect.RefillCells();
                return;
            }

            string[] lookUpSplit = lookUp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            bool isID = lookUpSplit.Length > 1 && lookUpSplit[0].Equals(":id");
            if (isID)
            {
                if (lookUpSplit.Length < 2)
                {
                    _gameListVariable.FilteredList = _allGames[_platformIndex].ToList();
                    _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
                    _scrollRect.RefillCells();
                    return;
                }

                _gameListVariable.FilteredList = _allGames[_platformIndex].Where(x =>
                {
                    for (int i = 1; i < lookUpSplit.Length; ++i)
                    {
                        string partialWord = lookUpSplit[i];
                        if (x.Name.IndexOf(partialWord, StringComparison.OrdinalIgnoreCase) == -1)
                            return false;
                    }
                    return true;
                }).ToList();
                _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
                _scrollRect.RefillCells();
                return;
            }

            _gameListVariable.FilteredList = _allGames[_platformIndex].Where(x =>
            {
                for (int i = 0; i < lookUpSplit.Length; ++i)
                {
                    string partialWord = lookUpSplit[i];
                    if (x.Description.IndexOf(partialWord, StringComparison.OrdinalIgnoreCase) == -1)
                        return false;
                }
                return true;
            }).ToList();
            _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
            _scrollRect.RefillCells();
        }

        public void ResetLists()
        {
            _searchInputField.SetTextWithoutNotify("");
            _allGames.Clear();
            _gameListVariable.FilteredList.Clear();
            _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
            _scrollRect.RefillCells();
        }

        private async UniTaskVoid RefreshAsync()
        {
            _searchInputField.SetTextWithoutNotify("");
            await UniTask.Run(() =>
            {
                GameConfiguration[] games = _databases.Games.GetGames(_databases.Platforms[_platformIndex].MasterList);
                _allGames.Add(_platformIndex, games);
                if (games is null)
                    _gameListVariable.FilteredList.Clear();
                else
                    _gameListVariable.FilteredList = games.ToList();
            });
            _scrollRect.totalCount = _gameListVariable.FilteredList.Count();
            _scrollRect.RefillCells();
        }
    }
}
