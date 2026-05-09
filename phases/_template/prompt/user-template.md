## Target

- **Repo**: `{{REPO}}` (commit `{{REPO_SHA}}`)
- **File**: `{{TARGET_FILE}}`
- **Line**: `{{TARGET_LINE}}`
- **Static call**: `{{RECEIVER_TYPE}}.{{METHOD}}` (kind: `{{KIND}}`, defined on `{{CONTAINING_TYPE}}`)

## Production source containing the target line

Lines `{{SOURCE_WINDOW_START}}` through `{{SOURCE_WINDOW_END}}` of `{{TARGET_FILE}}`. The target line is marked with `>>>`.

```csharp
{{SOURCE_WINDOW}}
```

## Test project conventions for this repo

- **Test framework**: `{{TEST_FRAMEWORK}}`
- **Target framework**: `{{TARGET_TFM}}`
- **Existing test project to add the file to**: `{{TEST_PROJECT_PATH}}`
- **Namespace convention**: `{{TEST_NAMESPACE_HINT}}`
- **One example test class from the same project (for style reference)**:

```csharp
{{EXAMPLE_TEST}}
```

## Task

Write a single .cs test file that, when added to `{{TEST_PROJECT_PATH}}` and run, causes line `{{TARGET_LINE}}` of `{{TARGET_FILE}}` to execute and at least one test method to pass.
