# TapRush Workflow Guardrails

## Canonical Path
- Use this path for Unity + VS Code + git:
  - `C:\Users\osreyes\Documents\TapRush\My project`

## Canonical Remote/Branch
- Remote: `origin`
- Branch: `main`

## Rules
1. Make gameplay/code changes only in the canonical path.
2. Commit and push only from the canonical path.
3. If another local clone/worktree is opened, treat it as read-only unless explicitly migrating.
4. Keep Unity and git pointed at the same folder at all times.
5. Before any release/milestone push, run:
   - `git status --short`
   - `git rev-parse --short HEAD`
   - `git branch --show-current`

## MCP Rule
- MCP-connected Copilot must edit only this canonical path.
- If MCP session points elsewhere, stop and re-point before continuing.

## Safety Check Prompt
Use this quick self-check before starting work:
- "Am I in C:\Users\osreyes\Documents\TapRush\My project on branch main?"

If no, stop and fix path/branch first.
