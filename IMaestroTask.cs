using System.Threading.Tasks;

namespace LegendaryTools.Maestro
{
    public interface IMaestroTask
    {
        int TimeOut { get; }
        bool ThreadSafe { get; }
        Task<bool> DoTaskOperation();
    }
}