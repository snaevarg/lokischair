# Loki's Chair

![Loki's Chair](thumbnail.png)

Valheim BepInEx mod that adds **Body** (Male / Female) and **Skin Tone** rows to the Barber Station UI.

- Game: Valheim l-1.0.16 (network 40), Unity 6000.0.75f1, BepInExPack 5.4.2351
- Client-only. Not needed on the server or on other players' clients.

## Build

```sh
./build.sh            # build only
./build.sh --install  # build and copy into BepInEx/plugins/LokisChair/
./package.sh          # build and create dist/LokisChair-<version>.zip (Thunderstore layout)
```

Needs a .NET SDK (6+). Pass a Valheim path as the last argument if the game is not in the default
Steam location.

## Releasing

Bump `Version` in `LokisChair/Plugin.cs` (the single source; `package.sh` writes it into the
packaged `manifest.json`), add a changelog line to `package/README.md`, then run `./package.sh`.
`package/` holds the store-page README, `manifest.json` and the icon (`icon.svg` is the source for
`icon.png`: `rsvg-convert -w 256 -h 256 icon.svg -o icon.png`).

## How it works

The barber GUI is a second instance of `PlayerCustomizaton`, the same class as character creation,
so `SetPlayerModel(0|1)` already exists on it (it also clears the beard when switching to female).
The mod:

- clones the Beard row (label, arrows, value) into a Body row and the Hair Tone label + slider into
  a Skin Tone slider, both above Hair, then pushes the rows below down and grows the panel
  (`BodySection.cs`)
- maps the slider with the barber's own `m_skinColor0`/`m_skinColor1` (white to 0.3 grey, the same
  range as the character creator's `NewCharacterPanel`)
- snapshots model and skin colour when the barber opens and restores them on Cancel
  (`BarberPatches.cs`)

The model index and skin colour live in the player's ZDO (`ZDOVars.s_modelIndex`,
`ZDOVars.s_skinColor`) and in the character save, so vanilla already syncs them to other players
and persists them.

On first open the mod logs the barber GUI hierarchy at Debug level (search for
`Barber GUI hierarchy`). To see it, add `Debug` to `LogLevels` under `[Logging.Disk]` in
`BepInEx/config/BepInEx.cfg`; it then appears in `BepInEx/LogOutput.log`.
