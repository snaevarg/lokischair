# Loki's Chair

Loki changed shape whenever it suited him. Now your Viking can too: sit down at a **Barber Station**
and pick a new body type and skin tone alongside your hair and beard, no new character required.

> **Warning: early release, not thoroughly tested.** It has only had a short run on one setup.
> **Back up your characters before using it.** Steam Cloud saves are in
> `Steam/userdata/<id>/892970/remote/characters`; local saves are in the `characters*` folders
> under `AppData\LocalLow\IronGate\Valheim` (Windows) or `~/.config/unity3d/IronGate/Valheim`
> (Linux). Please report problems.

## Features

- Two new rows at the top of the Barber Station, in character-creation order:
  - **Body**: ◀ Male / Female ▶
  - **Skin Tone**: the same slider and range as at character creation
- Switching to Female removes the beard, as at character creation
- **Cancel** reverts body type and skin tone along with hair and beard
- Uses the game's own body type and skin settings, so they are saved with your character and other
  players see the change without the mod

## Installation

- **Mod manager (r2modman, Thunderstore):** install as usual.
- **Manual:** install [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/),
  then copy `LokisChair.dll` into `Valheim/BepInEx/plugins/`.

**Client-side only.** The server and other players do not need it, and it does not trigger mod
version checks. Removing it later is safe; your character keeps whichever body type and skin tone were last saved.

## Not tested yet

- Gamepad / Steam Deck controls (only tested with mouse)
- Equipped armour changing shape correctly right after a switch
- Other mods that change the Barber Station UI

Built and tested against Valheim l-1.0.16 with BepInExPack 5.4.2351.

## Changelog

- **0.2.0**: Skin Tone slider.
- **0.1.0**: First release.

## License

MIT. Source: https://github.com/snaevarg/lokischair
