---
paths: ["Assets/**/{Networking,networking,Network,network}/**/*.cs"]
extends: rules/network-code.md
---

# Unity Network Code Rules

These add to `rules/network-code.md` for C# under a `Networking` or `Network` folder.

- Check `engine-reference/unity/modules/networking.md` for the API of the networking package in use before writing code; it changed between versions.
- Run game-state changes on the server or host. Clients send input through RPCs and never write authoritative state.
- Validate every RPC argument on the receiving side.
- Use `NetworkVariable` for state that late joiners need and RPCs for one-off events.
- Never rely on `Update` order between network objects; spawn and despawn through the network manager only.
