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

using SK.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arcade
{
    public sealed class ListFromMameXmlGenerator : IGameConfigurationListGenerator
    {
        private Dictionary<string, string> _gameGenreDictionary;

        public void Generate(FileExplorer fileExplorer, GameConfigurationsEvent gameConfigurationsEvent)
            => fileExplorer.OpenFileDialog("Select MAME Xml", filePaths =>
            {
                if (filePaths is null || filePaths.Length == 0)
                    return;

                string xmlPath       = filePaths[0];
                string directory     = Path.GetDirectoryName(xmlPath);
                string iniGenrePath  = $"{directory}/genre.ini";
                string iniMaturePath = $"{directory}/mature.ini";

                try
                {
                    GameConfiguration[] games = ParseMameXML(xmlPath, iniGenrePath, iniMaturePath);
                    gameConfigurationsEvent.Raise(games);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            });

        private GameConfiguration[] ParseMameXML(string listXmlPath, string genreInipath, string matureIniPath)
        {
            ParseGenres(genreInipath);

            IniFile iniMature = new IniFile();
            iniMature.Load(matureIniPath);

            return ParseGames(listXmlPath, iniMature);
        }

        private void ParseGenres(string iniPath)
        {
            IniFile ini = new IniFile();
            ini.Load(iniPath);

            if (ini.Count < 3)
                return;

            int iniIndex         = 0;
            _gameGenreDictionary = new Dictionary<string, string>();

            foreach (KeyValuePair<string, IniSection> iniSection in ini)
            {
                if (iniIndex < 2)
                {
                    ++iniIndex;
                    continue;
                }

                string genre = iniSection.Key;
                if (string.IsNullOrEmpty(genre))
                    continue;

                foreach (KeyValuePair<string, IniValue> iniValue in iniSection.Value)
                {
                    string gameName = iniValue.Key;
                    if (!string.IsNullOrEmpty(gameName))
                        _gameGenreDictionary.Add(gameName, genre);
                }
            }
        }

        private GameConfiguration[] ParseGames(string listXmlPath, IniFile iniMature)
        {
            Emulation.Mame.v2003Plus.Data mameData = XMLUtils.Deserialize<Emulation.Mame.v2003Plus.Data>(listXmlPath);
            if (mameData == null || mameData.Games == null || mameData.Games.Length == 0)
                return null;

            IniSection matureList = null;
            _ = iniMature?.TryGetSection("ROOT_FOLDER", out matureList);

            List<GameConfiguration> games = new List<GameConfiguration>();

            foreach (Emulation.Mame.v2003Plus.Game machine in mameData.Games)
            {
                GameScreenType screenType               = GameScreenType.Default;
                GameScreenOrientation screenOrientation = GameScreenOrientation.Default;

                if (machine.Video != null)
                {
                    screenType = machine.Video.Screen switch
                    {
                        "raster" => GameScreenType.Raster,
                        "vector" => GameScreenType.Vector,
                        _        => GameScreenType.Default
                    };

                    screenOrientation = machine.Video.Orientation switch
                    {
                        "horizontal" => GameScreenOrientation.Horizontal,
                        "vertical"   => GameScreenOrientation.Vertical,
                        _            => GameScreenOrientation.Default
                    };
                }

                string gameName = machine.Name;
                string genre    = _gameGenreDictionary != null && _gameGenreDictionary.TryGetValue(gameName, out string foundGenre) ? foundGenre : null;
                bool mature     = matureList != null && matureList.ContainsKey(gameName);
                bool playable   = machine.Runnable != "no" && machine.Driver != null && (machine.Driver.Status.Equals("good") || machine.Driver.Status.Equals("imperfect"));

                GameConfiguration game = new GameConfiguration
                {
                    Name              = gameName,
                    Description       = machine.Description,
                    CloneOf           = machine.CloneOf,
                    RomOf             = machine.RomOf,
                    Genre             = genre,
                    Year              = machine.Year,
                    Manufacturer      = machine.Manufacturer,
                    ScreenType        = screenType,
                    ScreenOrientation = screenOrientation,
                    Mature            = mature,
                    Playable          = playable,
                    Available         = false
                };

                games.Add(game);
            }

            return games.ToArray();
        }
    }
}
