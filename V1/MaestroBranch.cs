using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LegendaryTools.Graph;
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

        public virtual async Task RunOrchestrableTask()
        {
            if (!HasPrerequisites || IsCompleted || IsRunning || HasError) return;
            
            IsRunning = true;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool orchestrableTaskResult = false;
            try
            {
                if (OrchestrableObject.TimeOut > 0)
                {
                    Task<bool> orchestrableTask = OrchestrableObject.OrchestrableTask();
                    if (await Task.WhenAny(orchestrableTask, Task.Delay(OrchestrableObject.TimeOut * 1000)) !=
                        orchestrableTask)
                    {
                        orchestrableTaskResult = false;
                        throw new TimeoutException(
                            $"{OrchestrableObject.GetType()} has time out while doing orchestrable task.");
                    }

                    orchestrableTaskResult = orchestrableTask.Result;
                }
                else
                {
                    orchestrableTaskResult = await OrchestrableObject.OrchestrableTask();
                }

                if (!orchestrableTaskResult)
                {
                    Debug.LogError($"[MaestroBranch:RunOrchestrableTask] -> {OrchestrableObject.GetType()} dependencies were not met in order to complete the task.");
                }
            }
            catch (Exception e)
            {
                IsRunning = false;
                HasError = true;
                Error = e;
                Debug.LogError($"[MaestroBranch:RunOrchestrableTask] -> {OrchestrableObject.GetType()} got a error while doing a task.");
                Debug.LogException(e);
                sw.Stop();
                TimeSpentMilliseconds = sw.Elapsed.Milliseconds;
                OnTaskCompleted?.Invoke(this, false);
                return;
            }

            IsRunning = false;
            IsCompleted = true;
            sw.Stop();
            TimeSpentMilliseconds = sw.Elapsed.Milliseconds;
            
            if(Owner.Verbose)
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

        public override async Task RunOrchestrableTask()
        {
            await MaestroBranch.RunOrchestrableTask();
        }
        
        public MaestroReferenceBranch(MaestroBranch maestroBranch)
        {
            MaestroBranch = maestroBranch;
        }
    }
}