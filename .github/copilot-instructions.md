# Copilot Guardrails for TapRush

## On Session Start (AUTOMATIC — do this FIRST every time)
1. Read `C:\Users\osreyes\.copilot\COPILOT_MEMORY.md` (identity, infra, keys, projects)
2. Read `Project_Status.md` in this repo (current game state)
3. Read `WORKFLOW_GUARDRAILS.md` in this repo (rules)
4. Run: `git rev-parse --short HEAD` + `git branch --show-current`
5. Confirm readiness in 3 lines max. Don't summarize back. Just proceed.

## Canonical Workspace (Single Source of Truth)
- Always operate in this repository path:
  - `C:\Users\osreyes\Documents\TapRush\My project`
- Do **not** create or apply edits in other local clones or worktrees unless explicitly requested.
- If a user message references another path, ask for confirmation before editing there.

## Git and Branch Guardrails
- Default branch for sync is `main`.
- Before commits/pushes, verify current path and branch.
- Do not stage unrelated generated/transient files.
- Push uses Windows Credential Manager — just `git push origin main` works.
- After pushing, also sync VPS: `ssh vps-calo "cd /opt/SKILLZ-TAP-RUSH && git pull origin main"`

## Unity + MCP Workflow Guardrails
- Unity Editor and git operations must target the same canonical path.
- Treat this repo as authoritative for all gameplay and build changes.
- If path divergence is detected, stop and request reconciliation confirmation.
- MCP config: `.vscode/mcp.json` — HTTP transport to `127.0.0.1:8080`
- If MCP tools drop, server is likely still running — try calling anyway.

## Build/Artifact Guardrails
- Do not commit temporary build outputs (Library, Temp, Logs, .utmp, etc.).
- Prefer committing source assets/scripts/settings only.

## End of Session (ALWAYS do before signing off)
1. Commit + push all changes to GitHub
2. Sync VPS: `ssh vps-calo "cd /opt/SKILLZ-TAP-RUSH && git pull origin main"`
3. Update `COPILOT_MEMORY.md` SESSION HANDOFF section with what was done
4. Update `Project_Status.md` if milestones changed

## Decision Rule
- If there is any ambiguity about which local copy is active, pause and ask one confirmation question:
  - "Should I continue in C:\Users\osreyes\Documents\TapRush\My project as the canonical source?"
