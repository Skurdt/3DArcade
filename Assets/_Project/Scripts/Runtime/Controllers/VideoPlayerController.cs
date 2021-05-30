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
using UnityEngine.Video;

namespace Arcade
{
    public abstract class VideoPlayerController
    {
        protected const int NUM_CABS_WITH_VIDEOS_PLAYING = 3;

        protected readonly List<VideoPlayer> _activeVideos = new List<VideoPlayer>();

        protected readonly Player _player;

        protected VideoPlayerController(Player player) => _player = player;

        public void UpdateVideosState()
        {
            VideoPlayer[] toEnable = GetVideosToEnable();
            if (toEnable is null)
                return;

            foreach (VideoPlayer videoPlayer in toEnable)
            {
                if (!_activeVideos.Contains(videoPlayer))
                    _activeVideos.Add(videoPlayer);

                PlayVideo(videoPlayer);
            }

            VideoPlayer[] toDisable = _activeVideos.Except(toEnable).ToArray();
            foreach (VideoPlayer videoPlayer in toDisable)
            {
                StopVideo(videoPlayer);
                _ = _activeVideos.Remove(videoPlayer);
            }
        }

        public void StopAllVideos()
        {
            foreach (VideoPlayer videoPlayer in _activeVideos)
                StopVideo(videoPlayer);
            _activeVideos.Clear();
        }

        public void PlayVideo(VideoPlayer videoPlayer) => VideoSetPlayingState(videoPlayer, true);

        public void StopVideo(VideoPlayer videoPlayer) => VideoSetPlayingState(videoPlayer, false);

        protected abstract VideoPlayer[] GetVideosToEnable();

        private static void VideoSetPlayingState(VideoPlayer videoPlayer, bool state)
        {
            if (videoPlayer == null || !videoPlayer.enabled)
                return;

            videoPlayer.EnableAudioTrack(0, state && videoPlayer.GetComponent<ScreenNodeTag>() != null);

            if (state)
                videoPlayer.Play();
            else
                videoPlayer.Stop();
        }
    }
}
