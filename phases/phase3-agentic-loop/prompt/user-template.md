Please write xUnit unit tests for the following code in the `{{REPO}}` repository.

File: `{{TARGET_FILE}}`
Type: `{{CONTAINING_TYPE}}`

I'd particularly like coverage of the call on line {{TARGET_LINE}}, where this code calls `{{METHOD}}` on `{{RECEIVER_TYPE}}` ({{KIND}} call).

Here's the relevant source (lines {{SOURCE_WINDOW_START}}–{{SOURCE_WINDOW_END}}):

```csharp
{{SOURCE_WINDOW}}
```

Use the read_file / list_dir tools if you need to look at related types. When you're ready, submit one xUnit test file with submit_test.
