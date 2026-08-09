using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public sealed class CopilotUnityBridge : MonoBehaviour
{
    [Serializable]
    private sealed class BridgeCommand
    {
        public string id;
        public string command;
        public string text;
        public float number;
    }

    [Serializable]
    private sealed class BridgeResponse
    {
        public string id;
        public string status;
        public string message;
        public string activeScene;
        public float timeScale;
        public string timestampUtc;
    }

    [Serializable]
    private sealed class BridgeState
    {
        public string status;
        public string activeScene;
        public float timeScale;
        public string timestampUtc;
    }

    private const float PollIntervalSeconds = 0.2f;

    private static CopilotUnityBridge _instance;

    private string _bridgeRoot;
    private string _commandPath;
    private string _responsePath;
    private string _statePath;
    private float _nextPollTime;
    private string _lastCommandId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureBridge()
    {
        if (_instance != null)
        {
            return;
        }

        var existing = FindFirstObjectByType<CopilotUnityBridge>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        var go = new GameObject("CopilotUnityBridge");
        _instance = go.AddComponent<CopilotUnityBridge>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _bridgeRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "CopilotBridge"));
        _commandPath = Path.Combine(_bridgeRoot, "command.json");
        _responsePath = Path.Combine(_bridgeRoot, "response.json");
        _statePath = Path.Combine(_bridgeRoot, "state.json");

        Directory.CreateDirectory(_bridgeRoot);
        WriteState("ready");
        Debug.Log($"[CopilotBridge] Ready at {_bridgeRoot}");
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextPollTime)
        {
            return;
        }

        _nextPollTime = Time.unscaledTime + PollIntervalSeconds;

        ProcessIncomingCommand();
        WriteState("ready");
    }

    private void ProcessIncomingCommand()
    {
        if (!File.Exists(_commandPath))
        {
            return;
        }

        BridgeCommand command;
        try
        {
            var json = File.ReadAllText(_commandPath);
            command = JsonUtility.FromJson<BridgeCommand>(json);
        }
        catch (Exception ex)
        {
            WriteResponse(new BridgeResponse
            {
                id = Guid.NewGuid().ToString("N"),
                status = "error",
                message = "Invalid command JSON: " + ex.Message,
                activeScene = SceneManager.GetActiveScene().name,
                timeScale = Time.timeScale,
                timestampUtc = DateTime.UtcNow.ToString("O")
            });
            SafeDeleteCommand();
            return;
        }

        if (command == null || string.IsNullOrWhiteSpace(command.command))
        {
            WriteResponse(new BridgeResponse
            {
                id = Guid.NewGuid().ToString("N"),
                status = "error",
                message = "Command is empty. Expected fields: id, command, text, number.",
                activeScene = SceneManager.GetActiveScene().name,
                timeScale = Time.timeScale,
                timestampUtc = DateTime.UtcNow.ToString("O")
            });
            SafeDeleteCommand();
            return;
        }

        if (!string.IsNullOrWhiteSpace(command.id) && command.id == _lastCommandId)
        {
            SafeDeleteCommand();
            return;
        }

        var response = Execute(command);
        _lastCommandId = command.id;
        WriteResponse(response);
        SafeDeleteCommand();
    }

    private BridgeResponse Execute(BridgeCommand command)
    {
        var response = new BridgeResponse
        {
            id = string.IsNullOrWhiteSpace(command.id) ? Guid.NewGuid().ToString("N") : command.id,
            status = "ok",
            activeScene = SceneManager.GetActiveScene().name,
            timeScale = Time.timeScale,
            timestampUtc = DateTime.UtcNow.ToString("O")
        };

        try
        {
            switch (command.command.Trim().ToLowerInvariant())
            {
                case "ping":
                    response.message = "pong";
                    break;

                case "load_scene":
                    if (string.IsNullOrWhiteSpace(command.text))
                    {
                        response.status = "error";
                        response.message = "text is required for load_scene.";
                        break;
                    }

                    SceneManager.LoadScene(command.text);
                    response.message = "Loading scene: " + command.text;
                    break;

                case "set_timescale":
                    if (command.number < 0f)
                    {
                        response.status = "error";
                        response.message = "number must be >= 0 for set_timescale.";
                        break;
                    }

                    Time.timeScale = command.number;
                    response.timeScale = Time.timeScale;
                    response.message = "Time.timeScale set to " + Time.timeScale;
                    break;

                case "find_object":
                    if (string.IsNullOrWhiteSpace(command.text))
                    {
                        response.status = "error";
                        response.message = "text is required for find_object.";
                        break;
                    }

                    var found = GameObject.Find(command.text) != null;
                    response.message = found ? "found" : "not_found";
                    break;

                default:
                    response.status = "error";
                    response.message = "Unknown command: " + command.command;
                    break;
            }
        }
        catch (Exception ex)
        {
            response.status = "error";
            response.message = ex.Message;
        }

        response.activeScene = SceneManager.GetActiveScene().name;
        response.timeScale = Time.timeScale;
        response.timestampUtc = DateTime.UtcNow.ToString("O");

        return response;
    }

    private void WriteResponse(BridgeResponse response)
    {
        var json = JsonUtility.ToJson(response, true);
        File.WriteAllText(_responsePath, json);
    }

    private void WriteState(string status)
    {
        var state = new BridgeState
        {
            status = status,
            activeScene = SceneManager.GetActiveScene().name,
            timeScale = Time.timeScale,
            timestampUtc = DateTime.UtcNow.ToString("O")
        };

        File.WriteAllText(_statePath, JsonUtility.ToJson(state, true));
    }

    private void SafeDeleteCommand()
    {
        try
        {
            if (File.Exists(_commandPath))
            {
                File.Delete(_commandPath);
            }
        }
        catch
        {
            // Ignore file delete races while Unity and tools are both active.
        }
    }
}
#endif
