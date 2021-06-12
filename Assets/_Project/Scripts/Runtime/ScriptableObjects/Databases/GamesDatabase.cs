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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Database/GamesDatabase", fileName = "GamesDatabase")]
    public sealed class GamesDatabase : ScriptableObject
    {
        public sealed class DBGame : SQLiteDatabase.ReflectedEntry
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string CloneOf { get; set; }
            public string RomOf { get; set; }
            public string Genre { get; set; }
            public string Year { get; set; }
            public string Manufacturer { get; set; }
            public string ScreenType { get; set; }
            public string ScreenRotation { get; set; }
            public bool Mature { get; set; }
            public bool Playable { get; set; }
            public bool IsBios { get; set; }
            public bool IsDevice { get; set; }
            public bool IsMechanical { get; set; }
            public bool Available { get; set; }

            public static readonly string[] Columns = new string[]
            {
                "Id             INTEGER",
                "Name           TEXT NOT NULL UNIQUE ON CONFLICT IGNORE",
                "Description    TEXT NOT NULL UNIQUE ON CONFLICT IGNORE",
                "CloneOf        TEXT",
                "RomOf          TEXT",
                "Genre          TEXT",
                "Year           TEXT",
                "Manufacturer   TEXT",
                "ScreenType     TEXT",
                "ScreenRotation TEXT",
                "Mature         INTEGER",
                "Playable       INTEGER",
                "IsBios         INTEGER",
                "IsDevice       INTEGER",
                "IsMechanical   INTEGER",
                "Available      INTEGER",
                "PRIMARY KEY(Id)"
            };
        }

        [SerializeField] private VirtualFileSystem _virtualFileSystem;

        private const string INTERNAL_TABLE_NAME_STATS = "_stats_";

        [System.NonSerialized] private SQLiteDatabase _database;

        public void Initialize()
        {
            if (!_virtualFileSystem.TryGetFile("game_database", out string path))
                throw new System.Exception("File with alias 'game_database' not mapped in VirtualFileSystem.");

            _database = new SQLiteDatabase(path);
            CreateInternalTables();
        }

        public List<string> GetGameLists() => _database.GetTableNames().Where(x => !x.StartsWith("_")).ToList();

        public GameConfiguration[] GetGames(string gameListName)
            => _database.SelectAllFrom<GameConfiguration>(gameListName, new string[] { "*" }, new GameConfiguration())
                       ?.OrderBy(x => x.Description)
                        .ToArray();

        public bool TryGet(string gameListName, string gameName, string[] returnFields, string[] searchFields, out GameConfiguration outGame)
        {
            if (string.IsNullOrEmpty(gameListName) || string.IsNullOrEmpty(gameName))
            {
                outGame = null;
                return false;
            }

            outGame = _database.SelectSingleFrom<GameConfiguration>(gameListName, returnFields, searchFields, new { Name = gameName });
            return !(outGame is null);
        }

        public void AddGameList(string name) => _database.CreateTable(name, false, DBGame.Columns);

        public void RemoveGameList(string name) => _database.DropTable(name, false);

        public void AddGame(string gameListName, GameConfiguration game)
        {
            if (string.IsNullOrEmpty(gameListName) || game is null)
                return;

            DBGame gameToInsert = GetDatabaseGame(game);
            _database.InsertInto(gameListName, gameToInsert);
        }

        public void AddGames(string gameListName, IReadOnlyCollection<GameConfiguration> games)
        {
            if (string.IsNullOrEmpty(gameListName) || games is null || games.Count == 0)
                return;

            IEnumerable<DBGame> gamesToInsert = games.Select(game => GetDatabaseGame(game)).ToArray();
            _database.InsertAllInto(gameListName, gamesToInsert);
        }

        public void RemoveGame(string gameListName, GameConfiguration game)
        {
            if (string.IsNullOrEmpty(gameListName) || game is null)
                return;

            _database.DeleteFrom(gameListName, "Name", game.Name);
        }

        private void CreateInternalTables() => CreateStatsTable();

        private void CreateStatsTable() => _database.CreateTable(INTERNAL_TABLE_NAME_STATS, false,
            "Id        INTEGER",
            "GameList  TEXT    NOT NULL",
            "GameName  TEXT    NOT NULL",
            "PlayCount INTEGER NOT NULL",
            "PlayTime  REAL    NOT NULL",
            "PRIMARY KEY(Id)");

        private DBGame GetDatabaseGame(GameConfiguration game)
        {
            DBGame result = new DBGame
            {
                Name           = game.Name,
                Description    = game.Description,
                CloneOf        = game.CloneOf,
                RomOf          = game.RomOf,
                Genre          = game.Genre,
                Year           = game.Year,
                Manufacturer   = game.Manufacturer,
                ScreenType     = game.ScreenType.ToString(),
                ScreenRotation = game.ScreenOrientation.ToString(),
                Mature         = game.Mature,
                Playable       = game.Playable,
                IsBios         = game.IsBios,
                IsDevice       = game.IsDevice,
                IsMechanical   = game.IsMechanical,
                Available      = game.Available
            };

            return result;
        }
    }
}
