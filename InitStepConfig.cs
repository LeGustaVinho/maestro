using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LegendaryTools.Maestro
{
    public abstract class InitStepConfig : 
#if ODIN_INSPECTOR
        Sirenix.OdinInspector.SerializedScriptableObject,
#else
        UnityEngine.ScriptableObject,
#endif
        IMaestroTaskWithDependency, IDisposable
    {
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        [SerializeField] private int timeOut;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public int TimeOut
        {
            set => timeOut = value;
            get => timeOut;
        }
        
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        [SerializeField] private bool threadSafe;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public bool ThreadSafe
        {
            set => threadSafe = value;
            get => threadSafe;
        }

        public IMaestroTask[] Dependencies 
        {
            get
            {
                List<IMaestroTask> dependencies = new List<IMaestroTask>(StepDependencies.Length);
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

        public abstract Task<bool> DoTaskOperation();
        public virtual void Dispose()
        {
            
        }
    }
}