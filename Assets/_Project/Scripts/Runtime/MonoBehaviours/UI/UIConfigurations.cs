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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public abstract class UIConfigurations<TDatabase, TConfiguration, TUIConfiguration> : MonoBehaviour, IUIVisibility
        where TDatabase : Database<TConfiguration>
        where TConfiguration : DatabaseEntry, new()
        where TUIConfiguration : IUIConfiguration<TConfiguration>
    {
        [SerializeField] private UIAddEntryBox _addEntryBox;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private Button _closeButton;

        private readonly List<UIListButton> _buttons = new List<UIListButton>();

        private TDatabase _database;
        private TUIConfiguration _uiConfiguration;
        private UIListButton _listButtonPrefab;
        private ArcadeStandardFpsNormalState _arcadeStandardFpsNormalState;
        private FloatVariable _animationDuration;
        private RectTransform _transform;
        private float _animationStartPosition;
        private float _animationEndPosition;
        private bool _visible;

        [Inject]
        public void Construct(TDatabase database,
                              TUIConfiguration uiConfiguration,
                              UIListButton listButtonPrefab,
                              ArcadeStandardFpsNormalState arcadeStandardFpsNormalState,
                              FloatVariable animationDuration)
        {
            _database                     = database;
            _uiConfiguration              = uiConfiguration;
            _listButtonPrefab             = listButtonPrefab;
            _arcadeStandardFpsNormalState = arcadeStandardFpsNormalState;
            _animationDuration            = animationDuration;
            _transform                    = transform as RectTransform;
            _animationStartPosition       = -_transform.rect.width;
            _animationEndPosition         = 0f;

            _closeButton.onClick.AddListener(() =>
            {
                _ = Hide();
                _arcadeStandardFpsNormalState.EnableInput();
            });
        }

        private void OnDestroy() => _closeButton.onClick.RemoveAllListeners();

        public Tween SetVisibility(bool visible) => visible ? Show() : Hide();

        public Tween Show()
        {
            if (_visible)
                return null;

            _visible = true;

            gameObject.SetActive(true);

            _addEntryBox.AddButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(_addEntryBox.IdInputField.text))
                    return;

                TConfiguration cfg = new TConfiguration { Id = _addEntryBox.IdInputField.text, Description = _addEntryBox.DescriptionInputField.text };
                if (_database.Add(cfg) is null)
                    return;

                _addEntryBox.DescriptionInputField.SetTextWithoutNotify("");
                _addEntryBox.IdInputField.SetTextWithoutNotify("");
                InitializeList();
            });

            InitializeList();
            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationEndPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic);
        }

        public Tween Hide()
        {
            if (!_visible)
                return null;

            _visible = false;

            _addEntryBox.AddButton.onClick.RemoveAllListeners();
            _addEntryBox.ResetFields();
            _ = _transform.DOKill();
            return _transform.DOAnchorPosX(_animationStartPosition, _animationDuration.Value)
                             .SetEase(Ease.InOutCubic)
                             .OnComplete(() => gameObject.SetActive(false));
        }

        private void InitializeList()
        {
            foreach (UIListButton button in _buttons)
                Destroy(button.gameObject);
            _buttons.Clear();

            _database.Initialize();

            TConfiguration[] configurations = _database.Values;
            foreach (TConfiguration configuration in configurations)
            {
                UIListButton buttonObject = Instantiate(_listButtonPrefab, _listContent);
                _buttons.Add(buttonObject);

                buttonObject.name = configuration.Id;

                buttonObject.SelectButtonText.SetText(configuration.Description);

                buttonObject.SelectButton.onClick.RemoveAllListeners();
                buttonObject.SelectButton.onClick.AddListener(() =>
                {
                    _ = Hide();
                    _uiConfiguration.TitleText.SetText(configuration.Id);
                    _uiConfiguration.Show(configuration);
                });

                buttonObject.DeleteButton.onClick.RemoveAllListeners();
                buttonObject.DeleteButton.onClick.AddListener(() =>
                {
                    _ = _database.Delete(configuration.Id);
                    InitializeList();
                });
            }
        }
    }
}
