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

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGameListConfiguration : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }

        [SerializeField] private GameListVariable _gameListVariable;
        [SerializeField] private FloatVariable _animationDuration;
        [SerializeField] private LoopScrollRect _scrollRect;

        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;

        private void Awake()
        {
            _transform              = transform as RectTransform;
            _animationStartPosition = -_transform.rect.width;
            _animationEndPosition   = 0f;
        }

        public void Show(GameConfiguration[] configurations)
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);

            _gameListVariable.Value = configurations;

            _scrollRect.totalCount = configurations.Length;
            _scrollRect.RefillCells();

            _ = _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            _gameListVariable.Value = null;
            _scrollRect.totalCount = 0;
            _scrollRect.RefillCells();
            _ = _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                          .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
