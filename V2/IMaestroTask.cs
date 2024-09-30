using System.Threading.Tasks;

namespace LegendaryTools.Systems.MaestroV2
{
    public interface IMaestroTask
    {
        int TimeOut { get; }
        bool ThreadSafe { get; }
        Task<bool> DoTaskOperation();
    }
}