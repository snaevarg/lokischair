#!/bin/sh
# Builds LokisChair and assembles a Thunderstore/Nexus zip in dist/.
# Usage: ./package.sh [ValheimDir]
# The version comes from LokisChair/Plugin.cs and is written into the packaged manifest.json.
set -e
cd "$(dirname "$0")"
./build.sh "$@"
VERSION=$(sed -n 's/.*const string Version = "\([0-9.]*\)".*/\1/p' LokisChair/Plugin.cs)
OUT="dist/LokisChair-$VERSION"
rm -rf "$OUT" "$OUT.zip"
mkdir -p "$OUT"
cp LokisChair/bin/Release/net472/LokisChair.dll package/README.md package/icon.png "$OUT/"
sed "s/\"version_number\": \"[^\"]*\"/\"version_number\": \"$VERSION\"/" package/manifest.json > "$OUT/manifest.json"
(cd "$OUT" && python3 -m zipfile -c "../LokisChair-$VERSION.zip" LokisChair.dll manifest.json README.md icon.png)
echo "Package: $(pwd)/$OUT.zip"
