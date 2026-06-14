# Estragon

A Godot 4.6 game project template demonstrating a fully working C# game with encrypted PCK builds via [GodotSecureAction](https://github.com/emabrey/GodotSecureAction).

This project exists to show the complete end-to-end toolchain: a playable game built on the [maaacks_game_template](https://github.com/Maaack/Godot-Game-Template) addon (fully converted to C#), compiled against a custom-built Godot editor with PCK encryption baked in, and built reproducibly in CI using GodotSecureAction.

---

## What this project is

Estragon is a template, not a game. It ships with:

- A main menu with animations and transitions
- Options menus (audio, video, input rebinding)
- A pause menu
- A loading screen with shader caching
- An opening splash sequence
- Three placeholder game levels with win/lose/level-select flow
- End credits scene
- Persistent settings and save state

All game logic and addon code is written in C#. No GDScript remains in the runtime path.

---

## PCK encryption

Estragon's export PCK files are encrypted. The encryption key is published intentionally — this is an example project whose purpose is to demonstrate the toolchain, not to protect proprietary assets.

**Do not do this with a real game.** See the [GodotSecureAction README](https://github.com/emabrey/GodotSecureAction) for the full warning and instructions on keeping your own key secret.

### Example encryption key

```
7740b801d92b9a201af5650dad9054f6d52de047992c44df5a633b9c0953a149
```

Enter this key in **Project → Export → Resources → Script encryption key** when using a GodotSecureAction-built editor to export or verify the project.

---

## Building the editor locally

Estragon vendors the Godot source as a submodule and provides a PowerShell build script. Requires Python, SCons, and the .NET SDK.

On first run the script prompts for the PCK encryption key and stores it locally in `godot.gdkey`. Enter the example key above, or generate your own with:

```sh
python -c "import secrets; print(secrets.token_hex(32))"
```

The script builds the editor and both export templates for the host platform and architecture, then produces the `GodotSharp` NuGet packages needed for C# support.

### Windows

`godot.gdkey` is stored as a Windows DPAPI-encrypted blob tied to your user account, so the key is never on disk in plaintext.

```powershell
./vendored_godot_build.ps1
```

### macOS

Install PowerShell via Homebrew, then install the build dependencies:

```sh
brew install powershell python scons
```

Also install MoltenVK (required for Vulkan support in the Godot editor on macOS):

```sh
brew install molten-vk
```

Then run the build script:

```sh
pwsh ./vendored_godot_build.ps1
```

On macOS `godot.gdkey` is stored using PowerShell's `ConvertFrom-SecureString` without DPAPI, which means the blob is encrypted with a machine-generated AES key stored alongside it. It is still not plaintext, but the protection is weaker than Windows DPAPI. Keep this in mind if your machine is shared.

### Linux

Install PowerShell and the build dependencies. On Debian/Ubuntu:

```sh
# PowerShell — follow the Microsoft install guide for your distro:
# https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-linux
sudo apt-get install python3 scons build-essential pkg-config \
  libx11-dev libxcursor-dev libxinerama-dev libgl1-mesa-dev \
  libgles2-mesa-dev libasound2-dev libpulse-dev libudev-dev \
  libxi-dev libxrandr-dev libwayland-dev
```

Then run the build script:

```sh
pwsh ./vendored_godot_build.ps1
```

The same `godot.gdkey` caveat as macOS applies on Linux.

---

## Project structure

```
estragon/                        Godot project root
  addons/maaacks_game_template/  Menu/UI framework (converted to C#)
    base/                        Core: menus, loading, music, windows, state
    extras/                      Level management, win/lose, scene listing
  scenes/                        Game scenes
    menus/                       Main menu, options, level select
    game_scene/                  Game UI, viewport, level container
    loading_screen/              Loading screens with shader caching
    opening/                     Splash/opening sequence
    windows/                     Pause, win, lose, game won overlays
    end_credits/                 Credits scene
  scripts/                       Shared game scripts (GameState, LevelState)
vendored/godot/                  Godot engine source (submodule)
vendored_godot_build.ps1         Developer build script
```

---

## Dependencies

- [maaacks_game_template](https://github.com/Maaack/Godot-Game-Template) — menu and UI framework, fully converted to C# for this project
- [GodotSecureAction](https://github.com/emabrey/GodotSecureAction) — CI action that builds a Godot editor with custom PCK encryption
- [Godot-Secure](https://github.com/emabrey/Godot-Secure) — the underlying patch applied to the Godot source

---

## License

Estragon is released under the [MIT License](LICENSE). Copyright (c) 2026 Emily Mabrey.

The bundled maaacks_game_template addon is under its own license. See [estragon/addons/maaacks_game_template/LICENSE.txt](estragon/addons/maaacks_game_template/LICENSE.txt).
