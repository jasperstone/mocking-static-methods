#!/usr/bin/env python3
"""Smoke-test the Foundry adapter with a tiny C# test-generation prompt.

One cell × all panel models. Reports per-model:
  - status (OK / FAIL)
  - latency
  - tokens (in/out)
  - whether response contains a fenced ```csharp block
  - first 2 lines of generated content

Total cost target: < $0.05.
"""
from __future__ import annotations
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO_ROOT))

from tools.generation.adapters import foundry  # noqa: E402

SYSTEM = (
    "You are a senior C# test engineer. Write clear, minimal xUnit tests. "
    "Always wrap code in fenced ```csharp blocks."
)
USER = """Write a single xUnit test method that verifies `Calculator.Add(2, 3)` returns 5.
Use Xunit. Class under test:

```csharp
public static class Calculator {
    public static int Add(int a, int b) => a + b;
}
```

Reply with the test inside one ```csharp block. Nothing else."""

CSHARP_RE = re.compile(r"```(?:csharp|cs|c#)?\s*\n(.*?)```", re.DOTALL | re.IGNORECASE)


def main() -> int:
    panel = foundry.list_panel()
    print(f"Panel size: {len(panel)}")
    print(f"{'model':<28} {'status':<7} {'lat':<8} {'p_tok':<6} {'c_tok':<6} {'csblk':<6} preview")
    print("-" * 110)
    fails = 0
    for m in panel:
        try:
            r = foundry.generate(
                model_id=m,
                system_prompt=SYSTEM,
                user_prompt=USER,
                temperature=0.0,
                max_output_tokens=400,
                timeout_s=180,
            )
            blocks = CSHARP_RE.findall(r.text)
            preview = (r.text.strip().splitlines() or [""])[0][:60]
            print(f"{m:<28} {'OK':<7} {str(r.latency_ms)+'ms':<8} {r.prompt_tokens:<6} {r.completion_tokens:<6} {len(blocks):<6} {preview!r}")
        except foundry.FoundryError as e:
            fails += 1
            print(f"{m:<28} {'FAIL':<7} {'-':<8} {'-':<6} {'-':<6} {'-':<6} {str(e)[:80]}")
        except Exception as e:
            fails += 1
            print(f"{m:<28} {'ERR ':<7} {'-':<8} {'-':<6} {'-':<6} {'-':<6} {type(e).__name__}: {str(e)[:60]}")
    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
