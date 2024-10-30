using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LegendaryTools.Maestro
{
    public class Maestro :  IMaestro
    {
        public event Action<Maestro, bool> OnFinished;
        public event Action<MaestroTaskInfo, bool> OnTaskFinished;

        private readonly Dictionary<IMaestroTask, List<IMaestroTask>> maestroTaskDependencyMap =
            new Dictionary<IMaestroTask, List<IMaestroTask>>();

        public void Add(IMaestroTask task, params IMaestroTask[] dependencies)
        {
            if (!maestroTaskDependencyMap.ContainsKey(task))
                maestroTaskDependencyMap.Add(task, new List<IMaestroTask>());

            foreach (IMaestroTask dependency in dependencies)
            {
                List<IMaestroTask> cyclePath = GetDependencyPath(dependency, task);
                if (cyclePath != null)
                {
                    List<string> cycleTasks = cyclePath.Select(t => t.GetType().Name).ToList();
                    cycleTasks.Add(task.GetType().Name);
                    string cycleMessage = string.Join(" -> ", cycleTasks);
                    throw new InvalidOperationException(
                        $"Adding a dependency from {task.GetType().Name} to {dependency.GetType().Name} would create a cyclic dependency: {cycleMessage}");
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
            
            List<MaestroTaskInfo> allReady = allMaestroNodes.FindAll(item => item.HasPrerequisites && !item.IsDone && item.Enabled);
            bool repeat = !IsAllDone(allMaestroNodes);
            while (repeat)
            {
                List<Task> runningTasks = new List<Task>(allReady.Count);
                foreach (MaestroTaskInfo maestroNode in allReady)
                {
                    maestroNode.OnTaskCompleted += OnTaskCompleted;
                    runningTasks.Add(maestroNode.DoTaskOperation());
                }

                await Task.WhenAll(runningTasks);
                allReady = allMaestroNodes.FindAll(item => item.HasPrerequisites && !item.IsDone);
                repeat = !IsAllDone(allMaestroNodes);

                if (repeat && allReady.Count == 0)
                {
                    OnFinished?.Invoke(this, false);
                    Debugger.LogError<Maestro>("Maestro execution could not continue because no tasks were ready, no tasks had their" +
                                   " prerequisites completed. This usually occurs due to cyclic dependencies.");
                    return;
                }
            }

            OnFinished?.Invoke(this, IsSuccess(allMaestroNodes));
        }

        private List<IMaestroTask> GetDependencyPath(IMaestroTask startTask, IMaestroTask targetTask)
        {
            // Perform a depth-first search to find a path from startTask to targetTask
            HashSet<IMaestroTask> visited = new HashSet<IMaestroTask>();
            Stack<(IMaestroTask task, List<IMaestroTask> path)> stack =
                new Stack<(IMaestroTask task, List<IMaestroTask> path)>();
            stack.Push((startTask, new List<IMaestroTask> { startTask }));

            while (stack.Count > 0)
            {
                (IMaestroTask currentTask, List<IMaestroTask> path) = stack.Pop();

                if (currentTask == targetTask)
                {
                    return path;
                }

                if (!visited.Contains(currentTask))
                {
                    visited.Add(currentTask);
                    if (maestroTaskDependencyMap.TryGetValue(currentTask, out List<IMaestroTask> dependencies))
                    {
                        foreach (IMaestroTask dep in dependencies)
                        {
                            List<IMaestroTask> newPath = new List<IMaestroTask>(path) { dep };
                            stack.Push((dep, newPath));
                        }
                    }
                }
            }

            return null;
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