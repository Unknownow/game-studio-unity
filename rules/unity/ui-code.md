---
paths: ["Assets/**/{UI,Ui,ui}/**/*.cs"]
extends: rules/ui-code.md
---

# Unity UI Code Rules

These add to `rules/ui-code.md` for C# under a `UI` folder.

- Prefer UI Toolkit (UXML and USS) for new UI; use UGUI only where the project already does.
- Read `engine-reference/unity/modules/ui.md` before using data binding; the API differs between versions.
- UI code reads game state and raises events. It never changes game state directly.
- Do not rebuild layouts every frame: update only the elements that changed.
- Read input through the Input System package, not `UnityEngine.Input`.
