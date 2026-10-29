"""Unity engine plugin: editor pin checks, the test command, the test scaffold and the package setup."""
import json
import os
import re
import shutil
import sys

from ..common import git, read_text

PACKAGE = "com.unknownow.studio-tools"
PACKAGE_DIR = "Packages/" + PACKAGE
PACKAGE_URL = "https://github.com/Unknownow/game-studio-unity.git?path=/" + PACKAGE_DIR + "#v%s"
TEMPLATE_ORIGIN = re.compile(r"Unknownow/game-studio-unity(\.git)?$")
VERSION_MD = "engine-reference/unity/VERSION.md"
PROJECT_VERSION = "ProjectSettings/ProjectVersion.txt"


def _project_version(ctx):
    path = os.path.join(ctx.root, PROJECT_VERSION)
    if not os.path.isfile(path):
        return None
    m = re.search(r"^m_EditorVersion:\s*(\S+)", read_text(path), re.M)
    return m.group(1) if m else None


def _editor_path(ctx):
    set_path = ctx.cfg.value("engine.editor_path")
    if set_path:
        return str(set_path)
    v = _project_version(ctx)
    if not v:
        return None
    if sys.platform.startswith("win"):
        return "C:/Program Files/Unity/Hub/Editor/%s/Editor/Unity.exe" % v
    if sys.platform == "darwin":
        return "/Applications/Unity/Hub/Editor/%s/Unity.app/Contents/MacOS/Unity" % v
    return os.path.expanduser("~/Unity/Hub/Editor/%s/Editor/Unity" % v)


def _on_line(version, line):
    return version.startswith(line + ".")


def status(ctx):
    v = _project_version(ctx)
    line = ctx.engine_toml.get("editor_line", "")
    lts = ctx.engine_toml.get("lts", "")
    if not v:
        return ["editor: unknown (pinned line %s, Unity %s LTS)" % (line, lts)]
    out = ["editor: %s (pinned line %s, Unity %s LTS)" % (v, line, lts)]
    if line and not _on_line(v, line):
        out.append("WARN engine-pin: editor %s is not on the pinned line %s (Unity %s LTS); see the setup-engine upgrade mode" % (v, line, lts))
    return out


def check(ctx, report):
    lts = ctx.engine_toml.get("lts")
    line = ctx.engine_toml.get("editor_line")
    if not lts or not line:
        report.error("engine-pin", "engine.toml", "engine.toml needs lts and editor_line for the Unity pin")
        return
    ref = os.path.join(ctx.root, VERSION_MD)
    if not os.path.isfile(ref):
        report.error("engine-pin", VERSION_MD, "missing; it holds the Engine Version row the pin must match")
    else:
        rows = [ln for ln in read_text(ref).split("\n") if ln.startswith("| **Engine Version** |")]
        if not rows or "Unity %s LTS" % lts not in rows[0]:
            report.error("engine-pin", VERSION_MD, "the Engine Version row does not say Unity %s LTS (the engine.toml pin)" % lts)
    v = _project_version(ctx)
    if v and not _on_line(v, line):
        report.warn("engine-pin", PROJECT_VERSION, "editor %s is not on the pinned line %s (Unity %s LTS)" % (v, line, lts))


def installed_version(ctx):
    editor = _editor_path(ctx)
    if editor and os.path.isfile(editor):
        return _project_version(ctx)
    return None


def test_command(ctx):
    editor = _editor_path(ctx)
    if not editor or not os.path.isfile(editor):
        sys.stderr.write("studio: Unity editor not found: %s; set engine.editor_path in project.local.toml\n" % (editor or "-"))
        sys.exit(2)
    return [editor, "-batchmode", "-projectPath", ctx.root, "-executeMethod", "Unknownow.StudioTools.Editor.BatchTests.Run",
            "-testPlatform", "All", "-logFile", "-"]


ASMDEF = {
    "name": "",
    "references": ["UnityEngine.TestRunner", "UnityEditor.TestRunner"],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": False,
    "overrideReferences": True,
    "precompiledReferences": ["nunit.framework.dll"],
    "autoReferenced": False,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": False,
}

SAMPLE = """using NUnit.Framework;

public class Sample{mode}Test
{{
    [Test]
    public void Arithmetic_AddsTwoNumbers()
    {{
        // Arrange
        int a = 2;
        int b = 3;

        // Act
        int sum = a + b;

        // Assert
        Assert.AreEqual(5, sum);
    }}
}}
"""


def scaffold_files(ctx):
    name = re.sub(r"[^A-Za-z0-9]", "", str(ctx.cfg.value("project.name") or "")) or "Game"
    out = {}
    for mode in ("EditMode", "PlayMode"):
        asmdef = dict(ASMDEF, name="%s.Tests.%s" % (name, mode))
        if mode == "EditMode":
            asmdef["includePlatforms"] = ["Editor"]
        out["Assets/Tests/%s/%s.Tests.%s.asmdef" % (mode, name, mode)] = json.dumps(asmdef, indent=2) + "\n"
        out["Assets/Tests/%s/Sample%sTest.cs" % (mode, mode)] = SAMPLE.format(mode=mode)
    return out


def setup(ctx):
    code, origin = git(ctx.root, "remote", "get-url", "origin")
    if code == 0 and TEMPLATE_ORIGIN.search(origin.strip()):
        return []
    pkg_dir = os.path.join(ctx.root, PACKAGE_DIR)
    pkg_json = os.path.join(pkg_dir, "package.json")
    if not os.path.isfile(pkg_json):
        return []
    manifest_path = os.path.join(ctx.root, "Packages", "manifest.json")
    try:
        version = json.loads(read_text(pkg_json))["version"]
        manifest = json.loads(read_text(manifest_path))
    except (OSError, ValueError, KeyError, TypeError) as e:
        return ["WARN unity setup: cannot convert the embedded package (%s: %s); package left embedded" % (type(e).__name__, e)]
    if not isinstance(manifest, dict):
        return ["WARN unity setup: Packages/manifest.json is not an object; package left embedded"]
    shutil.rmtree(pkg_dir)
    meta = pkg_dir + ".meta"
    if os.path.isfile(meta):
        os.remove(meta)
    url = PACKAGE_URL % version
    manifest.setdefault("dependencies", {})[PACKAGE] = url
    testables = [t for t in manifest.get("testables", []) if t != PACKAGE]
    if testables:
        manifest["testables"] = testables
    else:
        manifest.pop("testables", None)
    with open(manifest_path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(manifest, indent=2) + "\n")
    return ["removed %s (embedded copy)" % PACKAGE_DIR, "Packages/manifest.json: %s -> %s" % (PACKAGE, url)]
