# TAP RUSH — Project Status

> **Last Updated:** 2026-07-27  
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
│   ├── NeonTargetFX.cs         — Pure 2D neon visuals (glow rings, particles, pulse)
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
- Shrink-over-lifetime urgency (45% floor)
- Gauge power-up (cyan, 8 taps → 2x for 8s)
- Win/Lose screen with Play Again / Main Menu
- Pixel-perfect tap accuracy (0.01 radius)
- Multi-target spawning (2-3 per tick, 13 initial burst)
- Full-screen dynamic spawn bounds (camera-based)
- Gauge cancels on wrong tap + missed target penalty

### Phase 3: Juice & Polish 🔥 IN PROGRESS

| Feature | Status | Notes |
|---------|--------|-------|
| Neon 2D visuals | ✅ Done | NeonTargetFX: glow rings, color-coded, pulse animation |
| Particle bursts on tap | ✅ Done | 16-24 particles, color-matched, detach on destroy |
| URP Bloom post-processing | ✅ Done | Global volume, intensity 2.5, threshold 0.6 |
| Dark background | ✅ Done | Near-black (0.02, 0.02, 0.06) |
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
| Timer pulse when low | ❌ TODO | Scale/color at ≤10s |
| Better fonts | ❌ TODO | Replace default TMP |
| Button animations | ❌ TODO | Press/hover feedback |
| Score animations | ❌ TODO | Counter roll-up |
| Animated background | ❌ TODO | Subtle floating particles/stars |

### Phase 4: Chaos Finale 🔥 MOSTLY DONE

| Feature | Status | Notes |
|---------|--------|-------|
| "OH GOD, HERE WE GO!" text | ✅ Done | Big red pulsing text |
| Slow-mo intro (0.5s) | ✅ Done | Time.timeScale=0.2 |
| White flash | ✅ Done | Full-screen fade |
| Red flood (2-4 per wave) | ✅ Done | 55% red chance (rebalanced) |
| Green targets in chaos | ✅ Done | 75% green chance |
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
| 1 | Music doesn't auto-play at game start | Medium | ✅ Fixed (clipChanged tracking) |
| 2 | Combo resets from expired targets in multi-mode | Medium | ✅ Fixed (miss no longer penalizes) |
| 3 | Object pooling not used — GC spikes during chaos mode on mobile | Medium | Open |
| 4 | Square default particles (should be round soft sprites) | Low | Open |

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
| Base Spawn Interval | 0.30s | TargetSpawner |
| Min Spawn Interval | 0.35s | TargetSpawner |
| Chaos Spawn Floor | 0.08s | TargetSpawner |
| Targets Per Tick | 2-3 normal | TargetSpawner |
| Initial Burst | 10 normal + 3 decoy | TargetSpawner |
| Decoy Chance | 30% | TargetSpawner |
| Bonus Chance | 8% | TargetSpawner |
| Chaos Red Chance | 55% | TargetSpawner |
| Chaos Green Chance | 75% | TargetSpawner |
| Chaos Red Per Wave | 2-4 | TargetSpawner |
| Gauge Spawn Every | 22s | TargetSpawner |
| Gauge Taps Required | 8 | GaugeTarget |
| Power-Up Duration | 8s (2x) | GaugeTarget |
| Base Tap Points | 100 | GameManager |
| Wrong Tap Penalty | -75 | GameManager |
| Combo Milestone | Every x5 | GameManager |
| Combo 1.2x | At 5 hits | GameManager |
| Combo 1.5x | At 10 hits | GameManager |
| Tap Accuracy Radius | 0.01 units (pixel-perfect) | WrongTapDetector |
| Target Lifetime | 2s | Target |
| Shrink Floor | 45% of original | Target |

---

## 📦 Imported Assets (Unity Asset Store)

- **Timofei Shukshin UI Sounds Pack** — UI SFX (taps, notifications)
- **Free Pack** — General audio
- **Scifi Loops Pack 1** — Background music (Supernova, Battlestations)
- **Simple Object Pooler** — Performance optimization (not yet integrated)
- **Yughues Free Metal Materials** — Imported but not used (3D materials don't work in 2D)
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

1. Add floating score text (+100, +300 BONUS flying up)
2. Add countdown beeps (3-2-1)
3. Add timer pulse animation (scale/color when ≤10s)
4. Screen shake on wrong taps
5. Round soft particle sprites (replace square defaults)
6. Animated background (subtle floating particles/stars)
7. Integrate object pooler for chaos mode
8. Flashing timer during chaos
9. Screen glow during chaos
10. Skillz SDK integration

---

*This document is the single source of truth for Tap Rush project status.*
