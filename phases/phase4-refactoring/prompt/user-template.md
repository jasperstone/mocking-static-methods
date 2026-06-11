Please write xUnit unit tests for the following code in the `{{REPO}}` repository.

File: `{{TARGET_FILE}}`
Type: `{{CONTAINING_TYPE}}`

I'd particularly like coverage of the call on line {{TARGET_LINE}}, where this code calls `{{METHOD}}` on `{{RECEIVER_TYPE}}` ({{KIND}} call). This is a Mode #1 site — it isn't directly mockable as-is, so you'll likely need to introduce a testability seam with apply_refactor before you can assert on it.

Here's the relevant source (lines {{SOURCE_WINDOW_START}}–{{SOURCE_WINDOW_END}}):

```csharp
{{SOURCE_WINDOW}}
```

Test framework: {{TEST_FRAMEWORK}}. Target framework: {{TARGET_TFM}}.

Use the read_file / list_dir tools if you need to look at related types, apply_refactor to open a seam, and submit_test when you're ready. Remember: at least one `[Fact]` method that reaches `{{METHOD}}` through the seam and asserts on what it does.
