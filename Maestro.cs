using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LegendaryTools.Systems.Maestro
{
    public interface IOrchestrable
    {
        int TimeOut { get; }
        
        Task OrchestrableTask();
    }

    public interface IOrchestrableDependable : IOrchestrable
    {
        IOrchestrable[] Dependencies { get; set; }
    }

    public class Maestro
    {
        public MaestroTree Tree { private set; get; }

        private readonly Dictionary<IOrchestrable, MaestroBranch> objectBranchMap =
            new Dictionary<IOrchestrable, MaestroBranch>();

        public event Action<Maestro, bool> OnFinished;

        private class MaestroRoot : IOrchestrable
        {
            public IOrchestrable[] Dependencies => Array.Empty<IOrchestrable>();
            public int TimeOut => 1;

            public async Task OrchestrableTask()
            {
                await Task.Yield();
            }
        }

        public Maestro()
        {
            Tree = new MaestroTree(new MaestroBranch(new MaestroRoot()));
        }

        public void Add(IOrchestrable orchestrableObject, params IOrchestrable[] dependencies)
        {
            if (!objectBranchMap.ContainsKey(orchestrableObject))
            {
                MaestroBranch newBranch = new MaestroBranch(orchestrableObject);
                objectBranchMap.Add(orchestrableObject, newBranch);
            }

            MaestroBranch objectBranch = objectBranchMap[orchestrableObject];

            foreach (IOrchestrable dependency in dependencies)
            {
                if (!objectBranchMap.ContainsKey(dependency))
                {
                    MaestroBranch newBranch = new MaestroBranch(dependency);
                    objectBranchMap.Add(dependency, newBranch);
                }

                MaestroBranchBase dependencyBranch = objectBranchMap[dependency];

                if (dependencyBranch.Parent != null)
                {
                    if (Array.Exists(objectBranch.BranchHierarchy, item => ReferenceEquals(item, dependencyBranch)))
                    {
                        Debug.LogError($"{orchestrableObject.GetType()} cannot be added because a circular reference would occur");
                        return;
                    }
                    
                    MaestroReferenceBranch newReferenceBranch = new MaestroReferenceBranch((MaestroBranch)dependencyBranch);
                    dependencyBranch = newReferenceBranch;
                }

                objectBranch.Add(dependencyBranch);
            }
        }

        public void AddWithDependency(params IOrchestrableDependable[] orchestrableObjects)
        {
            foreach (IOrchestrableDependable orchestrableObject in orchestrableObjects)
            {
                Add(orchestrableObject, orchestrableObject.Dependencies);    
            }
        }
        
        public void AddManyWithNoDependency(params IOrchestrable[] orchestrableObjects)
        {
            foreach (IOrchestrable orchestrableObject in orchestrableObjects)
            {
                MaestroBranch newBranch = new MaestroBranch(orchestrableObject);
                objectBranchMap.Add(orchestrableObject, newBranch);
            }
        }

        public async Task Start()
        {
            //Make all parentless branchs to root node
            foreach (KeyValuePair<IOrchestrable, MaestroBranch> pair in objectBranchMap)
            {
                if (pair.Value.Parent == null)
                {
                    Tree.StartOrRootNode.Add(pair.Value);
                }
            }
            
            objectBranchMap.Clear();

            Tree.OnCompleted += OnMaestroTreeCompleted;

            await Tree.Start();
            while (Tree.IsRunning)
            {
                await Task.Delay(25);
            }
            
            Tree.OnCompleted -= OnMaestroTreeCompleted;
        }

        private void OnMaestroTreeCompleted(MaestroTree maestroTree)
        {
            OnFinished?.Invoke(this, maestroTree.IsCompleted);
        }
    }
}