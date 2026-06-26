# Enhanced Wrapper Pattern: Static Utility Wrapping

## Problem
Framework types like `HttpClient`, `IServiceProvider`, `LoggerExtensions` have no source and can't be injected directly. Current approach fails on these.

## Solution: Wrap the Static Utility, Not the Receiver

Instead of trying to inject the receiver object, create an interface wrapping the **static utility methods** themselves.

### Example 1: HttpClient Static Extensions

**BEFORE**
```csharp
public class OrderProcessor
{
    public async Task FetchOrder(string id)
    {
        var client = new HttpClient();
        var response = await client.GetAsync($"https://api.example.com/orders/{id}");
        // ...
    }
}
```

**AFTER - Current wrapper fails** ❌
- Can't inject HttpClient (no source, framework type)
- Rejected as `no_receiver_source`

**AFTER - Enhanced wrapper approach** ✅
```csharp
// Create wrapper interface for the static utility
public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string url);
    Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
}

public class HttpClientWrapper : IHttpClientWrapper
{
    private readonly HttpClient _client;
    
    public HttpClientWrapper() => _client = new HttpClient();
    
    public Task<HttpResponseMessage> GetAsync(string url) => _client.GetAsync(url);
    public Task<HttpResponseMessage> PostAsync(string url, HttpContent content) 
        => _client.PostAsync(url, content);
}

public class OrderProcessor
{
    private readonly IHttpClientWrapper _httpClient;
    
    public OrderProcessor(IHttpClientWrapper httpClient)
        => _httpClient = httpClient;
    
    public async Task FetchOrder(string id)
    {
        var response = await _httpClient.GetAsync($"https://api.example.com/orders/{id}");
        // ...
    }
}
```

### Example 2: IServiceProvider.GetRequiredService<T>()

**BEFORE**
```csharp
public class UserService
{
    public void RegisterUser(User user)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        logger.Info($"Registering {user.Name}");
    }
}
```

**AFTER - Enhanced wrapper**
```csharp
public interface IServiceProviderWrapper
{
    T GetRequiredService<T>() where T : class;
}

public class ServiceProviderWrapper : IServiceProviderWrapper
{
    private readonly IServiceProvider _provider;
    
    public ServiceProviderWrapper(IServiceProvider provider)
        => _provider = provider;
    
    public T GetRequiredService<T>() where T : class
        => _provider.GetRequiredService<T>();
}

public class UserService
{
    private readonly IServiceProviderWrapper _services;
    
    public UserService(IServiceProviderWrapper services)
        => _services = services;
    
    public void RegisterUser(User user)
    {
        var logger = _services.GetRequiredService<ILogger>();
        logger.Info($"Registering {user.Name}");
    }
}
```

### Example 3: Static Extension Methods (ProcessorExtensions)

**BEFORE**
```csharp
public class OrderService
{
    public void Process()
    {
        ProcessorExtensions.Process(this);  // Static call on instance
    }
}
```

**AFTER - Enhanced wrapper**
```csharp
public interface IProcessorExtensionsWrapper
{
    void Process(OrderService service);
}

public class ProcessorExtensionsWrapper : IProcessorExtensionsWrapper
{
    public void Process(OrderService service)
        => ProcessorExtensions.Process(service);
}

public class OrderService
{
    private readonly IProcessorExtensionsWrapper _processors;
    
    public OrderService(IProcessorExtensionsWrapper processors)
        => _processors = processors;
    
    public void Process()
    {
        _processors.Process(this);
    }
}
```

## Key Differences from Current Wrapper Pattern

| Aspect | Current Wrapper | Enhanced Wrapper |
|--------|-----------------|------------------|
| **What's being wrapped** | The receiver object instance | The static utility class/extensions |
| **Interface contains** | Instance methods that delegate to receiver | Static methods wrapped as instance methods |
| **Injection point** | Receiver object | Static utility reference |
| **Applicable to** | Instance fields, constructor params | Framework types, external utilities |
| **Failure cases** | Framework types, external dependencies | N/A (wraps the utility, not the receiver) |

## Coverage Impact

From analysis of rejection patterns:

- **`no_receiver_source`** (25 cases): 
  - Current: ❌ Hard blocked
  - Enhanced: ✅ Wrap the static utility
  - **Impact**: ~25 cases recovered

- **`receiver_is_this` / static on instance** (6 cases):
  - Current: ❌ Rejected as non-injectable
  - Enhanced: ✅ Wrap extension methods
  - **Impact**: ~6 cases recovered

- **Local variables with unbound types** (partial coverage):
  - Current: ❌ Can't inject local
  - Enhanced: Still problematic, but parameterize might help
  - **Impact**: Partial, needs investigation

**Estimated new coverage**: 21.1% + (31 / 5,154) ≈ **21.7%** on first pass

But when combined with other patterns:
- Wrapper (current): ~46%
- Enhanced wrapper for static utilities: +~8-12%
- Parameterize + make_virtual: ~20%
- **Total**: ~65-74% (getting closer to 90%)

## Implementation Strategy

### Phase 1: Detect Static Utility Patterns
1. Identify calls to static methods on types with no source
2. Recognize extension method calls on framework types
3. Classify as "static_utility_pattern"

### Phase 2: Generate Wrapper Interface
1. Extract method signatures from static utility
2. Create `I{UtilityName}Wrapper` interface
3. Create concrete wrapper class
4. Generate constructor injection

### Phase 3: Rewrite Call Sites
1. Replace static calls with instance method calls through wrapper
2. Ensure type safety (generics, ref parameters)

## Challenges

1. **Static method overloading**: HttpClient has multiple GetAsync overloads
2. **Generic methods**: ServiceProvider.GetRequiredService<T>() must preserve generics
3. **Out/ref parameters**: Extension methods with ref params
4. **Naming conflicts**: When multiple utilities need wrappers
5. **Dependency resolution**: How does DI know which wrapper to create?

## Detection vs. Applicability

This enhanced pattern is applicable when:
- ✅ Static method on framework/external type
- ✅ Method has extractable signature
- ✅ No circular dependencies created
- ❌ Static method is pure utility (no side effects assumed in test)
- ❌ Generics preserve correctly through wrapper

Should NOT wrap:
- ❌ Static methods on user types (use make_virtual)
- ❌ Methods called only once (overkill)
- ❌ Methods with complex parameter types that can't be wrapped
