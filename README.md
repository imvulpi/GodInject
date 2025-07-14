# GodInject

A powerful and flexible **Dependency Injection (DI) generator** for **Godot, and other frameworks**. Based on **DryIoc**, it provides **automatic property injection**, **service keys**, **factories** and more

## Features

- **Automatic Property Injection** – Annotate fields with `[Inject]`, and dependencies will be resolved automatically.
- **Supports Service Keys** – Inject specific implementations using service keys.
- **Factory & Lazy Injection** – Resolve dependencies on demand.
- **Partial Class Constructor Generation** – Injects dependencies through generated parameterless constructors
- **Thread-Safe, Global Container** – Uses **DryIoc**, ensuring thread safety and performance.

### No Reflection – AOT Safe!
GodInject ensures dependency injection without using reflection, relying entirely on code generation for efficiency and AOT safety.

⚠️ While DryIoc can in certain situations use reflection, it is disabled by default in GodInject.

If you plan to configure DryIoc manually, keep this in mind if you need AOT compatibility.

## Installation
### Download the Release Bundle
Includes:
  - Injection Generation Mod (used by the framework)
  - Injection API (used in your project)
  - Generator Framework (runs standalone, not bundled into your code)

### 2. Add Prebuild Hook to Your .csproj
exec.conf is not needed but it's very useful to make clean project outputs
```cs
<Target Name="GenerationFrameworkRun" BeforeTargets="BeforeBuild">
  <Exec Command='dotnet "$(ProjectDir).../GodInject.Prebuild.dll" "$(ProjectDir)YourProject.csproj" "$(ProjectDir)/tools/Generators/exec.conf"' />
</Target>
```
💡 Tips:
  - $(ProjectDir) ends with / – avoid double slashes.
  - dotnet is used for cross-platform execution.
  - Paths must point to:
      - Prebuild DLL
      - Your .csproj
      - exec.conf (optional but helps with clean outputs)

### 3. Run a Build
Confirm the generator runs and your output structure looks right.

### 4. Add the Generator Mod
Place GodInject.Generator.dll into the Mods folder of the generator framework.

### 5. Reference the Injection API in Your Project
  ```
    <Reference Include="GodInject">
      <HintPath>PATH_TO\GodInject.dll</HintPath>
    </Reference>
  ```
### 6. Ensure DryIoc is Included
  ```
    <ItemGroup>
      <PackageReference Include="DryIoc.dll" Version="5.4.3" />
    </ItemGroup>
  ```
### 7. FOR GODOT:
In `exec.conf` include path to the GodotSharp.dll
ReferencesRelativePaths = [ "./.godot/mono/temp/bin/Debug/GodotSharp.dll" ]

## Recommended project structure

```
tools/
  Generators/
    Generator/                # Contains GodInject.Prebuild.dll + dependencies
    Output/                   # Auto-generated
    Mods/                     # Auto-generated (drop GodInject.Generator.dll here)
    exec.conf                 # Configuration file
```

`exec.conf` example:
```toml
GeneratorFilesRelativePath = "./tools/Generators/"
GeneratedFilesOutputDirName = "Output"
ReferencesRelativePaths = [ ]
GeneratorModsRelativePath = "./tools/Generators/Mods"
```
With this setup:
  - Your build process remains clean.
  - All tools stay decoupled from your main project runtime.

## Quick Note on the New Generation System

Since v2.0.0, code is now generated before build time using a custom standalone generator framework. This improves compatibility with Godot and other engines. The generator runs separately and does not affect your project build output.

## Getting Started

### 1. Register Services
```csharp
InjectContainer.Register<IWeapon, Sword>();
InjectContainer.Register<IEnemyAI, HardModeAI>("HardMode");
```

### 2. Inject Dependencies (Auto-Inject)
```csharp
public partial class Player : CharacterBody2D
{
    [Inject] public IWeapon Weapon { get; set; }
    
    private void Start()
    {
        Weapon.Attack();
    }
}
```

### 3. Inject Specific Implementations Using Service Keys
```csharp
public partial class Enemy : CharacterBody2D
{
    [Inject("HardMode")] public IEnemyAI EnemyAI { get; set; }
}
```

### 4. Manually Resolve Dependencies
```csharp
var weapon = InjectContainer.Resolve<IWeapon>();
// ...
```

### 5. Using Factories
```csharp
var factory = InjectContainer.Resolve<Func<IWeapon>>();
var newWeapon = factory();
```

### 6. Disable Auto-Inject & Use InjectAll() Instead
Auto injections work through parameterless class constructor generator, blocking users from defining it normally.

You can annotate a class with `[ManagedInjection(false)]` to prevent parameterless constructors from being generated.
This however means you will need to call InjectAll() if you want to inject dependencies.
```csharp
[ManagedInjection(false)]
public partial class GameManager
{
    [Inject] public IGameSettings Settings { get; set; }

    // Allows user parameterless constructors
    public GameManager()
    {
        InjectAll();  // Manually trigger injection
    }
}
```
If you set,  `[ManagedInjection(true)]` to true, the parameterless constructor is generated and InjectAll() method is available
This can be useful in situations like this:

```csharp
[ManagedInjection(true)]
public partial class GameManager
{
    [Inject] public IGameSettings Settings { get; set; }
    public User User { get; set; }
    // If we don't want to inject User, 
    // We can make an explicit constructor and inject the rest annotated with [Inject]
    public GameManager(User user){
        InjectAll();
        User = user;
    }
}
```


## Accessing the Base Container:

For advanced use cases, you can access the underlying DryIoc container directly to customize registration and configuration further:
```csharp
var baseContainer = InjectContainer.GetContainer();
// ...
```
This allows full flexibility for registering dependencies dynamically while maintaining the power of DryIoc.

## Best Practices

✔ **Register dependencies at startup** to ensure everything is set up before resolving.

✔ **Register dependencies in clearly defined spaces** to allow flexibility and better maintainability
  
✔ **Use service keys for multiple implementations** (e.g., different AI behaviors for different enemies).

✔ **Use manual resolution only when necessary** – prefer auto-injection when possible.

## Debugging & Troubleshooting

- **Injection failed?** Ensure the service is registered before resolving.
- **Null reference on injected properties?** Make sure `[Inject]` is correctly applied.
- Make sure the class with [Inject] Properties is **partial**.
- Confirm you have all of the necessities installed (look below).

## Necessities

- Classes need to be partial so the generator can extend it without trouble
- GodInject.Analyzers and GodInject itself are required for generator and container.
- DryIoc.dll is required (in my experience normal DryIoc (source one) can trigger errors)

There is no NuGet package, you can download .dll files from Releases or build it yourself.

(Could be added to NuGet in the future)

## License
MIT License. Feel free to use, modify, and contribute!

## Contributing
Pull requests are welcome! If you have suggestions, feel free to open an issue or contribute to the project.

---
