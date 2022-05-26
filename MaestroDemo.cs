using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LegendaryTools.Systems.Maestro
{
    public class BaseModule: IOrchestrable
    {
        public int TimeOut => 2;

        public async Task OrchestrableTask()
        {
            await Task.Delay(Mathf.RoundToInt(Random.Range(0.25f, 1.1f) * 1000));
        }
    }
    
    public class ModuleA : BaseModule
    { }
    
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
            ModuleA moduleA = new ModuleA();
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

            Maestro.Add(moduleA, moduleB, moduleC, moduleI);
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