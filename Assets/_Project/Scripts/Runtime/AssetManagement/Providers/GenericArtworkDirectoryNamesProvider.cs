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

namespace Arcade
{
    public sealed class GenericArtworkDirectoryNamesProvider : IArtworkDirectoryNamesProvider
    {
        private const string IMAGES_DIRECTORY_NAME = "GenericsImages";
        private const string VIDEOS_DIRECTORY_NAME = "GenericsVideos";

        string[] IArtworkDirectoryNamesProvider.DefaultImageDirectories { get; } = new string[] { $"{ArtworkController.DefaultMediaDirectory}/{IMAGES_DIRECTORY_NAME}" };
        string[] IArtworkDirectoryNamesProvider.DefaultVideoDirectories { get; } = new string[] { $"{ArtworkController.DefaultMediaDirectory}/{VIDEOS_DIRECTORY_NAME}" };

        string[] IArtworkDirectoryNamesProvider.GetModelImageDirectories(ModelConfiguration modelConfiguration)
            => ArtworkUtils.GetDirectories(modelConfiguration.GenericImageDirectories);

        string[] IArtworkDirectoryNamesProvider.GetModelVideoDirectories(ModelConfiguration modelConfiguration)
            => ArtworkUtils.GetDirectories(modelConfiguration.GenericVideoDirectories);

        string[] IArtworkDirectoryNamesProvider.GetPlatformImageDirectories(PlatformConfiguration platform)
            => ArtworkUtils.GetDirectories(platform?.GenericImagesDirectories);

        string[] IArtworkDirectoryNamesProvider.GetPlatformVideoDirectories(PlatformConfiguration platform)
            => ArtworkUtils.GetDirectories(platform?.GenericVideosDirectories);
    }
}
