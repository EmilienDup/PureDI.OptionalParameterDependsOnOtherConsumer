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
