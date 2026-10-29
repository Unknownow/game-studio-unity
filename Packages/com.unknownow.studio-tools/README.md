# Game Studio Tools

Editor tools for projects made from the `game-studio-unity` template. They need Unity 6000.3 or newer.

## Install

The template embeds this package. A game made from the template runs `python tools/studio.py setup`, which replaces the embedded copy with a git dependency:

```
https://github.com/Unknownow/game-studio-unity.git?path=/Packages/com.unknownow.studio-tools#v0.1.0
```

To add it by hand, put that URL (with the release tag you want after `#`) in `Packages/manifest.json` under `dependencies`, and add the package name to `testables` if you want its tests to run in your project.

## Menu

| Menu item | What it does |
|---|---|
| `Tools/Game Studio/Capture Evidence` | Saves a screenshot of the open scene to `production/qa/evidence/<scene>/`. In Play Mode it captures the Game view. |
| `Tools/Game Studio/Run All Tests` | Runs the EditMode and PlayMode tests and writes the result files. |
| `Tools/Game Studio/Missing Reference Audit` | Scans scenes and prefabs under `Assets/` for missing scripts and dangling references. |
| `Tools/Game Studio/Project Settings Check` | Checks Force Text serialization, Visible Meta Files and the Input System setting. |

## Batch methods

Every method ends the editor with exit code 0 (pass), 1 (findings) or 2 (bad argument or setup error). Reports go under `<project>/production/qa/`.

```
# Evidence capture. Needs a GPU: never pass -nographics.
Unity -batchmode -projectPath . -executeMethod Unknownow.StudioTools.Editor.EvidenceCapture.CaptureBatch -story story-001 -scene Assets/Scenes/Main.unity -width 1280 -height 720 -logFile -

# Tests. -testPlatform is EditMode, PlayMode or All. Do not pass -quit.
Unity -batchmode -nographics -projectPath . -executeMethod Unknownow.StudioTools.Editor.BatchTests.Run -testPlatform All -logFile -

# Audits.
Unity -batchmode -nographics -quit -projectPath . -executeMethod Unknownow.StudioTools.Editor.MissingReferenceAudit.RunBatch -logFile -
Unity -batchmode -nographics -quit -projectPath . -executeMethod Unknownow.StudioTools.Editor.ProjectSettingsCheck.RunBatch -logFile -
```

## Output files

- `production/qa/evidence/<slug>/<slug>-<yyyyMMdd-HHmmss>.png`
- `production/qa/test-results/editmode.xml` and `playmode.xml`
- `production/qa/audit/missing-references.json`
- `production/qa/audit/project-settings.json`

The editor cannot open a project that is already open in another editor. Close it first.
