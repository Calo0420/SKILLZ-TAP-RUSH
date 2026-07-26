# New Chat Bootstrap (TapRush)

Use this every time you start a new chat.

## Copy/Paste Prompt (Copilot or any coding assistant)

You are now working on TapRush.

Canonical workspace path:
C:\Users\osreyes\Documents\TapRush\My project

Required startup actions:
1. Confirm your current working path and branch before any edits.
2. If path is not the canonical path, stop and ask for confirmation.
3. Read these files first, then summarize current state in 8 bullets max:
- Project_Status.md
- WORKFLOW_GUARDRAILS.md
- .github/copilot-instructions.md
4. Show current commit and remote status:
- git rev-parse --short HEAD
- git branch --show-current
- git status --short
- git ls-remote origin refs/heads/main
5. Continue from current repo state only. Do not assume old chat context.

Hard rules:
- Do not edit or commit from any other local clone/worktree.
- Commit/push only from canonical path to origin/main unless explicitly told otherwise.
- If ambiguity exists, ask one confirmation question and pause.

Current milestone intent:
- Continue from latest milestone in Project_Status.md.
- Prioritize deterministic progress, minimal-risk changes, and clear verification.

## Quick Prompt (Short Version)

Use canonical path only:
C:\Users\osreyes\Documents\TapRush\My project

Read first:
- Project_Status.md
- WORKFLOW_GUARDRAILS.md
- .github/copilot-instructions.md

Then print:
- current path
- branch
- HEAD commit
- origin/main commit
- brief plan

Stop if path is different from canonical.

## Tim-Specific Handoff Prompt

Tim, new chat handoff for TapRush:
1. Use only C:\Users\osreyes\Documents\TapRush\My project
2. Read Project_Status.md + WORKFLOW_GUARDRAILS.md + .github/copilot-instructions.md
3. Confirm path/branch/HEAD/origin-main
4. Proceed with smallest safe step toward current milestone
5. If any path mismatch appears, stop and ask before editing
