# Project Context & AI Guidelines: Typing Legends

> **Note for AI Assistants**: Read this file thoroughly before analyzing, modifying, or creating code, assets, or scenes in this repository.

---

## 1. Project Overview

- **Project Name**: Typing Legends
- **Engine & Version**: Unity (C# / .NET) with Universal Render Pipeline (URP 2D)
- **Key Packages**: TextMesh Pro (TMP), Unity Input System, 2D Animation, 2D PSD/Sprite Importer
- **Genre**: 2D Educational / Action Typing Combat Game
- **Theme**: Mythical Fantasy inspired by Thai folklore & the Ramayana epic (featuring characters/bosses such as Yaksha soldiers, Yaksha maidens, Indrajit, sea serpents, etc.)

---

## 2. Repository & Folder Structure

```
typing-legends/
├── Assets/
│   ├── Animation/              # 2D character/monster animation clips and controllers
│   ├── Char1/                  # Character artwork & multi-layer PSD/PSB sprites for all 10 stages
│   │   ├── Lvl 1 Bear/         # Stage 1 Boss
│   │   ├── Lvl 2 Ostrich/      # Stage 2 Boss
│   │   ├── Lvl 3 Ghost/        # Stage 3 Boss
│   │   ├── Lvl 4 Sea Serpent/  # Stage 4 Boss
│   │   ├── Lvl 5 Giant Octopus/# Stage 5 Boss
│   │   ├── Lvl 6 Mermaid/      # Stage 6 Boss
│   │   ├── Lvl 7 Yaksha Soldier/ # Stage 7 Boss (Ramayana)
│   │   ├── Lvl 8 Yaksha Maid/  # Stage 8 Boss (Ramayana)
│   │   └── Lvl 9 Indrajit/     # Stage 9 Boss (Ramayana)
│   ├── Editor/                 # Custom Unity editor tools and extensions
│   ├── Picture/                # UI sprites, background art, materials, manual overlays
│   ├── Resources/
│   │   ├── Scenes/             # All game scenes (Level 1 - 10, MainMenu, LevelSelection, Options)
│   │   └── Wordbanks/          # ScriptableObject word datasets (e.g. Ramayana_TieredList_*.asset)
│   ├── Scripts/                # C# source code (see Section 3)
│   ├── Settings/               # URP pipeline assets, 2D renderer data, quality settings
│   ├── Sound/                  # BGM tracks and SFX clips
│   ├── Sprites/                # UI icons, hearts, items, consumables
│   └── TextMesh Pro/           # TMP font assets, glyph tables, and materials
├── Packages/                   # Unity Package Manager manifest and lockfiles
└── ProjectSettings/            # Unity project-level settings
```

---

## 3. Architecture & Code Modules (`Assets/Scripts/`)

### 3.1 Core Gameplay Loop & Typing Engine
- `Typer.cs`: Core controller for keystroke listening, character matching, color highlighting, word completion validation, score counting, and damage dispatch.
- `Timer.cs`: Round/word countdown timer that triggers timeouts if words are not typed in time.
- `PlayerHealth.cs` & `MonsterHealth.cs`: Manage HP states, invulnerability frames, and damage calculation.
- `HeartUI.cs`, `MonsterHeartUI.cs`, `MonsterPortraitUI.cs`: UI visualizers for player/monster HP and boss portraits.

### 3.2 AI & Adaptive Difficulty Systems
- `AI/DynamicPacingAI.cs`: Dynamically scales monster attack frequency and tempo based on player typing performance.
- `AI/TypeMasterAI.cs` & `AI/TypeMasterAIDifficultySettings.cs`: High-level AI controller configuring parameters across difficulty presets.
- `AI/TypingStrategyProfiler.cs`: Telemetry profiler computing user metrics (WPM, accuracy, streak, error latency).
- `Wordbank/AdaptiveWordbankAI.cs` & `Wordbank/AIWordSelector.cs`: Chooses appropriate word difficulty tiers dynamically.
- `Wordbank/PlayerSkillState.cs` & `Wordbank/PlayerSkillPersistence.cs`: Saves and restores player skill progression across sessions.

### 3.3 Wordbank Architecture
- Individual level word banks: `Wordbank/Wordbank1.cs` through `Wordbank9.cs`.
- ScriptableObject tiered word database: `Wordbank/WordbankTieredList.cs` with assets loaded from `Resources/Wordbanks/` (Ramayana mythology vocabulary, English/Thai terms).

### 3.4 Items & Inventory System
- `Items/ItemHotkeys.cs`: Hotkey listening (1, 2, 3, etc.) for using active consumables in battle.
- `Items/TomyumShrimpItem.cs` & `Food.cs`: Consumable logic (healing player HP, granting time slow buffs, etc.).
- `RewardManager.cs`: Handles item drop rewards upon boss defeat.

### 3.5 Game Progression, UI & Navigation
- `GameOver/GameOverScreen*.cs`: Modular game-over screen with stat breakdowns, scores, accuracy, and actions (`GameOverScreen.Actions.cs`, `GameOverScreen.Points.cs`, `GameOverScreen.Wiring.cs`).
- `GameOver/GameWinScreen.cs` & `ScoreKeeper.cs`: Victory screen logic and cumulative score tracking.
- `LevelSelection.cs` & `SingleLevel.cs`: Stage select map with unlock progression.
- `เกี่ยวกับGoToอยู่นี้ทั้งหมด/`: Scene loading triggers (`GoToLevel1` – `GoToLevel10`, `GoToMenu`, `GoToOptions`, `GoToLevelSelection`). Note: Keep folder name intact as scenes and scripts reference this structure.
- `PauseOverlay.cs` & `HowToPlayOverlay.cs`: Pause menus and game manual modal popups.

### 3.6 Audio & Settings
- `Music & Sfx/Music Manager.cs` & `Music & Sfx/SceneMusicController.cs`: Background music persistence and track switching.
- `Music & Sfx/SfxPlayer.cs` & `Music & Sfx/GameEndSfx.cs`: SFX playback for typing sounds, damage, victory, and defeat.
- `Volume Setting.cs`, `Sfx Setting.cs`, `Brightness Setting.cs`, `Resolution.cs`: Audio and video preferences.

### 3.7 Analytics & Telemetry
- `Analytics/RawTypingEventLogger.cs`: Records granular typing timestamps, keystroke deltas, errors, and session metadata.

---

## 4. Coding & Modification Guidelines for AI

1. **Unity Asset Meta Files (`.meta`)**:
   - Whenever creating, renaming, moving, or deleting files in `Assets/`, remember that Unity tracks them via associated `.meta` files (containing GUIDs).
   - Never delete `.meta` files without deleting the corresponding asset, and do not introduce duplicate GUIDs.

2. **Encoding & Thai Language Support**:
   - The game supports Thai and English text (including Thai characters with tone marks and vowels).
   - Always ensure files containing Thai strings are encoded in **UTF-8 with or without BOM** to avoid character corruption.
   - When rendering text in UI, always use TextMesh Pro components configured with fonts that support Thai glyphs.

3. **Input Handling**:
   - Keystroke reading is primarily managed in `Typer.cs` using both `Input.inputString` / Unity's Input System for alphanumeric typing and hotkey listeners in `ItemHotkeys.cs`.
   - Take care not to consume typing keys when typing hotkeys, and vice versa.

4. **Scene & Component References**:
   - When editing scripts with serialized fields (`[SerializeField]`), preserve field names or use `[FormerlySerializedAs("oldName")]` to prevent losing references in `.unity` scene or `.prefab` files.

5. **Directory Naming**:
   - Preserve existing naming conventions, including Thai-named folders such as `Assets/Scripts/เกี่ยวกับGoToอยู่นี้ทั้งหมด/`.
