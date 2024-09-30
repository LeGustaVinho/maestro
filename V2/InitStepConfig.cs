using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendaryTools.Systems.MaestroV2
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
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        public bool ThreadSafe { set; get; }
#else
        [UnityEngine.SerializeField] private bool threadSafe;
        public bool ThreadSafe
        {
            set => threadSafe = value;
            get => threadSafe;
        }
#endif
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