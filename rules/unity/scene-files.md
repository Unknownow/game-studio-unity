---
paths: ["**/*.unity", "**/*.prefab", "Assets/**/*.asset"]
---

# Unity Scene and Asset File Rules

The Editor writes these files. A hand edit that breaks one can stop a scene loading, or drop references without an error. Change them in the Editor when you can.

- Never hand-edit `.unity`, `.prefab` or `.asset` YAML while a Unity Editor is reachable: open on this project, or reachable through its command-line or MCP tools. The Editor holds its own copy, and its next save can overwrite or conflict with the edit. Make the change through the Editor, or hand the user the steps. If you cannot tell whether an Editor is reachable, treat it as reachable.
- With no Editor reachable, keep every `fileID` and `guid` reference intact, never invent a GUID, never edit or delete a `.meta` file, and open the scene in the Editor afterwards to confirm it loads.
- Commit every `.meta` file with its asset. Move and rename assets in the Editor so the `.meta` file moves with them.
- The project must use Force Text serialization and Visible Meta Files; the Project Settings Check in the `com.unknownow.studio-tools` package (`Tools/Game Studio/Project Settings Check`) checks both.
- Resolve merge conflicts with UnityYAMLMerge (`.gitattributes` already routes these files to it); never resolve one by hand in the YAML.
