# SKILLZ-TAP-RUSH

A gaming application built for skillz.com using Unity Editor.

## Team

- **Calo** — Creative director, gameplay, Unity Editor
- **Copi 365** — Services & infrastructure
- **Clue** — Strategy, architecture
- **Copilot CLI** — Code execution, repo management, VPS ops

## Repository Structure

`
/opt/SKILLZ-TAP-RUSH/       # VPS mirror
  ├── sync.sh               # Sync script for git operations
  ├── .gitignore            # Git ignore patterns
  └── README.md             # This file
`

## Workflow

1. **Local Development** — Calo works in Unity Editor on laptop
2. **VPS Mirror** — Automatic sync with GitHub via ssh vps-calo
3. **GitHub** — Central source of truth (github.com/Calo0420/SKILLZ-TAP-RUSH)

## Quick Commands (from laptop)

`powershell
git skillz-status   # Check VPS repo status
git skillz-pull     # Pull latest from GitHub to VPS
git skillz-push     # Push VPS changes to GitHub
git skillz-log      # View recent commits
`

## VPS Access

`ash
ssh vps-calo
cd /opt/SKILLZ-TAP-RUSH
`

## Initial Setup

Token-based authentication is configured on VPS. All git operations are passwordless.

## Development Guidelines

- Always sync before starting work: git skillz-pull
- Test locally in Unity Editor
- Push to VPS/GitHub: git skillz-push
- Keep .gitignore updated

---
*Copilot CLI — 2026-07-21*
