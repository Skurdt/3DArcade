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

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/Interaction/EditModeEditPositions", fileName = "EditModeEditPositionsInteractionController")]
    public sealed class EditModeEditPositionsInteractionController : InteractionController
    {
        [SerializeField, Layer] private int _highlightLayer;
        [SerializeField] private Color _outlineColor;

        public override void UpdateCurrentTarget(Camera camera)
        {
            ModelConfigurationComponent target = _raycaster.GetCurrentTarget(camera);
            if (target == null)
            {
                if (InteractionData.Current != null)
                    InteractionData.Current.RestoreLayerToOriginal();
                return;
            }

            if (InteractionData.Current != null && target != InteractionData.Current && InteractionData.Current.gameObject.TryGetComponent(out QuickOutline quickOutlineComponent))
                Destroy(quickOutlineComponent);

            if (!target.gameObject.TryGetComponent(out QuickOutline _))
            {
                QuickOutline outlineComponent = target.gameObject.AddComponent<QuickOutline>();
                outlineComponent.OutlineColor = _outlineColor;
                outlineComponent.OutlineWidth = 6f;
            }

            InteractionData.Set(target, _highlightLayer);
        }

        public void ManualMoveAndRotate(Vector2 direction, float rotation)
        {
            ModelConfigurationComponent current = InteractionData.Current;
            if (current == null || current.Rigidbody == null)
                return;

            Transform tr = current.transform;
            Rigidbody rb = current.Rigidbody;

            rb.constraints = RigidbodyConstraints.None;

            // Position
            if (direction.sqrMagnitude > 0.001f)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                rb.AddForce(-tr.forward * direction.y, ForceMode.VelocityChange);
                rb.AddForce(-tr.right * direction.x, ForceMode.VelocityChange);
            }
            rb.AddForce(Vector3.right * -rb.velocity.x, ForceMode.VelocityChange);
            rb.AddForce(Vector3.forward * -rb.velocity.z, ForceMode.VelocityChange);

            // Rotation
            if (rotation < -0.5f || rotation > 0.5f)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;

                float angle           = Mathf.Atan2(tr.forward.x, tr.forward.z) * Mathf.Rad2Deg;
                float targetAngle     = angle + rotation;
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

        public void AutoMoveAndRotate(Ray ray, Vector3 forward, float maxDistance, LayerMask layerMask)
        {
            if (!Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
                return;

            ModelConfigurationComponent current = InteractionData.Current;

            Vector3 newPosition;
            Transform transform = current.transform;
            Vector3 position    = hitInfo.point;
            Vector3 normal      = hitInfo.normal;
            float dot           = Vector3.Dot(Vector3.up, normal);

            // Floor
            if (dot > 0.05f)
            {
                newPosition             = new Vector3(position.x, position.y + 0.1f, position.z);
                transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.LookRotation(-forward);
                return;
            }

            // Ceiling
            if (dot < -0.05f)
            {
                newPosition             = new Vector3(position.x, transform.position.y, position.z);
                transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.FromToRotation(Vector3.up, -normal) * Quaternion.LookRotation(-forward);
                return;
            }

            // Vertical surface
            Collider collider = current.Collider;
            if (collider == null)
                return;

            Vector3 positionOffset  = normal * Mathf.Max(collider.bounds.extents.x + 0.1f, collider.bounds.extents.z + 0.1f);
            newPosition             = new Vector3(position.x, transform.position.y, position.z) + positionOffset;
            transform.position      = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 12f);
            transform.localRotation = Quaternion.LookRotation(normal);
        }
    }
}
