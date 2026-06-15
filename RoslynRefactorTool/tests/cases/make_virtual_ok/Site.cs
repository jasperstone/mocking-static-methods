namespace Demo;

public class MvCalculator
{
    /// <summary>Adds two numbers together.</summary>
    /// <param name="a">First addend.</param>
    /// <param name="b">Second addend.</param>
    public int Add(int a, int b) => a + b;
}

public class MvOkWorker
{
    public int Run(MvCalculator calc) => calc.Add(1, 2);
}
