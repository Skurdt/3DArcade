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
    public sealed class ArtworkFileNamesProvider
    {
        private readonly MultiFileDatabase<PlatformConfiguration> _platformDatabase;
        private readonly MultiFileDatabase<EmulatorConfiguration> _emulatorDatabase;
        private readonly GamesDatabase _gameDatabase;

        public ArtworkFileNamesProvider(Databases databases)
        {
            _platformDatabase = databases.Platforms;
            _emulatorDatabase = databases.Emulators;
            _gameDatabase     = databases.Games;
        }

        public string[] GetNamesToTry(GameEntityConfiguration configuration)
        {
            if (configuration is null)
                return null;

            _ = _platformDatabase.TryGet(configuration.Platform, out PlatformConfiguration platform);
            _ = _emulatorDatabase.TryGet(configuration.Emulator, out EmulatorConfiguration overrideEmulator);
            _ = _emulatorDatabase.TryGet(platform?.Emulator, out EmulatorConfiguration platformEmulator);

            string[] gameReturnFields = new string[] { "CloneOf", "RomOf" };
            string[] gameSearchFields = new string[] { "Name" };
            if (!_gameDatabase.TryGet(platform?.MasterList, configuration.Name, gameReturnFields, gameSearchFields, out GameConfiguration game))
                _ = _gameDatabase.TryGet(platform?.MasterList, configuration.Id, gameReturnFields, gameSearchFields, out game);

            ImageSequence imageSequence = new ImageSequence();

            // TODO: From files overrides
            // TODO: From directories overrides

            imageSequence.Add(configuration.Name);
            imageSequence.Add(configuration.CloneOf);
            imageSequence.Add(configuration.RomOf);

            imageSequence.Add(game?.Name);
            imageSequence.Add(game?.CloneOf);
            imageSequence.Add(game?.RomOf);

            imageSequence.Add(configuration.Id);

            imageSequence.Add(overrideEmulator?.Id);

            imageSequence.Add(platformEmulator?.Id);

            return imageSequence.Images;
        }
    }
}
