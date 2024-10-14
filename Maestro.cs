using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LegendaryTools.Maestro
{
    public class Maestro :  IMaestro
    {
        public bool Verbose;
        public event Action<Maestro, bool> OnFinished;
        public event Action<MaestroTaskInfo, bool> OnTaskFinished;

        private readonly Dictionary<IMaestroTask, List<IMaestroTask>> maestroTaskDependencyMap =
            new Dictionary<IMaestroTask, List<IMaestroTask>>();

        public Maestro(bool verbose = false)
        {
            Verbose = true;
        }

        public void Add(IMaestroTask task, params IMaestroTask[] dependencies)
        {
            if (!maestroTaskDependencyMap.ContainsKey(task))
            {
                maestroTaskDependencyMap.Add(task, new List<IMaestroTask>());
            }

            foreach (IMaestroTask dependency in dependencies)
            {
                if (maestroTaskDependencyMap.TryGetValue(dependency, out List<IMaestroTask> dependencyTasks))
                {
                    if (dependencyTasks.Contains(task))
                        throw new InvalidOperationException($"{task.GetType()} cannot be added because a circular reference would occur");
                }
                
                if (!maestroTaskDependencyMap[task].Contains(dependency))
                    maestroTaskDependencyMap[task].Add(dependency);
            }
        }

        public void Add(params IMaestroTaskWithDependency[] tasks)
        {
            foreach (IMaestroTaskWithDependency task in tasks) Add(task, task.Dependencies);
        }

        public void Add(params IMaestroTask[] tasks)
        {
            foreach (IMaestroTask task in tasks) Add(task);
        }

        public async Task Start()
        {
            List<MaestroTaskInfo> allMaestroNodes = new List<MaestroTaskInfo>();
            Dictionary<IMaestroTask, MaestroTaskInfo> maestroTasksLookup =
                new Dictionary<IMaestroTask, MaestroTaskInfo>();
            
            foreach (KeyValuePair<IMaestroTask, List<IMaestroTask>> pair in maestroTaskDependencyMap)
            {
                if (!maestroTasksLookup.ContainsKey(pair.Key))
                {
                    MaestroTaskInfo taskInfo = new MaestroTaskInfo(pair.Key);
                    maestroTasksLookup.Add(pair.Key, taskInfo);
                    allMaestroNodes.Add(taskInfo);
                }
                foreach (IMaestroTask dependency in pair.Value)
                {
                    if (!maestroTasksLookup.ContainsKey(dependency))
                    {
                        MaestroTaskInfo dependencyInfo = new MaestroTaskInfo(dependency);
                        maestroTasksLookup.Add(dependency, dependencyInfo);
                        allMaestroNodes.Add(dependencyInfo);
                    }
                    maestroTasksLookup[pair.Key].DependenciesInternal.Add(maestroTasksLookup[dependency]);
                }
            }
            
            List<MaestroTaskInfo> allReady = allMaestroNodes.FindAll(item => item.HasPrerequisites && !item.IsDone);
            bool repeat = !IsAllDone(allMaestroNodes);
            while (repeat)
            {
                List<Task> runningTasks = new List<Task>(allReady.Count);
                foreach (MaestroTaskInfo maestroNode in allReady)
                {
                    maestroNode.Verbose = Verbose;
                    maestroNode.OnTaskCompleted += OnTaskCompleted;
                    runningTasks.Add(maestroNode.DoTaskOperation());
                }

                await Task.WhenAll(runningTasks);
                allReady = allMaestroNodes.FindAll(item => item.HasPrerequisites && !item.IsDone);
                repeat = !IsAllDone(allMaestroNodes);

                if (repeat && allReady.Count == 0)
                {
                    OnFinished?.Invoke(this, false);
                    Debug.LogError("Unable to get the next step to get the references, no Task has the requirements to run.");
                    return;
                }
            }

            OnFinished?.Invoke(this, IsSuccess(allMaestroNodes));
        }

        private bool IsAllDone(List<MaestroTaskInfo> allMaestroNodes)
        {
            foreach (MaestroTaskInfo task in allMaestroNodes)
            {
                if (!task.IsDone)
                    return false;
            }

            return true;
        }

        private bool IsSuccess(List<MaestroTaskInfo> allMaestroNodes)
        {
            foreach (MaestroTaskInfo task in allMaestroNodes)
            {
                if (!task.IsCompleted || task.HasError)
                    return false;
            }

            return true;
        }
        
        public void Dispose()
        {
            maestroTaskDependencyMap.Clear();
        }

        private void OnTaskCompleted(MaestroTaskInfo taskInfo, bool result)
        {
            OnTaskFinished?.Invoke(taskInfo, result);
        }
    }
}