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

using System.Collections.Generic;
using UnityEngine;

namespace Arcade
{
    public sealed class GamePrefabAddressesProvider : IGamePrefabAddressesProvider
    {
        private const string GAMES_ADDRESSABLES_PREFIX       = "Games/";
        private const string DEFAULT_GAME_70_HOR_PREFAB_NAME = "_70_horizontal";
        private const string DEFAULT_GAME_80_HOR_PREFAB_NAME = "_80_horizontal";
        private const string DEFAULT_GAME_90_HOR_PREFAB_NAME = "_90_horizontal";
        private const string DEFAULT_GAME_70_VER_PREFAB_NAME = "_70_vertical";
        private const string DEFAULT_GAME_80_VER_PREFAB_NAME = "_80_vertical";
        private const string DEFAULT_GAME_90_VER_PREFAB_NAME = "_90_vertical";
        private const string DEFAULT_GAME_HOR_PREFAB_NAME    = DEFAULT_GAME_80_HOR_PREFAB_NAME;
        private const string DEFAULT_GAME_VER_PREFAB_NAME    = DEFAULT_GAME_80_VER_PREFAB_NAME;

        public IEnumerable<string> GetNamesToTry(ModelConfiguration cfg, PlatformConfiguration platform, GameConfiguration game)
        {
            if (cfg == null || string.IsNullOrEmpty(cfg.Id))
                return null;

            List<string> result = new List<string>();

            AddToList(cfg.Model);
            AddToList(cfg.Id);
            AddToList(game?.CloneOf);
            AddToList(game?.RomOf);
            AddToList(platform?.Model);

            // Generic model from orientation/year
            if (!string.IsNullOrEmpty(cfg.Year))
            {
                switch (cfg.ScreenOrientation)
                {
                    case GameScreenOrientation.Default:
                        if (!string.IsNullOrEmpty(GetModelNameForGame(game)))
                            return result;
                        break;
                    case GameScreenOrientation.Horizontal:
                        result.Add(GetHorizontalModelNameForYear(cfg.Year));
                        return result;
                    case GameScreenOrientation.Vertical:
                        result.Add(GetVerticalModelNameForYear(cfg.Year));
                        return result;
                    default:
                        throw new System.Exception($"Unhandled switch case statement: {cfg.ScreenOrientation}");
                }
            }

            AddToList(GetModelNameForGame(game));
            AddToList(DEFAULT_GAME_HOR_PREFAB_NAME);

            return result;

            void AddToList(string name) => AssetAddressUtilities.AddToList(result, name, AssetAddressUtilities.PREFAB_FILE_EXTENSION, GAMES_ADDRESSABLES_PREFIX);
        }

        private static string GetModelNameForGame(GameConfiguration game)
        {
            if (game == null)
                return null;

            return game.ScreenOrientation switch
            {
                GameScreenOrientation.Horizontal => GetHorizontalModelNameForYear(game.Year),
                GameScreenOrientation.Vertical   => GetVerticalModelNameForYear(game.Year),
                _ => throw new System.NotImplementedException($"Unhandled switch case for GameScreenOrientation: {game.ScreenOrientation}")
            };
        }

        private static string GetHorizontalModelNameForYear(string yearString) => GetModelNameForYear(yearString,
                                                                                                      DEFAULT_GAME_70_HOR_PREFAB_NAME,
                                                                                                      DEFAULT_GAME_80_HOR_PREFAB_NAME,
                                                                                                      DEFAULT_GAME_90_HOR_PREFAB_NAME,
                                                                                                      DEFAULT_GAME_HOR_PREFAB_NAME);

        private static string GetVerticalModelNameForYear(string yearString) => GetModelNameForYear(yearString,
                                                                                                    DEFAULT_GAME_70_VER_PREFAB_NAME,
                                                                                                    DEFAULT_GAME_80_VER_PREFAB_NAME,
                                                                                                    DEFAULT_GAME_90_VER_PREFAB_NAME,
                                                                                                    DEFAULT_GAME_VER_PREFAB_NAME);

        private static string GetModelNameForYear(string yearString, string model70, string model80, string model90, string modelDefault)
        {
            if (string.IsNullOrEmpty(yearString) && int.TryParse(yearString, out int year) && year > 0)
            {
                if (year >= 1970 && year < 1980)
                    return GetGameAssetPath(model70);

                if (year < 1990)
                    return GetGameAssetPath(model80);

                if (year < 2000)
                    return GetGameAssetPath(model90);
            }

            return GetGameAssetPath(modelDefault);
        }

        private static string GetGameAssetPath(string name)
        {
            if (!Application.isPlaying)
                return $"{AssetAddressUtilities.EDITOR_ADDRESSABLES_PATH}{GAMES_ADDRESSABLES_PREFIX}{name}.{AssetAddressUtilities.PREFAB_FILE_EXTENSION}";

            return $"{GAMES_ADDRESSABLES_PREFIX}/{name}";
        }
    }
}
