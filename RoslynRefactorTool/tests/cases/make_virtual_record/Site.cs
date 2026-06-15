namespace Demo;

public record MvRecordService
{
    public void Ping() { }
}

public class MvRecordWorker
{
    public void Run(MvRecordService svc) => svc.Ping();
}
