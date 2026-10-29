---
engine: unity
---

# Engine layer: Unity 6.3 LTS

This file is the Unity side of the studio. Skills and roles stop at an "Engine step" and
follow the section of this file with the matching heading. Each section is a numbered
list for Unity 6.3 LTS (editor line `6000.3`). The pin lives in `engine.toml`; the
reference docs are in `engine-reference/unity/`. Read `engine-reference/unity/VERSION.md`
before you suggest a Unity API: the model's knowledge of Unity 6 is incomplete.

In the commands below, `<Unity editor>` is the editor executable. Use `engine.editor_path`
from `project.local.toml` if it is set. Otherwise use the Hub path for the version in
`ProjectSettings/ProjectVersion.txt` (`m_EditorVersion`):

- Windows: `C:/Program Files/Unity/Hub/Editor/<version>/Editor/Unity.exe`
- macOS: `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity`
- Linux: `~/Unity/Hub/Editor/<version>/Editor/Unity`

`Unity` on `PATH` may be Unity's separate command-line tool, not the editor. Never search
the disk for an editor.

The package `com.unknownow.studio-tools` (`Packages/com.unknownow.studio-tools/`) adds the
batch methods and the `Tools/Game Studio/*` menu items used below. Its exit codes are
0 (pass), 1 (findings) and 2 (bad argument or setup error).

## hook: setup-engine/version

1. Read the installed editor version for this project: the `m_EditorVersion` line of
   `ProjectSettings/ProjectVersion.txt`, if the file exists.
2. Otherwise list the editor folders under the Hub path above and pick the newest one
   whose name starts with `6000.3.`. Confirm it with `"<Unity editor>" -version` when the
   executable exists.
3. No editor found: treat the version as unknown, never as absent. Ask the user where the
   editor lives.
4. Keep the executable path you found. Section 5.5 writes it to `engine.editor_path` in
   `project.local.toml`.
5. The pin is Unity 6.3 LTS (`6000.3`). An editor on another line is a mismatch: offer to
   pin the installed one, pin 6.3 and upgrade the editor, or stop. Pinning another line
   changes `engine.toml`, `engine-reference/unity/VERSION.md` and the package's `unity`
   field together; see `setup-engine/upgrade`.

## hook: setup-engine/language

1. Unity has one primary language: C#. Ask nothing.
2. Record `C#` as the language.

## hook: setup-engine/stack

1. `- **Build System**: Unity Build Pipeline (BuildPipeline / Build Profiles)`
2. `- **Asset Pipeline**: Unity Asset Import Pipeline + Addressables`
3. Leave Addressables out of the line when the project does not use it.

## hook: setup-engine/naming

1. Classes and structs: PascalCase (`PlayerController`).
2. Public fields and properties: PascalCase (`MoveSpeed`).
3. Private fields: `_camelCase` (`_moveSpeed`).
4. Methods: PascalCase (`TakeDamage()`).
5. Files: PascalCase and the same name as the class (`PlayerController.cs`).
6. Constants: PascalCase or UPPER_SNAKE_CASE.
7. Events: PascalCase, `On<Event>` (`OnHealthChanged`).
8. Test classes: `[System]Tests`; test methods: `[Scenario]_[Expected]`.

## hook: setup-engine/testing

1. The framework is the Unity Test Framework (`com.unity.test-framework`) with NUnit.
2. The package `com.unknownow.studio-tools` depends on it, so it is already in the project.
3. EditMode tests live in `Assets/Tests/EditMode/`, PlayMode tests in `Assets/Tests/PlayMode/`,
   each with its own asmdef.
4. The runner is `python tools/studio.py test-run`, which runs the editor with the package's
   `BatchTests.Run` method. Ask before adding the framework to a project that lacks it.

## hook: setup-engine/specialists

1. Fill the Engine Specialists section like this:
   - **Primary**: `unity-specialist`
   - **Language/Code Specialist**: `unity-specialist` (C# review; the primary covers it)
   - **Shader Specialist**: `unity-shader-specialist` (Shader Graph, HLSL, URP/HDRP materials)
   - **UI Specialist**: `unity-ui-specialist` (UI Toolkit UXML/USS, UGUI Canvas, runtime UI)
   - **Additional Specialists**: `unity-dots-specialist` (ECS, Jobs system, Burst compiler),
     `unity-addressables-specialist` (asset loading, memory, content catalogs)
2. Routing notes: use the primary for architecture and general C# review, the DOTS specialist
   for any ECS, Jobs or Burst code, the shader specialist for rendering and visual effects,
   the UI specialist for all interface work, and the Addressables specialist for asset
   management.
3. File-extension routing:

   | File or type | Role |
   |---|---|
   | Game code (`.cs`) | `unity-specialist` |
   | Shader and material files (`.shader`, `.shadergraph`, `.mat`) | `unity-shader-specialist` |
   | UI and screen files (`.uxml`, `.uss`, Canvas prefabs) | `unity-ui-specialist` |
   | Scene, prefab and level files (`.unity`, `.prefab`) | `unity-specialist` |
   | Native extensions and plugins (`.dll`, native plugins) | `unity-specialist` |
   | General architecture review | `unity-specialist` |

## hook: setup-engine/commands

1. `engine.name` = `unity`. `engine.version` = the version from the version step (for
   example `6.3 LTS`). `engine.language` = `C#`.
2. `engine.rendering` = `URP` and `engine.physics` = `PhysX`. Unity has one row for both, so
   ask nothing about 2D or 3D. Say that `/settings` can change them.
3. `testing.command` is not needed: the Unity plugin supplies the test command. Leave it
   unset unless the user wants a different one.
4. `commands.smoke` = `"<Unity editor>" -batchmode -nographics -quit -projectPath . -logFile -`.
   It opens the project, compiles it and exits; exit 1 with `error CS` lines means a script
   does not compile.
5. `commands.run`: the player build, launched windowed
   (`<player> -screen-width 1280 -screen-height 720 -screen-fullscreen 0`). Leave it unset
   until the project has a build.
6. `commands.build`: ask the user for the target platform and write the value they give.
   `-buildTarget` alone only switches the active platform and builds nothing; the command
   also needs a player-build flag such as `-buildWindows64Player <output path>`. Confirm
   any other platform's flag against Unity's command-line documentation. Never write a
   `-buildTarget` value from memory, and leave the value unset when nobody can source it.
7. `engine.editor_path` goes to `project.local.toml` (use `--local`), because it differs
   per machine.

## hook: setup-engine/scaffold

1. The Unity project is already in the template: `Assets/`, `Packages/` and
   `ProjectSettings/` exist. Say so and skip the scaffold.
2. If `ProjectSettings/ProjectVersion.txt` is missing (a repo that was not made from the
   template), do not fabricate a project. Tell the user to create it with Unity Hub at the
   repository root, using the template that matches 2D or 3D, then to re-run `/setup-engine`.
3. Report the result either way: "already present" or "not created, the editor must create
   it".

## hook: setup-engine/upgrade

1. Install the new LTS editor with Unity Hub.
2. Update `engine.toml`: set `lts` and `editor_line` to the new release (for example
   `lts = "6.4"`, `editor_line = "6000.4"`) and `[defaults]` `"engine.version"`.
3. Update the `| **Engine Version** |` row of `engine-reference/unity/VERSION.md` to the new
   LTS, and refresh the rest of that directory from Unity's release notes.
4. Update the `unity` field of `Packages/com.unknownow.studio-tools/package.json` (or move
   the package dependency to a release that supports the new line).
5. Open the project in the new editor and commit the upgraded `ProjectSettings/` and
   `Packages/packages-lock.json`.
6. Run `python tools/studio.py check` and the EditMode suite
   (`python tools/studio.py test-run`).
7. For the pre-upgrade audit, also scan `Packages/manifest.json` for package versions the
   new editor no longer supports and `ProjectSettings/ProjectSettings.asset` for settings
   named in `engine-reference/unity/breaking-changes.md` and `deprecated-apis.md`.

## hook: dev-story/specialists

1. Code stories (`.cs` files): `unity-specialist`.
2. Shader and material stories: `unity-shader-specialist`.
3. UI stories (UI Toolkit, UGUI): `unity-ui-specialist`.
4. ECS, Jobs and Burst stories: `unity-dots-specialist`.
5. Asset loading and memory stories: `unity-addressables-specialist`.
6. Act as the role by reading `roles/<role>.md` first. Name no other specialist.

## hook: dev-story/engine-checks

1. The parse check is the compile: run
   `"<Unity editor>" -batchmode -nographics -quit -projectPath . -logFile -`
   (or `commands.smoke` when it is set). Exit 0 means every script compiled. Exit 1 with
   `error CS` lines means a script does not compile; report those lines.
2. The executable comes from `engine.editor_path` or the Hub path described at the top of
   this file. If it cannot run, report `parse NOT VERIFIED — engine binary not available`.
3. A running Editor holds the project: Unity cannot open the same project twice. If the
   Editor is open on this project, ask the user to close it, or to run the check from the
   Editor (Console shows compile errors).
4. Capture needs a Unity-side script only when you use the player path in
   `run-and-observe/capture`. The package's `EvidenceCapture` needs none.

## hook: security-audit/patterns

1. Category 1 (save and serialization): NOT SOURCEABLE. `engine-reference/unity/` has no
   save or serialization module, so do not write API names from memory. Report the
   category as NOT ASSESSED. To close it, add a save module under
   `engine-reference/unity/modules/` and take the names from it.
2. Category 2 (network and multiplayer): `ServerRpc`, `ClientRpc`, `NetworkVariable`,
   `NetworkObject`, `NetworkManager`, `NetworkBehaviour`, `IsServer`, `IsOwner`. These come
   from `engine-reference/unity/modules/networking.md`; check them against it and add what
   it documents beyond this floor.
3. Category 3 (input): `InputSystem`, `PlayerInput`, `ReadValue`, `InputValue`. These come
   from `engine-reference/unity/modules/input.md`.
4. Category 4 (data exposure) is engine-agnostic; its print call on Unity is `Debug.Log`.
5. Cite the reference file for every pattern you use in the report.

## hook: smoke-check/run-tests

1. Confirm the Unity Test Framework is installed:
   `grep -q 'com.unity.test-framework' Packages/manifest.json Packages/packages-lock.json`.
   A project without it hangs instead of failing. Absent: report
   `NOT ASSESSED — Unity Test Framework not installed`, and name the fix (add
   `com.unity.test-framework` to `Packages/manifest.json`).
2. Delete the old results first: `production/qa/test-results/editmode.xml` and
   `production/qa/test-results/playmode.xml`. A stale file must never be read as a new
   result.
3. Run `python tools/studio.py test-run`. It runs the editor with the package's
   `BatchTests.Run -testPlatform All` and writes both result files. For one platform, run
   the editor yourself with `-testPlatform EditMode` or `-testPlatform PlayMode`.
4. Exit 0 is a pass. Exit 1 is a test failure or a compile error (the editor log, which `test-run`
   keeps in its `log` row, has the `error CS` lines). Exit 2 is a bad
   argument or an editor that was not found. Anything else is a FAIL.
5. Read the `<test-run>` element of each results file for `testcasecount`, `passed` and
   `failed`. `testcasecount="0"` is NOT ASSESSED, never a pass: tests outside `Assets/` are
   never compiled and Unity then reports a pass over zero tests.
6. Report each platform on its own line: `EditMode: PASS (12/12)`,
   `PlayMode: NOT RUN — [reason]`. The EditMode result does not cover PlayMode tests.
7. The editor cannot run on a project that is open in another Editor. Ask the user to close it.

## hook: soak-test/memory-tool

1. Memory Profiler: `Window > Analysis > Memory Profiler` (the `com.unity.memoryprofiler`
   package). Record Total Reserved Memory, GC Allocated and the object count at each
   checkpoint, in the units as displayed.
2. Profiler: `Window > Analysis > Profiler`, Memory module, as a second source.
3. These names are pointers, not verified paths: `engine-reference/unity/` does not cover
   them. Confirm them in your own Editor at T+0 and record the tool as it is named there.
4. Alert on GC Allocated growing without leveling off across three or more checkpoints.
5. No memory tool found: nothing was measured, and NOT ASSESSED applies.

## hook: test-helpers/engine-helpers

1. Put the helpers in `Assets/Tests/EditMode/Helpers/` (in the EditMode assembly), as C# with
   NUnit's `Assert` only. Never invent an assertion API.
2. `GameAssertions.cs`: a static class with `AssertInRange(float value, float min, float max,
   string label = "value")` using `Assert.That(value, Is.InRange(min, max), message)`, and
   `AssertHasComponent<T>(GameObject obj)` using `Assert.IsNotNull(obj.GetComponent<T>(),
   message)`. For events, add `AssertEventRaised(ref bool wasCalled, System.Action action,
   string eventName)`.
3. `GameFactory.cs`: `MakeGameObject(string name = "TestObject")` and
   `MakeScriptableObject<T>() where T : ScriptableObject`. Tests destroy what they create with
   `Object.DestroyImmediate`.
4. Scene helper: a plain class (never a `[TestFixture]`) with a method that opens a scene with
   `EditorSceneManager.OpenScene` in EditMode, or `SceneManager.LoadSceneAsync` and a
   `yield return` in a `[UnityTest]`. It returns the root GameObjects.
5. The EditMode asmdef must reference the game's own assembly; see `test-setup/engine-files`.

## hook: test-setup/engine-files

1. Unity compiles only `Assets/` and `Packages/`. Tests go under `Assets/Tests/`: a test under
   `tests/` is never compiled and the run reports zero tests and a pass.
2. The Test Framework package comes from `com.unknownow.studio-tools`. Without it, add
   `com.unity.test-framework` to `Packages/manifest.json` at the version the editor bundles
   (read it from `Data/Resources/PackageManager/BuiltInPackages/com.unity.test-framework/package.json`
   next to the editor executable; do not copy a number).
3. `python tools/studio.py test-scaffold` writes these four files:
   - `Assets/Tests/EditMode/<Name>.Tests.EditMode.asmdef`
   - `Assets/Tests/EditMode/SampleEditModeTest.cs`
   - `Assets/Tests/PlayMode/<Name>.Tests.PlayMode.asmdef`
   - `Assets/Tests/PlayMode/SamplePlayModeTest.cs`

   Both asmdefs reference `UnityEngine.TestRunner` and `UnityEditor.TestRunner`, set
   `overrideReferences` with `nunit.framework.dll`, and use the define constraint
   `UNITY_INCLUDE_TESTS`. The EditMode one is limited to the Editor platform.
4. A test assembly cannot see code in the default `Assembly-CSharp`. Ask before creating an
   asmdef for the game's scripts (for example `Assets/Scripts/Game.asmdef`), add it to both test
   assemblies' `references`, and list the packages the scripts use (`Unity.InputSystem`,
   `Unity.TextMeshPro`, `UnityEngine.UI`) in that asmdef. Give each `Editor/` folder under it
   its own asmdef limited to the Editor platform, so `using UnityEditor;` never enters the
   player build.
5. `testing.command` is the Unity plugin's command, `python tools/studio.py test-run`. The run
   must neither hang nor open a window: it uses `-batchmode`. Do not add `-quit`; the batch
   method exits by itself.
6. Read the result files `production/qa/test-results/editmode.xml` and `playmode.xml`. A
   `testcasecount="0"` is NOT ASSESSED. Delete the old file before each run.
7. Note in `tests/README.md`: run the suite with `python tools/studio.py test-run`.

## hook: test-setup/ci-workflow

1. Unity tests in CI are left to the game: the template ships no Unity workflow, because CI
   needs a Unity licence secret.
2. When the game wants one, write `.github/workflows/tests.yml` with `actions/checkout`
   (with `lfs: true`), `game-ci/unity-test-runner@v4` for `editmode` and for `playmode`, each
   with `artifactsPath: test-results/<mode>`, and `actions/upload-artifact` with
   `if: always()`. Pin every action to a full release tag.
3. The runner needs the `UNITY_LICENSE` secret (and `UNITY_EMAIL` and `UNITY_PASSWORD` for a
   personal licence). Tell the user to add them to the repository secrets before the first run.
4. The studio's own checks (`python tools/studio.py check`) need no Unity and run in the
   template's `check.yml`.

## hook: run-and-observe/capture

1. Capture with the package's `EvidenceCapture`:
   `"<Unity editor>" -batchmode -projectPath . -executeMethod Unknownow.StudioTools.Editor.EvidenceCapture.CaptureBatch -story <story-slug> -scene <Assets/…/Scene.unity> -width 1280 -height 720 -quit -logFile -`
2. `<story-slug>` matches `^[a-z0-9][a-z0-9._-]*$` (the story file's name without `.md`).
   `-width` and `-height` default to 1920 and 1080; use 1280 and 720 unless the project says
   otherwise.
3. The output lands in `production/qa/evidence/<story-slug>/<story-slug>-<yyyyMMdd-HHmmss>.png`.
   Open the PNG and compare it with the acceptance criteria, then reference it from the
   evidence doc.
4. Never pass `-nographics`: it skips rendering, and `EvidenceCapture` needs a GPU. The
   capture renders the main camera (or the first camera) in the opened scene; a scene with no
   camera exits 2.
5. A Play Mode capture, when the story needs one, runs from the Editor menu
   `Tools/Game Studio/Capture Evidence`.
6. The Editor cannot be open on the same project. Ask the user to close it first.
7. Exit codes: 0 captured, 2 bad argument or no camera. No editor found or the project fails
   to open: `Run result: NOT VERIFIED — <reason>`.

## hook: qa-tester/test-pattern

1. The file goes under `Assets/Tests/EditMode/` (or `PlayMode/`), in the test assembly that
   `/test-setup` creates. Unity compiles nothing outside `Assets/`.
2. Use NUnit: `[TestFixture]` class `[System]Tests`, `[Test]` methods named
   `[Scenario]_[Expected]` in PascalCase. Use `[UnityTest]` (a coroutine returning
   `IEnumerator`) for tests that need frames, in the PlayMode assembly.
3. Arrange, act, assert:

   ```csharp
   using NUnit.Framework;

   [TestFixture]
   public class [System]Tests
   {
       [Test]
       public void [Scenario]_[Expected]()
       {
           // Arrange
           var subject = new [ClassName]();

           // Act
           var result = subject.[Method]([args]);

           // Assert
           Assert.AreEqual([expected], result);
       }
   }
   ```

4. Destroy every `GameObject` and `ScriptableObject` the test creates in `TearDown`.
