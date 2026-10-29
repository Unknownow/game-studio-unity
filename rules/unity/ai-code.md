---
paths: ["Assets/**/{AI,Ai,ai}/**/*.cs"]
extends: rules/ai-code.md
---

# Unity AI Code Rules

These add to `rules/ai-code.md` for C# under an `AI` folder.

- Keep AI decisions out of `Update`: tick agents from one manager at a fixed rate, and spread heavy queries across frames.
- Use `Physics.OverlapSphereNonAlloc` and `Physics.RaycastNonAlloc` with buffers allocated once; never allocate per perception check.
- Put tunable AI values (perception ranges, timers, behaviour weights) in a `ScriptableObject`, not in serialized scene fields.
- Draw AI state with `OnDrawGizmos` / `OnDrawGizmosSelected`, guarded by `#if UNITY_EDITOR`.
- Use the NavMesh API from the main thread only.
