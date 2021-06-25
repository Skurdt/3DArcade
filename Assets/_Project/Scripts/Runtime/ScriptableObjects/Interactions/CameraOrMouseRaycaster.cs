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

using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Interaction/CameraOrMouseRaycaster", fileName = "CameraOrMouseRaycaster")]
    public sealed class CameraOrMouseRaycaster : InteractionRaycasterBase
    {
        protected override Ray GetRay(Camera camera, Vector2 offset)
        {
            if (Mouse.current is null || Cursor.lockState == CursorLockMode.Locked)
                return camera.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f) + offset);

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            return camera.ScreenPointToRay(mousePosition);
        }
    }
}
