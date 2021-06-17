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

        [SerializeField] private InputAction _button1;
        [SerializeField] private InputAction _button2;
        [SerializeField] private InputAction _joystick;
        [SerializeField] private InputAction _trigger;
        [SerializeField] private InputAction _grip;

        private const string ANIMATOR_BUTTON_1_STRING   = "Button 1";
        private const string ANIMATOR_BUTTON_2_STRING   = "Button 2";
        private const string ANIMATOR_JOYSTICK_X_STRING = "Joy X";
        private const string ANIMATOR_JOYSTICK_Y_STRING = "Joy Y";
        private const string ANIMATOR_TRIGGER_STRING    = "Trigger";
        private const string ANIMATOR_GRIP_STRING       = "Grip";

        private static int _animatorButton1Id = int.MinValue;
        private static int _animatorButton2Id;
        private static int _animatorJoystickXId;
        private static int _animatorJoystickYId;
        private static int _animatorTriggerId;
        private static int _animatorGripId;

        private InputDevice _device = null;
        private Animator _animator  = null;

        private void Awake()
        {
            if (_animatorButton1Id == int.MinValue)
            {
                _animatorButton1Id   = Animator.StringToHash(ANIMATOR_BUTTON_1_STRING);
                _animatorButton2Id   = Animator.StringToHash(ANIMATOR_BUTTON_2_STRING);
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
            _button1.Enable();
            _button2.Enable();
            _joystick.Enable();
            _trigger.Enable();
            _grip.Enable();
        }

        private void OnDisable()
        {
            _button1.Disable();
            _button2.Disable();
            _joystick.Disable();
            _trigger.Disable();
            _grip.Disable();
        }

        private void Update()
        {
            TryInitDevice();
            AnimateControllers();
        }

        private void TryInitDevice()
        {
            if (!(_device is null) || _button1.controls.Count == 0)
                return;

            _modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
            _modelOculusTouchQuestAndRiftSRightController.SetActive(false);
            _modelOculusTouchRiftLeftController.SetActive(false);
            _modelOculusTouchRiftRightController.SetActive(false);
            _modelOculusTouchQuest2LeftController.SetActive(false);
            _modelOculusTouchQuest2RightController.SetActive(false);

            _device = _button1.controls[0].device;

            ControllerType type = _device.name.StartsWith("OculusTouchControllerOpenXR") ? ControllerType.Quest2 : ControllerType.Rift;
            if (_device.name.Equals("OculusTouchControllerOpenXR"))
            {
                switch (type)
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

            if (_device.name.Equals("OculusTouchControllerOpenXR1"))
            {
                switch (type)
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

        private void AnimateControllers()
        {
            if (_device is null || _animator == null)
                return;

            _animator.SetFloat(_animatorButton1Id, _button1.ReadValue<float>());
            _animator.SetFloat(_animatorButton2Id, _button2.ReadValue<float>());

            Vector2 axis = _joystick.ReadValue<Vector2>();
            _animator.SetFloat(_animatorJoystickXId, axis.x);
            _animator.SetFloat(_animatorJoystickYId, axis.y);

            _animator.SetFloat(_animatorTriggerId, _trigger.ReadValue<float>());
            _animator.SetFloat(_animatorGripId, _grip.ReadValue<float>());
        }
    }
}
