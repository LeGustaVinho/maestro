namespace LegendaryTools.Systems.MaestroV2
{
    public interface IMaestroTaskWithDependency : IMaestroTask
    {
        IMaestroTask[] Dependencies { get; set; }
    }
}