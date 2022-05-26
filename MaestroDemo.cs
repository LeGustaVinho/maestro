using System;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LegendaryTools.Systems.Maestro
{
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
    
    public class ModuleB : BaseModule
    { }
    
    public class ModuleC : BaseModule
    { }
    
    public class ModuleD : BaseModule
    { }
    
    public class ModuleE : BaseModule
    { }
    
    public class ModuleF : BaseModule
    { }
    
    public class ModuleG : BaseModule
    { }
    
    public class ModuleH : BaseModule
    { }
    
    public class ModuleI : BaseModule
    { }
    
    public class ModuleJ : BaseModule
    { }
    
    public class ModuleK : BaseModule
    { }

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
}