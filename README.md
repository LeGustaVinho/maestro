# Maestro

**Maestro** is a dependency tree resolver

With Maestro you can create an initialization tree of services/modules/systems where each, where probably one or more services depend on others and cannot be initialized while its dependent is not.

Features:

- Clear and intuitive API to organize modules and declare their dependencies
- Already thought to work asynchronously using async/await
- Detects and reports errors if a module causes an exception during resolution and does not lock the stack or crash.
- You can configure a timeout time to prevent the module from getting stuck trying to be resolved.

Dependencies:

- [Legendary Tools - Graphs](https://github.com/LeGustaVinho/graphs "Legendary Tools - Graphs")

### Creating modules

You can assemble the dependency tree by code as in the following example:

```csharp
    public class BaseModule: IOrchestrableDependable
    {
        public IOrchestrable[] Dependencies { get; set; } = Array.Empty<IOrchestrable>();
        public int TimeOut => 2;

        public async Task OrchestrableTask()
        {
            await Task.Delay(Mathf.RoundToInt(Random.Range(0.25f, 1.1f) * 1000));
        }
    }

    public class ModuleA : BaseModule
    {
        public ModuleA(ModuleB b, ModuleC c, ModuleH h, ModuleI i)
        {
            Dependencies = new IOrchestrable[4];
            Dependencies[0] = b;
            Dependencies[1] = c;
            Dependencies[2] = h;
            Dependencies[3] = i;
        }
    }
```
### Organizing them in a tree or in sequence, indicating which module depends on which

```csharp
public class MaestroDemo : MonoBehaviour
    {
        public Maestro Maestro = new Maestro();
        
        async void Start()
        {
            ModuleB moduleB = new ModuleB();
            ModuleC moduleC = new ModuleC();
            ModuleD moduleD = new ModuleD();
            ModuleE moduleE = new ModuleE();
            ModuleF moduleF = new ModuleF();
            ModuleG moduleG = new ModuleG();
            ModuleH moduleH = new ModuleH();
            ModuleI moduleI = new ModuleI();
            ModuleJ moduleJ = new ModuleJ();
            ModuleK moduleK = new ModuleK();
            ModuleA moduleA = new ModuleA(moduleB, moduleC, moduleH, moduleI);

            Maestro.AddWithDependency(moduleA);
            Maestro.Add(moduleB, moduleD, moduleE);
            Maestro.Add(moduleD, moduleG);
            Maestro.Add(moduleE, moduleJ);
            Maestro.Add(moduleC, moduleF);
            Maestro.Add(moduleH, moduleK, moduleE);
            Maestro.Add(moduleI, moduleH, moduleB);

            Maestro.OnFinished += OnMaestroFinished;
            
            await Maestro.Start();
            
            Debug.Log("Finished");
        }

        private void OnMaestroFinished(Maestro maestro, bool success)
        {
            Debug.Log($"OnMaestroFinished, Success: {success}");
            
            Maestro.OnFinished -= OnMaestroFinished;
        }
    }
```
After arranging the modules just call` Maestro.Start()` and wait for the result using await, if all goes well all the modules will be started in the correct order.

