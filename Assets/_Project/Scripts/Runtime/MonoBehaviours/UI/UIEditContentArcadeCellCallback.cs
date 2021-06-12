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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    public sealed class UIEditContentArcadeCellCallback : MonoBehaviour, IUICellHighlighting
    {
        [SerializeField] private FilterableArcadeListVariable _arcadesList;
        [SerializeField] private StringEvent _onButtonClicked;

        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _button;

        [SerializeField] private Color _evenColor;
        [SerializeField] private Color _oddColor;
        [SerializeField] private Color _highlightColor;

        private Color _backgroundColor;

        public void ScrollCellIndex(int index)
        {
            if (index >= _arcadesList.Filtered.Count)
                return;

            ArcadeConfiguration arcadeConfiguration = _arcadesList.Filtered[index];
            string arcadeId                         = arcadeConfiguration.Id;
		    gameObject.name                         = arcadeId;

            _backgroundColor  = index % 2 == 0 ? _evenColor : _oddColor;
            _background.color = _backgroundColor;
			_description.SetText(arcadeConfiguration.ToString());
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onButtonClicked.Raise(arcadeId));
	    }

        public void StartHighlight() => _background.color = _highlightColor;

        public void StopHighlight() => _background.color = _backgroundColor;
    }
}
