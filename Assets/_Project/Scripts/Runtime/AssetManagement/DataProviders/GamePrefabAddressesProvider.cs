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

namespace Arcade
{
    public sealed class GamePrefabAddressesProvider : PrefabAddressesProvider<GameEntityConfiguration>
    {
        protected override string AddressablesPrefix { get; } = "Games/";

        private const string DEFAULT_70_HOR_PREFAB_NAME = "_70_horizontal";
        private const string DEFAULT_80_HOR_PREFAB_NAME = "_80_horizontal";
        private const string DEFAULT_90_HOR_PREFAB_NAME = "_90_horizontal";
        private const string DEFAULT_70_VER_PREFAB_NAME = "_70_vertical";
        private const string DEFAULT_80_VER_PREFAB_NAME = "_80_vertical";
        private const string DEFAULT_90_VER_PREFAB_NAME = "_90_vertical";
        private const string DEFAULT_HOR_PREFAB_NAME    = DEFAULT_80_HOR_PREFAB_NAME;
        private const string DEFAULT_VER_PREFAB_NAME    = DEFAULT_80_VER_PREFAB_NAME;

        protected override void AddValues(AssetAddresses addresses, GameEntityConfiguration cfg)
        {
            addresses.TryAdd(cfg.Model);

            addresses.TryAdd(cfg.Name);
            addresses.TryAdd(cfg.CloneOf);
            addresses.TryAdd(cfg.RomOf);

            addresses.TryAdd(cfg.GameConfiguration?.Name);
            addresses.TryAdd(cfg.GameConfiguration?.CloneOf);
            addresses.TryAdd(cfg.GameConfiguration?.RomOf);

            addresses.TryAdd(cfg.Id);

            addresses.TryAdd(cfg.PlatformConfiguration?.Model);

            addresses.TryAdd(GetGenericName(cfg.GameConfiguration));

            addresses.TryAdd(DEFAULT_HOR_PREFAB_NAME);
        }

        private static string GetGenericName(GameConfiguration game)
        {
            if (game is null)
                return null;

            switch (game.ScreenOrientation)
            {
                case GameScreenOrientation.Default:
                case GameScreenOrientation.Horizontal:
                case GameScreenOrientation.FlippedHorizontal:
                    return GetGenericHorizontalNameForYear(game.Year);
                case GameScreenOrientation.Vertical:
                case GameScreenOrientation.FlippedVertical:
                    return GetGenericVerticalNameForYear(game.Year);
                default:
                    throw new System.NotImplementedException($"Unhandled switch case for GameScreenOrientation: {game.ScreenOrientation}");
            }
        }

        private static string GetGenericHorizontalNameForYear(string yearString)
            => GetGenericNameForYear(yearString,
                                     DEFAULT_70_HOR_PREFAB_NAME,
                                     DEFAULT_80_HOR_PREFAB_NAME,
                                     DEFAULT_90_HOR_PREFAB_NAME,
                                     DEFAULT_HOR_PREFAB_NAME);

        private static string GetGenericVerticalNameForYear(string yearString)
            => GetGenericNameForYear(yearString,
                                     DEFAULT_70_VER_PREFAB_NAME,
                                     DEFAULT_80_VER_PREFAB_NAME,
                                     DEFAULT_90_VER_PREFAB_NAME,
                                     DEFAULT_VER_PREFAB_NAME);

        private static string GetGenericNameForYear(string yearString, string model70, string model80, string model90, string modelDefault)
        {
            if (!string.IsNullOrEmpty(yearString) && int.TryParse(yearString, out int year) && year > 0)
            {
                if (year >= 1970 && year < 1980)
                    return model70;

                if (year < 1990)
                    return model80;

                if (year < 2000)
                    return model90;
            }

            return modelDefault;
        }
    }
}
