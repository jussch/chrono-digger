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

- `Assets/ChronoDigger/Logic/`: game logic (`ChronoDigger.Logic` asmdef, `noEngineReferences: true`). Namespace root `ChronoDigger`, e.g. `ChronoDigger.Combat`.
- `Assets/ChronoDigger/Tests/Editor/`: NUnit tests (namespace `ChronoDigger.Tests`).
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
- Use one class per file. Folders can group files more finely than namespaces do.
- Keep namespaces shallow: `ChronoDigger` plus one layer (`ChronoDigger.Combat`, `ChronoDigger.Tests`). Go deeper only when complexity actually calls for it, not just to separate things.
- Keep comments few. In game code, write a `///` doc comment only when a member's behaviour, constraints or exceptions aren't obvious from its name, and never one that just repeats the name. Marple.Core is a shared library, so document its public API more fully.
- Everything public is PascalCase: types, methods, properties and fields. Private fields are `_camelCase`.
- Choose fields or properties by what kind of class it is:
  - **Authored or serialized data** (definitions, profiles, plain data like `ActorState`): public fields, e.g. `public int Windup;`. Unity serializes fields, not properties. Where an interface needs a property (e.g. `IDescribable.DisplayName`), use an auto-property with a `// [field: SerializeField]` comment.
  - **Runtime objects that guard their own rules** (`Actor`, `Intent`, `ActionSlot`, `StatBlock`): properties with `{ get; }`, `{ get; private set; }` or `{ get; internal set; }`, changed only through methods, e.g. `public int TimeRemaining { get; private set; }`. Use plain `{ get; set; }` only when anyone may freely change it.
  - Prefer a property over a `GetX()` method for cheap reads. Use a method when it takes arguments, does real work, or changes state.
- Always write the access modifier on members such as fields, properties, methods and constructors, including `private`. Don't rely on the default.
- Put opening braces on the same line for classes, methods and blocks: `public class StatBlock {`, `if (x) {`.
- Test names follow `Method_Condition_Result`, e.g. `Get_NotOverridden_ReturnsDefault`.
- Write tests with NUnit 3 (classic `Assert.AreEqual` style), which matches Unity's test framework so tests port over.

## Scope

Build what the design doc or request asks for. Don't add caching, optimization, or features listed as out of scope. Prefer simple and easy to change over complete.
