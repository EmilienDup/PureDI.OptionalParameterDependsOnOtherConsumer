# The value of an optional parameter depends on whether another class requires the same unbound type

**Pure.DI 2.5.7 and master (392efa0)**. .NET SDK 10.0.403, macOS. Repro project attached (`dotnet run`, `dotnet run -p:Variant=OTHER_CONSUMER`).

## Minimal code

```csharp
using Pure.DI;

var composition = new Composition();
Console.WriteLine($"Viewer.Options:  {(composition.Viewer.Options is { } options ? $"not null ({options})" : "null")}");
Console.WriteLine($"Viewer.Settings: {(composition.Viewer.Settings is null ? "null" : "not null")}");

public readonly struct Options
{
    public bool ShowLabels { get; init; }

    public override string ToString() => $"ShowLabels={ShowLabels}";
}

public sealed class Settings;

// Nothing binds Options, Options? or Settings
public sealed class Viewer(Options? options = null, Settings? settings = null)
{
    public Options? Options => options;

    public Settings? Settings => settings;
}

#if OTHER_CONSUMER
// Unrelated to Viewer: it requires the same types
public sealed class Editor(Options? options, Settings settings)
{
    public Options? Options => options;

    public Settings Settings => settings;
}
#endif

internal partial class Composition
{
    private static void Setup()
    {
        DI.Setup(nameof(Composition))
            .Root<Viewer>("Viewer");

#if OTHER_CONSUMER
        DI.Setup(nameof(Composition))
            .Root<Editor>("Editor");
#endif
    }
}
```

## Actual

`Viewer` alone:

```
Viewer.Options:  null
Viewer.Settings: null
```

```csharp
return new global::Viewer(null, null);
```

With `Editor` in the composition (`OTHER_CONSUMER`), `Viewer` itself unchanged:

```
Viewer.Options:  not null (ShowLabels=False)
Viewer.Settings: not null
```

```csharp
return new global::Editor(new global::Options?(new global::Options()), new global::Settings());
...
return new global::Viewer(new global::Options?(new global::Options()), new global::Settings());
```

## Expected

The same arguments for `Viewer` in both cases. What an optional parameter receives should not depend on an unrelated class of the composition.

Either rule would be consistent:

- an optional parameter of a type without a binding keeps its default value, even if the type can be created (what happens for `Viewer` alone), or
- an optional parameter is injected whenever its type can be created (what happens once `Editor` is there).

The first one matches the default value the author wrote: `Options? options = null` means "no options unless something provides them", and `new Options?(new Options())` is a value nobody asked for. This is how it showed up in a real composition: a view model with an optional nullable struct parameter started to receive a default struct, which disabled the branch that handles `null`, after an unrelated factory began to inject the same type.

## Notes

- Not a regression: 2.5.7 behaves the same.
- A factory that calls `ctx.Inject(out Options? options)` has the same effect on `Viewer` as the `Editor` class.
- Workaround: bind the type to its default explicitly, `.Bind<Options?>().To(_ => (Options?)null)`.
