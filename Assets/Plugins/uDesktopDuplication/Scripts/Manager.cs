using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uDesktopDuplication
{
    public class Manager : MonoBehaviour
    {
        private static Manager _instance;
        public static Manager Instance => CreateInstance();

        public static Manager CreateInstance()
        {
            if (_instance != null)
                return _instance;

            Manager manager = FindObjectOfType<Manager>();
            if (manager)
            {
                _instance = manager;
                return manager;
            }

            GameObject go = new GameObject("uDesktopDuplicationManager");
            _instance = go.AddComponent<Manager>();
            return _instance;
        }

        private readonly List<Monitor> _monitors = new List<Monitor>();
        static public List<Monitor> Monitors => Instance._monitors;

        static public int MonitorCount => Lib.GetMonitorCount();

        static public int CursorMonitorId => Lib.GetCursorMonitorId();

        static public Monitor Primary => Instance._monitors.Find(monitor => monitor.IsPrimary);

        [Tooltip("Debug mode is not applied while running.")]
        [SerializeField] private DebugMode _debugMode = DebugMode.File;

        [SerializeField] private float _retryReinitializationDuration = 1f;

        private Coroutine _renderCoroutine = null;
        private bool _shouldReinitialize = false;
        private float _reinitializationTimer = 0f;
        private bool _isFirstFrame = true;

        public static event Lib.DebugLogDelegate OnDebugLog = DebugLogCallback;
        public static event Lib.DebugLogDelegate OnDebugErr = DebugErrCallback;

        [AOT.MonoPInvokeCallback(typeof(Lib.DebugLogDelegate))]
        private static void DebugLogCallback(string msg) => Debug.Log(msg);
        [AOT.MonoPInvokeCallback(typeof(Lib.DebugLogDelegate))]
        private static void DebugErrCallback(string msg) => Debug.LogError(msg);

        public delegate void ReinitializeHandler();
        public static event ReinitializeHandler OnReinitialized;

        public static Monitor GetMonitor(int id)
        {
            if (id < 0 || id >= Monitors.Count)
            {
                Debug.LogErrorFormat("[uDD::Error] there is no monitor whose id is {0}.", id);
                return Primary;
            }
            return Monitors[Mathf.Clamp(id, 0, MonitorCount - 1)];
        }

        private void Awake()
        {
            // for simple singleton
            if (_instance == this)
                return;

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            Lib.SetDebugMode(_debugMode);
            Lib.SetLogFunc(OnDebugLog);
            Lib.SetErrorFunc(OnDebugErr);

            Lib.Initialize();

            CreateMonitors();

#if UNITY_2018_1_OR_NEWER
            Shader.DisableKeyword("USE_GAMMA_TO_LINEAR_SPACE");
#else
        Shader.EnableKeyword("USE_GAMMA_TO_LINEAR_SPACE");
#endif
        }

        private void OnApplicationQuit()
        {
            Lib.Finalize();
            DestroyMonitors();
        }

        private void OnEnable()
        {
            _renderCoroutine = StartCoroutine(OnRender());
            if (!_isFirstFrame)
            {
                Reinitialize();
            }

            Lib.SetDebugMode(_debugMode);
            Lib.SetLogFunc(OnDebugLog);
        }

        private void OnDisable()
        {
            if (_renderCoroutine != null)
            {
                StopCoroutine(_renderCoroutine);
                _renderCoroutine = null;
            }

            Lib.SetLogFunc(null);
            Lib.SetErrorFunc(null);
        }

        private void Update()
        {
            Lib.Update();
            ReinitializeIfNeeded();
            UpdateMessage();
            _isFirstFrame = false;
        }

        [ContextMenu("Reinitialize")]
        public void Reinitialize()
        {
            Debug.Log("[uDD] Reinitialize");
            Lib.Reinitialize();
            CreateMonitors();
            OnReinitialized?.Invoke();
        }

        private void ReinitializeIfNeeded()
        {
            bool reinitializeNeeded = false;

            for (int i = 0; i < Monitors.Count; ++i)
            {
                Monitor monitor = Monitors[i];
                DuplicatorState state = monitor.State;
                if (
                    state == DuplicatorState.NotSet ||
                    state == DuplicatorState.AccessLost ||
                    state == DuplicatorState.AccessDenied ||
                    state == DuplicatorState.SessionDisconnected ||
                    state == DuplicatorState.Unknown
                )
                {
                    reinitializeNeeded = true;
                    break;
                }
            }

            if (Lib.HasMonitorCountChanged())
            {
                reinitializeNeeded = true;
            }

            if (!_shouldReinitialize && reinitializeNeeded)
            {
                _shouldReinitialize = true;
                _reinitializationTimer = 0f;
            }

            if (_shouldReinitialize)
            {
                if (_reinitializationTimer > _retryReinitializationDuration)
                {
                    Reinitialize();
                    _shouldReinitialize = false;
                }
                _reinitializationTimer += Time.deltaTime;
            }
        }

        private void UpdateMessage()
        {
            Message message = Lib.PopMessage();
            while (message != Message.None)
            {
                Debug.Log("[uDD] " + message);
                switch (message)
                {
                    case Message.Reinitialized:
                        ReinitializeMonitors();
                        break;
                    case Message.TextureSizeChanged:
                        RecreateTextures();
                        break;
                    default:
                        break;
                }
                message = Lib.PopMessage();
            }
        }

        private IEnumerator OnRender()
        {
            for (; ; )
            {
                yield return new WaitForEndOfFrame();
                for (int i = 0; i < Monitors.Count; ++i)
                {
                    Monitor monitor = Monitors[i];
                    if (monitor.ShouldBeUpdated)
                    {
                        monitor.Render();
                    }
                    monitor.ShouldBeUpdated = false;
                }
            }
        }

        private void CreateMonitors()
        {
            DestroyMonitors();
            for (int i = 0; i < MonitorCount; ++i)
            {
                Monitors.Add(new Monitor(i));
            }
        }

        private void DestroyMonitors()
        {
            for (int i = 0; i < Monitors.Count; ++i)
            {
                Monitors[i].DestroyTexture();
            }
            Monitors.Clear();
        }

        private void ReinitializeMonitors()
        {
            for (int i = 0; i < MonitorCount; ++i)
            {
                if (i == Monitors.Count)
                {
                    Monitors.Add(new Monitor(i));
                }
                else
                {
                    Monitors[i].Reinitialize();
                }
            }
        }

        private void RecreateTextures()
        {
            for (int i = 0; i < MonitorCount; ++i)
            {
                Monitors[i].CreateTextureIfNeeded();
            }
        }
    }
}
