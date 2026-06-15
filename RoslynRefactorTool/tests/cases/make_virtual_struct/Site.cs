namespace Demo;

public struct MvStructService
{
    public void Ping() { }
}

public class MvStructWorker
{
    public void Run(MvStructService svc) => svc.Ping();
}
