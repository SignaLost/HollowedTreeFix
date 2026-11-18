# How do I build it from source?

## The plugin

```bash
dotnet build -c Release
```

Same arrangement as Valchitecture: the build finds Valheim and the `Default` Gale or r2modman profile, publicizes the game assemblies, and installs into `BepInEx/plugins/NullSignal-HollowedTreeFix`. Override a wrong guess in `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <GamePath>/path/to/steamapps/common/Valheim</GamePath>
    <BepInExPath>/path/to/profile/BepInEx</BepInExPath>
  </PropertyGroup>
</Project>
```

## The structure

The edited trees live in a Unity project outside this repository, exported from Valheim Cuisine with AssetRipper.

1. **Get Valheim Cuisine's source and bundle.** One command writes both under `reference/` (in `.gitignore`):
   ```bash
   profile=~/.local/share/com.kesomannen.gale/valheim/profiles/Default
   ilspycmd -p -o reference/vc-src "$profile/BepInEx/plugins/Xutz-ValheimCuisine/ValheimCuisine.dll"
   find reference/vc-src -iname '*valheimcuisine*' -size +10M
   ```
   The large file it finds is the asset bundle.
2. **Export it.** In AssetRipper, load that bundle together with the game's `valheim_Data` folder, so `Location`, `Container` and the other game components resolve, then export as a Unity project.
3. **Edit.** Open the project in a 6000.0 editor no newer than the game's and work on the `VC_HollowedTree1` and `VC_HollowedTree2` prefabs. Keep the prefab names and the material names: the mod swaps materials for the game's by name.
4. **Bundle.** Say where the project is in `Structure/build.local`:
   ```sh
   PROJECT=/path/to/the/exported/project
   # UNITY=/path/to/Hub/Editor/6000.0.75f1/Editor/Unity
   ```
   then, with the project closed in Unity:
   ```bash
   ./Structure/build-structure-bundle.sh
   ```
   Or build from the editor's **HollowedTreeFix > Build Structure Bundle** menu and run the script with `copy`. Either way the bundle lands in `Resources/hollowedtree`.
5. `dotnet build -c Release` to embed it.

## Source layout

| Folder | Contents |
|---|---|
| `patch/` | Every Harmony patch |
| `Resources/` | The structure bundle, built into the DLL |
| `Structure/` | The Unity script that bundles the edited trees; not compiled by `dotnet` |
| `Thunderstore/` | The package icon, copied into the output folder |
| `faq/` | These pages, copied into the output folder |
