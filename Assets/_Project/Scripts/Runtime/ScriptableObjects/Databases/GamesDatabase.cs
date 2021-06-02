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

using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Database/GamesDatabase", fileName = "GamesDatabase")]
    public sealed class GamesDatabase : ScriptableObject
    {
        [Table(INTERNAL_TABLE_NAME_GENRES)]
        public sealed class DBGenre : SQLiteDatabase.MappedEntry
        {
            public string Genre { get; set; }
        }

        [Table(INTERNAL_TABLE_NAME_YEARS)]
        public sealed class DBYear : SQLiteDatabase.MappedEntry
        {
            public string Year { get; set; }
        }

        [Table(INTERNAL_TABLE_NAME_MANUFACTURERS)]
        public sealed class DBManufacturer : SQLiteDatabase.MappedEntry
        {
            public string Manufacturer { get; set; }
        }

        [Table(INTERNAL_TABLE_NAME_SCREEN_TYPES)]
        public sealed class DBScreenType : SQLiteDatabase.MappedEntry
        {
            public string ScreenType { get; set; }
        }

        [Table(INTERNAL_TABLE_NAME_SCREEN_ROTATIONS)]
        public sealed class DBScreenRotation : SQLiteDatabase.MappedEntry
        {
            public int ScreenRotation { get; set; }
        }

        public sealed class DBGame : SQLiteDatabase.ReflectedEntry
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string CloneOf { get; set; }
            public string RomOf { get; set; }
            public int Genre { get; set; }
            public int Year { get; set; }
            public int Manufacturer { get; set; }
            public int ScreenType { get; set; }
            public int ScreenRotation { get; set; }
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
                "Genre          INTEGER",
                "Year           INTEGER",
                "Manufacturer   INTEGER",
                "ScreenType     INTEGER",
                "ScreenRotation INTEGER",
                "Mature         INTEGER",
                "Playable       INTEGER",
                "IsBios         INTEGER",
                "IsDevice       INTEGER",
                "IsMechanical   INTEGER",
                "Available      INTEGER",
                "PRIMARY KEY(Id)",
               $"FOREIGN KEY(Genre)          REFERENCES {INTERNAL_TABLE_NAME_GENRES}(Id)           ON UPDATE CASCADE ON DELETE RESTRICT",
               $"FOREIGN KEY(Year)           REFERENCES {INTERNAL_TABLE_NAME_YEARS}(Id)            ON UPDATE CASCADE ON DELETE RESTRICT",
               $"FOREIGN KEY(Manufacturer)   REFERENCES {INTERNAL_TABLE_NAME_MANUFACTURERS}(Id)    ON UPDATE CASCADE ON DELETE RESTRICT",
               $"FOREIGN KEY(ScreenType)     REFERENCES {INTERNAL_TABLE_NAME_SCREEN_TYPES}(Id)     ON UPDATE CASCADE ON DELETE RESTRICT",
               $"FOREIGN KEY(ScreenRotation) REFERENCES {INTERNAL_TABLE_NAME_SCREEN_ROTATIONS}(Id) ON UPDATE CASCADE ON DELETE RESTRICT"
            };
        }

        private const string INTERNAL_TABLE_NAME_GENRES           = "_genres_";
        private const string INTERNAL_TABLE_NAME_YEARS            = "_years_";
        private const string INTERNAL_TABLE_NAME_MANUFACTURERS    = "_manufacturers_";
        private const string INTERNAL_TABLE_NAME_SCREEN_TYPES     = "_screen_types_";
        private const string INTERNAL_TABLE_NAME_SCREEN_ROTATIONS = "_screen_rotations_";
        private const string INTERNAL_TABLE_NAME_STATS            = "_stats_";

        [SerializeField] private VirtualFileSystem _virtualFileSystem;
        [System.NonSerialized] private SQLiteDatabase _database;

        public void Initialize()
        {
            if (!_virtualFileSystem.TryGetFile("game_database", out string path))
                throw new System.Exception("File with alias 'game_database' not mapped in VirtualFileSystem.");

            _database = new SQLiteDatabase(path);
            CreateInternalTables();
        }

        public List<string> GetGameLists() => _database.GetTables().Where(x => !x.StartsWith("_")).ToList();

        public GameConfiguration[] GetGames(string gameListName)
            => _database.GetTableEntries<GameConfiguration>(gameListName, new string[] { "*" }, new GameConfiguration())
                       ?.OrderBy(x => x.Description)
                        .ToArray();

        public bool TryGet(string gameListName, string gameName, string[] returnFields, string[] searchFields, out GameConfiguration outGame)
        {
            if (string.IsNullOrEmpty(gameListName) || string.IsNullOrEmpty(gameName))
            {
                outGame = null;
                return false;
            }

            outGame = _database.Get<GameConfiguration>(gameListName, returnFields, searchFields, new { Name = gameName });
            return !(outGame is null);
        }

        public void AddGenre(string value)
        {
            if (!string.IsNullOrEmpty(value))
                _database.Insert(new DBGenre { Genre = value });
        }

        public void AddYear(string value)
        {
            if (!string.IsNullOrEmpty(value))
                _database.Insert(new DBYear { Year = value });
        }

        public void AddManufacturer(string value)
        {
            if (!string.IsNullOrEmpty(value))
                _database.Insert(new DBManufacturer { Manufacturer = value });
        }

        public void AddScreenType(string value)
        {
            if (!string.IsNullOrEmpty(value))
                _database.Insert(new DBScreenType { ScreenType = value });
        }

        public void AddScreenRotation(int value) => _database.Insert(new DBScreenRotation { ScreenRotation = value });

        public void AddGenres(IReadOnlyCollection<string> values)
        {
            if (values is null || values.Count == 0)
                return;

            _database.Insert(values.Select(x => new DBGenre { Genre = x })
                                   .OrderBy(x => x.Genre)
                                   .ToArray());
        }

        public void AddYears(IReadOnlyCollection<string> values)
        {
            if (values is null || values.Count == 0)
                return;

            _database.Insert(values.Select(x => new DBYear { Year = x })
                                   .OrderBy(x => x.Year)
                                   .ToArray());
        }

        public void AddManufacturers(IReadOnlyCollection<string> values)
        {
            if (values is null || values.Count == 0)
                return;

            _database.Insert(values.Select(x => new DBManufacturer { Manufacturer = x })
                                   .OrderBy(x => x.Manufacturer)
                                   .ToArray());
        }

        public void AddScreenTypes(IReadOnlyCollection<string> values)
        {
            if (values is null || values.Count == 0)
                return;

            _database.Insert(values.Select(x => new DBScreenType { ScreenType = x })
                                   .OrderBy(x => x.ScreenType)
                                   .ToArray());
        }

        public void AddScreenRotations(IReadOnlyCollection<int> values)
        {
            if (values is null || values.Count == 0)
                return;

            _database.Insert(values.Select(x => new DBScreenRotation { ScreenRotation = x })
                                   .OrderBy(x => x.ScreenRotation)
                                   .ToArray());
        }

        public void AddGameList(string name) => _database.CreateTable(name, false, DBGame.Columns);

        public void RemoveGameList(string name) => _database.DropTable(name, false);

        public void AddGame(string gameListName, GameConfiguration game)
        {
            if (string.IsNullOrEmpty(gameListName) || game is null)
                return;

            DBGame gameToInsert = GetDatabaseGame(game);
            _database.Insert(gameListName, gameToInsert);
        }

        public void AddGames(string gameListName, IReadOnlyCollection<GameConfiguration> games)
        {
            if (string.IsNullOrEmpty(gameListName) || games is null || games.Count == 0)
                return;

            IEnumerable<DBGame> gamesToInsert = games.Select(game => GetDatabaseGame(game)).ToArray();
            _database.Insert(gameListName, gamesToInsert);
        }

        private void CreateInternalTables()
        {
            CreateGenresTable();
            CreateYearsTable();
            CreateManufacturersTable();
            CreateScreenTypesTable();
            CreateScreenRotationsTable();
            CreateStatsTable();
        }

        private void CreateGenresTable()
        {
            _database.CreateTable(INTERNAL_TABLE_NAME_GENRES, false,
                "Id    INTEGER",
                "Genre TEXT NOT NULL UNIQUE ON CONFLICT IGNORE",
                "PRIMARY KEY(Id)");

            _database.Insert(new DBGenre { Genre = "Not Set" });
        }

        private void CreateYearsTable()
        {
            _database.CreateTable(INTERNAL_TABLE_NAME_YEARS, false,
                "Id   INTEGER",
                "Year TEXT NOT NULL UNIQUE ON CONFLICT IGNORE",
                "PRIMARY KEY(Id)");

            _database.Insert(new DBYear { Year = "Not Set" });
        }

        private void CreateManufacturersTable()
        {
            _database.CreateTable(INTERNAL_TABLE_NAME_MANUFACTURERS, false,
                "Id           INTEGER",
                "Manufacturer TEXT NOT NULL UNIQUE ON CONFLICT IGNORE",
                "PRIMARY KEY(Id)");

            _database.Insert(new DBManufacturer { Manufacturer = "Not Set" });
        }

        private void CreateScreenTypesTable()
        {
            _database.CreateTable(INTERNAL_TABLE_NAME_SCREEN_TYPES, false,
                "Id   INTEGER",
                "ScreenType TEXT NOT NULL UNIQUE ON CONFLICT IGNORE",
                "PRIMARY KEY(Id)");

            _database.Insert(new DBScreenType { ScreenType = "default" });
        }

        private void CreateScreenRotationsTable()
        {
            _database.CreateTable(INTERNAL_TABLE_NAME_SCREEN_ROTATIONS, false,
                "Id       INTEGER",
                "ScreenRotation INTEGER NOT NULL UNIQUE ON CONFLICT IGNORE",
                "PRIMARY KEY(Id)");

            _database.Insert(new DBScreenRotation { ScreenRotation = 0 });
        }

        private void CreateStatsTable() => _database.CreateTable(INTERNAL_TABLE_NAME_STATS, false,
            "Id        INTEGER",
            "GameList  TEXT    NOT NULL",
            "GameName  TEXT    NOT NULL",
            "PlayCount INTEGER NOT NULL",
            "PlayTime  REAL    NOT NULL",
            "PRIMARY KEY(Id)");

        private DBGame GetDatabaseGame(GameConfiguration game)
        {
            int genreId          = !string.IsNullOrEmpty(game.Genre) ? GetGenreId(game.Genre) : 1;
            int yearId           = !string.IsNullOrEmpty(game.Year) ? GetYearId(game.Year) : 1;
            int manufacturerId   = !string.IsNullOrEmpty(game.Manufacturer) ? GetManufacturerId(game.Manufacturer) : 1;
            int screenTypeId     = GetScreenTypeId(game.ScreenType);
            int screenRotationId = GetScreenRotationId(game.ScreenOrientation);

            DBGame result = new DBGame
            {
                Name           = game.Name,
                Description    = game.Description,
                CloneOf        = game.CloneOf,
                RomOf          = game.RomOf,
                Genre          = genreId,
                Year           = yearId,
                Manufacturer   = manufacturerId,
                ScreenType     = screenTypeId,
                ScreenRotation = screenRotationId,
                Mature         = game.Mature,
                Playable       = game.Playable,
                IsBios         = game.IsBios,
                IsDevice       = game.IsDevice,
                IsMechanical   = game.IsMechanical,
                Available      = game.Available
            };

            return result;
        }

        private int GetGenreId(string genre)
            => _database.GetId(INTERNAL_TABLE_NAME_GENRES, "Genre", new DBGenre { Genre = genre });

        private int GetYearId(string year)
            => _database.GetId(INTERNAL_TABLE_NAME_YEARS, "Year", new DBYear { Year = year });

        private int GetManufacturerId(string manufacturer)
            => _database.GetId(INTERNAL_TABLE_NAME_MANUFACTURERS, "Manufacturer", new DBManufacturer { Manufacturer = manufacturer });

        private int GetScreenTypeId(GameScreenType type)
        {
            string typeString = type switch
            {
                GameScreenType.Default => "default",
                GameScreenType.Lcd     => "lcd",
                GameScreenType.Raster  => "raster",
                GameScreenType.Svg     => "svg",
                GameScreenType.Vector  => "vector",
                _ => throw new System.NotImplementedException($"Unhandled switch case for GameScreenType: {type}")
            };

            return _database.GetId(INTERNAL_TABLE_NAME_SCREEN_TYPES, "ScreenType", new DBScreenType { ScreenType = typeString });
        }

        private int GetScreenRotationId(GameScreenOrientation orientation)
        {
            int rotation = orientation switch
            {
                GameScreenOrientation.Default           => 0,
                GameScreenOrientation.Horizontal        => 0,
                GameScreenOrientation.Vertical          => 90,
                GameScreenOrientation.FlippedHorizontal => 180,
                GameScreenOrientation.FlippedVertical   => 270,
                _ => throw new System.NotImplementedException($"Unhandled switch case for GameScreenOrientation: {orientation}")
            };

            return _database.GetId(INTERNAL_TABLE_NAME_SCREEN_ROTATIONS, "ScreenRotation", new DBScreenRotation { ScreenRotation = rotation });
        }
    }
}
