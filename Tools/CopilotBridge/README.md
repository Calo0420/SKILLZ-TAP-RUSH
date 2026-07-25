# Copilot Unity Bridge

This bridge lets tools send simple runtime commands to Unity through JSON files.

## Runtime files

When Unity is running in Editor or a Development build, this script creates:

- `CopilotBridge/command.json` (incoming command)
- `CopilotBridge/response.json` (last response)
- `CopilotBridge/state.json` (heartbeat/status)

Bridge root is project-level:

- `<ProjectRoot>/CopilotBridge`

## Supported commands

- `ping`
- `load_scene` (uses `text` as scene name)
- `set_timescale` (uses `number`)
- `find_object` (uses `text` as GameObject name)

## Send commands from PowerShell

From project root:

```powershell
./Tools/CopilotBridge/send-command.ps1 -Command ping
./Tools/CopilotBridge/send-command.ps1 -Command load_scene -Text GameScene
./Tools/CopilotBridge/send-command.ps1 -Command set_timescale -Number 0
./Tools/CopilotBridge/send-command.ps1 -Command set_timescale -Number 1
./Tools/CopilotBridge/send-command.ps1 -Command find_object -Text GameManager
```

## Notes

- Bridge is enabled only for `UNITY_EDITOR` or `DEVELOPMENT_BUILD`.
- It does not run in non-development player builds.
