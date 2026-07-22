# Phase `phase4-refactoring` — cost report (autogen)

Auto-generated from `tools/cost/estimate.py`. The narrative version with
reconciliation, panel decisions, and projections lives in `COSTS.md`.


=== phase4-refactoring: projected Azure bill (Foundry Tools + Models) ===
(Azure AI Search excluded by design; not modelled.)

| Model | Bill | Calls | Token (list) | Token (recon x1.95) | Tools overhead | Total |
|-------|------|------:|-------------:|--------------------:|---------------:|------:|
| `codestral-2501` | marketplace | 900 | $18.88 | $36.82 | $30.38 | $67.20 |
| `gpt-4.1-mini` | credit | 846 | $18.03 | $35.16 | $28.55 | $63.71 |
| `gpt-4.1-nano` | credit | 792 | $5.26 | $10.27 | $26.73 | $37.00 |
| `grok-4-1-fast` | marketplace | 900 | $6.10 | $11.90 | $30.38 | $42.28 |
| `llama-3.3-70b-instruct` | marketplace | 897 | $29.03 | $56.60 | $30.28 | $86.88 |
| `phi-4` | credit | 900 | $0.00 | $0.00 | $30.38 | $30.38 |
| **Total** | | **5235** | **$77.31** | **$150.76** | **$176.69** | **$327.45** |

  Credit-billed subtotal      : $222.12   (model tokens on credit surface + $176.69 Foundry Tools)
  Marketplace-billed subtotal : $105.33   (card; does not draw credit)
  COMBINED total              : $327.45   <- the number the cap measures

  vs --cap $250         : 131.0%  (OVER by $77.45)
  vs $150 credit (credit only): 148.1%  (credit exhausted; +$72.12 to card)
  Implied card spend          : $177.45  (credit overage $72.12 + marketplace $105.33)

=== phase 5 projection :: full scope (runs=3, cycles=3) ===
  runs_per_cell=3  max_review_cycles=3  panel=6 models (full)
  calls/cell : 4.3 realized  (theoretical max 7 = 1 + 2x3)
             = 1 writer + 1.8 reviewer + 1.5 fixer

| Role | Invocations | Token (list) | Token (recon x1.95) |
|------|------------:|-------------:|--------------------:|
| writer | 5,397 | $40.24 | $78.48 |
| reviewer | 9,715 | $50.00 | $97.50 |
| fixer | 8,096 | $80.00 | $156.00 |
| **Total** | **23,207** | **$170.24** | **$331.98** |

  Foundry Tools overhead      : $783.28  (23,207 invocations @ $0.03375)
  Foundry Models (token recon): $331.98  (list $170.24 x 1.95)
  Credit-billed subtotal      : $815.94   (credit-surface tokens + all Foundry Tools)
  Marketplace-billed subtotal : $299.32   (card; does not draw credit)
  COMBINED projected total    : $1115.26   <- the number the cap measures

  vs --cap $250         : 446.1%  (OVER by $865.26)
  vs $150 credit (credit only): 544.0%  (credit exhausted; +$665.94 to card)
  Implied card spend          : $965.26  (credit overage $665.94 + marketplace $299.32)

  Sensitivity (Tools half-attributed to phase 2) and the runs/cycle-reduced
  Configs A/B/C: run with --project-phase5.

=== phase 4 projection :: go/no-go dispatch (runs=1; single writer + refactor tool) ===
  runs_per_cell=1  panel=6 models (full)  LLM roles=1 (writer only; NO reviewer/fixer)
  apply_refactor : LOCAL zero-token tool; ~1.2 call(s)/cell (+1.2x writer invocations on the Foundry Tools surface)
  token inflation: x1.50 on the phase-3 single-writer base (more turns/cell, not an extra agent)

| Role | Invocations | Token (list) | Token (recon x1.95) |
|------|------------:|-------------:|--------------------:|
| writer | 1,799 | $13.41 | $26.16 |
| apply_refactor _(local; 0 tok)_ | 2,159 | $0.00 | $0.00 |
| **Total** | **3,958** | **$20.12** | **$39.24** |

  Foundry Tools overhead      : $133.58  (3,958 invocations @ $0.03375 = 1,799 writer + 2,159 apply_refactor)
  Foundry Models (token recon): $39.24  (writer list $13.41 x 1.50 inflation x 1.95)
  Credit-billed subtotal      : $137.44   (credit-surface tokens + all Foundry Tools)
  Marketplace-billed subtotal : $35.38   (card; does not draw credit)
  COMBINED projected total    : $172.82   <- the number the cap measures

  vs --cap $250         :  69.1%  (under by $77.18)
  vs $150 credit (credit only):  91.6%  ($12.56 credit left)
  Implied card spend          : $35.38  (credit overage $0.00 + marketplace $35.38)

  SANITY CHECK: phase 4 carries ONE LLM role (writer) + a LOCAL zero-token
  refactor tool -- no reviewer/fixer multiplying spend. At runs=1 the
  combined $172.82 is UNDER the $250 cap, and FAR below phase 5's
  ~$1,197 full-scope projection (phase 4 has no 2nd/3rd model role). It sits
  modestly above the SAME-runs phase-3 single-writer base ($86.88), about 1.99x.
  For context, the full 3-run set (runs=3) projects ~$518 (207% of cap)
  -- over cap, but ~43% of phase 5's $1,197, i.e. roughly half.

  Phase-4 ad-hoc runs/refactor-call sweeps: run with --project-phase4 [--runs N] [--refactor-calls F].

--- residual gap ---
  Phase-3 actual Foundry (Tools $182.26 + Models $160.45) = $342.71.
  This model's phase-3 combined = $327.45.
  Residual = $-15.26. The two reconciliation
  knobs (TOKEN_RECON_FACTOR=1.95, Tools $0.03375/call) are calibrated to this
  anchor; remaining gap is phase-2 token overlap inside the May window
  and sub-dollar Container Registry/storage, both intentionally unmodeled.
