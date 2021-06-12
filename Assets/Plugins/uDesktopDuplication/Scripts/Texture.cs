using UnityEngine;

namespace uDesktopDuplication
{
    public enum Culling
    {
        Off = 0,
        Front = 1,
        Back = 2,
    }

    public enum MeshForwardDirection
    {
        Y = 0,
        Z = 1,
    }

    [AddComponentMenu("uDesktopDuplication/Texture"), RequireComponent(typeof(Renderer))]
    public class Texture : MonoBehaviour
    {
        Monitor _monitor;
        public Monitor Monitor
        {
            get => _monitor;
            set
            {
                _monitor = value;
                if (_monitor != null)
                {
                    Material.mainTexture = _monitor.Texture;
                    Width = transform.localScale.x;
                    Rotation = Monitor.Rotation;
                    InvertX = _invertX;
                    InvertY = _invertY;
                    UseClip = _useClip;
                }
            }
        }

        int _lastMonitorId = 0;
        public int MonitorId
        {
            get => Monitor.Id;
            set => Monitor = Manager.GetMonitor(value);
        }

        [SerializeField] bool _invertX = false;
        public bool InvertX
        {
            get => _invertX;
            set
            {
                _invertX = value;
                if (_invertX)
                {
                    Material.EnableKeyword("INVERT_X");
                }
                else
                {
                    Material.DisableKeyword("INVERT_X");
                }
            }
        }

        [SerializeField] bool _invertY = false;
        public bool InvertY
        {
            get => _invertY;
            set
            {
                _invertY = value;
                if (_invertY)
                {
                    Material.EnableKeyword("INVERT_Y");
                }
                else
                {
                    Material.DisableKeyword("INVERT_Y");
                }
            }
        }

        public MonitorRotation Rotation
        {
            get => Monitor.Rotation;
            private set
            {
                switch (value)
                {
                    case MonitorRotation.Identity:
                        Material.DisableKeyword("ROTATE90");
                        Material.DisableKeyword("ROTATE180");
                        Material.DisableKeyword("ROTATE270");
                        break;
                    case MonitorRotation.Rotate90:
                        Material.EnableKeyword("ROTATE90");
                        Material.DisableKeyword("ROTATE180");
                        Material.DisableKeyword("ROTATE270");
                        break;
                    case MonitorRotation.Rotate180:
                        Material.DisableKeyword("ROTATE90");
                        Material.EnableKeyword("ROTATE180");
                        Material.DisableKeyword("ROTATE270");
                        break;
                    case MonitorRotation.Rotate270:
                        Material.DisableKeyword("ROTATE90");
                        Material.DisableKeyword("ROTATE180");
                        Material.EnableKeyword("ROTATE270");
                        break;
                    default:
                        break;
                }
            }
        }

        [SerializeField] bool _useClip = false;
        public Vector2 clipPos = Vector2.zero;
        public Vector2 clipScale = new Vector2(0.2f, 0.2f);
        public bool UseClip
        {
            get => _useClip;
            set
            {
                _useClip = value;
                if (_useClip)
                    Material.EnableKeyword("USE_CLIP");
                else
                    Material.DisableKeyword("USE_CLIP");
            }
        }

        public bool Bend
        {
            get => Material.GetInt("_Bend") != 0;
            set
            {
                if (value)
                {
                    Material.EnableKeyword("BEND_ON");
                    Material.SetInt("_Bend", 1);
                }
                else
                {
                    Material.DisableKeyword("BEND_ON");
                    Material.SetInt("_Bend", 0);
                }
            }
        }

        public MeshForwardDirection MeshForwardDirection
        {
            get => (MeshForwardDirection)Material.GetInt("_Forward");
            set
            {
                switch (value)
                {
                    case MeshForwardDirection.Y:
                        Material.SetInt("_Forward", 0);
                        Material.EnableKeyword("_FORWARD_Y");
                        Material.DisableKeyword("_FORWARD_Z");
                        break;
                    case MeshForwardDirection.Z:
                        Material.SetInt("_Forward", 1);
                        Material.DisableKeyword("_FORWARD_Y");
                        Material.EnableKeyword("_FORWARD_Z");
                        break;
                }
            }
        }

        public Culling Culling
        {
            get => (Culling)Material.GetInt("_Cull");
            set => Material.SetInt("_Cull", (int)value);
        }

        public float Radius
        {
            get => Material.GetFloat("_Radius");
            set => Material.SetFloat("_Radius", value);
        }

        public float Width
        {
            get => Material.GetFloat("_Width");
            set => Material.SetFloat("_Width", value);
        }

        public float Thickness
        {
            get => Material.GetFloat("_Thickness");
            set => Material.SetFloat("_Thickness", value);
        }

        Material _material;
        public Material Material
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (_material == null)
                        _material = GetComponent<Renderer>().material;
                    return _material; // clone
                }
                else
                    return GetComponent<Renderer>().sharedMaterial;
            }
        }

        Mesh Mesh => GetComponent<MeshFilter>().sharedMesh;
        public float WorldWidth => transform.localScale.x * (Mesh.bounds.extents.x * 2f);
        public float WorldHeight => transform.localScale.y * (Mesh.bounds.extents.y * 2f);

        int _clipPositionScaleKey;

        void Awake() => _clipPositionScaleKey = Shader.PropertyToID("_ClipPositionScale");

        void OnEnable()
        {
            Monitor ??= Manager.Primary;
            Manager.OnReinitialized += Reinitialize;
        }

        void OnDisable() => Manager.OnReinitialized -= Reinitialize;

        void Update()
        {
            KeepMonitor();
            RequireUpdate();
            UpdateMaterial();
        }

        void KeepMonitor()
        {
            if (Monitor == null)
            {
                Reinitialize();
            }
            else
            {
                _lastMonitorId = MonitorId;
            }
        }

        void RequireUpdate()
        {
            if (Monitor != null)
            {
                Monitor.ShouldBeUpdated = true;
            }
        }

        void Reinitialize() => Monitor = Manager.GetMonitor(_lastMonitorId); // Monitor instance is released here when initialized.

        void UpdateMaterial()
        {
            Width = transform.localScale.x;

            if (Monitor != null)
                Rotation = Monitor.Rotation;

            Material.SetVector(_clipPositionScaleKey, new Vector4(clipPos.x, clipPos.y, clipScale.x, clipScale.y));
        }

        public Vector3 GetWorldPositionFromCoord(Vector2 coord) => GetWorldPositionFromCoord((int)coord.x, (int)coord.y);

        public Vector3 GetWorldPositionFromCoord(int u, int v)
        {
            // Local position (scale included).
            float x = (float)(u - (Monitor.Width / 2)) / Monitor.Width;
            float y = -(float)(v - (Monitor.Height / 2)) / Monitor.Height;
            Vector3 localPos = new Vector3(WorldWidth * x, WorldHeight * y, 0f);

            // Bending
            if (Bend)
            {
                float angle = localPos.x / Radius;
                if (MeshForwardDirection == MeshForwardDirection.Y)
                    localPos.y -= Radius * (1f - Mathf.Cos(angle));
                else
                    localPos.z -= Radius * (1f - Mathf.Cos(angle));
                localPos.x = Radius * Mathf.Sin(angle);
            }

            // To world position
            return transform.position + (transform.rotation * localPos);
        }

        public struct RayCastResult
        {
            public bool hit;
            public Texture texture;
            public Vector3 position;
            public Vector3 normal;
            public Vector2 coords;
            public Vector2 desktopCoord;
        }

        static readonly RayCastResult _raycastFailedResult = new RayCastResult
        {
            hit = false,
            texture = null,
            position = Vector3.zero,
            normal = Vector3.forward,
            coords = Vector2.zero,
            desktopCoord = Vector2.zero,
        };

        // This function can be used only for vertical (= MeshForwardDirection.Z) plane.
        public RayCastResult RayCast(Vector3 from, Vector3 dir)
        {
            float r = Radius;
            Vector3 center = transform.position - (transform.forward * r);

            // Localize the start point of the ray and the direction.
            Matrix4x4 trs = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Matrix4x4 invTrs = trs.inverse;
            Vector3 localFrom = invTrs.MultiplyPoint3x4(from);
            Vector3 localDir = invTrs.MultiplyVector(dir).normalized;

            // Calculate the intersection points of circle and line on the X-Z plane.
            float a = localDir.z / localDir.x;
            float b = localFrom.z - (a * localFrom.x);

            float aa = a * a;
            float bb = b * b;
            float ab = a * b;
            float rr = r * r;

            float s = (aa * rr) - bb + rr;
            if (s < 0f)
            {
                return _raycastFailedResult;
            }
            s = Mathf.Sqrt(s);

            float t = aa + 1;

            float lx0 = (-s - ab) / t;
            float lz0 = (b - (a * s)) / t;
            Vector3 to0 = new Vector3(lx0, 0f, lz0);

            float lx1 = (s - ab) / t;
            float lz1 = ((a * s) + b) / t;
            Vector3 to1 = new Vector3(lx1, 0f, lz1);

            Vector3 to = (Vector3.Dot(localDir, to0) > 0f) ? to0 : to1;

            // Check if the point is inner angle of mesh width.
            float toAngle = Mathf.Atan2(to.x, to.z);
            float halfWidthAngle = WorldWidth / Radius * 0.5f;
            if (Mathf.Abs(toAngle) > halfWidthAngle)
            {
                return _raycastFailedResult;
            }

            // Calculate the intersection points on XZ-Y plane.
            Vector3 v = to - localFrom;
            float l = Mathf.Sqrt(Mathf.Pow(v.x, 2f) + Mathf.Pow(v.z, 2f));
            float ly = localFrom.y + (l * localDir.y / Mathf.Sqrt(Mathf.Pow(localDir.x, 2f) + Mathf.Pow(localDir.z, 2f)));

            // Check if the point is inner mesh height.
            float halfHeight = WorldHeight * 0.5f;
            if (Mathf.Abs(ly) > halfHeight)
            {
                return _raycastFailedResult;
            }

            // Check hit position is in the range of the ray.
            to.y = ly;
            Vector3 hitPos = trs.MultiplyPoint(to);

            if ((hitPos - from).magnitude > dir.magnitude)
            {
                return _raycastFailedResult;
            }

            // Calculate coordinates.
            float coordX = toAngle / halfWidthAngle * 0.5f;
            float coordY = ly / halfHeight * 0.5f;

            // Zoom
            if (UseClip)
            {
                coordX = clipPos.x + ((0.5f + coordX) * clipScale.x);
                coordX -= Mathf.Floor(coordX);
                coordX -= 0.5f;

                coordY = 1f - clipPos.y + ((coordY - 0.5f) * clipScale.y);
                coordY -= Mathf.Floor(coordY);
                coordY -= 0.5f;
            }

            // Desktop position
            int desktopX = Monitor.Left + (int)((coordX + 0.5f) * Monitor.Width);
            int desktopY = Monitor.Top + (int)((0.5f - coordY) * Monitor.Height);

            // Calculate normal.
            Vector3 normal = new Vector3(-to.x, 0f, -to.z);

            // Result
            return new RayCastResult
            {
                hit = true,
                texture = this,
                position = trs.MultiplyPoint(to),
                normal = trs.MultiplyVector(normal).normalized,
                coords = new Vector2(coordX, coordY),
                desktopCoord = new Vector2(desktopX, desktopY)
            };
        }

        public static RayCastResult RayCastAll(Vector3 from, Vector3 dir)
        {
            foreach (Texture uddTexture in FindObjectsOfType<Texture>())
            {
                RayCastResult result = uddTexture.RayCast(from, dir);
                if (result.hit)
                    return result;
            }
            return _raycastFailedResult;
        }
    }
}
