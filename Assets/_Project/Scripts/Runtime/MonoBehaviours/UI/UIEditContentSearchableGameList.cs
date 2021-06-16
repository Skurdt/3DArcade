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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIEditContentSearchableGameList : UIEditContentSearchableList<GameConfiguration, FilterableGameListVariable>
    {
        public override int Count => _allGames.ContainsKey(_platformIndex) ? _allGames[_platformIndex].Length : 0;

        private readonly Dictionary<int, GameConfiguration[]> _allGames = new Dictionary<int, GameConfiguration[]>();
        private readonly object _lock = new object();

        private int _platformIndex;
        private UIEditContentGameCellCallback _currentCell;
        private CancellationTokenSource _cancellationTokenSource;

        private void OnDestroy() => _cancellationTokenSource?.Dispose();

        private void Update()
        {
            if (!_visible)
                return;

            UI.HandleCellHighlighting(ref _currentCell);
        }

        public void Refresh(int platformDropdownValue)
        {
            if (platformDropdownValue == 0)
            {
                _searchInputField.DeactivateInputField(true);
                _searchInputField.SetTextWithoutNotify("");
                _filterableList.Filtered.Clear();
                _scrollRect.totalCount = 0;
                _scrollRect.RefillCells();
                Hide();
                return;
            }

            int platformIndex = platformDropdownValue - 1;

            if (_platformIndex != platformIndex)
            {
                _searchInputField.DeactivateInputField(true);
                _searchInputField.SetTextWithoutNotify("");
            }

            _platformIndex = platformIndex;
            if (!_allGames.ContainsKey(_platformIndex))
            {
                RefreshAsync().Forget();
                return;
            }

            if (_allGames[_platformIndex] is null)
            {
                _searchInputField.DeactivateInputField(true);
                _searchInputField.SetTextWithoutNotify("");
                _filterableList.Filtered.Clear();
            }
            else
                _filterableList.Filtered = _allGames[_platformIndex].ToList();

            _scrollRect.totalCount = _filterableList.Filtered.Count;
            _scrollRect.RefillCells();
            if (_filterableList.Filtered.Count > 0)
                Show();
            else
                Hide();
        }

        protected override void OnInit()
        {
            _cancellationTokenSource?.Dispose();
            _allGames.Clear();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override void OnDeinit()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _allGames.Clear();
        }

        protected override void OnShow()
        {
            _scrollRect.totalCount = _filterableList.Filtered.Count;
            _scrollRect.RefillCells();
        }

        protected override void OnHide()
        {
            _scrollRect.totalCount = 0;
            _scrollRect.RefillCells();
        }

        protected override void Search(string lookUp)
        {
            if (!_allGames.ContainsKey(_platformIndex))
            {
                _filterableList.Filtered.Clear();
                _scrollRect.totalCount = 0;
                _scrollRect.RefillCells();
                return;
            }

            if (string.IsNullOrEmpty(lookUp))
            {
                _filterableList.Filtered = _allGames[_platformIndex].ToList();
                _scrollRect.totalCount = _filterableList.Filtered.Count;
                _scrollRect.RefillCells();
                return;
            }

            string[] lookUpSplit = lookUp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            bool isID = lookUpSplit.Length > 1 && lookUpSplit[0].Equals(":id");
            if (isID)
            {
                if (lookUpSplit.Length < 2)
                {
                    _filterableList.Filtered = _allGames[_platformIndex].ToList();
                    _scrollRect.totalCount = _filterableList.Filtered.Count;
                    _scrollRect.RefillCells();
                    return;
                }

                _filterableList.Filtered = _allGames[_platformIndex].Where(x =>
                {
                    for (int i = 1; i < lookUpSplit.Length; ++i)
                    {
                        string partialWord = lookUpSplit[i];
                        if (x.Name.IndexOf(partialWord, StringComparison.OrdinalIgnoreCase) == -1)
                            return false;
                    }
                    return true;
                }).ToList();
            }
            else
            {
                _filterableList.Filtered = _allGames[_platformIndex].Where(x =>
                {
                    for (int i = 0; i < lookUpSplit.Length; ++i)
                    {
                        string partialWord = lookUpSplit[i];
                        if (x.Description.IndexOf(partialWord, StringComparison.OrdinalIgnoreCase) == -1)
                            return false;
                    }
                    return true;
                }).ToList();
            }

            _scrollRect.totalCount = _filterableList.Filtered.Count;
            _scrollRect.RefillCells();
        }

        private async UniTaskVoid RefreshAsync()
        {
            _searchInputField.DeactivateInputField(true);
            _searchInputField.SetTextWithoutNotify("");
            _ = await UniTask.Run(() =>
            {
                lock (_lock)
                {
                    GameConfiguration[] games = _databases.Games.GetGames(_databases.Platforms[_platformIndex].MasterList);
                    _allGames.Add(_platformIndex, games);
                    if (games is null)
                        _filterableList.Filtered.Clear();
                    else
                        _filterableList.Filtered = games.ToList();
                }
            }, cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();
            _scrollRect.totalCount = _filterableList.Filtered.Count;
            _scrollRect.RefillCells();
            if (_filterableList.Filtered.Count > 0)
                Show();
            else
                Hide();
        }
    }
}
