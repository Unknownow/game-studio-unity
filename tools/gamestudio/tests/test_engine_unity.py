import json
import os
import unittest

from ._util import REPO, RepoCase, git

def read(path):
    with open(path, encoding="utf-8") as fh:
        return fh.read()


ENGINE_TOML = read(os.path.join(REPO, "engine.toml"))
PACKAGE = "com.unknownow.studio-tools"
VERSION_MD = "| Field | Value |\n|---|---|\n| **Engine Version** | Unity %s LTS |\n"


def version_file(v):
    return {"ProjectSettings/ProjectVersion.txt": "m_EditorVersion: %s\nm_EditorVersionWithRevision: %s (abc)\n" % (v, v)}


class UnityPluginTest(RepoCase):
    def unity(self, extra=None, toml=None):
        files = {"engine.toml": ENGINE_TOML, "engine-reference/unity/VERSION.md": VERSION_MD % "6.3"}
        files.update(extra or {})
        self.with_toml(files)
        if toml:
            self.add({"project.toml": toml})

    def test_status_warns_off_line_editor(self):
        self.unity(version_file("6000.0.64f1"))
        code, out, _ = self.run_cli("status")
        self.assertEqual(code, 0)
        self.assertIn("engine-pin: editor 6000.0.64f1 is not on the pinned line 6000.3", out)

    def test_status_on_line_editor_has_no_warning(self):
        self.unity(version_file("6000.3.25f1"))
        _, out, _ = self.run_cli("status")
        self.assertIn("editor: 6000.3.25f1 (pinned line 6000.3, Unity 6.3 LTS)", out)
        self.assertNotIn("engine-pin", out)

    def test_status_without_project_version(self):
        self.unity()
        _, out, _ = self.run_cli("status")
        self.assertIn("editor: unknown", out)

    def test_check_version_md_mismatch_errors(self):
        self.unity(dict(version_file("6000.3.25f1"), **{"engine-reference/unity/VERSION.md": VERSION_MD % "6.4"}))
        code, out, _ = self.run_cli("check")
        self.assertEqual(code, 1)
        self.assertIn("ERROR engine-pin engine-reference/unity/VERSION.md", out)

    def test_check_matching_pin_has_no_engine_pin_finding(self):
        self.unity(version_file("6000.3.25f1"))
        code, out, _ = self.run_cli("check")
        self.assertNotIn("engine-pin", out)
        self.assertEqual(code, 0)

    def test_check_missing_version_md_errors(self):
        self.unity()
        os.remove(os.path.join(self.root, "engine-reference/unity/VERSION.md"))
        code, out, _ = self.run_cli("check")
        self.assertEqual(code, 1)
        self.assertIn("engine-pin", out)

    def test_check_warns_on_other_line_editor(self):
        self.unity(version_file("6000.0.64f1"))
        code, out, _ = self.run_cli("check")
        self.assertEqual(code, 0)
        self.assertIn("WARN engine-pin ProjectSettings/ProjectVersion.txt", out)

    def embedded(self, testables):
        manifest = {"dependencies": {"com.unity.inputsystem": "1.20.0"}, "scopedRegistries": [], "testables": testables}
        return {
            "Packages/manifest.json": json.dumps(manifest, indent=2) + "\n",
            "Packages/%s/package.json" % PACKAGE: json.dumps({"name": PACKAGE, "version": "0.1.0"}),
            "Packages/%s/Editor/Tool.cs" % PACKAGE: "// x\n",
        }

    def test_setup_converts_embedded_package(self):
        self.unity(self.embedded([PACKAGE]))
        self.git_init()
        git(self.root, "remote", "add", "origin", "https://github.com/someone/game.git")
        code, out, _ = self.run_cli("setup")
        self.assertEqual(code, 0)
        self.assertIn("removed Packages/%s (embedded copy)" % PACKAGE, out)
        self.assertFalse(os.path.exists(os.path.join(self.root, "Packages", PACKAGE)))
        text = read(os.path.join(self.root, "Packages/manifest.json"))
        data = json.loads(text)
        self.assertEqual(data["dependencies"][PACKAGE],
                         "https://github.com/Unknownow/game-studio-unity.git?path=/Packages/%s#v0.1.0" % PACKAGE)
        self.assertNotIn("testables", data)
        self.assertEqual(list(data), ["dependencies", "scopedRegistries"])
        self.assertTrue(text.endswith("}\n"))

    def test_setup_keeps_other_testables(self):
        self.unity(self.embedded([PACKAGE, "com.other.pkg"]))
        self.git_init()
        git(self.root, "remote", "add", "origin", "https://github.com/someone/game.git")
        self.run_cli("setup")
        data = json.loads(read(os.path.join(self.root, "Packages/manifest.json")))
        self.assertEqual(data["testables"], ["com.other.pkg"])

    def test_setup_without_origin_converts(self):
        self.unity(self.embedded([PACKAGE]))
        self.git_init()
        self.run_cli("setup")
        self.assertFalse(os.path.exists(os.path.join(self.root, "Packages", PACKAGE)))

    def test_setup_template_origin_noop(self):
        files = self.embedded([PACKAGE])
        self.unity(files)
        self.git_init()
        git(self.root, "remote", "add", "origin", "git@github-personal:Unknownow/game-studio-unity.git")
        before = read(os.path.join(self.root, "Packages/manifest.json"))
        code, out, _ = self.run_cli("setup")
        self.assertEqual(code, 0)
        self.assertNotIn("removed", out)
        self.assertTrue(os.path.isfile(os.path.join(self.root, "Packages", PACKAGE, "package.json")))
        self.assertEqual(read(os.path.join(self.root, "Packages/manifest.json")), before)

    def test_setup_without_manifest_leaves_the_package(self):
        files = self.embedded([PACKAGE])
        del files["Packages/manifest.json"]
        self.unity(files)
        self.git_init()
        code, out, _ = self.run_cli("setup")
        self.assertEqual(code, 0)
        self.assertIn("package left embedded", out)
        self.assertTrue(os.path.isfile(os.path.join(self.root, "Packages", PACKAGE, "package.json")))

    def test_setup_package_without_version_leaves_the_package(self):
        files = self.embedded([PACKAGE])
        files["Packages/%s/package.json" % PACKAGE] = "{}"
        self.unity(files)
        self.git_init()
        _, out, _ = self.run_cli("setup")
        self.assertIn("package left embedded", out)

    def test_test_run_missing_editor(self):
        self.unity(toml='schema_version = 1\n\n[modes]\nrigor = "minimal"\n\n[engine]\neditor_path = "%s"\n'
                   % os.path.join(self.root, "nope", "Unity.exe").replace("\\", "/"))
        code, _, err = self.run_cli("test-run")
        self.assertEqual(code, 2)
        self.assertTrue(err.startswith("studio: Unity editor not found:"), err)

    def test_test_run_dry_run_command(self):
        fake = os.path.join(self.root, "Unity.exe")
        self.unity(toml='schema_version = 1\n\n[modes]\nrigor = "minimal"\n\n[engine]\neditor_path = "%s"\n'
                   % fake.replace("\\", "/"))
        self.add({"Unity.exe": "x"})
        code, out, _ = self.run_cli("test-run", "--dry-run")
        self.assertEqual(code, 0)
        self.assertIn("Unknownow.StudioTools.Editor.BatchTests.Run", out)
        self.assertIn("-testPlatform All", out)
        self.assertIn("-logFile -", out)

    def test_scaffold_files(self):
        self.unity()
        self.add({"project.toml": 'schema_version = 1\n\n[modes]\nrigor = "minimal"\n\n[project]\nname = "My Game!"\n'})
        code, _, _ = self.run_cli("test-scaffold", "unity")
        self.assertEqual(code, 0)
        edit = os.path.join(self.root, "Assets/Tests/EditMode/MyGame.Tests.EditMode.asmdef")
        play = os.path.join(self.root, "Assets/Tests/PlayMode/MyGame.Tests.PlayMode.asmdef")
        for rel in ("Assets/Tests/EditMode/SampleEditModeTest.cs", "Assets/Tests/PlayMode/SamplePlayModeTest.cs"):
            self.assertTrue(os.path.isfile(os.path.join(self.root, rel)), rel)
        e = json.loads(read(edit))
        p = json.loads(read(play))
        self.assertEqual(e["includePlatforms"], ["Editor"])
        self.assertEqual(p["includePlatforms"], [])
        self.assertEqual(e["defineConstraints"], ["UNITY_INCLUDE_TESTS"])
        self.assertTrue(e["overrideReferences"])

    def test_scaffold_name_falls_back_to_game(self):
        self.unity()
        self.run_cli("test-scaffold", "unity")
        self.assertTrue(os.path.isfile(os.path.join(self.root, "Assets/Tests/EditMode/Game.Tests.EditMode.asmdef")))


class RepoUnityTest(unittest.TestCase):
    def test_package_dir_has_manifest_and_matching_name(self):
        pkg = os.path.join(REPO, "Packages", PACKAGE, "package.json")
        if not os.path.isfile(pkg):
            self.skipTest("package not present")
        data = json.loads(read(pkg))
        self.assertEqual(data["name"], PACKAGE)
        self.assertEqual(data["unity"], "6000.3")

    def test_unity_manifest_lines_exist(self):
        with open(os.path.join(REPO, "files.manifest.d", "unity.manifest"), encoding="utf-8") as fh:
            lines = [ln.strip() for ln in fh if ln.strip() and not ln.startswith("#")]
        self.assertIn("ENGINE.md", lines)
        self.assertIn("tools/gamestudio/engines/unity.py", lines)
