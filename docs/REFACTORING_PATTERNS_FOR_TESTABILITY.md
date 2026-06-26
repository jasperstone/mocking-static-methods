# Refactoring Patterns for Testability

This guide shows patterns that enable testing of code with static dependencies. Use these as reference when asking models to improve testability.

**TL;DR**: Static dependencies (logging, HTTP, config) block unit tests. Replace them with injected dependencies so tests can mock them.

---

## Pattern 1: Wrapper Interface Injection (Constructor)

**When to use**: Instance class with static method calls; dependency is straightforward.

**Example: ILogger in Event Handler**
```csharp
// ❌ BEFORE: Hard to test (can't mock logging)
public class GracePeriodConfirmedIntegrationEventHandler
{
    public async Task Handle(GracePeriodConfirmedIntegrationEvent evt)
    {
        // Static call blocks testing
        _logger.LogInformation("Order confirmed for {OrderId}", evt.OrderId);
    }
}

// ✅ AFTER: Injected and testable
public class GracePeriodConfirmedIntegrationEventHandler
{
    private readonly ILoggerWrapper _loggerWrapper;
    
    public GracePeriodConfirmedIntegrationEventHandler(ILoggerWrapper loggerWrapper)
    {
        _loggerWrapper = loggerWrapper;
    }
    
    public async Task Handle(GracePeriodConfirmedIntegrationEvent evt)
    {
        _loggerWrapper.LogInformation("Order confirmed for {OrderId}", evt.OrderId);
    }
}

// ✅ TEST CODE
[Fact]
public async Task Handle_LogsOrderConfirmation()
{
    var mockLogger = new Mock<ILoggerWrapper>();
    var handler = new GracePeriodConfirmedIntegrationEventHandler(mockLogger);
    
    await handler.Handle(new GracePeriodConfirmedIntegrationEvent { OrderId = 123 });
    
    mockLogger.Verify(x => x.LogInformation("Order confirmed for {OrderId}", 123));
}
```

**Real Success**: eShop:0016 (LogInformation on GracePeriodConfirmedIntegrationEventHandler)
- Status: ✅ Applied, build passed, 2 call sites rewritten
- Receiver: ILogger (most common for this pattern)

**Pattern**: 
- Create wrapper interface: `IXxxWrapper`
- Add constructor parameter
- Replace all static calls with wrapper calls

---

## Pattern 2: Dependency Overload (Parameterize)

**When to use**: Can't easily change constructor; multiple call sites with different test needs.

**Example: IServiceProvider with Multiple Contexts**
```csharp
// ❌ BEFORE: Single signature, hard to test different dependency states
public class DebugProxyLauncher
{
    public string LaunchAndGetUrl(string debugServer, bool debug)
    {
        var serviceProvider = GetServiceProvider();  // Static call!
        var proxy = serviceProvider.GetRequiredService<IDebugProxy>();
        return proxy.Launch(debugServer, debug);
    }
}

// ✅ AFTER: Overload lets tests inject mock
public class DebugProxyLauncher
{
    public string LaunchAndGetUrl(string debugServer, bool debug)
    {
        var serviceProvider = GetServiceProvider();
        return LaunchAndGetUrl(debugServer, debug, new IServiceProviderWrapper(serviceProvider));
    }
    
    public string LaunchAndGetUrl(
        string debugServer, 
        bool debug, 
        IServiceProviderWrapper serviceProviderWrapper)
    {
        var proxy = serviceProviderWrapper.GetRequiredService<IDebugProxy>();
        return proxy.Launch(debugServer, debug);
    }
}

// ✅ TEST CODE
[Fact]
public void LaunchAndGetUrl_WithMockServiceProvider()
{
    var mockServiceProvider = new Mock<IServiceProviderWrapper>();
    var mockProxy = new Mock<IDebugProxy>();
    mockServiceProvider.Setup(x => x.GetRequiredService<IDebugProxy>())
        .Returns(mockProxy);
    
    var launcher = new DebugProxyLauncher();
    var url = launcher.LaunchAndGetUrl("localhost", true, mockServiceProvider);
    
    Assert.NotEmpty(url);
}
```

**Real Success**: aspnetcore:0084 (GetRequiredService on DebugProxyLauncher)
- Status: ✅ Applied, build passed, 2 files changed
- Receiver: IServiceProvider

**Pattern**:
- Keep original method unchanged (backward compatible)
- Add overload with extra dependency parameter
- Original delegates to overload with real dependency
- Tests call overload with mock

---

## Pattern 3: Static Field Injection (For Static Classes)

**When to use**: Static methods that need dependency injection; replacement via static setter.

**Example: Static Logger in Singleton**
```csharp
// ❌ BEFORE: Hard to test (static logger, can't replace)
public class IISHttpServer
{
    private static readonly ILogger _logger = 
        LoggerFactory.Create(b => b.AddConsole());
    
    public static void HandleRequest(HttpRequest req)
    {
        _logger.LogError("Request failed: {Error}", error);
    }
}

// ✅ AFTER: Static field + setter for test injection
public class IISHttpServer
{
    private static ILoggerWrapper _loggerWrapper = 
        new ILoggerWrapper(LoggerFactory.Create(b => b.AddConsole()));
    
    public static void SetLoggerWrapperForTesting(ILoggerWrapper logger)
    {
        _loggerWrapper = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public static void HandleRequest(HttpRequest req)
    {
        _loggerWrapper.LogError("Request failed: {Error}", error);
    }
}

// ✅ TEST CODE
[Fact]
public void HandleRequest_LogsErrors()
{
    var mockLogger = new Mock<ILoggerWrapper>();
    IISHttpServer.SetLoggerWrapperForTesting(mockLogger);
    
    IISHttpServer.HandleRequest(new HttpRequest { Error = "test" });
    
    mockLogger.Verify(x => x.LogError("Request failed: {Error}", "test"));
}
```

**Real Success**: aspnetcore:0599 (LogError on IISHttpServer)
- Status: ✅ Applied, build passed, 3 call sites rewritten
- Receiver: ILogger (best fit for static field injection)

**Pattern**:
- Replace `new Dependency()` with injected field
- Add public static setter with null check
- Replace all call sites with field reference
- Tests call setter before exercising code

---

## Pattern 4: Virtual Method Override (Subclass-and-Override)

**When to use**: Logic deeply embedded; can't easily change architecture; willing to subclass for testing.

**Example: Virtual Logging Point**
```csharp
// ❌ BEFORE: Core method logs directly (hard to replace behavior)
public class OrderProcessor
{
    public void Process(Order order)
    {
        // ... processing ...
        Logger.Log("Order processed");  // Static, can't replace
    }
}

// ✅ AFTER: Protected virtual method for subclass override
public class OrderProcessor
{
    public void Process(Order order)
    {
        // ... processing ...
        OnOrderProcessed(order);
    }
    
    protected virtual void OnOrderProcessed(Order order)
    {
        Logger.Log("Order processed");
    }
}

// ✅ TEST CODE
public class TestableOrderProcessor : OrderProcessor
{
    public List<Order> ProcessedOrders { get; } = new();
    
    protected override void OnOrderProcessed(Order order)
    {
        ProcessedOrders.Add(order);  // Capture instead of logging
    }
}

[Fact]
public void Process_CapturesOrderEvents()
{
    var processor = new TestableOrderProcessor();
    processor.Process(new Order { Id = 123 });
    
    Assert.Single(processor.ProcessedOrders);
}
```

**Pattern**:
- Extract dependency call to protected virtual method
- Subclass in test and override
- Verify behavior through capture list or mock calls

---

## How to Apply These Patterns

### Step 1: Identify Static Dependency
```csharp
Logger.LogInformation(...)        // ← static call blocks testing
HttpClient.GetAsync(...)          // ← static call blocks testing
ServiceProvider.GetService(...)   // ← static call blocks testing
```

### Step 2: Choose Pattern

| Pattern | Best For | Effort | Compatibility |
|---------|----------|--------|---------------|
| **Wrapper Injection** | Instance classes, simple deps | Low | 100% backward compatible |
| **Dependency Overload** | Multiple call sites, variant needs | Medium | 100% backward compatible |
| **Static Field Injection** | Static methods in instance classes | Medium | 100% backward compatible |
| **Virtual Override** | Complex logic, reluctant to refactor | High | 100% backward compatible |

### Step 3: Generate Wrapper Interface
Tool automatically creates `IXxxWrapper` interface:
```csharp
public interface ILoggerWrapper
{
    void LogInformation(string message, params object[] args);
    void LogError(Exception? ex, string message, params object[] args);
    void LogWarning(string message, params object[] args);
    // ... other methods ...
}
```

### Step 4: Update Call Sites
Find all static calls and replace:
```csharp
Logger.LogInformation(...)  →  _loggerWrapper.LogInformation(...)
```

### Step 5: Write Tests
```csharp
var mockLogger = new Mock<ILoggerWrapper>();
var handler = new MyHandler(mockLogger);
// ... exercise code ...
mockLogger.Verify(x => x.LogInformation(...));
```

---

## Receiver Types (Most Common)

| Receiver | Pattern | Success Rate | Best Use |
|----------|---------|--------------|----------|
| **ILogger** | Wrapper Injection | 6.1% | Logging, event handlers |
| **IServiceProvider** | Dependency Overload | 5%+ | Configuration, service resolution |
| **HttpClient** | Wrapper Injection | 2%+ | HTTP calls, API clients |
| **IConfigurationBuilder** | Dependency Overload | 3%+ | Configuration setup |

---

## Common Blockers (What Won't Work)

❌ **Static methods in static classes** (language constraint)
- Can't inject; use dependency injection at entry point instead

❌ **Unbound receivers** (receiver not accessible)
- Can't be refactored; need architectural change

❌ **Complex state dependencies** (multiple interdependent statics)
- Consider extracting to service class first

---

## Next Steps

When you see code with static dependencies and ask a model to make it testable:

1. **Share this guide** so the model understands the patterns
2. **Run the refactoring tool** to automatically apply patterns
3. **Write tests** against the injected dependencies
4. **Verify** tests pass with real and mocked dependencies

**Tool availability**: These refactorings can be applied automatically via:
```bash
dotnet RoslynRefactorTool.dll \
  --transform wrapper_interface \
  --target-file MyCode.cs \
  --receiver-type "System.ILogger"
```
