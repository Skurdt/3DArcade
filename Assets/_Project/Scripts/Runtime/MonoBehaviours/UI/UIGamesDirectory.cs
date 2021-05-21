﻿/* MIT License

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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UIGamesDirectory : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _pathInputField;
        [SerializeField] private Button _browseButton;
        [SerializeField] private Button _moveUpButton;
        [SerializeField] private Button _moveDownButton;
        [SerializeField] private Button _deleteButton;

        public string InputFieldText => _pathInputField.text;

        private void OnDestroy()
        {
            _pathInputField.text = "";
            _browseButton.onClick.RemoveAllListeners();
            _moveUpButton.onClick.RemoveAllListeners();
            _moveDownButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
        }

        public void Initialize(UIGamesDirectories uiGamesDirectories, FileExplorer fileExplorer, string path)
        {
            gameObject.name      = $"GamesDirectory_Value";
            _pathInputField.text = path;

            _browseButton.onClick.AddListener(()
                => fileExplorer.OpenSingleDirectoryDialog(paths =>
                {
                    if (paths != null && paths.Length > 0)
                        _pathInputField.text = paths[0];
                }));

            _moveUpButton.onClick.AddListener(()   => uiGamesDirectories.MoveDirectoryUp(this));
            _moveDownButton.onClick.AddListener(() => uiGamesDirectories.MoveDirectoryDown(this));
            _deleteButton.onClick.AddListener(()   => uiGamesDirectories.RemoveDirectory(this));
        }
    }
}
