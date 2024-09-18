using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LegendaryTools.Graph;

namespace LegendaryTools.Systems.Maestro
{
    public class MaestroTree : Tree<MaestroTree, MaestroBranchBase>
    {
        public bool IsRunning { get; private set; }
        public bool IsCompleted { get; private set; }
        
        public event Action<MaestroTree> OnCompleted;
        public event Action<MaestroBranchBase, bool> OnBranchFinished;
        
        public MaestroTree(MaestroBranchBase rootNode) : base(rootNode)
        {
            rootNode.OnTaskCompleted += OnTreeCompleted;
        }

        public MaestroTree(MaestroBranchBase rootNode, MaestroBranchBase parentNode) : base(rootNode, parentNode)
        {
        }

        private void OnTreeCompleted(MaestroBranchBase branch, bool success)
        {
            IsRunning = false;
            IsCompleted = success;
            StartOrRootNode.OnTaskCompleted -= OnTreeCompleted;
            
            OnCompleted?.Invoke(this);
        }
        
        private async void OnTaskCompleted(MaestroBranchBase branch, bool success)
        {
            if (success)
            {
                OnBranchFinished?.Invoke(branch, true);
                branch.OnTaskCompleted -= OnTaskCompleted;
                await RunBranchesWithPrerequisites();
            }
            else
            {
                OnBranchFinished?.Invoke(branch, false);
                OnTreeCompleted(branch, false);
            }
        }
        
        public async Task Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                List<MaestroBranchBase> allBranches = new List<MaestroBranchBase>() { StartOrRootNode };
                allBranches.AddRange(StartOrRootNode.GetAllChildrenNodes());

                foreach (MaestroBranchBase branch in allBranches)
                {
                    if (!branch.IsReference)
                    {
                        branch.OnTaskCompleted += OnTaskCompleted;
                    }
                }

                await RunBranchesWithPrerequisites();
            }
        }

        private async Task RunBranchesWithPrerequisites()
        {
            List<MaestroBranchBase> branchesToRun =
                StartOrRootNode.FindAll(item => !item.IsRunning &&
                                                !item.IsCompleted &&
                                                !item.HasError &&
                                                item.HasPrerequisites);

            if (branchesToRun.Count > 0)
            {
                List<Task> tasks = new List<Task>(branchesToRun.Count);
                foreach (MaestroBranchBase branchToRun in branchesToRun)
                {
                    tasks.Add(branchToRun.RunOrchestrableTask());
                }

                await Task.WhenAll(tasks);
            }
            else
            {
                if (!StartOrRootNode.IsRunning &&
                    !StartOrRootNode.IsCompleted &&
                    !StartOrRootNode.HasError &&
                    StartOrRootNode.HasPrerequisites)
                {
                    await StartOrRootNode.RunOrchestrableTask();
                }
            }
        }
    }
}