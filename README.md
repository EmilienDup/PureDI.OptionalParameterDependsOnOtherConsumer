# Pure.DI: the value of an optional parameter depends on whether another class requires the same type

Minimal repro for Pure.DI 2.5.7 and master (392efa0). `Viewer` has two optional constructor parameters, `Options? options = null` (a nullable struct) and `Settings? settings = null` (a class). Nothing binds `Options` or `Settings`. On its own, `Viewer` gets `null` for both. As soon as an unrelated class `Editor` with required parameters of the same types is added to the composition, `Viewer` gets `new Options?(new Options())` and `new Settings()` instead.

[`ISSUE.md`](ISSUE.md) is the text to paste into a Pure.DI GitHub issue.

```
dotnet run                                                                                     # 2.5.7 from nuget.org, Viewer alone
dotnet run -p:Variant=OTHER_CONSUMER                                                           # adds the unrelated Editor
dotnet run -p:PureDIVersion=<master build version> -p:RestoreAdditionalProjectSources=<folder with the package>
```

| variant | 2.5.7 | master 392efa0 |
|---|---|---|
| default: `Viewer` alone | `Options` null, `Settings` null | same |
| `OTHER_CONSUMER`: `Editor(Options? options, Settings settings)` is also a root | `Options` not null (`ShowLabels=False`), `Settings` not null | same |

The generated composition is written under `obj/Debug/net10.0/generated/Pure.DI`.
