---
paths: ["Assets/**/*.{shader,hlsl,cginc,compute,shadergraph}"]
extends: rules/shader-code.md
---

# Unity Shader Code Rules

These add to `rules/shader-code.md` for Unity shader files.

- Shader Graph assets are named `SG_[Category]_[Name]`, for example `SG_Env_Water`.
- Group related properties with `[Header]` and give each one a clear display name and a `[Tooltip]` where the unit is not obvious.
- HLSL uses `lerp()` where other languages use `mix()`; use `half` precision on mobile where full precision is not needed.
- Write for the render pipeline the project uses (URP or HDRP). Do not mix built-in pipeline shaders in; the built-in pipeline is on its way out.
- Keep the shader variant count in check: use `shader_feature` for options that vary per build and `multi_compile` only when a runtime switch needs the variant.
