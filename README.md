# GodInject

A powerful and flexible **Dependency Injection (DI) library** for **Godot, and other frameworks**. Based on **DryIoc**, it provides **automatic property injection**, **service keys**, **factories** and more

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

1. Add the Analyzer and GodInject .dll files to your project.
    - Add Analyzer as a Analyzer:
      ```
        <Analyzer Include="your_path\GodInject.Analyzers.dll"></Analyzer>
      ```
    - Add GodInject as a Reference:
      ```
        <Reference Include="GodInject">
          <HintPath>your_path\GodInject.dll</HintPath>
        </Reference>
      ```
3. Ensure **DryIoc.dll** is included in your project
   - Add this inside csproj:
      ```
      	<ItemGroup>
      		<PackageReference Include="DryIoc.dll" Version="5.4.3" />
      	</ItemGroup>
      ```
4. Initialize the DI container at startup.

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
