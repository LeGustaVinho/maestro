using System;
using System.Collections;
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
        
        public MaestroTree(MaestroBranchBase rootNode) : base(rootNode)
        {
            //rootNode.OnTaskCompleted += OnTreeCompleted;
        }

        public MaestroTree(MaestroBranchBase rootNode, MaestroBranchBase parentNode) : base(rootNode, parentNode)
        {
        }

        private void OnTreeCompleted(MaestroBranchBase branch, bool success)
        {
            IsRunning = false;
            IsCompleted = success;
            //StartOrRootNode.OnTaskCompleted -= OnTreeCompleted;
            
            OnCompleted?.Invoke(this);
        }
        
        private void OnTaskCompleted(MaestroBranchBase branch, bool success)
        {
            if (success)
            {
                //branch.OnTaskCompleted -= OnTaskCompleted;
                //await RunBranchesWithPrerequisites();
            }
            else
            {
                OnTreeCompleted(branch, false);
            }
        }
        
        public IEnumerator Start()
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
                        //branch.OnTaskCompleted += OnTaskCompleted;
                    }
                }

                yield return RunBranchesWithPrerequisites();
            }
        }

        private IEnumerator RunBranchesWithPrerequisites()
        {
            List<MaestroBranchBase> branchesToRun =
                StartOrRootNode.FindAll(item => !item.IsRunning &&
                                                !item.IsCompleted &&
                                                !item.HasError &&
                                                item.HasPrerequisites);
            do
            {
                foreach (MaestroBranchBase branchToRun in branchesToRun)
                {
                    yield return branchToRun.RunOrchestrableTask();
                }
                
                branchesToRun =
                    StartOrRootNode.FindAll(item => !item.IsRunning &&
                                                    !item.IsCompleted &&
                                                    !item.HasError &&
                                                    item.HasPrerequisites);

            } while (branchesToRun.Count > 0);
        }
    }
}