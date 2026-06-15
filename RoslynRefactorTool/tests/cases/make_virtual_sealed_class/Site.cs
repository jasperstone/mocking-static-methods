namespace Demo;

public sealed class MvSealedService
{
    public void Ping() { }
}

public class MvSealedWorker
{
    public void Run(MvSealedService svc) => svc.Ping();
}
