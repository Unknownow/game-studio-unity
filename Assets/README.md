# Assets

Unity project assets. The studio's check reads this folder as the code root (`engine.toml` `code_root`).

- `Assets/Tests/EditMode/` and `Assets/Tests/PlayMode/`: NUnit tests, each with its own asmdef. `python tools/studio.py test-scaffold` creates them.
- `Assets/**/Data/`: JSON data files; `python tools/studio.py check` validates them.
- Put game code under folders such as `Core`, `Gameplay`, `AI`, `UI` and `Networking`; the rules in `rules/unity/` match those names.

Commit every `.meta` file with its asset.
