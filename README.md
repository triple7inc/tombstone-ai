# Tombstone AI

Tombstone AI is a modular MelonLoader mod for Tombstone MMO. It provides a real-time bot scripting framework that allows players to create, manage, and run custom **bot scripts** in-game via a UI-based interface.

## 🔧 Features

- UI with hotkey toggle (F10)
- Modular bot script loading system
- Built-in tools (banking, dropping items, enemy targeting, etc.)
- HUD for player/bot status
- Debug tools and automatic relogin
- Auto-resume active bots after relog
- Bot restart, state sync, and error reporting

## 📦 Requirements

- Tombstone MMO (latest version)
- **MelonLoader Nightly v0.7.1+** (Required for Il2Cpp & UI support)  
  [Download Nightly Builds](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.0)

## 📁 File Structure

```
/Library
  ├── Bank.cs
  ├── BotGUI.cs
  ├── Datastub.cs
  ├── DNID.cs
  ├── Doopstrap.cs
  ├── DoopstrapV1.cs
  ├── Equipment.cs
  ├── IBotScript.cs
  ├── IBotScriptCombat.cs
  ├── Inventory.cs
  ├── Logger.cs
  ├── Player.cs
  ├── Position.cs
  ├── Progression.cs
  ├── Timer.cs
  ├── Utilities.cs
  └── World.cs
/Scripts
  ├── BlankScript.cs
  ├── BlankScriptCombat.cs
  ├── ...
  └── YourCustomBot.cs
/Tests
  └── Hooks.cs
```

## 🚀 Installation

1. Install **MelonLoader Nightly v0.7.1 or later**.
2. Drop `TombstoneAI.dll` into your game’s `Mods` folder.
3. Launch the game.

## 🧠 Creating a Bot Script

1. Go to `Scripts/`.
2. Copy `BlankScript.cs` or `BlankScriptCombat.cs`.
3. Rename and modify the file for your bot logic.
4. Add your bot class name to the `bots` array in `TombstoneBot`:

```csharp
private protected string[] bots = new string[] {
    "StopAllBots",
    "PowerKillCombat",
    ...
    "YourBotClassName"
};
```

5. Compile your new script into the main mod if required, or inject it as a separate DLL if built independently and following the interface.

### Available Interfaces

All bots must implement `BotScriptBase`. For combat logic, implement `CombatBotScriptBase`.
For tool logic, implement `ToolScriptBase`.

---

## 🧰 Usage

### UI Controls
- **F10**: Toggle AI HUD
- **F9**: Toggle debug panel

### Built-in Tools

- Walk to bank
- Drop inventory
- Tree cutting, shaft making, power fishing
- Combat grinding
- And more...

---

## 🔁 Auto Relog

If your session expires or disconnects, Tombstone AI will:
- Detect relog screen
- Auto-fill your username
- Re-login and auto-press Play
- Resume the last active bot script with previous progression state

> Make sure all logged-in accounts have the same password for this to function properly!

---

## 💬 Contributing

PRs welcome for bugfixes, new bots, or improvements. Use `BlankScript` and `BlankScriptCombat` as starting points. Keep bot logic modular.

---

## 🧪 Proof-Of-Concept

Tombstone AI is already fully functional as a proof-of-concept and actively used. All included bot scripts are built on the same extensible interface system, and the HUD allows managing them live in-game.

Below is a screenshot of the AI in action:

![Powerfisher](./powerfish.gif)

You can create your own bot script, add it to the list, and immediately run it in-game.

The system supports tools, combat bots, skilling bots, and any logic you can script through the game's exposed Il2Cpp API.

---

## 📄 License

MIT License

Copyright (c) 2025 triple7inc

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
