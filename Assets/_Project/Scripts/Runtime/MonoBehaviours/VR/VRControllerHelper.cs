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
using UnityEngine.XR;

namespace Arcade
{
    public sealed class VRControllerHelper : MonoBehaviour
    {
        private enum ControllerType
        {
            QuestAndRiftS = 1,
            Rift          = 2,
            Quest2        = 3,
        }

        [SerializeField] private GameObject _modelOculusTouchQuestAndRiftSLeftController;
        [SerializeField] private GameObject _modelOculusTouchQuestAndRiftSRightController;
        [SerializeField] private GameObject _modelOculusTouchRiftLeftController;
        [SerializeField] private GameObject _modelOculusTouchRiftRightController;
        [SerializeField] private GameObject _modelOculusTouchQuest2LeftController;
        [SerializeField] private GameObject _modelOculusTouchQuest2RightController;
        [SerializeField] private InputDeviceCharacteristics _controllerCharacteristics;

        private const InputDeviceCharacteristics HMD_CHARACTERISTICS = InputDeviceCharacteristics.HeadMounted;
        private const string ANIMATOR_BUTTON_1_STRING                = "Button 1";
        private const string ANIMATOR_BUTTON_2_STRING                = "Button 2";
        private const string ANIMATOR_BUTTON_3_STRING                = "Button 3";
        private const string ANIMATOR_JOYSTICK_X_STRING              = "Joy X";
        private const string ANIMATOR_JOYSTICK_Y_STRING              = "Joy Y";
        private const string ANIMATOR_TRIGGER_STRING                 = "Trigger";
        private const string ANIMATOR_GRIP_STRING                    = "Grip";

        private static int _animatorButton1Id = int.MinValue;
        private static int _animatorButton2Id;
        private static int _animatorButton3Id;
        private static int _animatorJoystickXId;
        private static int _animatorJoystickYId;
        private static int _animatorTriggerId;
        private static int _animatorGripId;

        private ControllerType _activeControllerType = ControllerType.Rift;
        private InputDevice? _controllerDevice       = null;
        private Animator _animator                   = null;

        private void Awake()
        {
            if (_animatorButton1Id == int.MinValue)
            {
                _animatorButton1Id   = Animator.StringToHash(ANIMATOR_BUTTON_1_STRING);
                _animatorButton2Id   = Animator.StringToHash(ANIMATOR_BUTTON_2_STRING);
                _animatorButton3Id   = Animator.StringToHash(ANIMATOR_BUTTON_3_STRING);
                _animatorJoystickXId = Animator.StringToHash(ANIMATOR_JOYSTICK_X_STRING);
                _animatorJoystickYId = Animator.StringToHash(ANIMATOR_JOYSTICK_Y_STRING);
                _animatorTriggerId   = Animator.StringToHash(ANIMATOR_TRIGGER_STRING);
                _animatorGripId      = Animator.StringToHash(ANIMATOR_GRIP_STRING);
            }

            _modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
            _modelOculusTouchQuestAndRiftSRightController.SetActive(false);
            _modelOculusTouchRiftLeftController.SetActive(false);
            _modelOculusTouchRiftRightController.SetActive(false);
            _modelOculusTouchQuest2LeftController.SetActive(false);
            _modelOculusTouchQuest2RightController.SetActive(false);
        }

        private void OnEnable()
        {
            InputDevices.deviceConnected    += InitDevice;
            InputDevices.deviceDisconnected += DeinitDevice;

            List<InputDevice> inputDevices = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(HMD_CHARACTERISTICS, inputDevices);
            if (inputDevices.Count > 0)
                InitDevice(inputDevices[0]);

            InputDevices.GetDevicesWithCharacteristics(_controllerCharacteristics, inputDevices);
            if (inputDevices.Count > 0)
                InitDevice(inputDevices[0]);
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected    -= InitDevice;
            InputDevices.deviceDisconnected -= DeinitDevice;
        }

        private void Update()
        {
            if (!_controllerDevice.HasValue || _animator == null)
                return;

            _animator.SetFloat(_animatorButton1Id, _controllerDevice.Value.TryGetFeatureValue(CommonUsages.primaryButton, out bool boolValue) && boolValue ? 1.0f : 0f);
            _animator.SetFloat(_animatorButton2Id, _controllerDevice.Value.TryGetFeatureValue(CommonUsages.secondaryButton, out boolValue) && boolValue ? 1.0f : 0f);
            _animator.SetFloat(_animatorButton3Id, _controllerDevice.Value.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out boolValue) && boolValue ? 1.0f : 0f);

            _animator.SetFloat(_animatorTriggerId, _controllerDevice.Value.TryGetFeatureValue(CommonUsages.trigger, out float floatValue) ? floatValue : 0f);
            _animator.SetFloat(_animatorGripId, _controllerDevice.Value.TryGetFeatureValue(CommonUsages.grip, out floatValue) ? floatValue : 0f);

            Vector2 axis = _controllerDevice.Value.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axisValue) ? axisValue : Vector2.zero;
            _animator.SetFloat(_animatorJoystickXId, axis.x);
            _animator.SetFloat(_animatorJoystickYId, axis.y);
        }

        private void InitDevice(InputDevice inputDevice)
        {
            _modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
            _modelOculusTouchQuestAndRiftSRightController.SetActive(false);
            _modelOculusTouchRiftLeftController.SetActive(false);
            _modelOculusTouchRiftRightController.SetActive(false);
            _modelOculusTouchQuest2LeftController.SetActive(false);
            _modelOculusTouchQuest2RightController.SetActive(false);

            if ((inputDevice.characteristics & HMD_CHARACTERISTICS) == HMD_CHARACTERISTICS)
            {
                _activeControllerType = inputDevice.name switch
                {
                    "Oculus Quest2" => ControllerType.Quest2,
                    _               => ControllerType.Rift
                };

                Debug.Log($"Active HMD: {_activeControllerType}");
                return;
            }

            if ((inputDevice.characteristics & InputDeviceCharacteristics.Controller) != InputDeviceCharacteristics.Controller)
                return;

            _controllerDevice = inputDevice;

            if ((inputDevice.characteristics & InputDeviceCharacteristics.Left) == InputDeviceCharacteristics.Left)
            {
                switch (_activeControllerType)
                {
                    case ControllerType.QuestAndRiftS:
                        _modelOculusTouchQuestAndRiftSLeftController.SetActive(true);
                        _animator = _modelOculusTouchQuestAndRiftSLeftController.GetComponent<Animator>();
                        break;
                    case ControllerType.Rift:
                        _modelOculusTouchRiftLeftController.SetActive(true);
                        _animator = _modelOculusTouchRiftLeftController.GetComponent<Animator>();
                        break;
                    case ControllerType.Quest2:
                        _modelOculusTouchQuest2LeftController.SetActive(true);
                        _animator = _modelOculusTouchQuest2LeftController.GetComponent<Animator>();
                        break;
                    default:
                        break;
                }
                return;
            }

            if ((inputDevice.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.Right)
            {
                switch (_activeControllerType)
                {
                    case ControllerType.QuestAndRiftS:
                        _modelOculusTouchQuestAndRiftSRightController.SetActive(true);
                        _animator = _modelOculusTouchQuestAndRiftSRightController.GetComponent<Animator>();
                        break;
                    case ControllerType.Rift:
                        _modelOculusTouchRiftRightController.SetActive(true);
                        _animator = _modelOculusTouchRiftRightController.GetComponent<Animator>();
                        break;
                    case ControllerType.Quest2:
                        _modelOculusTouchQuest2RightController.SetActive(true);
                        _animator = _modelOculusTouchQuest2RightController.GetComponent<Animator>();
                        break;
                    default:
                        break;
                }
                return;
            }
        }

        private void DeinitDevice(InputDevice inputDevice)
        {
            if ((inputDevice.characteristics & HMD_CHARACTERISTICS) == HMD_CHARACTERISTICS)
            {
                _modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
                _modelOculusTouchQuestAndRiftSRightController.SetActive(false);
                _modelOculusTouchRiftLeftController.SetActive(false);
                _modelOculusTouchRiftRightController.SetActive(false);
                _modelOculusTouchQuest2LeftController.SetActive(false);
                _modelOculusTouchQuest2RightController.SetActive(false);

                _controllerDevice = null;
                _animator         = null;
                return;
            }

            if ((inputDevice.characteristics & (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller)) == (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller))
            {
                _modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
                _modelOculusTouchRiftLeftController.SetActive(false);
                _modelOculusTouchQuest2LeftController.SetActive(false);

                _controllerDevice = null;
                _animator         = null;
                return;
            }

            if ((inputDevice.characteristics & (InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller)) == (InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller))
            {
                _modelOculusTouchQuestAndRiftSRightController.SetActive(false);
                _modelOculusTouchRiftRightController.SetActive(false);
                _modelOculusTouchQuest2RightController.SetActive(false);

                _controllerDevice = null;
                _animator         = null;
                return;
            }
        }
    }
}
