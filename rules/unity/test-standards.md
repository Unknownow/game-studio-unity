---
paths: ["Assets/Tests/**/*.cs"]
extends: rules/test-standards.md
---

# Unity Test Standards

These add to `rules/test-standards.md` for tests under `Assets/Tests/`.

- Use NUnit. Class `[System]Tests` in a file of the same name; method `[Scenario]_[Expected]` in PascalCase.
- Put EditMode tests in `Assets/Tests/EditMode/` and PlayMode tests in `Assets/Tests/PlayMode/`, each with its own asmdef (`python tools/studio.py test-scaffold` creates them).
- Use `[Test]` for plain tests and `[UnityTest]` (a coroutine) for tests that need frames.
- Arrange, act, assert:

  ```csharp
  using NUnit.Framework;

  public class HealthTests
  {
      [Test]
      public void TakeDamage_ReducesHealth()
      {
          // Arrange
          var health = new Health(maxHealth: 100);

          // Act
          health.TakeDamage(25);

          // Assert
          Assert.AreEqual(75, health.Current);
      }
  }
  ```

- Destroy every `GameObject` a test creates (`Object.DestroyImmediate` in EditMode) in `TearDown`.
- Run the suite with `python tools/studio.py test-run`.
