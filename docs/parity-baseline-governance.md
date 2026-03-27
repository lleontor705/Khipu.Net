# Parity Baseline Governance & Rollback Checkpoints

## Purpose
Define operational controls for updating parity baseline pass-sets and for rolling back safely when P0-P6 gates fail.

## Scope
- Baseline artifacts under `tools/parity/baselines/`
- Phase gates `P0` to `P6`
- Rollback evidence under `.ci/rollback-evidence.md`

## Governance for pass-set baseline updates

### 1) Change intent and scope
Any PR that modifies `tools/parity/baselines/*.json` MUST include:
- Why the baseline changed (new tests, renamed tests, intentional de-scope, etc.)
- Exact delta of pass-set members (added/removed/renamed)
- Updated expected counts and why they remain deterministic

### 2) Review controls
- Baseline updates require explicit reviewer approval before merge.
- If a previously passing case is removed, PR must link a tracking issue.
- Baseline updates must not be mixed with unrelated refactors.

### 3) Determinism controls
- Use versioned baseline files (for example: `p6-pass-set.v1.json`, `p6-pass-set.v2.json`).
- Do not silently mutate semantic meaning without a version bump.
- Keep `baselineCounts` aligned with actual enumerated pass-set items.

### 4) Merge gate checklist
Before merge, confirm:
1. P2-P5 filtered fixtures pass against proposed baseline.
2. P6 full regression remains green or has approved exception with issue link.
3. Rollback steps in `.ci/rollback-evidence.md` still apply to the updated baseline.

## Rollback checkpoints

### Standard checkpoints
- **Checkpoint A (Code)**: last known green commit SHA before baseline promotion.
- **Checkpoint B (Baseline)**: previous baseline file version in `tools/parity/baselines/`.
- **Checkpoint C (Evidence)**: CI artifacts for the green run (phase logs + regression summary).

### Rollback trigger
Rollback starts immediately if any phase gate from P0 to P6 fails after a baseline-related change.

### Rollback flow
1. Freeze promotion for the failing branch/PR.
2. Revert to Checkpoint A + Checkpoint B.
3. Re-run gates in order P0 → P6.
4. Record failing fixture IDs and diagnostics in incident notes.
5. Re-promote only after full green evidence is restored.

## Evidence retention requirements
Each parity run should retain at minimum:
- Build log (`P1`)
- Phase-gate logs with failing fixture/test identifiers (`P2`-`P5`)
- Full regression summary (`P6`)

For baseline update PRs, include:
- Baseline diff summary (old vs new pass-set)
- Updated baseline counts
- Link to rollback evidence procedure

## Operational PR template (recommended)
- **Baseline file(s) changed**:
- **Pass-set delta**:
- **Count delta**:
- **Reason**:
- **Linked issue (if removals)**:
- **Rollback checkpoint commit SHA**:
- **Rollback evidence run URL**:
