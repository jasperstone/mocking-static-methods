# Static Method Refactoring Coverage Gaps Analysis

## Executive Summary

**Current Coverage**: 1,087 / 5,154 production Mode #1 sites = **21.1%** baseline
**User Goal**: Improve to **90%+**
**Gap**: ~3,800 sites (74%) cannot be refactored with current transforms

---

## What ARE We Refactoring? (46% of wrapper cases, 71% of parameterize cases)

The tool successfully refactors:
1. **Instance method calls** on injectable receiver objects (stored in fields)
2. **Static extension methods** where we can inject the extended type
3. **Dependency injection patterns** where the dependency is visible in constructor

Example of successful wrapper_interface refactor:
```csharp
// BEFORE
public class OrderService
{
    public void Process()
    {
        var logger = LoggerExtensions.GetLogger();  // ← can't inject
    }
}

// AFTER (if receiver is constructor-reachable)
public interface ILoggerWrapper { Logger Get(); }
public class OrderService
{
    private readonly ILoggerWrapper _logger;
    public OrderService(ILoggerWrapper logger) => _logger = logger;
    
    public void Process()
    {
        var logger = _logger.Get();  // ← now injectable/mockable
    }
}
```

---

## What AREN'T We Refactoring? (54% rejection on wrapper, 29% on parameterize)

### 1. **Framework/Runtime Types with No Source** (Most Common)

**Example**: `HttpClient`, `IServiceProvider` (25+ cases in test set)

```csharp
// These FAIL refactoring:
client.GetAsync(url);                    // HttpClient — framework type
serviceProvider.GetRequiredService<T>(); // IServiceProvider — framework type
```

**Why**: 
- Source code unavailable → can't add `virtual` modifier (make_virtual blocked)
- Can't wrap interface (framework types are external dependencies)
- Receiver is "frozen" at runtime

**Impact**: Blocks **wrapper_interface** entirely for 25 cases (~50% of failures)

---

### 2. **Receiver Not Constructor-Reachable** (15 cases)

**Examples**:
- Local variables inside methods
- Loop variables
- Object initializers
- Method return values assigned to locals

```csharp
// BLOCKED: receiver is local variable
foreach (var item in collection)
{
    item.Process();  // ← item is loop var, not injectable
}

// BLOCKED: receiver from method call
var result = GetService().DoWork();  // ← result is local var

// BLOCKED: from object initializer
var config = new Configuration 
{
    logger = LoggerExtensions.GetLogger()  // ← assigned in initializer
};
```

**Why**: 
- Constructor can't declare a parameter for every local variable
- Breaks encapsulation to inject locals

**Impact**: Blocks **wrapper_interface** for 15 cases (~28% of failures)

**Could be fixable with**: Parameterize dependency (add param to method), but requires method modification

---

### 3. **Receiver Not in Method Scope** (Parameterize blocker)

**Examples**:
- HttpClient from constructor field
- Dependencies from outer scope (closures)
- Static fields

```csharp
private HttpClient _client;  // ← field, but can't parameterize existing methods

public void SendRequest(string url)
{
    _client.GetAsync(url);  // ← blocked for parameterize
}
```

**Why**:
- Parameterize adds a dependency parameter to the method
- But if receiver is already a field, adding a parameter is redundant/confusing
- Semantic check prevents adding a parameter that duplicates field

**Impact**: Blocks **parameterize_dependency** for 14+ cases

---

### 4. **Unbound Receiver** (6 cases each transform)

**Examples**:
- Receivers through `dynamic`
- Generic type parameter resolution fails
- Cross-project binding failures
- Assembly reference mismatch

```csharp
dynamic obj = GetDynamicObject();
obj.DoSomething();  // ← can't bind receiver type semantically

// Or cross-assembly:
ExternalType instance = GetExternalInstance();  // ← ref assembly doesn't match
```

**Why**: 
- Roslyn semantic analysis can't determine receiver type
- Without type info, can't validate refactoring safety

**Impact**: ~6 cases per transform (consistent)

---

### 5. **Static Calls on `this`** (6 cases)

**Examples**:
```csharp
public class OrderService
{
    public void Process()
    {
        // Static call on current instance (unusual but legal)
        ProcessorExtensions.Process(this);  // ← called as "static this"
    }
}
```

**Why**:
- Already on instance → can make method virtual instead
- But tool tries wrapper first, fails because `this` can't be injected

**Impact**: Could be fixed with **make_virtual pattern** instead

---

## Root Cause Classification

| Pattern | Count | Refactorability | Root Cause | Solution |
|---------|-------|-----------------|-----------|----------|
| Framework/runtime types | 25+ | ❌ Hard blocked | No source, frozen at runtime | Change approach entirely |
| Local/loop variables | 15+ | ⚠️ Partially fixable | Can't inject locals | Use parameterize + refactor body |
| Field receivers | 14+ | ⚠️ Partially fixable | Redundant with field | Use make_virtual instead |
| Dynamic/unbound | 6+ | ❌ Hard blocked | Type binding fails | Skip these (env. issue) |
| Static on `this` | 6+ | ✅ Fixable | Wrong pattern chosen | Try make_virtual first |
| Edge cases (sealed, private, etc.) | 5+ | ❌ Hard blocked | Language constraints | Can't refactor by design |

---

## Coverage Math

Assuming test set proportions hold across 5,154 production sites:

- **Wrapper** pattern success: ~46% → ~2,371 sites
- **Parameterize** pattern success: ~71% → ~3,659 sites
- **Combined with make_virtual**: estimated ~55-65%

**To reach 90%**, we need to handle:
- ✅ 46% wrapper + 71% parameterize = ~60% covered
- ❌ Need +30% more coverage

**Where the +30% comes from**:
1. Framework types → new specialized handling (~10-15%)
2. Local variables → enhanced parameterize (~5-10%)
3. Field receivers → make_virtual pattern (~5-10%)
4. Edge cases & make_virtual improvements (~3-5%)

---

## Recommended Fixes (Ranked by Impact)

### High Impact (10-15% each)

**1. Framework Type Handler**
- Detect `System.Net.Http.HttpClient`, `IServiceProvider`, etc.
- Don't try wrapper_interface (will fail)
- Skip to parameterize or custom patterns

**2. Enhanced Parameterize for Local Variables**
- Current: Fails on locals (receiver_not_ctor_reachable)
- Future: Add parameter to method, inject local at call site
- Risk: Creates new method signatures

**3. Make Virtual Pattern Prioritization**
- Current: Wrapper first, then parameterize, then make_virtual
- Future: For field receivers and static-on-this, try make_virtual first
- Benefit: Many patterns are actually virtual-able

### Medium Impact (5-10% each)

**4. Lazy Injection Pattern**
- For uncapturable locals: use factory/func injection
- Trade off: Adds complexity

**5. Method Extraction**
- Detect where refactoring needs method extraction
- Move code to make it injectable
- Risk: Significant code gen

### Lower Priority (1-5% each)

**6. Cross-Assembly Binding**
- Fix unbound_receiver cases
- Requires better reference resolution

**7. Dynamic Type Handling**
- Skip safely (too risky)
- Or use runtime type checking

---

## Test Data Source

From `tools/generation/results/*2026-06-19.csv`:
- **wrapper_first100_after_receiver_root_source_fix**: 46/100 applicable
- **parameterize_first100_after_receiver_root_symbol**: 71/100 applicable
- Combined rejection reasons: 54 + 29 = 83 out of 200 total

This represents the tool's current "reach" on 100-site samples from each transform type.
