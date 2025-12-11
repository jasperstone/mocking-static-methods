# Agent Framework Tools - Usage Examples

## Overview

The `agent_tools.py` module provides tools designed to work with Microsoft's Agent Framework. These tools can be used standalone or integrated with Azure OpenAI agents.

## Tool 1: analyze_static_method()

**Purpose**: Analyze a static method in a C# file to extract context

**Parameters**:
- `file_path` (str): Path to the C# file
- `method_signature` (str): The static method signature to analyze

**Returns**: Analysis text with context

**Example**:

```python
from agent_tools import TestGenerationTools

tools = TestGenerationTools()

result = tools.analyze_static_method(
    file_path="cloned_repos/abp/framework/src/Volo.Abp.Core/DateHelper.cs",
    method_signature="DateTime.Now"
)

print(result)
# Output:
# Found method 'DateTime.Now' in cloned_repos/abp/framework/src/Volo.Abp.Core/DateHelper.cs
# 
# Context:
# public static DateTime GetCurrentTime()
# {
#     return DateTime.Now;  // This method uses DateTime.Now
# }
# ...
```

**Use Case**: 
- Agent calls this to understand the context around static method usage
- Helps agent decide what kind of mock is needed
- Identifies dependencies and calling patterns

---

## Tool 2: generate_mock_test()

**Purpose**: Generate a mock-based unit test for a static method

**Parameters**:
- `class_name` (str): Name of the class being tested
- `method_name` (str): Name of the static method
- `return_type` (str): Return type of the method
- `parameters` (str, optional): Comma-separated parameter list

**Returns**: Complete test file content (xUnit + Moq)

**Example**:

```python
from agent_tools import TestGenerationTools

tools = TestGenerationTools()

# Generate test for DateTime.Now usage
test_code = tools.generate_mock_test(
    class_name="DateHelper",
    method_name="DateTime_Now",
    return_type="DateTime",
    parameters=""
)

print(test_code)
# Output:
# using Xunit;
# using Moq;
# using System;
#
# namespace GeneratedTests
# {
#     /// <summary>
#     /// Auto-generated test for DateHelper.DateTime_Now
#     /// </summary>
#     public class DateHelper_DateTime_Now_Tests
#     {
#         [Fact]
#         public void Test_DateTime_Now_MockedSuccessfully()
#         {
#             // Arrange
#             // TODO: Set up your mocks and test data here
#             
#             // Act
#             // TODO: Call the method being tested
#             // Example: var result = TargetClass.DateTime_Now();
#             
#             // Assert
#             // TODO: Verify the result or mock interactions
#             // Example: Assert.NotNull(result);
#             // Example: mockObject.Verify(x => x.SomeMethod(), Times.Once);
#             
#             // Placeholder assertion
#             Assert.True(true);
#         }
#     }
# }
```

**Use Case**:
- Agent calls to generate test scaffolds
- Output can be enhanced by agent with specific mock setup
- Provides consistent test structure

---

## Tool 3: get_moq_setup_template()

**Purpose**: Get a template for setting up mocks with Moq

**Parameters**:
- `mock_type` (str): The type to mock (e.g., "IRepository")

**Returns**: Moq setup template code

**Example**:

```python
from agent_tools import TestGenerationTools

tools = TestGenerationTools()

template = tools.get_moq_setup_template(mock_type="IFileService")

print(template)
# Output:
# // Setup mock for IFileService
# var mockIFileService = new Mock<IFileService>();
#
# // Configure mock behavior
# mockIFileService
#     .Setup(x => x.SomeMethod())
#     .Returns(/* expected value */);
#
# // Use the mock
# var instance = mockIFileService.Object;
# // Pass instance to your code...
#
# // Verify interactions
# mockIFileService.Verify(x => x.SomeMethod(), Times.Once);
```

**Use Case**:
- Agent uses this to understand Moq patterns
- Helps agent generate proper mock setup code
- Reference for mock verification patterns

---

## Integration with Microsoft Agent Framework

### Standalone Usage (Current PoC)

```python
from agent_tools import TestGenerationTools

tools = TestGenerationTools()

# Step 1: Analyze
analysis = tools.analyze_static_method(
    "path/to/file.cs",
    "Guid.NewGuid()"
)

# Step 2: Generate
test = tools.generate_mock_test(
    "GuidHelper",
    "Guid_NewGuid",
    "Guid",
    ""
)

# Step 3: Get template if needed
template = tools.get_moq_setup_template("IGuidGenerator")

# Write to file
with open("GeneratedTests/GuidHelper_NewGuid_Tests.cs", "w") as f:
    f.write(test)
```

### With Azure OpenAI Agent (Production)

```python
import asyncio
from agent_framework.azure import AzureOpenAIChatClient
from azure.identity import DefaultAzureCredential
from agent_tools import TestGenerationTools

async def generate_tests_with_agent():
    # Initialize tools
    tools = TestGenerationTools()
    
    # Create agent with tools
    agent = AzureOpenAIChatClient(
        credential=DefaultAzureCredential()
    ).create_agent(
        instructions="""
        You are an expert C# test engineer specializing in unit testing and mocking.
        
        Your task is to generate comprehensive unit tests for static method usage.
        
        Use these tools to:
        1. Analyze static methods in C# code
        2. Generate mock-based test scaffolds
        3. Reference Moq patterns when needed
        
        Each test should:
        - Use xUnit framework for assertions
        - Use Moq for mocking static methods
        - Have clear Arrange/Act/Assert sections
        - Include proper documentation
        - Be ready to extend with real test logic
        """,
        tools=[
            tools.analyze_static_method,
            tools.generate_mock_test,
            tools.get_moq_setup_template
        ]
    )
    
    # Ask agent to generate tests
    result = await agent.run("""
    Generate comprehensive unit tests for these static method usages:
    
    1. File: DateHelper.cs, Method: DateTime.Now
    2. File: FileValidator.cs, Method: File.Exists(string path)
    3. File: IdGenerator.cs, Method: Guid.NewGuid()
    
    For each:
    - Analyze the static method first
    - Generate a mock-based test
    - Include proper Moq setup patterns
    """)
    
    print(result.text)
    return result

# Run
if __name__ == "__main__":
    result = asyncio.run(generate_tests_with_agent())
```

---

## How test_orchestrator.py Uses These Tools

```python
def generate_unit_tests_with_agent(self, static_files):
    # Initialize tools
    agent_tools = TestGenerationTools()
    
    for pattern, files in static_files.items():
        for file_path in files:
            # Step 1: Find individual methods
            methods = self.find_static_methods_in_file(file_path)
            
            for method_name, _ in methods:
                # Step 2: Generate test using tools
                test_content = agent_tools.generate_mock_test(
                    class_name=os.path.basename(file_path).replace('.cs', ''),
                    method_name=method_name.replace(".", "_"),
                    return_type="dynamic",
                    parameters=""
                )
                
                # Step 3: Write to GeneratedTests directory
                test_file = os.path.join(GENERATED_TESTS_DIR, f"{method_name}_Tests.cs")
                with open(test_file, 'w') as f:
                    f.write(test_content)
```

---

## Example: Full Workflow

### Input
```csharp
// File: Volo.Abp.Core/DateTimeHelper.cs
public class DateTimeHelper
{
    public static DateTime GetCurrentTime()
    {
        return DateTime.Now;  // ⚠️ Hard to test!
    }
    
    public static bool IsFilePresent(string path)
    {
        return File.Exists(path);  // ⚠️ Hard to test!
    }
}
```

### Agent Process
```
1. Agent analyzes: "What static methods are in this file?"
   → Calls analyze_static_method()
   → Returns context about DateTime.Now and File.Exists usage

2. Agent generates: "Create tests for these methods"
   → Calls generate_mock_test() for DateTime.Now
   → Calls generate_mock_test() for File.Exists

3. Agent refines: "How do I mock these properly?"
   → Calls get_moq_setup_template()
   → Enhances generated tests with mock setup

4. Agent produces: "Here are the tests, ready to integrate"
   → Test files written to GeneratedTests/
```

### Output
```csharp
// File: GeneratedTests/DateTimeHelper_DateTime_Now_Tests.cs
namespace GeneratedTests
{
    public class DateTimeHelper_DateTime_Now_Tests
    {
        [Fact]
        public void Test_DateTime_Now_MockedSuccessfully()
        {
            // Arrange
            // Mock DateTime provider or extract to interface
            
            // Act
            var result = DateTimeHelper.GetCurrentTime();
            
            // Assert
            Assert.NotNull(result);
        }
    }
}

// File: GeneratedTests/DateTimeHelper_File_Exists_Tests.cs
namespace GeneratedTests
{
    public class DateTimeHelper_File_Exists_Tests
    {
        [Fact]
        public void Test_File_Exists_MockedSuccessfully()
        {
            // Arrange
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);
            
            // Act
            var result = DateTimeHelper.IsFilePresent("/some/path");
            
            // Assert
            Assert.True(result);
            mockFileService.Verify(x => x.Exists(It.IsAny<string>()), Times.Once);
        }
    }
}
```

---

## Future Enhancements

1. **Tool: generate_interface_wrapper()**
   - Generate IDateTime, IFileService interfaces
   - Extract static method calls to interfaces
   - Help refactor code for better testability

2. **Tool: generate_test_with_feedback()**
   - Take build/test feedback
   - Enhance test based on actual method signature
   - Iteratively improve tests

3. **Tool: verify_test_validity()**
   - Check if generated test is syntactically correct
   - Verify test can compile
   - Suggest improvements

4. **Tool: suggest_refactoring()**
   - Recommend dependency injection patterns
   - Suggest how to make methods more testable
   - Provide refactoring roadmap

---

## Documentation

Each tool includes:
- ✅ Type hints with Annotated descriptions
- ✅ Comprehensive docstrings
- ✅ Return value documentation
- ✅ Parameter descriptions
- ✅ Usage examples

This makes them discoverable and usable by both agents and developers.
