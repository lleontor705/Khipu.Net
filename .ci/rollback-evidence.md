# Parity Phase Rollback Evidence

## Objective
Provide deterministic rollback instructions when any explicit parity phase gate fails.

See also: `docs/parity-baseline-governance.md` for baseline update governance, review controls, and checkpoint policy.

## Phase gates
1. `P0 (p0-restore)`: restore baseline
2. `P1 (p1-build)`: build baseline
3. `P2 (p2-xml-golden-fixtures)`: canonical XML fixtures
4. `P3 (p3-validator-fixtures)`: validator matrix fixtures
5. `P4 (p4-soap-cdr-fixtures)`: SOAP/CDR fixture matrix
6. `P5 (p5-e2e-parity)`: E2E parity fixtures
7. `P6 (p6-full-regression)`: full suite

## Rollback procedure
1. Stop promotion when any phase gate fails.
2. Use last known good checkpoint/tag from CI artifacts.
3. Re-run phase gates against checkpoint in order:
   - `dotnet restore Khipu.Net.slnx`
   - `dotnet build Khipu.Net.slnx`
   - `dotnet test tests/Khipu.Tests/Khipu.Tests.csproj --filter "XmlGoldenFixtureTests"`
   - `dotnet test tests/Khipu.Tests/Khipu.Tests.csproj --filter "ValidatorFixtureMatrixTests"`
   - `dotnet test tests/Khipu.Tests/Khipu.Tests.csproj --filter "SunatSoapClientFixtureMatrixTests"`
   - `dotnet test tests/Khipu.Tests/Khipu.Tests.csproj --filter "SunatServiceE2EParityTests"`
   - `dotnet test Khipu.Net.slnx`
4. Open incident note including failed fixture IDs and XPath diagnostics.
5. Merge fix only after all P0-P6 gates are green.

## Rollback checkpoints
- **Checkpoint A (Code)**: last known green commit SHA before baseline promotion.
- **Checkpoint B (Baseline)**: previous versioned baseline file under `tools/parity/baselines/`.
- **Checkpoint C (Evidence)**: CI artifact set from the last green parity run.

## Expected evidence artifacts
- Build logs
- Phase fixture gate logs with failing scenario IDs
- Full regression summary
