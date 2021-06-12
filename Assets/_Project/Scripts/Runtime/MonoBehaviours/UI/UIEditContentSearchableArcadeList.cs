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

using System;
using System.Linq;
using UnityEngine;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIEditContentSearchableArcadeList : UIEditContentSearchableList<ArcadeConfiguration, FilterableArcadeListVariable>
    {
        public override int Count => _filterableList.Value.Count;

        private UIEditContentArcadeCellCallback _currentCell;

        private void Update()
        {
            if (!_visible)
                return;

            UI.HandleCellHighlighting(ref _currentCell);
        }

        protected override void OnInit() => _filterableList.Value = _databases.Arcades.Values.ToList();

        protected override void OnShow()
        {
            _filterableList.Filtered = _filterableList.Value.ToList();
            _scrollRect.totalCount   = _filterableList.Value.Count;
            _scrollRect.RefillCells();
        }

        protected override void OnHide()
        {
            _scrollRect.totalCount = 0;
            _scrollRect.RefillCells();
        }

        protected override void Search(string lookUp)
        {
            if (_filterableList.Value.Count == 0)
            {
                _filterableList.Filtered.Clear();
                _scrollRect.totalCount = 0;
                _scrollRect.RefillCells();
                return;
            }

            if (string.IsNullOrEmpty(lookUp))
            {
                _filterableList.Filtered = _filterableList.Value.ToList();
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
                    _filterableList.Filtered = _filterableList.Value.ToList();
                    _scrollRect.totalCount = _filterableList.Filtered.Count;
                    _scrollRect.RefillCells();
                    return;
                }

                _filterableList.Filtered = _filterableList.Value.Where(x =>
                {
                    for (int i = 1; i < lookUpSplit.Length; ++i)
                    {
                        string partialWord = lookUpSplit[i];
                        if (x.Id.IndexOf(partialWord, StringComparison.OrdinalIgnoreCase) == -1)
                            return false;
                    }
                    return true;
                }).ToList();
            }
            else
            {
                _filterableList.Filtered = _filterableList.Value.Where(x =>
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
    }
}
