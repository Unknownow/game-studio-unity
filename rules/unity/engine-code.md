---
paths: ["Assets/**/{Core,core}/**/*.cs"]
extends: rules/engine-code.md
---

# Unity Engine Code Rules

These add to `rules/engine-code.md` for C# under a `Core` folder.

- Allocate once, reuse every frame: use the `NonAlloc` physics calls with a buffer field.

  ```csharp
  private readonly Collider[] _nearbyCache = new Collider[64];  // allocated once

  void FixedUpdate()
  {
      int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _nearbyCache);
  }
  ```

- Cache `GetComponent` results in `Awake`; never call it, `Find` or `Camera.main` inside `Update`.
- No LINQ, string concatenation or boxing in per-frame code.
- Put heavy work in the Job System with Burst when it is data-parallel; keep `UnityEngine.Object` access on the main thread.
- Keep core code free of editor-only APIs, or wrap them in `#if UNITY_EDITOR`.
