# Game Studio Unity

The Unity 6.3 LTS edition of [Game Studio Base](https://github.com/Unknownow/game-studio-base): a starter template for game projects worked on by AI agents and people together. It adds a ready Unity project, the Unity engine layer and the `com.unknownow.studio-tools` editor package to the base's skills, roles, review gates, rules and command line. Adapted from [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) v1.2.0 (MIT); see `NOTICE`.

## Requirements

- Unity 6000.3.25f1 (6.3 LTS) through Unity Hub. The pin is in `ProjectSettings/ProjectVersion.txt`.
- Python 3.11 or newer on your PATH as `python`, git, and bash (Git Bash on Windows).

## Quick start

1. Create a repository from this template.
2. Delete `.github/README.md` (this file) and write your game's own `README.md`. GitHub shows this file instead of yours while it exists.
3. Run `python tools/studio.py setup`. It sets the git hooks, syncs the Claude Code add-on and replaces the embedded `Packages/com.unknownow.studio-tools` with a git dependency on the matching release tag.
4. Open the project in Unity Hub.
5. Run `python tools/studio.py status`, then ask your agent to follow the `start` skill (`.agents/skills/start/SKILL.md`). The `setup-engine` skill fills the engine placeholders in `AGENTS.md`.

## What this fork adds

| Path | What it is |
|---|---|
| `ENGINE.md` | The Unity steps every skill's "engine step" points to: tests, CI, naming, specialists, audits. |
| `engine.toml` | The engine pin. Code and assets live in `Assets/`, tests in `Assets/Tests/`. |
| `engine-reference/unity/` | Version notes for Unity 6.3, breaking changes, deprecated APIs and best practices. Start with `VERSION.md`. |
| `roles/unity-*.md`, `rules/unity/` | Unity specialist roles and path rules. |
| `Packages/com.unknownow.studio-tools/` | Editor tools: evidence capture, batch tests, a missing-reference audit and a project settings check. See its [README](../Packages/com.unknownow.studio-tools/README.md). |
| `tools/gamestudio/engines/unity.py` | The Unity plugin for `tools/studio.py`. |

`src/` and `tests/` come from the base and aren't used in Unity: put your code in `Assets/` and your tests in `Assets/Tests/`.

## Updating a game

Take updates from this repo, not from the base:

    git clone --branch <tag> https://github.com/Unknownow/game-studio-unity.git ../game-studio-unity
    python tools/studio.py update --from ../game-studio-unity --dry-run
    python tools/studio.py update --from ../game-studio-unity

## More

The root [`README.md`](../README.md) and `docs/studio/` cover the framework: skills, settings, pi and Claude Code, checks.

## Licence

MIT. See `LICENSE` and `NOTICE`.
