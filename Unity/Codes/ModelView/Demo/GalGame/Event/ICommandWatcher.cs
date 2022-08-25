namespace ET
{
    public interface ICommandWatcher
    {
        ETTask Run(GalGameEngineComponent engine, GalGameEnginePara para);
    }
}