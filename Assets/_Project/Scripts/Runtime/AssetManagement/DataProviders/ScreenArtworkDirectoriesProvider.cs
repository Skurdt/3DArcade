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

using System.Linq;

namespace Arcade
{
    public sealed class ScreenArtworkDirectoriesProvider : IArtworkDirectoriesProvider
    {
        private const string SNAPS_DIRECTORY_NAME  = "ScreensImages";
        private const string TITLES_DIRECTORY_NAME = "TitlesImages";
        private const string VIDEOS_DIRECTORY_NAME = "ScreensVideos";

        private string[] _defaultImageDirectories;
        private string[] _defaultVideoDirectories;

        public string[] DefaultImageDirectories
        {
            get
            {
                _defaultImageDirectories ??= new string[]
                {
                    $"{ArtworksController.DefaultMediaDirectory}/{SNAPS_DIRECTORY_NAME}",
                    $"{ArtworksController.DefaultMediaDirectory}/{TITLES_DIRECTORY_NAME}"
                };
                return _defaultImageDirectories;
            }
        }

        public string[] DefaultVideoDirectories
        {
            get
            {
                _defaultVideoDirectories ??= new string[]
                {
                    $"{ArtworksController.DefaultMediaDirectory}/{VIDEOS_DIRECTORY_NAME}"
                };
                return _defaultVideoDirectories;
            }
        }

        public string[] GetModelImageDirectories(EntityConfigurationBase configuration)
        {
            string[] screenSnapDirectories  = configuration.ArtworkDirectories.ScreenSnapDirectories;
            string[] screenTitleDirectories = configuration.ArtworkDirectories.ScreenTitleDirectories;

            if (screenSnapDirectories is null && screenTitleDirectories is null)
                return null;

            if (screenSnapDirectories is null && !(screenTitleDirectories is null))
                return screenTitleDirectories;

            if (screenTitleDirectories is null && !(screenSnapDirectories is null))
                return screenSnapDirectories;

            return screenSnapDirectories.Concat(screenTitleDirectories).ToArray();
        }

        public string[] GetPlatformImageDirectories(PlatformConfiguration platform)
        {
            if (platform is null)
                return null;

            string[] screenSnapsDirectories  = platform.ScreenSnapsDirectories;
            string[] screenTitlesDirectories = platform.ScreenTitlesDirectories;
            return screenSnapsDirectories.Concat(screenTitlesDirectories).ToArray();
        }

        public string[] GetModelVideoDirectories(EntityConfigurationBase configuration)
            => configuration.ArtworkDirectories.ScreenVideoDirectories;

        public string[] GetPlatformVideoDirectories(PlatformConfiguration platform)
            => platform?.ScreenVideosDirectories;
    }
}
