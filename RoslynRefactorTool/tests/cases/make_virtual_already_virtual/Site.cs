namespace Demo;

public class MvVirtualService
{
    public virtual void Ping() { }
}

public class MvVirtualWorker
{
    public void Run(MvVirtualService svc) => svc.Ping();
}
