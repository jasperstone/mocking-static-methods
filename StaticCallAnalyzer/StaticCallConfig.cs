public static class StaticCallConfig
{
    public static readonly List<(string ClassName, string MethodName)> Patterns = new()
    {
        ("DateTime", "Now"),
        ("DateTime", "UtcNow"),
        ("HttpClient", "GetAsync"),
        ("File", "Exists")
    };
}
