#!/bin/sh
# Builds LokisChair.dll against a local Valheim install.
# Usage: ./build.sh [--install] [ValheimDir]   (default: ~/.local/share/Steam/steamapps/common/Valheim)
# --install copies the DLL to <ValheimDir>/BepInEx/plugins/LokisChair/.
# Needs a .NET SDK (6+). The game's Managed folder and BepInEx/core must exist in ValheimDir.
set -e
INSTALL=
if [ "$1" = "--install" ]; then INSTALL=1; shift; fi
VALHEIM="${1:-$HOME/.local/share/Steam/steamapps/common/Valheim}"
cd "$(dirname "$0")/LokisChair"
dotnet build -c Release -p:Valheim="$VALHEIM"
DLL="$(pwd)/bin/Release/net472/LokisChair.dll"
echo "Output: $DLL"
if [ -n "$INSTALL" ]; then
  mkdir -p "$VALHEIM/BepInEx/plugins/LokisChair"
  cp "$DLL" "$VALHEIM/BepInEx/plugins/LokisChair/"
  echo "Installed to $VALHEIM/BepInEx/plugins/LokisChair/"
fi
