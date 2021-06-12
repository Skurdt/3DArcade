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

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Arcade.UnityEditor
{
    internal static class SceneUtilities
    {
        private const string BOOT_SCENE_NAME = "Boot";
        private const string MAIN_SCENE_NAME = "Main";
        private const string BOOT_SCENE_PATH = "Assets/_Project/Scenes/" + BOOT_SCENE_NAME + ".unity";
        private const string MAIN_SCENE_PATH = "Assets/_Project/Scenes/" + MAIN_SCENE_NAME + ".unity";

        public static void CloseAllScenes(bool keepFirst = true)
        {
            int sceneMax = keepFirst ? 1 : 0;
            while (EditorSceneManager.loadedSceneCount > sceneMax)
            {
                Scene scene = SceneManager.GetSceneAt(EditorSceneManager.loadedSceneCount - 1);
                _ = EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static void OpenBootScene()
        {
            Scene scene = SceneManager.GetSceneByName(BOOT_SCENE_NAME);
            if (!scene.isLoaded)
                _ = EditorSceneManager.OpenScene(BOOT_SCENE_PATH, OpenSceneMode.Single);
        }

        public static void OpenMainScene()
        {
            Scene scene = SceneManager.GetSceneByName(MAIN_SCENE_NAME);
            if (!scene.isLoaded)
                _ = EditorSceneManager.OpenScene(MAIN_SCENE_PATH, OpenSceneMode.Single);
        }
    }
}
