---
paths: ["Assets/**/{Gameplay,gameplay}/**/*.cs"]
extends: rules/gameplay-code.md
---

# Unity Gameplay Code Rules

These add to `rules/gameplay-code.md` for C# under a `Gameplay` folder.

- Read gameplay values from a `ScriptableObject`, not from literals:

  ```csharp
  float damage = combatConfig.BaseDamage;
  float speed = stats.MovementSpeed * Time.deltaTime;
  ```

- Multiply movement and timers by `Time.deltaTime` in `Update` and `Time.fixedDeltaTime` in `FixedUpdate`.
- Use `[SerializeField] private` fields, not public fields, for inspector values.
- Communicate with C# events or `UnityEvent` assets; avoid `SendMessage` and string-based lookups.
