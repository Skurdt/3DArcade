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
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent]
    public sealed class UI : MonoBehaviour
    {
        [SerializeField] private UICanvasController _standardUI;
        [SerializeField] private UICanvasController _virtualRealityUI;

        private UIContext _uiContext;
        private GeneralConfigurationVariable _generalConfigurationVariable;

        [Inject]
        public void Construct(UIContext uiContext, GeneralConfigurationVariable generalConfigurationVariable)
        {
            _uiContext                    = uiContext;
            _generalConfigurationVariable = generalConfigurationVariable;

            _uiContext.Initialize(_standardUI, _virtualRealityUI);
        }

        public void ArcadeStateChangeCallback(ArcadeState arcadeState)
        {
            _uiContext.TransitionTo<UIDisabledState>();

            bool enableVR = _generalConfigurationVariable.Value.EnableVR;

            if (_uiContext.ValidStandardUI)
                _standardUI.gameObject.SetActive(!enableVR);

            if (_uiContext.ValidVirtualRealityUI)
                _virtualRealityUI.gameObject.SetActive(enableVR);

            if (arcadeState is ArcadeStandardLoadingState || arcadeState is ArcadeVirtualRealityLoadingState)
            {
                if (enableVR)
                    _uiContext.TransitionTo<UIVirtualRealityLoadingState>();
                else
                    _uiContext.TransitionTo<UIStandardLoadingState>();
            }
            else if (arcadeState is ArcadeStandardFpsNormalState || arcadeState is ArcadeVirtualRealityFpsNormalState)
            {
                if (enableVR)
                    _uiContext.TransitionTo<UIVirtualRealityNormalState>();
                else
                    _uiContext.TransitionTo<UIStandardNormalState>();
            }
            else if (arcadeState is ArcadeStandardFpsEditPositionsState)
            {
                if (enableVR)
                    _uiContext.TransitionTo<UIVirtualRealityEditPositionsState>();
                else
                    _uiContext.TransitionTo<UIStandardEditPositionsState>();
            }
            else if (arcadeState is ArcadeStandardFpsEditContentState)
            {
                if (enableVR)
                    _uiContext.TransitionTo<UIVirtualRealityEditContentState>();
                else
                    _uiContext.TransitionTo<UIStandardEditContentState>();
            }
        }

        public static void HandleCellHighlighting<T>(ref T currentCell)
            where T : Component, IUICellHighlighting
        {
            if (Mouse.current is null)
                return;

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (raycastResults.Count == 0)
                return;

            T raycastCell = raycastResults[0].gameObject.GetComponentInParent<T>();
            if (currentCell == raycastCell)
                return;

            if (currentCell != null)
                currentCell.StopHighlight();

            currentCell = raycastCell;

            if (currentCell != null)
                currentCell.StartHighlight();
        }
    }
}
