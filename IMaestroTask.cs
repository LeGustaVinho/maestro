using System.Threading.Tasks;

namespace LegendaryTools.Maestro
{
    public interface IMaestroTask
    {
        bool Enabled { get; }
        int TimeOut { get; }
        bool ThreadSafe { get; }
        Task<bool> DoTaskOperation();
    }
}