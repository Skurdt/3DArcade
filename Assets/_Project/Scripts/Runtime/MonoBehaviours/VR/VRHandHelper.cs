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
using UnityEngine.XR;

namespace Arcade
{
    public sealed class VRHandHelper : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private InputDeviceCharacteristics _inputDeviceCharacteristics;

        //private const string ANIMATOR_LAYER_NAME_BASE  = "Base Layer";
        //private const string ANIMATOR_LAYER_NAME_THUMB = "Thumb Layer";
        //private const string ANIMATOR_LAYER_NAME_POINT = "Point Layer";

        //private const string ANIMATOR_PARAM_NAME_POSE  = "Pose";
        private const string ANIMATOR_PARAM_NAME_FLEX  = "Flex";
        private const string ANIMATOR_PARAM_NAME_PINCH = "Pinch";

        private static bool _animatorIdsSet = false;

        //private static int _animatorLayerBase;
        //private static int _animatorLayerThumb;
        //private static int _animatorLayerPoint;

        //private static int _animatorParamPose;
        private static int _animatorParamFlex;
        private static int _animatorParamPinch;

        private InputDevice _inputDevice;
        //private bool _isThumbUp;
        private bool _isPointing;

        private void Awake()
        {
            if (_animatorIdsSet)
                return;

            //_animatorLayerBase  = _animator.GetLayerIndex(ANIMATOR_LAYER_NAME_BASE);
            //_animatorLayerThumb = _animator.GetLayerIndex(ANIMATOR_LAYER_NAME_THUMB);
            //_animatorLayerPoint = _animator.GetLayerIndex(ANIMATOR_LAYER_NAME_POINT);

            //_animatorParamPose  = Animator.StringToHash(ANIMATOR_PARAM_NAME_POSE);
            _animatorParamFlex  = Animator.StringToHash(ANIMATOR_PARAM_NAME_FLEX);
            _animatorParamPinch = Animator.StringToHash(ANIMATOR_PARAM_NAME_PINCH);

            _animatorIdsSet = true;
        }

        private void OnEnable()
        {
            InputDevices.deviceConnected    += DeviceConnectedCallback;
            InputDevices.deviceDisconnected += DeviceDisconnectedCallback;
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected    -= DeviceConnectedCallback;
            InputDevices.deviceDisconnected -= DeviceDisconnectedCallback;
        }

        private void Update()
        {
            if (!_inputDevice.isValid)
                return;

            //_isThumbUp = _inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool primaryStickTouch) && !primaryStickTouch
            //          && _inputDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryButtonTouch) && !primaryButtonTouch
            //          && _inputDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryButtonTouch) && !secondaryButtonTouch;

            _isPointing = _inputDevice.TryGetFeatureValue(CommonUsages.secondary2DAxisTouch, out bool triggerTouch) && !triggerTouch;

            if (_inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
                _animator.SetFloat(_animatorParamFlex, gripValue);
            else
                _animator.SetFloat(_animatorParamFlex, 0f);

            if (!_isPointing && _inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                _animator.SetFloat(_animatorParamPinch, triggerValue);
            else
                _animator.SetFloat(_animatorParamPinch, 0f);
        }

        private void DeviceConnectedCallback(InputDevice inputDevice)
        {
            if ((inputDevice.characteristics & _inputDeviceCharacteristics) == _inputDeviceCharacteristics)
                _inputDevice = inputDevice;
        }

        private void DeviceDisconnectedCallback(InputDevice inputDevice)
        {
            if ((inputDevice.characteristics & _inputDeviceCharacteristics) == _inputDeviceCharacteristics)
                _inputDevice = default;
        }
    }
}
