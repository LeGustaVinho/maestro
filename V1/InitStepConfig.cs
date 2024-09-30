using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendaryTools.Systems.Maestro
{
    public abstract class InitStepConfig : 
#if ODIN_INSPECTOR
        Sirenix.OdinInspector.SerializedScriptableObject,
#else
        UnityEngine.ScriptableObject,
#endif
        IOrchestrableDependable, IDisposable
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        public int TimeOut { set; get; }
#else
        [UnityEngine.SerializeField] private int timeOut;
        public int TimeOut
        {
            set => timeOut = value;
            get => timeOut;
        }
#endif
        public IOrchestrable[] Dependencies 
        {
            get
            {
                List<IOrchestrable> dependencies = new List<IOrchestrable>(StepDependencies.Length);
                foreach (InitStepConfig initStepConfig in StepDependencies)
                {
                    dependencies.Add(initStepConfig);
                }
                return dependencies.ToArray();
            }
            set
            { }
        }
        
        public InitStepConfig[] StepDependencies;

        public abstract Task<bool> OrchestrableTask();
        public virtual void Dispose()
        {
            
        }
    }
}