# Design Doc: Combat Stat System (StatDefinition / StatProfile / StatBlock)

**Project:** Chrono Digger (C#, will eventually live in Unity with Odin Inspector)
**Status:** First draft. Prefer readable and easy to change over complete or optimized.

## Environment: Unity-free for now

This code is currently built and tested with plain `dotnet`, **without Unity**. The types are designed to become ScriptableObjects later, so for now:
- Every type described below as a ScriptableObject is a **plain C# class**. Leave the Unity parts in place but commented out, so they're easy to restore: `// : ScriptableObject`, `// [CreateAssetMenu(...)]`, `// Resources.Load(...)`.
- Comment out Odin attributes in the same way, with a note on what they'll do.
- `[Serializable]` from `System` is fine to keep.
- **Construct definition objects in one place only** (see `CoreStats` and "Sample data" below). ScriptableObjects can't be created with `new`, so centralizing construction keeps the later switch to a one-file change.
- No `UnityEngine` references anywhere.

## Goal

Build the stat layer for combat actors (the player and enemies). An actor's stable attributes (MaxHealth, Defence, Wizardry, Fire Resistance…) are stored sparsely, keyed by ScriptableObject assets instead of enums. Some stats derive from others (Wizardry raises MaxMana).

Runtime quantities such as current Health or current Mana are **out of scope**. They belong to a different system that reads from the StatBlock.

## Before writing code

1. **Find the existing `ModifiableAmount` and `AmountModifier` types** (they live in the MarpleCore library inside this repo). Use them as they are. Do not reimplement or rename them. Read their API first, especially:
   - how modifiers are added and removed on a `ModifiableAmount`
   - whether `ModifiableAmount` raises a change event (if it doesn't, see "Change events" below)
   - `AmountModifier.MultiplyScalar(x)`, which **returns a new instance** and does not mutate the original
   - which numeric type they use (`double` or the `Amount` struct). Use the same type everywhere in this system.
2. Find the existing namespace conventions and follow them. If none exist for combat, use `ChronoDigger.Combat.Stats`.
3. Odin attributes mentioned in this doc go in as comments for now (see Environment).

## Code style

- Each class does exactly what its name says and no more, with implementation hidden behind a small, clear public API.
- Organize with namespaces and keep related code together.
- Readability matters more than cleverness. Keep the public surface small.
- Separate **authored data** (ScriptableObjects, serialized profiles) from **runtime data** (StatBlock). Runtime classes never write back to assets.

## Types

### `StatDefinition` (ScriptableObject)

One asset per stat. It is the dictionary key, and keys compare by reference.

```csharp
[CreateAssetMenu(menuName = "Combat/Stat Definition")]
public class StatDefinition : ScriptableObject
{
    public string displayName;
    public double defaultValue;          // match ModifiableAmount's numeric type
    public StatCategory category;        // for UI / inspector grouping only
    public List<StatModifier> derivations; // stats this one feeds
}

public enum StatCategory { Combat, Class, Resistance, Misc }
```

`StatCategory` is a short, stable enum used only for grouping. Logic must never branch on it.

### `StatModifier` (serializable class)

A rule saying "for each point of this stat, apply `perPoint` to `target`."

```csharp
[Serializable]
public class StatModifier
{
    public StatDefinition target;
    public AmountModifier perPoint; // e.g. +5 flat MaxMana per Wizardry, or +2%
}
```

At runtime the live modifier is `perPoint.MultiplyScalar(sourceValue)`.

### `DamageType` (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "Combat/Damage Type")]
public class DamageType : ScriptableObject
{
    public string displayName;
    public StatDefinition resistanceStat;
}
```

Adding a new damage type means creating two assets (the DamageType and its resistance StatDefinition) and no code changes. Resistances are ordinary stats, so StatBlock stays flat with no sub-dictionaries.

### `CoreStats` (ScriptableObject) and `Stat` (static facade)

`CoreStats` is a registry for the few stats that **engine code** refers to by name. Most stats are reached through data (spell fields, `DamageType.resistanceStat`, equipment modifiers) and do not belong here.

```csharp
// [CreateAssetMenu(menuName = "Combat/Core Stats")]
public class CoreStats // : ScriptableObject
{
    public StatDefinition MaxHealth, MaxMana, Defence, Wizardry, Warrior;

    static CoreStats _instance;
    public static CoreStats Instance
    {
        get
        {
            // Unity version: _instance = Resources.Load<CoreStats>("CoreStats");
            if (_instance == null) _instance = CreateDefault();
            return _instance;
        }
    }

    // Temporary, Unity-free: builds the core definitions in code.
    // Replaced by the Resources asset once this moves into Unity.
    static CoreStats CreateDefault() { /* construct the five StatDefinitions here */ }

    // Lets tests swap in their own instance.
    public static void SetInstance(CoreStats instance) => _instance = instance;
}

public static class Stat
{
    static CoreStats Core => CoreStats.Instance;

    public static StatDefinition MaxHealth => Core.MaxHealth;
    public static StatDefinition MaxMana   => Core.MaxMana;
    public static StatDefinition Defence   => Core.Defence;
    public static StatDefinition Wizardry  => Core.Wizardry;
    public static StatDefinition Warrior   => Core.Warrior;
}
```

- Use expression-bodied properties (`=>`), not field initializers, so values are never captured stale.
- Use an explicit null check instead of `??=`, because Unity's overloaded `==` makes `??` unreliable for UnityEngine.Object once this becomes a ScriptableObject.

### `StatProfile` (authored data)

The authored template for an actor type ("what a goblin shaman is"). It stores **only overrides**; any stat not listed uses its `StatDefinition.defaultValue`.

Unity can't serialize dictionaries, so store a list:

```csharp
[Serializable]
public class StatEntry
{
    public StatDefinition stat;
    public double baseValue;
}

[CreateAssetMenu(menuName = "Combat/Stat Profile")]
public class StatProfile : ScriptableObject
{
    public List<StatEntry> overrides;
}
```

Odin, for later (leave as comments for now):
- `[TableList]` on `overrides`
- validation that flags null stats and duplicate stats
- a read-only preview of the resolved values (defaults merged with overrides) is welcome if it's cheap to add

### `StatBlock` (runtime)

The live stats for one actor instance. Built from a `StatProfile` and owns the `ModifiableAmount`s. Plain C# class, not a MonoBehaviour.

**Public API (keep it about this small):**

```csharp
public class StatBlock
{
    public StatBlock(StatProfile profile);

    public double Get(StatDefinition stat);              // final value
    public ModifiableAmount GetAmount(StatDefinition stat); // for adding equipment/buff modifiers

    public event Action<StatDefinition, double> StatChanged;
}
```

**Behaviour:**
- **Sparse storage.** `Dictionary<StatDefinition, ModifiableAmount>`. Create a `ModifiableAmount` lazily the first time a stat is requested through `GetAmount`, seeded from the profile override or the default.
- `Get` on a stat with no entry returns the profile override if there is one, otherwise `defaultValue`, and does **not** allocate an entry.
- **At construction**, create entries for every profile override, then apply derivations for every stat that has them (so an actor with Wizardry 3 starts with its MaxMana bonus applied).
- `StatChanged` fires whenever a stat's final value changes. Consumers (UI, runtime Health/Mana clamping) subscribe to it.

**Derived stats (push-based):**
- Keep one live modifier per derivation per StatBlock: `Dictionary<StatModifier, AmountModifier> _derived`.
- When a source stat changes:
  1. For each `StatModifier` on the source's definition, compute `perPoint.MultiplyScalar(newValue)`.
  2. Replace that derivation's live modifier on the target's `ModifiableAmount` (remove the old one, add the new one, since `MultiplyScalar` returns a new instance). If `AmountModifier` has a clean way to update its value in place, prefer that.
  3. The target's own change then fires and propagates naturally, so chains (A → B → C) work without extra code.
- **Never mutate** `StatModifier.perPoint`. It is shared asset data.
- Tag derived modifiers with their source stat if `AmountModifier` supports a source or tag field, so debug views can show where a value came from.

**Change events:** if `ModifiableAmount` already raises an event when its value changes, subscribe to it. If it doesn't, **stop and ask** before modifying MarpleCore, and propose the smallest change (a single `event Action Changed` is probably enough).

### Cycle validation

Derivations must not form a cycle (A feeds B feeds A), or propagation will recurse forever.
- Write a plain static method, e.g. `StatDerivationValidator.FindCycle(StatDefinition start)`, that walks the derivation graph and returns the cycle path (or null). It's Unity-free and testable now; later an Odin `[ValidateInput]` on `StatDefinition.derivations` can just call it (leave that attribute commented).
- At runtime, add a cheap recursion-depth guard in propagation that reports an error and stops, as a safety net. Use whatever logging MarpleCore already uses; otherwise throw an `InvalidOperationException`.

## Out of scope (don't build yet)

- Runtime Health/Mana, damage calculation, spells, equipment
- Profile-level overrides of derivation rules (e.g. a "battlemage" that scales differently)
- Code generation for `Stat`
- Caching or performance work beyond what's described

## Deliverables

1. The types above, one class per file, in the agreed namespace.
2. **Sample data, built in code** in one place (e.g. a `SampleStatData` class), standing in for future assets: `MaxHealth`, `MaxMana`, `Defence`, `Wizardry`, `Warrior`, `FireResistance`; a `Fire` DamageType; the `CoreStats` defaults; one sample `StatProfile` (e.g. Goblin Shaman with Wizardry 3).
3. Wizardry gets one derivation: +N flat MaxMana per point.
4. Tests using whatever test framework the repo already uses with `dotnet test` (NUnit if there's none; it matches Unity's test framework, so tests port over later). Cover:
   - default value returned when a stat isn't overridden
   - profile override returned
   - adding a modifier through `GetAmount` changes `Get`
   - derived stat updates when the source changes (Wizardry +1 → MaxMana goes up by N)
   - a chain of two derivations propagates
   - `perPoint` on the asset is unchanged after many updates
   - cycle validation catches A → B → A
5. A short summary of any places where the existing `ModifiableAmount`/`AmountModifier` API forced a deviation from this doc.

## Later: moving into Unity

When this moves into Unity, the definition types get `: ScriptableObject` back, the commented attributes and `Resources.Load` are restored, and the code-built sample data is replaced by assets. `StatBlock`'s logic should need no changes. Keep it that way: no `UnityEngine` APIs inside `StatBlock`, `StatProfile` resolution, or derivation logic.
