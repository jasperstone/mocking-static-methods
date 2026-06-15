namespace Demo;

public static class MvStaticUtil
{
    public static void Log(string message) { }
}

public class MvStaticWorker
{
    public void Run() => MvStaticUtil.Log("hello");
}
