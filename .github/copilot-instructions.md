# Copilot Guardrails for TapRush

## Canonical Workspace (Single Source of Truth)
- Always operate in this repository path:
  - `C:\Users\osreyes\Documents\TapRush\My project`
- Do **not** create or apply edits in other local clones or worktrees unless explicitly requested.
- If a user message references another path, ask for confirmation before editing there.

## Git and Branch Guardrails
- Default branch for sync is `main`.
- Before commits/pushes, verify current path and branch.
- Do not stage unrelated generated/transient files.

## Unity + MCP Workflow Guardrails
- Unity Editor and git operations must target the same canonical path.
- Treat this repo as authoritative for all gameplay and build changes.
- If path divergence is detected, stop and request reconciliation confirmation.

## Build/Artifact Guardrails
- Do not commit temporary build outputs (Library, Temp, Logs, .utmp, etc.).
- Prefer committing source assets/scripts/settings only.

## Decision Rule
- If there is any ambiguity about which local copy is active, pause and ask one confirmation question:
  - "Should I continue in C:\Users\osreyes\Documents\TapRush\My project as the canonical source?"
