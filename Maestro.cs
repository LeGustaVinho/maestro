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

        private readonly Dictionary<IMaestroTask, MaestroTaskInfo> maestroNodeMapping =
            new Dictionary<IMaestroTask, MaestroTaskInfo>();

        public Maestro(bool verbose = false)
        {
            Verbose = true;
        }

        public void Add(IMaestroTask task, params IMaestroTask[] dependencies)
        {
            List<MaestroTaskInfo> dependencyTasks = new List<MaestroTaskInfo>();
            foreach (IMaestroTask dependency in dependencies)
            {
                if (!maestroNodeMapping.TryGetValue(dependency, out MaestroTaskInfo mappedTask))
                {
                    MaestroTaskInfo newDependencyTaskInfo = new MaestroTaskInfo(dependency);
                    maestroNodeMapping.Add(dependency, newDependencyTaskInfo);
                    dependencyTasks.Add(newDependencyTaskInfo);
                }
                else
                {
                    dependencyTasks.Add(mappedTask);
                }
            }

            foreach (MaestroTaskInfo dependencyTask in dependencyTasks)
            {
                if (dependencyTask.MaestroTaskObject == task)
                    throw new InvalidOperationException($"{task.GetType()} cannot be added because a circular reference would occur");
            }

            if (!maestroNodeMapping.ContainsKey(task))
            {
                MaestroTaskInfo maestroTaskInfo = new MaestroTaskInfo(task, dependencyTasks.ToArray());
                maestroNodeMapping.Add(task, maestroTaskInfo);
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
            List<MaestroTaskInfo> allMaestroNodes = maestroNodeMapping.Values.ToList();
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
            maestroNodeMapping.Clear();
        }

        private void OnTaskCompleted(MaestroTaskInfo taskInfo, bool result)
        {
            OnTaskFinished?.Invoke(taskInfo, result);
        }
    }
}