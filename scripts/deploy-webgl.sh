#!/usr/bin/env bash
# Publishes the WebGL build sitting in the cannons-build deploy repo
# (E:\Users\Alejandro\Opal\Builds\Cannons — produced by the Editor's
# Dev > Build WebGL menu, see Assets/Editor/WebGLBuildScript.cs).
#
# Files over 95MB can't go in git (GitHub's 100MB hard limit) — those get
# uploaded as GitHub Release assets on cannons-build instead, and
# index.html's dataUrl/frameworkUrl/codeUrl get pointed at the release
# download URL.
#
# Usage: scripts/deploy-webgl.sh
# (run after Dev > Build WebGL; commits + pushes cannons-build when done)

set -euo pipefail

BUILD_REPO="/e/Users/Alejandro/Opal/Builds/Cannons"
RELEASE_TAG="webgl-build-assets"
SIZE_LIMIT=$((95 * 1024 * 1024))

[[ -f "$BUILD_REPO/index.html" && -d "$BUILD_REPO/Build" ]] || {
  echo "error: $BUILD_REPO doesn't look like a built WebGL output (missing index.html or Build/)" >&2
  echo "Run Dev > Build WebGL in the Unity Editor first." >&2
  exit 1
}

cd "$BUILD_REPO"

gh release view "$RELEASE_TAG" --repo alejandroZumbado/cannons-build >/dev/null 2>&1 || \
  gh release create "$RELEASE_TAG" --repo alejandroZumbado/cannons-build \
    --title "WebGL build assets (oversized for git)" \
    --notes "Files here are referenced by index.html when they exceed GitHub's 100MB per-file push limit." >/dev/null

for f in "$BUILD_REPO"/Build/*; do
  size=$(stat -c%s "$f" 2>/dev/null || stat -f%z "$f")
  name=$(basename "$f")
  if (( size > SIZE_LIMIT )); then
    echo "Offloading $name (${size} bytes) to release '$RELEASE_TAG'"
    gh release upload "$RELEASE_TAG" "$f" --repo alejandroZumbado/cannons-build --clobber
    url=$(gh release view "$RELEASE_TAG" --repo alejandroZumbado/cannons-build --json assets \
      --jq ".assets[] | select(.name==\"$name\") | .url")
    escaped_name=$(printf '%s' "$name" | sed 's/[&/\]/\\&/g')
    sed -i "s|buildUrl + \"/${escaped_name}\"|\"${url}\"|g" "$BUILD_REPO/index.html"
    rm "$f"
  fi
done

git add -A
if git diff --cached --quiet; then
  echo "Nothing changed, nothing to push."
  exit 0
fi
git commit -m "Rebuild WebGL - $(date -u '+%Y-%m-%d %H:%M UTC')"
git push origin main
echo "Published: https://alejandrozumbado.github.io/cannons-build/"
