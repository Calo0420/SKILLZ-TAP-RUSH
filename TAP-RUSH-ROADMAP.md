# TAP RUSH Project Roadmap

Lead: C/C/C
Product Owner: Oscar Reyes (Calo0420)
Target: 2-4 weeks to Skillz submission

## PHASE 0 - Setup (Day 1)
Exit: Empty Unity opens, SDK loads without errors.

## PHASE 1 - Core Gameplay (Days 2-6)
Exit: Tap targets 60s, see score change.

## PHASE 2 - Scoring & Feel (Days 7-9)
Exit: Full match feels like a game.

Status: COMPLETE (2026-07-22)
Delivered:
- Stable wrong-tap penalty and combo reset behavior
- Combo milestones at x5/x10 with dedicated milestone sound
- Combo popup feedback and camera shake
- Runtime end screen fallback flow
- Input-system compatible tap detection

## PHASE 3 - Deterministic/Seeded (Days 10-13) CRITICAL
Exit: Same seed = identical spawns on every device.

Next Actions:
- Deterministic RNG seed path for target spawns
- Seed handoff through match/session bootstrap
- Replay validation: same seed -> same spawn sequence

## PHASE 4 - Skillz SDK Integration (Days 14-18)
Exit: Full match through actual Skillz sandbox.

## PHASE 5 - Polish Pass (Days 19-22)
Exit: Looks intentional, not a prototype.

## PHASE 6 - QA & Submission (Days 23-28)
Exit: Submitted.

Ground Rules:
- No new features until Phase 6
- Stuck > 1 day: escalate
- Phase 3 is critical: Claude reviews
- Rough > perfect

Workspace:
GitHub: git@github.com:Calo0420/SKILLZ-TAP-RUSH.git
VPS: /opt/SKILLZ-TAP-RUSH/
Local: C:\Users\osreyes\Documents\TapRush
