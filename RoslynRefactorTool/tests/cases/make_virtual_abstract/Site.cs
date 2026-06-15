namespace Demo;

public abstract class MvAbstractService
{
    public abstract void Ping();
}

public class MvAbstractWorker
{
    public void Run(MvAbstractService svc) => svc.Ping();
}
