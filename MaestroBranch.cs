using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LegendaryTools.Graph;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LegendaryTools.Systems.Maestro
{
    public abstract class MaestroBranchBase : Branch<MaestroTree, MaestroBranchBase>
    {
        public abstract bool IsReference { get; }
        public virtual bool IsRunning { private set; get; }
        public virtual bool IsCompleted { private set; get; }
        public virtual bool HasError { private set; get; }
        public virtual Exception Error { private set; get; }
        public virtual float TimeSpentMilliseconds { private set; get; }
        public virtual bool HasPrerequisites =>
            Children.Count == 0 || Children.TrueForAll(item => item.IsCompleted);
        public virtual IOrchestrable OrchestrableObject { get; protected set; }

        public virtual event Action<MaestroBranchBase, bool> OnTaskCompleted;

        public virtual IEnumerator RunOrchestrableTask()
        {
            if (!HasPrerequisites || IsCompleted || IsRunning || HasError) yield break;
            
            IsRunning = true;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            yield return OrchestrableObject.OrchestrableTask();

            IsRunning = false;
            IsCompleted = true;
            sw.Stop();
            TimeSpentMilliseconds = sw.Elapsed.Milliseconds;
            
            Debug.Log($"[MaestroBranch:RunOrchestrableTask] -> {OrchestrableObject.GetType()} finished task, time: {sw.Elapsed.TotalSeconds} seconds");
            
            OnTaskCompleted?.Invoke(this, true);
        }
    }

    public class MaestroBranch : MaestroBranchBase
    {
        public override bool IsReference => false;
        
        public MaestroBranch(IOrchestrable orchestrableObject)
        {
            OrchestrableObject = orchestrableObject;
        }
    }

    public class MaestroReferenceBranch : MaestroBranchBase
    {
        public MaestroBranch MaestroBranch { get; private set; }

        public override bool IsReference => true;
        public override bool IsRunning => MaestroBranch.IsRunning;
        public override bool IsCompleted => MaestroBranch.IsCompleted;
        public override bool HasError => MaestroBranch.HasError;
        public override Exception Error => MaestroBranch.Error;
        public override float TimeSpentMilliseconds => MaestroBranch.TimeSpentMilliseconds;
        public override bool HasPrerequisites => MaestroBranch.HasPrerequisites;
        public override IOrchestrable OrchestrableObject => MaestroBranch.OrchestrableObject;

        public override event Action<MaestroBranchBase, bool> OnTaskCompleted
        {
            add => MaestroBranch.OnTaskCompleted += value;
            remove => MaestroBranch.OnTaskCompleted -= value;
        }

        public override IEnumerator RunOrchestrableTask()
        {
            yield return MaestroBranch.RunOrchestrableTask();
        }
        
        public MaestroReferenceBranch(MaestroBranch maestroBranch)
        {
            MaestroBranch = maestroBranch;
        }
    }
}