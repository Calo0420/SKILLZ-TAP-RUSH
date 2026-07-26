# TAP RUSH — Project Status

> **Last Updated:** 2026-07-25  
> **Unity Version:** 6000+  
> **Target Platform:** Mobile (iOS/Android) via Skillz  
> **Repository:** Calo0420/SKILLZ-TAP-RUSH

---

## 🎮 Game Overview

Tap Rush is a competitive mobile tapping game targeting the Skillz platform. Players tap spawning targets for points within a 60-second match. Features escalating difficulty, decoy targets (red), bonus targets (gold), a gauge power-up mechanic, and a chaos finale in the last 15 seconds.

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── GameManager.cs          — Central game state, scoring, combo, power-ups
│   ├── TargetSpawner.cs        — All target spawning logic, chaos mode
│   ├── Target.cs               — Individual target behavior (shrink, drift, variants)
│   ├── GaugeTarget.cs          — Gauge power-up (8 taps → 2x multiplier)
│   ├── AudioManager.cs         — SFX + music system (normal/chaos switching)
│   ├── TimerManager.cs         — Match timer + chaos finale trigger
│   ├── UIManager.cs            — All HUD: score, combo, gauge, chaos announcement
│   ├── WrongTapDetector.cs     — Input detection + tap forgiveness radius
│   ├── GameSceneBootstrap.cs   — Scene initialization + auto-wiring
│   ├── GameSessionData.cs      — Static score pass between scenes
│   ├── MainMenu.cs             — Main menu button handler
│   ├── EndScreenManager.cs     — End screen display + navigation
│   └── Bridge/
│       └── CopilotUnityBridge.cs — MCP development bridge (not for production)
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── EndScreen.unity
├── Audio/ (imported packs)
│   ├── Timofei Shukshin UI Sounds Pack/
│   ├── Free Pack/
│   └── Scifi Loops Pack 1/
├── Simple Object Pooler/        — Imported, not yet integrated
└── YughuesFreeMetalMaterials/   — Imported for future visual polish
```

---

## 🗺️ Phase Roadmap

### Phase 1: Foundation ✅ COMPLETE
- Unity installed & configured
- Skillz account created, SDK imported
- Project created, VS Code connected
- Scene structure established

### Phase 2: Core Gameplay ✅ COMPLETE
- Main Menu → Play → GameScene → EndScreen flow
- 60-second countdown timer
- Tap targets spawn with correct/wrong scoring
- Score counter + combo system (x5 milestones)
- Decoy targets (red, penalty on tap)
- Bonus targets (gold, 3x points)
- Variable target sizes + drift movement
- Shrink-over-lifetime urgency
- Gauge power-up (cyan, 8 taps → 2x for 8s)
- Win/Lose screen with Play Again / Main Menu
- Tap forgiveness radius (0.42 units)

### Phase 3: Juice & Polish 🔥 IN PROGRESS

| Feature | Status | Notes |
|---------|--------|-------|
| Correct tap sound | ✅ Done | WAV_UI-007.wav |
| Wrong tap buzz | ✅ Done | WAV_UI-021.wav |
| Combo milestone sound | ✅ Done | WAV_UI-029.wav |
| Bonus hit sound | ✅ Done | Louder (1.5x vol) |
| Gauge tap/complete SFX | ✅ Done | WAV_UI-003 / WAV_UI-014 |
| Music (normal) | ✅ Done | 06_Supernova.ogg |
| Music (chaos) | ✅ Done | 08_Battlestations.ogg |
| Camera shake (combos) | ✅ Done | 0.16s duration |
| Combo text animation | ✅ Done | Pulse + gold color |
| Countdown beeps (3-2-1) | ❌ TODO | |
| Screen shake on mistakes | ❌ TODO | |
| Floating +points text | ❌ TODO | Numbers fly up on hit |
| Particle effects on tap | ❌ TODO | Burst on correct tap |
| Timer pulse when low | ❌ TODO | Scale/color at ≤10s |
| Better fonts | ❌ TODO | Replace default TMP |
| Button animations | ❌ TODO | Press/hover feedback |
| Score animations | ❌ TODO | Counter roll-up |

### Phase 4: Chaos Finale 🔥 MOSTLY DONE

| Feature | Status | Notes |
|---------|--------|-------|
| "OH GOD, HERE WE GO!" text | ✅ Done | Big red pulsing text |
| Slow-mo intro (0.5s) | ✅ Done | Time.timeScale=0.2 |
| White flash | ✅ Done | Full-screen fade |
| Red flood (4-7 per wave) | ✅ Done | 92% red chance |
| Faster spawn (0.08s floor) | ✅ Done | Multi-target enabled |
| Music switch + pitch up | ✅ Done | 1.2x pitch |
| Flashing timer | ❌ TODO | |
| Screen glow effect | ❌ TODO | |
| Fire/particle effects | ❌ TODO | |

### Phase 5: Skillz Integration 🏆 NOT STARTED
- Leaderboards
- Tournaments / PvP matchmaking
- Prize support via Skillz infrastructure
- SDK already imported, needs gameplay wiring

### Phase 6: Monetization Testing 💰 NOT STARTED
- Data gathering (match completion, session time, retention)
- Soft launch (friends, family, test users)

### Phase 7: Publish on Skillz 🎉 NOT STARTED
- Stable build, no major bugs
- Fair competition validation
- Skillz review submission

### Phase 8: Marketing & Scale 📈 NOT STARTED
- TikTok/YouTube Shorts/Reddit organic
- Meta Ads / TikTok Ads / Influencer testing

### Phase 9: Tap Rush 2.0 🌟 NOT STARTED
- Endless Mode, Survival Mode, Precision Mode
- Cosmetics (themes, backgrounds, effects)
- Weekend tournaments, leaderboard challenges

---

## 🐛 Known Bugs

| # | Bug | Severity | Status |
|---|-----|----------|--------|
| 1 | Music doesn't auto-play at game start | Medium | Open |
| 2 | Combo appears to not count first hits (miss from expired target silently resets) | Medium | Open |
| 3 | Object pooling not used — GC spikes during chaos mode on mobile | Medium | Open |

---

## 🔊 Audio Assignments (GameScene)

| Slot | File | Volume |
|------|------|--------|
| correctTapClip | WAV_UI-007.wav | 1.0x |
| wrongTapClip | WAV_UI-021.wav | 1.0x |
| comboMilestoneClip | WAV_UI-029.wav | 1.1x |
| gaugeTapClip | WAV_UI-003.wav | 0.9x |
| gaugeCompleteClip | WAV_UI-014.wav | 1.1x |
| chaosStartClip | Magic Spell_Electricity Spell_1.wav | 1.15x |
| gameplayMusicClip | 06_Supernova.ogg | 0.42 |
| chaosMusicClip | 08_Battlestations.ogg | 0.42 |

---

## ⚙️ Key Game Parameters

| Parameter | Value | Location |
|-----------|-------|----------|
| Match Duration | 60s | TimerManager |
| Chaos Trigger | 15s remaining | TimerManager |
| Base Spawn Interval | 0.75s | TargetSpawner |
| Min Spawn Interval | 0.35s | TargetSpawner |
| Chaos Spawn Floor | 0.08s | TargetSpawner |
| Decoy Chance | 30% | TargetSpawner |
| Bonus Chance | 8% | TargetSpawner |
| Gauge Spawn Every | 22s | TargetSpawner |
| Gauge Taps Required | 8 | GaugeTarget |
| Power-Up Duration | 8s (2x) | GaugeTarget |
| Base Tap Points | 100 | GameManager |
| Wrong Tap Penalty | -75 | GameManager |
| Combo Milestone | Every x5 | GameManager |
| Tap Forgiveness Radius | 0.42 units | WrongTapDetector |
| Target Lifetime | 2s | Target |

---

## 📦 Imported Assets (Unity Asset Store)

- **Timofei Shukshin UI Sounds Pack** — UI SFX (taps, notifications)
- **Free Pack** — General audio
- **Scifi Loops Pack 1** — Background music (Supernova, Battlestations)
- **Simple Object Pooler** — Performance optimization (not yet integrated)
- **Yughues Free Metal Materials** — Future visual polish for targets
- **Skillz SDK** — Competition platform (not yet integrated into gameplay)

---

## 🛠️ Development Environment

- **Unity Editor**: Connected via MCP for Unity v10.1.0 (HTTP, port 8080)
- **IDE**: VS Code
- **Git**: GitHub (Calo0420/SKILLZ-TAP-RUSH)
- **Target**: Mobile (iOS/Android)
- **Rendering**: URP (Universal Render Pipeline)

---

## 📌 Next Actions (Priority Order)

1. Fix music autoplay bug (quick fix in GameSceneBootstrap or AudioManager)
2. Add floating score text (+100, +300 BONUS flying up)
3. Add countdown beeps (3-2-1)
4. Add timer pulse animation (scale/color when ≤10s)
5. Screen shake on wrong taps
6. Particle burst on correct taps
7. Integrate object pooler for chaos mode
8. Flashing timer during chaos
9. Screen glow during chaos

---

*This document is the single source of truth for Tap Rush project status.*
