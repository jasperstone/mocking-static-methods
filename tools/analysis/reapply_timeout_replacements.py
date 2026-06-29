import json
from pathlib import Path


def load_keys(path: Path) -> set[tuple[str, str, str, str]]:
    return {tuple(x) for x in json.loads(path.read_text())}


def patch_jsonl(path: Path, phase: str, model: str, run: str, repl_map: dict[tuple[str, str, str, str], str]) -> int:
    if not path.exists():
        return 0

    out: list[str] = []
    seen: set[tuple[str, str, str, str]] = set()
    changed = 0

    for ln in path.read_text(errors="ignore").splitlines():
        if not ln.strip():
            continue
        try:
            row = json.loads(ln)
        except Exception:
            out.append(ln)
            continue

        key = (phase, model, run, row.get("target_id"))
        if key in repl_map:
            out.append(repl_map[key])
            seen.add(key)
            changed += 1
        else:
            out.append(ln)

    for key, val in repl_map.items():
        if key[0] == phase and key[1] == model and key[2] == run and key not in seen:
            out.append(val)
            changed += 1

    path.write_text("\n".join(out) + "\n")
    return changed


def main() -> int:
    repo = Path(".")
    keyset = load_keys(Path("/tmp/timeout_keys.json"))

    phase_by_run = {
        "28371688691": "phase2-agentic",
        "28371690723": "phase3-agentic-loop",
        "28371692304": "phase3-agentic-loop",
        "28371694017": "phase3-agentic-loop",
        "28374528487": "phase2-agentic",
        "28374530715": "phase3-agentic-loop",
        "28374535154": "phase3-agentic-loop",
        "28374693498": "phase3-agentic-loop",
        "28374532902": "phase3-agentic-loop",
        "28326809887": "phase2-agentic",
        "28326810611": "phase3-agentic-loop",
        "28326811941": "phase3-agentic-loop",
        "28326486829": "phase2-agentic",
        "28326487543": "phase3-agentic-loop",
        "28326488295": "phase3-agentic-loop",
        "28326489233": "phase3-agentic-loop",
        "28326385342": "phase2-agentic",
        "28326386219": "phase3-agentic-loop",
        "28326387770": "phase3-agentic-loop",
        "28326023244": "phase2-agentic",
        "28326026383": "phase3-agentic-loop",
        "28326027053": "phase3-agentic-loop",
        "28326027659": "phase3-agentic-loop",
        "28325806262": "phase3-agentic-loop",
        "28325764028": "phase3-agentic-loop",
        "28325765470": "phase3-agentic-loop",
        "28325381684": "phase2-agentic",
        "28325263527": "phase3-agentic-loop",
        "28325264089": "phase3-agentic-loop",
        "28325264564": "phase3-agentic-loop",
    }

    attempt_repl: dict[tuple[str, str, str, str], str] = {}
    eval_repl: dict[tuple[str, str, str, str], str] = {}

    for root in sorted(Path("/home/jastone/gha-downloads").glob("backfill-*")):
        run_id = root.name.split("-")[-1]
        phase = phase_by_run.get(run_id)
        if not phase:
            continue

        for p in root.rglob("attempts.jsonl"):
            try:
                model, run = p.parts[-3], p.parts[-2]
            except Exception:
                continue
            for ln in p.read_text(errors="ignore").splitlines():
                if not ln.strip():
                    continue
                try:
                    row = json.loads(ln)
                except Exception:
                    continue
                key = (phase, model, run, row.get("target_id"))
                if key in keyset:
                    attempt_repl[key] = ln

        for p in root.rglob("evaluation.jsonl"):
            try:
                model, run = p.parts[-3], p.parts[-2]
            except Exception:
                continue
            for ln in p.read_text(errors="ignore").splitlines():
                if not ln.strip():
                    continue
                try:
                    row = json.loads(ln)
                except Exception:
                    continue
                key = (phase, model, run, row.get("target_id"))
                if key in keyset:
                    eval_repl[key] = ln

    changed_attempts = 0
    changed_evals = 0
    for phase in ["phase2-agentic", "phase3-agentic-loop"]:
        base = repo / "phases" / phase / "results"
        if not base.exists():
            continue
        for mdir in base.iterdir():
            if not mdir.is_dir():
                continue
            model = mdir.name
            for rdir in mdir.iterdir():
                if not (rdir.is_dir() and rdir.name.startswith("run_")):
                    continue
                run = rdir.name
                changed_attempts += patch_jsonl(rdir / "attempts.jsonl", phase, model, run, attempt_repl)
                changed_evals += patch_jsonl(rdir / "evaluation.jsonl", phase, model, run, eval_repl)

    print(f"patched_attempts={changed_attempts}")
    print(f"patched_evals={changed_evals}")

    noisy: list[tuple[str, str, str, str, str]] = []
    for phase, model, run, target_id in sorted(keyset):
        p = repo / "phases" / phase / "results" / model / run / "attempts.jsonl"
        row = None
        for ln in p.read_text(errors="ignore").splitlines():
            if not ln.strip():
                continue
            r = json.loads(ln)
            if r.get("target_id") == target_id:
                row = r
                break

        txt = (((row or {}).get("halt_reason") or "") + " " + ((row or {}).get("error") or "")).lower()
        if "timeout" in txt or "429" in txt or "ratelimit" in txt or "503" in txt:
            noisy.append((phase, model, run, target_id, (row or {}).get("halt_reason") or ""))

    print(f"residual_noisy={len(noisy)}")
    for item in noisy:
        print("NOISY", item)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
