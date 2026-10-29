---
paths: ["Assets/**/{Data,data}/**/*.json"]
extends: rules/data-files.md
---

# Unity Data File Rules

These add to `rules/data-files.md` for JSON under an `Assets/**/Data` folder.

- `JsonUtility` binds by exact field name, and C# fields are camelCase, so a snake_case file cannot bind directly to the type that uses it.
- Read such a file through a private nested DTO whose fields are the wire format, and map once into your real type. Or use a library with a naming policy (Newtonsoft `SnakeCaseNamingStrategy`, `System.Text.Json` `JsonNamingPolicy`).
- Do not switch the file to camelCase to avoid the DTO: the keys are the shared contract. Document the DTO as the wire-format boundary.
- Put data files under `Assets/**/Data/` so `python tools/studio.py check` validates them (`engine.toml` `data_globs`).
- Commit the `.meta` file with every data file.
