"""
Agent Tools for Test Generation

This module provides tools that can be used by the Microsoft Agent Framework
to generate unit tests for static methods.

Usage with Agent Framework:
    from agent_framework.azure import AzureOpenAIChatClient
    from agent_tools import TestGenerationTools
    
    tools = TestGenerationTools()
    agent = AzureOpenAIChatClient(credential=...).create_agent(
        instructions="You are a test generation expert",
        tools=[tools.analyze_static_method, tools.generate_mock_test]
    )
"""

from typing import Annotated, Optional
from pydantic import Field
import re
import os


class TestGenerationTools:
    """Tools for generating unit tests for static methods."""
    
    def analyze_static_method(
        self,
        file_path: Annotated[str, Field(description="Path to the C# file containing the static method")],
        method_signature: Annotated[str, Field(description="The static method signature to analyze")]
    ) -> str:
        """
        Analyze a static method in a C# file.
        
        Returns information about the method including:
        - Return type
        - Parameters
        - Usage context
        - Dependencies
        """
        if not os.path.exists(file_path):
            return f"Error: File {file_path} not found"
        
        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            
            # Look for the method
            method_pattern = re.escape(method_signature)
            if re.search(method_pattern, content):
                # Extract surrounding code for context
                lines = content.split('\n')
                method_line = None
                for i, line in enumerate(lines):
                    if method_signature in line:
                        method_line = i
                        break
                
                if method_line is not None:
                    # Get context around the method
                    start = max(0, method_line - 5)
                    end = min(len(lines), method_line + 10)
                    context = '\n'.join(lines[start:end])
                    
                    return f"Found method '{method_signature}' in {file_path}\n\nContext:\n{context}"
                else:
                    return f"Method pattern '{method_signature}' found but could not extract context"
            else:
                return f"Method '{method_signature}' not found in {file_path}"
        
        except Exception as e:
            return f"Error analyzing file: {str(e)}"
    
    def generate_mock_test(
        self,
        class_name: Annotated[str, Field(description="Name of the class being tested")],
        method_name: Annotated[str, Field(description="Name of the method to test")],
        return_type: Annotated[str, Field(description="Return type of the method (e.g., 'DateTime', 'bool', 'void')")] = "dynamic",
        parameters: Annotated[str, Field(description="Comma-separated list of parameters (e.g., 'string path' or 'Guid id, int count')")] = "",
        is_static: Annotated[bool, Field(description="Whether the target method is static")] = False,
    ) -> str:
        """
        Generate a mock-based unit test for a static method.
        
        Creates a basic test scaffold using xUnit and Moq that can be extended.
        """
        
        # Clean up parameters
        param_list = []
        if parameters.strip():
            for param in parameters.split(','):
                param = param.strip()
                if param:
                    param_list.append(param)
        
        # Generate test class name
        test_class_name = f"{class_name}_{method_name}_Tests"

        # Generate test method name
        test_method_name = f"Test_{method_name}_Behavior"

        # Determine invocation: static vs instance
        if is_static:
            invocation = f"var result = {class_name}.{method_name}({', '.join(['/*arg*/' for _ in param_list])});"
        else:
            invocation = (
                f"var target = new {class_name}();\n            var result = target.{method_name}({', '.join(['/*arg*/' for _ in param_list])});"
            )

        # Build the test file content with Arrange/Act/Assert sections
        test_content = f"""using Xunit;
using System;

namespace GeneratedTests
{{
    /// <summary>
    /// Auto-generated test for {class_name}.{method_name}
    /// This test focuses on the behavior of the containing method/class, not on the static call itself.
    /// </summary>
    public class {test_class_name}
    {{
        [Fact]
        public void {test_method_name}()
        {{
            // Arrange
            // TODO: Create necessary test data and configure dependencies
            {(''.join(['// Parameter placeholder\n            ' for _ in param_list]))}

            // Act
            {invocation}

            // Assert
            // TODO: Add assertions that validate the behavior of the method
            Assert.True(true);
        }}
    }}
}}
"""
        
        return test_content
    
    def get_moq_setup_template(
        self,
        mock_type: Annotated[str, Field(description="The type to mock (e.g., 'IRepository', 'ILogger')")],
    ) -> str:
        """Get a template for setting up mocks with Moq framework."""
        
        template = f"""// Setup mock for {mock_type}
var mock{mock_type} = new Mock<{mock_type}>();

// Configure mock behavior
mock{mock_type}
    .Setup(x => x.SomeMethod())
    .Returns(/* expected value */);

// Use the mock
var instance = mock{mock_type}.Object;
// Pass instance to your code...

// Verify interactions
mock{mock_type}.Verify(x => x.SomeMethod(), Times.Once);
"""
        
        return template


# Example usage (can be called directly for testing)
if __name__ == "__main__":
    tools = TestGenerationTools()
    
    # Example 1: Analyze a static method
    print("Example 1: Analyzing a static method")
    print("-" * 50)
    
    # Example 2: Generate a mock test
    print("\nExample 2: Generating a mock test")
    print("-" * 50)
    test_code = tools.generate_mock_test(
        class_name="DateTimeHelper",
        method_name="GetCurrentTime",
        return_type="DateTime",
        parameters=""
    )
    print(test_code)
    
    # Example 3: Get Moq setup template
    print("\nExample 3: Getting Moq setup template")
    print("-" * 50)
    template = tools.get_moq_setup_template(mock_type="IRepository")
    print(template)
