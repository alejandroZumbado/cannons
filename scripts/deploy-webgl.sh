#!/usr/bin/env bash
# Publishes a Unity WebGL build to docs/ (GitHub Pages source).
# Files over 95MB can't go in git (GitHub's 100MB hard limit) — those get
# uploaded as GitHub Release assets instead, and index.html's dataUrl/
# frameworkUrl/codeUrl get pointed at the release download URL.
#
# Usage: scripts/deploy-webgl.sh <path-to-unity-webgl-build-output>
# The source folder must contain index.html, Build/, TemplateData/
# (i.e. what Unity's File > Build Settings > WebGL > Build produces).

set -euo pipefail

SRC="${1:?Usage: deploy-webgl.sh <path-to-webgl-build-output>}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOCS="$REPO_ROOT/docs"
RELEASE_TAG="webgl-build-assets"
SIZE_LIMIT=$((95 * 1024 * 1024))

[[ -f "$SRC/index.html" && -d "$SRC/Build" ]] || {
  echo "error: $SRC doesn't look like a Unity WebGL build (missing index.html or Build/)" >&2
  exit 1
}

echo "Syncing $SRC -> $DOCS"
rm -rf "$DOCS/Build" "$DOCS/TemplateData"
cp -r "$SRC/Build" "$SRC/TemplateData" "$DOCS/"
cp "$SRC/index.html" "$DOCS/index.html"

gh release view "$RELEASE_TAG" >/dev/null 2>&1 || \
  gh release create "$RELEASE_TAG" --title "WebGL build assets (oversized for git)" \
    --notes "Files here are referenced by docs/index.html when they exceed GitHub's 100MB per-file push limit." >/dev/null

for f in "$DOCS"/Build/*; do
  size=$(stat -c%s "$f" 2>/dev/null || stat -f%z "$f")
  name=$(basename "$f")
  if (( size > SIZE_LIMIT )); then
    echo "Offloading $name (${size} bytes) to release '$RELEASE_TAG'"
    gh release upload "$RELEASE_TAG" "$f" --clobber
    url=$(gh release view "$RELEASE_TAG" --json assets \
      --jq ".assets[] | select(.name==\"$name\") | .url")
    escaped_name=$(printf '%s' "$name" | sed 's/[&/\]/\\&/g')
    sed -i "s|buildUrl + \"/${escaped_name}\"|\"${url}\"|g" "$DOCS/index.html"
    rm "$f"
  fi
done

echo "Done. Review docs/index.html, then commit + push to publish."
