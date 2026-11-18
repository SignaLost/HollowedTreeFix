#!/usr/bin/env bash
# Bundles the edited Hollowed Tree prefabs from the Unity project and installs the bundle
# under Resources/, where the mod build embeds it.
#
#   ./build-structure-bundle.sh         build headless, then install (close the project in Unity first)
#   ./build-structure-bundle.sh copy    install a bundle already built from the editor's
#                                       HollowedTreeFix > Build Structure Bundle menu
#
# Reads UNITY and PROJECT from build.local beside this script (not shipped), or from the
# environment. PROJECT is the Unity project AssetRipper exported from Valheim Cuisine's bundle.

set -u

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo="$(cd "$here/.." && pwd)"

[ -f "$here/build.local" ] && . "$here/build.local"

if [ -z "${PROJECT:-}" ] || [ ! -d "$PROJECT/Assets" ]; then
    echo "No Unity project. Put PROJECT=/path/to/the/exported/project in $here/build.local."
    exit 1
fi

# The editor script lives in the repository; the project gets a copy each time.
mkdir -p "$PROJECT/Assets/Editor/HollowedTreeFix" || exit 1
cp "$here/Editor/BuildHollowedTreeBundle.cs" "$PROJECT/Assets/Editor/HollowedTreeFix/" || exit 1

if [ "${1:-}" != "copy" ]; then
    if [ -z "${UNITY:-}" ]; then
        # The game is on the 6000.0 stream; a bundle from a newer editor will not load.
        for candidate in "$HOME"/Unity/Hub/Editor/6000.0.*/Editor/Unity; do
            [ -x "$candidate" ] && UNITY="$candidate" && break
        done
    fi

    if [ -z "${UNITY:-}" ] || [ ! -x "$UNITY" ]; then
        echo "No Unity editor found. Put UNITY=/path/to/Editor/Unity in $here/build.local."
        exit 1
    fi

    echo "Editor:  $UNITY"
    echo "Project: $PROJECT"

    log="$PROJECT/structure-build.log"
    "$UNITY" -batchmode -quit -nographics -projectPath "$PROJECT" \
             -executeMethod BuildHollowedTreeBundle.Build -logFile "$log"
    status=$?
    grep -E "^Built |^Bundling |expected one prefab|produced nothing|Build Support" "$log"
    [ $status -eq 0 ] || { echo "Unity exited with $status; see $log"; exit 1; }
fi

bundle="$PROJECT/StructureBundles/hollowedtree"
[ -s "$bundle" ] || { echo "No bundle at $bundle."; exit 1; }

mkdir -p "$repo/Resources" || exit 1
cp "$bundle" "$repo/Resources/hollowedtree" || exit 1
echo "Installed Resources/hollowedtree ($(du -h "$bundle" | cut -f1)). Run dotnet build to embed it."
