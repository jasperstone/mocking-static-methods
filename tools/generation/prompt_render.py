"""Mustache-style {{...}} substitution for prompt templates.

Deliberately minimal: no conditionals, no loops, no escaping. The template
files in phases/<phase-id>/prompt/ are authored by humans; if they need
logic, the logic belongs in runner.py before substitution.
"""
from __future__ import annotations
import re

_PLACEHOLDER = re.compile(r"\{\{\s*([A-Z_][A-Z0-9_]*)\s*\}\}")


def render(template: str, values: dict[str, str]) -> str:
    def sub(m: re.Match[str]) -> str:
        key = m.group(1)
        if key not in values:
            raise KeyError(f"prompt template references unknown placeholder: {{{{ {key} }}}}")
        return values[key]
    return _PLACEHOLDER.sub(sub, template)


def required_placeholders(template: str) -> set[str]:
    return {m.group(1) for m in _PLACEHOLDER.finditer(template)}
