using UnityEngine;

namespace uDesktopDuplication
{
    public class Monitor
    {
        public Monitor(int id)
        {
            Id = id;

            switch (State)
            {
                case DuplicatorState.Ready:
                case DuplicatorState.Running:
                    break;
                case DuplicatorState.InvalidArg:
                    Debug.LogErrorFormat("[uDD] {0}:{1} => Invalid.", id, Name);
                    break;
                case DuplicatorState.AccessDenied:
                    Debug.LogWarningFormat("[uDD] {0}:{1} => Access Denied.", id, Name);
                    break;
                case DuplicatorState.Unsupported:
                    Debug.LogWarningFormat("[uDD] {0}:{1} => Unsupported.", id, Name);
                    break;
                case DuplicatorState.SessionDisconnected:
                    Debug.LogWarningFormat("[uDD] {0}:{1} => Disconnected.", id, Name);
                    break;
                case DuplicatorState.NotSet:
                    Debug.LogErrorFormat("[uDD] {0}:{1} => Something wrong.", id, Name);
                    break;
                default:
                    Debug.LogErrorFormat("[uDD] {0}:{1} => Unknown error.", id, Name);
                    break;
            }

            if (DpiX == 0 || DpiY == 0)
                Debug.LogWarningFormat("[uDD] {0}:{1} => Could not get DPI", id, Name);
        }

        public int Id
        {
            get;
            private set;
        }

        public bool Exists => Id < Manager.MonitorCount;

        public DuplicatorState State => Lib.GetState(Id);

        public bool Available => State == DuplicatorState.Ready || State == DuplicatorState.Running;

        public string Name => Lib.GetName(Id);

        public bool IsPrimary => Lib.IsPrimary(Id);

        public int Left => Lib.GetLeft(Id);

        public int Right => Lib.GetRight(Id);

        public int Top => Lib.GetTop(Id);

        public int Bottom => Lib.GetBottom(Id);

        public int Width => Lib.GetWidth(Id);

        public int Height => Lib.GetHeight(Id);

        public int DpiX
        {
            get
            {
                int dpi = Lib.GetDpiX(Id);
                if (dpi == 0)
                    dpi = 100; // when monitors are duplicated
                return dpi;
            }
        }

        public int DpiY
        {
            get
            {
                int dpi = Lib.GetDpiY(Id);
                if (dpi == 0)
                    dpi = 100; // when monitors are duplicated
                return dpi;
            }
        }

        public float WidthMeter => Width / DpiX * 0.0254f;

        public float HeightMeter => Height / DpiY * 0.0254f;

        public MonitorRotation Rotation => Lib.GetRotation(Id);

        public float Aspect => (float)Width / Height;

        public bool IsHorizontal => (Rotation == MonitorRotation.Identity) ||
                    (Rotation == MonitorRotation.Rotate180);

        public bool IsVertical => (Rotation == MonitorRotation.Rotate90) ||
                    (Rotation == MonitorRotation.Rotate270);

        public bool IsCursorVisible => Lib.IsCursorVisible();

        public int CursorX => Lib.GetCursorMonitorId() == Id ? Lib.GetCursorX() : -1;

        public int CursorY => Lib.GetCursorMonitorId() == Id ? Lib.GetCursorY() : -1;

        public int SystemCursorX
        {
            get
            {
                MousePoint p = Utility.GetCursorPos();
                return p.x - Left;
            }
        }

        public int SystemCursorY
        {
            get
            {
                MousePoint p = Utility.GetCursorPos();
                return p.y - Top;
            }
        }

        public int CursorShapeWidth => Lib.GetCursorShapeWidth();

        public int CursorShapeHeight => Lib.GetCursorShapeHeight();

        public CursorShapeType CursorShapeType => Lib.GetCursorShapeType();

        public int MoveRectCount => Lib.GetMoveRectCount(Id);

        public DXGI_OUTDUPL_MOVE_RECT[] MoveRects => Lib.GetMoveRects(Id);

        public int DirtyRectCount => Lib.GetDirtyRectCount(Id);

        public RECT[] DirtyRects => Lib.GetDirtyRects(Id);

        public System.IntPtr Buffer => Lib.GetBuffer(Id);

        public bool HasBeenUpdated => Lib.HasBeenUpdated(Id);

        bool _useGetPixels = false;
        public bool UseGetPixels
        {
            get => _useGetPixels;
            set
            {
                _useGetPixels = value;
                _ = Lib.UseGetPixels(Id, value);
            }
        }

        public bool ShouldBeUpdated
        {
            get;
            set;
        }

        private static Texture2D _errorTexture;
        private static readonly string _errorTexturePath = "uDesktopDuplication/Textures/NotAvailable";
        private Texture2D ErrorTexture
        {
            get
            {
                if (_errorTexture == null)
                    _errorTexture = Resources.Load<Texture2D>(_errorTexturePath);
                return _errorTexture;
            }
        }

        private Texture2D _texture;
        private System.IntPtr _texturePtr;
        public Texture2D Texture
        {
            get
            {
                if (!Available)
                    return ErrorTexture;
                if (_texture == null)
                    CreateTextureIfNeeded();
                return _texture;
            }
        }

        public void Render()
        {
            if (_texture && Available && _texturePtr != System.IntPtr.Zero)
            {
                _ = Lib.SetTexturePtr(Id, _texturePtr);
                GL.IssuePluginEvent(Lib.GetRenderEventFunc(), Id);
            }
        }

        public void GetCursorTexture(System.IntPtr ptr) => Lib.GetCursorTexture(ptr);

        public void CreateTextureIfNeeded()
        {
            if (!Available)
                return;

            int w = IsHorizontal ? Width : Height;
            int h = IsHorizontal ? Height : Width;
            bool shouldCreate = true;

            if (_texture && _texture.width == w && _texture.height == h)
            {
                shouldCreate = false;
            }

            if (w <= 0 || h <= 0)
            {
                shouldCreate = false;
            }

            if (shouldCreate)
            {
                CreateTexture();
            }
        }

        void CreateTexture()
        {
            DestroyTexture();
            int w = IsHorizontal ? Width : Height;
            int h = IsHorizontal ? Height : Width;
            _texture = new Texture2D(w, h, TextureFormat.BGRA32, false);
            _texturePtr = _texture.GetNativeTexturePtr();
        }

        public void DestroyTexture()
        {
            if (_texture)
            {
                Object.Destroy(_texture);
                _texture = null;
                _texturePtr = System.IntPtr.Zero;
            }
        }

        public void Reinitialize() => CreateTextureIfNeeded();

        public Color32[] GetPixels(int x, int y, int width, int height)
        {
            if (!_useGetPixels)
            {
                Debug.LogErrorFormat("Please set Monitor[{0}].useGetPixels as true.", Id);
                return null;
            }
            return Lib.GetPixels(Id, x, y, width, height);
        }

        public bool GetPixels(Color32[] colors, int x, int y, int width, int height)
        {
            if (!_useGetPixels)
            {
                Debug.LogErrorFormat("Please set Monitor[{0}].useGetPixels as true.", Id);
                return false;
            }
            return Lib.GetPixels(Id, colors, x, y, width, height);
        }

        public Color32 GetPixel(int x, int y)
        {
            if (!_useGetPixels)
            {
                Debug.LogErrorFormat("Please set Monitor[{0}].useGetPixels as true.", Id);
                return Color.black;
            }
            return Lib.GetPixel(Id, x, y);
        }
    }
}
