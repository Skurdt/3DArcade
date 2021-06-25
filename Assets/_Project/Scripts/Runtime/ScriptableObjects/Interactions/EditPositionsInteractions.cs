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

using SK.Utilities.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Interaction/EditPositions", fileName = "EditPositionsInteractions")]
    public sealed class EditPositionsInteractions : InteractionsBase
    {
        [SerializeField, Layer] private int _highlightLayer;

        [System.NonSerialized] private float _rotationOffset = 0f;

        public override void UpdateCurrentTarget(Camera camera)
        {
            GameEntity target = Raycaster.GetCurrentTarget(camera, Vector2.zero);
            if (CurrentTarget == target || target == null)
                return;

            if (CurrentTarget != null)
            {
                CurrentTarget.RestoreLayerToOriginal();
                CurrentTarget.RemoveOutline();
            }

            CurrentTarget = target;

            CurrentTarget.AddOutline(_outlineColor);

            _onCurrentGameTargetChange.Raise(CurrentTarget);
        }

        public void ManualMoveAndRotate(Vector2 directionInput, float rotationInput)
        {
            if (CurrentTarget == null || CurrentTarget.Rigidbody == null)
                return;

            Transform tr = CurrentTarget.transform;
            Rigidbody rb = CurrentTarget.Rigidbody;

            rb.constraints = RigidbodyConstraints.None;

            // Position
            if (directionInput.sqrMagnitude > 0.001f)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                rb.AddForce(-tr.forward * directionInput.y, ForceMode.VelocityChange);
                rb.AddForce(-tr.right * directionInput.x, ForceMode.VelocityChange);
            }
            rb.AddForce(Vector3.right * -rb.velocity.x, ForceMode.VelocityChange);
            rb.AddForce(Vector3.forward * -rb.velocity.z, ForceMode.VelocityChange);

            // Rotation
            if (rotationInput != 0f)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;

                float angle           = Mathf.Atan2(tr.forward.x, tr.forward.z) * Mathf.Rad2Deg;
                float targetAngle     = angle + rotationInput;
                float angleDifference = targetAngle - angle;

                if (Mathf.Abs(angleDifference) > 180f)
                {
                    if (angleDifference < 0f)
                        angleDifference = 360f + angleDifference;
                    else if (angleDifference > 0f)
                        angleDifference = (360f - angleDifference) * -1f;
                }

                rb.AddTorque(Vector3.up * angleDifference, ForceMode.VelocityChange);
                rb.AddTorque(Vector3.up * -rb.angularVelocity.y, ForceMode.VelocityChange);
            }
        }

        public void AutoMoveAndRotate(InputActions inputActions, Ray ray, Vector3 forward, float maxDistance, LayerMask layerMask)
        {
            if (CurrentTarget == null)
                return;

            if (!Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
                return;

            Vector3 newPosition;
            Transform transform = CurrentTarget.transform;
            Vector3 position    = hitInfo.point;
            Vector3 normal      = hitInfo.normal;
            float dot           = Vector3.Dot(Vector3.up, normal);

            InputAction rotateAction = inputActions.FpsEditPositions.Rotate;
            if (!(rotateAction.activeControl is null) && !(rotateAction.activeControl.device is Mouse))
                _rotationOffset += rotateAction.ReadValue<float>() * Time.deltaTime * 100f;
            else
                _rotationOffset += rotateAction.ReadValue<float>();

            // Floor
            if (dot > 0.05f)
            {
                newPosition             = new Vector3(position.x, position.y + 0.05f, position.z);
                transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.LookRotation(-forward) * Quaternion.AngleAxis(transform.localRotation.y, Vector3.up) * Quaternion.AngleAxis(_rotationOffset, Vector3.up);
                return;
            }

            // Ceiling
            if (dot < -0.05f)
            {
                newPosition             = new Vector3(position.x, transform.position.y + 0.05f, position.z);
                transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.FromToRotation(Vector3.up, -normal) * Quaternion.LookRotation(-forward) * Quaternion.AngleAxis(transform.localRotation.y, Vector3.up)  * Quaternion.AngleAxis(_rotationOffset, Vector3.up);
                return;
            }

            _rotationOffset = 0f;

            // Vertical surface
            Bounds bounds           = CurrentTarget.Bounds;
            Vector3 positionOffset  = normal * Mathf.Max(bounds.extents.x + 0.05f, bounds.extents.z + 0.05f);
            newPosition             = new Vector3(position.x, transform.position.y, position.z) + positionOffset;
            transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
            transform.localRotation = Quaternion.LookRotation(normal);
        }
    }
}
