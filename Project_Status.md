# TAP RUSH — Project Status

> **Last Updated:** 2026-08-18 (Copilot, via Unity Editor MCP — polish pass: fixed the Orbitron font asset and applied it across all screens, removed dead Simple Object Pooler asset, fixed placeholder text bug in GameScene HUD)
> **THIS IS THE CANONICAL ROADMAP.** TAP-RUSH-ROADMAP.md is now a redirect stub — this file is the single source of truth going forward.
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

### Phase 3: Juice & Polish ✅ COMPLETE

| Feature | Status | Notes |
|---------|--------|-------|
| Neon 2D visuals | ✅ Done | NeonTargetFX: glow rings, color-coded, pulse animation |
| Particle bursts on tap | ✅ Done | 16-24 soft round particles, color-matched, detach on destroy |
| URP Bloom post-processing | ✅ Done | Combo-reactive bloom (ScreenFX), chromatic aberration on wrong tap |
| Dark background | ✅ Done | ReactiveBackground: shifts with combo/chaos |
| Dynamic music system | ✅ Done | Crossfade, combo pitch ramp, tap ducking, musical pentatonic taps |
| Music (normal) | ✅ Done | WS Data Breach (Wirescapes cyberpunk) |
| Music (chaos) | ✅ Done | WS Urban Decay + SciFi ForceField chaos cue |
| All SFX | ✅ Done | Correct/wrong/combo/gauge/countdown with pitch scaling |
| Camera shake | ✅ Done | Combo milestones + wrong taps |
| Combo text animation | ✅ Done | Pulse + gold color |
| Countdown beeps (3-2-1) | ✅ Done | Rising pitch for tension |
| Floating +points text | ✅ Done | Scale punch + horizontal drift |
| Timer pulse when low | ✅ Done | Scale/color pulse ≤10s |
| Button animations | ✅ Done | Subtle pulse on menu buttons |
| Score animations | ✅ Done | ScorePunchFX on hit, count-up on end screen |
| Animated background | ✅ Done | AmbientParticles: neon dust intensifies with combo |
| Tap ripple shockwave | ✅ Done | Color-coded expanding ring on every tap |
| Target spawn animation | ✅ Done | Elastic pop-in with wobble |
| Combo streak bar | ✅ Done | Screen-bottom glow bar, color ramps with combo |
| Reyeso Studio intro | ✅ Done | Video splash scene with fade + skip support |
| Better fonts | ✅ Done | Orbitron SDF applied project-wide (TMP Settings default + explicit overrides on Main Menu/HUD/EndScreen scene text). See Known Bugs #5 for the root cause of the earlier revert. |

### Phase 4: Chaos Finale ✅ COMPLETE

| Feature | Status | Notes |
|---------|--------|-------|
| "OH GOD, HERE WE GO!" text | ✅ Done | Big red pulsing text |
| Slow-mo intro (0.5s) | ✅ Done | Time.timeScale=0.2 |
| White flash | ✅ Done | Full-screen fade |
| Red flood (2-4 per wave) | ✅ Done | 55% red chance (rebalanced) |
| Green targets in chaos | ✅ Done | 75% green chance |
| Faster spawn (0.08s floor) | ✅ Done | Multi-target enabled |
| Music switch + pitch up | ✅ Done | 1.2x pitch + crossfade |
| Flashing timer | ✅ Done | Rapid red/white alternation during chaos |
| Screen glow effect | ✅ Done | ScreenFX: vignette + bloom surge |
| Chaos particles | ✅ Done | Ambient particles intensify |

### Phase 4.5: Deterministic/Seeded RNG 🔒 CRITICAL — ✅ VERIFIED PASS (2026-08-02)

> **Phase 5 is now unblocked.**
> Implemented by Clue (VPS), verified by Copilot CLI in the Unity Editor via replay test.
> Test: forced seed=12345, ran two independent Play-mode sessions, compared the full
> `[TapRush][ReplayTest]` spawn log for each via the Editor.log file. **31 overlapping
> spawn entries, zero diffs** — identical kind/position/size/drift on every entry, e.g.:
> `#1 kind=normal pos=(-6.8756,-3.7826) size=1.1973 drift=(-1.0420,-0.0731)` matched exactly
> across both runs. Debug fields (`debugForceSeed`, `debugLogSpawnSequence`) reset to
> off/0 afterward, scene saved.

### Phase 5: Skillz Integration 🏆 WIRED — READY FOR TESTING

| Feature | Status | Notes |
|---------|--------|-------|
| SkillzManager in MainMenu | ✅ Done | DontDestroyOnLoad, scene fields configured |
| SkillzMatchController | ✅ Done | Bridges match lifecycle: seed, score, abort |
| Skillz seed → GameSessionData | ✅ Done | OnMatchWillBegin extracts Skillz Random seed |
| Score submission | ✅ Done | SubmitScore + DisplayTournamentResultsWithScore |
| MainMenu Practice button | ✅ Done | Plays without Skillz |
| MainMenu Compete button | ✅ Done | LaunchSkillz() into tournament UI |
| EndScreen Skillz flow | ✅ Done | Returns to Skillz results in tournament mode |
| SDK already imported | ✅ Done | Needs device testing (Editor uses SIDEkick) |

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
| 3 | Object pooling not used — GC spikes during chaos mode on mobile | Medium | ✅ Fixed (TargetPool.cs, commit 464fd4c) |
| 4 | Square default particles (should be round soft sprites) | Low | ✅ Fixed (procedural soft round texture) |
| 5 | Orbitron SDF font asset was hand-written YAML, not built via Font Asset Creator — `m_Material: {fileID: 0}` (null), Dynamic atlas mode with an empty character/glyph table, `m_ClearDynamicDataOnBuild: 1`. This is almost certainly why setting it as the TMP default broke text on-device (commit b758da6, 2026-08-08) | High | ✅ Fixed (2026-08-18) — rebuilt in place via `TMP_FontAsset.CreateFontAsset` + `TryAddCharacters` over the full character set actually used in-game (printable ASCII + em dash), baked as **Static** atlas mode with a real material/atlas texture, `m_ClearDynamicDataOnBuild: 0`. GUID preserved. Verified visually in Play mode (Android target) on Main Menu, HUD, and End Screen — clean rendering, no missing glyphs. Still worth a real on-device pass before the next cert build. |
| 6 | GameScene HUD's `ScoreText`/`TimerText` had leftover placeholder content with stray literal quote marks (`"Score: 0"`, `"60"`) baked into the scene instead of clean values. Runtime code overwrites these correctly during actual gameplay, but the raw placeholder is briefly visible during the `LoadingScreen` "Loading game..." transition since that overlay didn't appear to fully obscure the HUD in testing (unconfirmed root cause — worth a closer look if it recurs) | Low | ✅ Fixed (2026-08-18) — placeholder text corrected to `0` / `60` to match what the code actually sets |

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

1. ~~Floating score text~~ ✅ DONE
2. ~~Countdown beeps~~ ✅ DONE
3. ~~Timer pulse animation~~ ✅ DONE
4. ~~Screen shake on wrong taps~~ ✅ DONE
5. ~~Round soft particle sprites~~ ✅ DONE
6. ~~Animated background~~ ✅ DONE
7. ~~Integrate object pooler for chaos mode GC performance~~ ✅ DONE (TargetPool.cs, commit 464fd4c)
8. ~~Flashing timer during chaos~~ ✅ DONE
9. ~~Screen glow during chaos~~ ✅ DONE
10. ~~Deterministic/Seeded RNG (Phase 4.5)~~ ✅ DONE
11. ~~Skillz SDK integration~~ ✅ WIRED — needs device testing
12. Device build + Skillz SIDEkick testing — release keystore wired + verified (464fd4c), APK release-signed, ready to test
13. ~~Better fonts~~ ✅ DONE (2026-08-18) — see Known Bugs #5
14. ~~Mid-match backgrounding/abort handling~~ ✅ DONE — `GameManager.OnApplicationPause()` added per official Skillz Unity docs (freezes `Time.timeScale` on background, resumes on return). Deliberately does NOT call `AbortMatch()` — confirmed via https://docs.skillz.com/docs/aborts that Skillz's SDK auto-detects/categorizes backgrounded/terminated/timeout/crash aborts server-side; calling AbortMatch() ourselves for routine backgrounding would inflate the game's real abort-rate metric. `AbortMatch()` reserved for a possible future in-game "forfeit" button (which per the same doc should submit a real score, not an abort).
15. ~~Loading scene / SDK-handoff state cleanup~~ ✅ DONE — Copilot built `LoadingScreen.cs` (code-based overlay, not a separate Unity scene) covering the `LaunchSkillz()` handoff. Clue closed the remaining gap on the score-submission handoff (`ReportScore`/`SubmitScore` → `DisplayTournamentResultsWithScore`), which had no loading transition (commit 8e62a79). Both Skillz SDK handoff points now covered per https://docs.skillz.com/docs/launch-skillz-ui.
16. ~~Remove unused Simple Object Pooler asset~~ ✅ DONE (2026-08-18) — confirmed zero references anywhere in the project (grepped all 4 core script GUIDs against every scene/prefab) before removing; `TargetPool.cs` (Known Bug #3) is the real, in-use pooling implementation. Cut ~1.7MB of dead weight.
17. APK size / cold-start sanity check — **not verified this pass.** Every existing build in `Builds/` (local, gitignored) predates the Firebase/FCM commit (0a559d8, 2026-08-09 22:08); most recent is `TapRush_Production.apk` at 135MB from earlier the same day. Needs a fresh Android build *after* Firebase to get real numbers — deliberately not built here since Oscar is mid-flight on FCM testing separately and a rebuild would package his in-progress state.
18. Gauge power-up first-time clarity — soft flag, not acted on. `GaugeTarget.cs` already has decent progressive feedback (color lerp toward white per tap, "CHARGE! x/8" counter, completion sound, "2x POWER!" text), so it likely reads fine without a tutorial, but this is a judgment call for Oscar to make by actually watching a first-time player rather than from code alone.

---

## 🔒 Skillz Abort/Backgrounding Policy (added 2026-08-08)

- **Do NOT call `SkillzCrossPlatform.AbortMatch()`** for crashes, backgrounding, or force-quits — Skillz's SDK handles and categorizes these automatically server-side (Backgrounded / Terminated / Timeout / Unintentional Crash). Calling it ourselves adds noise to the abort-rate stability metric Skillz tracks during cert review.
- **`GameManager.OnApplicationPause(bool)`** freezes `Time.timeScale` to 0 while backgrounded mid-match, resumes to 1 on return — this is the Unity-recommended defensive pattern (https://docs.skillz.com/docs/unity-script-execution/), not an abort mechanism.
- Score already floors at 0 (`GameManager.RegisterWrongTap`), matching Skillz's guidance that a forfeiting/interrupted player should never show a negative or exploitable score.
- If an in-game "forfeit"/"quit match" button is ever added, it should submit the player's real current score (or 0 by design), NOT call `AbortMatch()` — per https://docs.skillz.com/docs/aborts, an abort should never be a way to dodge a worse score.
- Source: official Skillz docs, verified 2026-08-08 — https://docs.skillz.com/docs/aborts and https://docs.skillz.com/docs/unity-script-execution/

---

*This document is the single source of truth for Tap Rush project status.*
