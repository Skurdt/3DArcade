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

using Cinemachine;
using UnityEngine;
using Zenject;

namespace Arcade
{
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterController))]
    public abstract class PlayerController : MonoBehaviour
    {
        [SerializeField] protected CinemachineNewVirtualCamera _virtualCamera;
        [SerializeField] protected float _walkSpeed            = 3f;
        [SerializeField] protected float _turnSensitivity      = 8f;
        [SerializeField] protected float _minVerticalLookAngle = -89f;
        [SerializeField] protected float _maxVerticalLookAngle = 89f;

        public abstract bool MovementEnabled { get; set; }
        public abstract bool LookEnabled { get; set; }

        protected InputActions _inputActions;
        protected CharacterController _characterController;
        protected CinemachineTransposer _cinemachineTransposer;
        protected Vector3 _moveVelocity;
        protected float _lookVertical;

        [Inject]
        public void Construct(InputActions inputActions)
        {
            _inputActions          = inputActions;
            _characterController   = GetComponent<CharacterController>();
            _cinemachineTransposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        }

        private void OnEnable()
        {
            _moveVelocity = Vector3.zero;
            _lookVertical = 0f;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            HandleHeight(deltaTime);
            HandleMovement(deltaTime);
            HandleLook();
        }

        public void SetSpeed(float speed) => _walkSpeed = Mathf.Clamp(speed, 0.01f, 20f);

        public void SetVerticalLookLimits(float min, float max)
        {
            _minVerticalLookAngle = Mathf.Clamp(min, -89f, 0f);
            _maxVerticalLookAngle = Mathf.Clamp(max, 0f, 89f);
        }

        protected abstract void HandleHeight(float dt);

        protected abstract void HandleMovement(float dt);

        protected abstract void HandleLook();

        protected void SetHeight(float heightInput)
        {
            _cinemachineTransposer.m_FollowOffset += new Vector3(0f, heightInput, 0f);
            _cinemachineTransposer.m_FollowOffset = new Vector3(0f, Mathf.Clamp(_cinemachineTransposer.m_FollowOffset.y, 0.5f, 3f), 0f);
        }
    }
}
