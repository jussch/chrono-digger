# Chrono Digger

A C# game that will eventually be a Unity project (Unity 6, Odin Inspector). **For now there is no Unity project.** Everything is built and tested with plain `dotnet`, and the code is laid out the way Unity expects so it can move over later without restructuring.

## Build and test

```sh
cd Dotnet
dotnet test ChronoDigger.slnx         # game logic + Marple.Core tests
dotnet watch test --project Tests     # rerun game tests on change
```

Run the tests after every change. Marple.Core can also be tested on its own from `Packages/com.marple.core/Dotnet~/`.

## Layout

- `Assets/ChronoDigger/Logic/`: game logic (`ChronoDigger.Logic` asmdef, `noEngineReferences: true`). Namespace root `ChronoDigger`, e.g. `ChronoDigger.Combat.Stats`.
- `Assets/ChronoDigger/Tests/Editor/`: NUnit tests (namespace `ChronoDigger.Tests.*`).
- `Packages/com.marple.core/`: **Marple.Core**, a reusable library embedded as a **git subtree**. `Runtime/` holds `Amount`, `ModifiableAmount`, `AmountModifier`, `PassiveObservable`, `IDescribable`, and more.
- `Dotnet/`, `Packages/com.marple.core/Dotnet~/`: `.csproj` files that glob the `.cs` files above (`EnableDefaultCompileItems=false`). Don't put source files here. New files under `Assets/` or `Runtime/` are picked up automatically.
- `docs/design/`: design docs. Read the relevant one before working on a system (e.g. `statblock-design.md` for combat stats).

## Unity-free rules

- **No `UnityEngine` references.** Logic targets `netstandard2.1` with C# 9 (`LangVersion 9.0`); don't use newer language features.
- Types that will become ScriptableObjects are plain classes for now, with the Unity parts left as comments so they're easy to restore: `/* : ScriptableObject */`, `// [CreateAssetMenu(...)]`, `// Resources.Load(...)`.
- Odin attributes go in as comments too, with a note on what they'll do (`// Odin: [TableList] ...`).
- `[Serializable]` from `System` is fine.
- Construct definition objects (e.g. `StatDefinition`) **in one place only**: `SampleStatData` in logic, `TestStats` in tests. ScriptableObjects can't be created with `new`, so this keeps the later switch to a one-file change.
- Use explicit null checks rather than `??`/`??=` on types that will become `UnityEngine.Object`, because Unity overloads `==`.

## Marple.Core

- Use the existing types as they are (`Amount`, `ModifiableAmount`, `AmountModifier`, ...). Read their API before using them, and don't reimplement or wrap them. Note: `AmountModifier.MultiplyScalar` returns a new instance.
- **Ask before modifying Marple.Core**, and propose the smallest change. It's shared across projects through a subtree.
- Keep Marple.Core changes in their own commits, separate from game changes, so they can be split out with `git subtree push`.

## Code style

- Each class does what its name says and no more. Keep public APIs small. Readability over cleverness.
- Keep **authored data** (definitions, profiles; future assets) separate from **runtime data** (e.g. `StatBlock`). Runtime classes never write back to authored data.
- Use one class per file, organized by namespace and folder.
- Keep comments few. In game code, write a `///` doc comment only when a member's behaviour, constraints or exceptions aren't obvious from its name, and never one that just repeats the name. Marple.Core is a shared library, so document its public API more fully.
- Match the brace style of the file you're editing (logic uses same-line braces; some test files use next-line braces).
- Test names follow `Method_Condition_Result`, e.g. `Get_NotOverridden_ReturnsDefault`.
- Write tests with NUnit 3 (classic `Assert.AreEqual` style), which matches Unity's test framework so tests port over.

## Scope

Build what the design doc or request asks for. Don't add caching, optimization, or features listed as out of scope. Prefer simple and easy to change over complete.
