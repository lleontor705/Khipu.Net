#!/usr/bin/env python3
"""Compare P6 passed test set against the parity baseline pass-set."""

from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path
from xml.etree import ElementTree as ET


def _iso_utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def _parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--trx", required=True, help="Path to P6 TRX results file")
    parser.add_argument("--baseline", required=True, help="Path to baseline JSON")
    parser.add_argument("--output", required=True, help="Path to write comparison JSON")
    return parser.parse_args()


def _load_baseline(path: Path) -> list[str]:
    payload = json.loads(path.read_text(encoding="utf-8"))
    return list(payload.get("passSet", {}).get("xunitFacts", []))


def _load_passed_tests_from_trx(path: Path) -> list[str]:
    root = ET.parse(path).getroot()
    namespace = ""
    if root.tag.startswith("{"):
        namespace = root.tag.split("}", 1)[0] + "}"

    results = root.findall(f".//{namespace}UnitTestResult")
    passed = []
    for result in results:
        if result.attrib.get("outcome") == "Passed":
            name = result.attrib.get("testName", "").strip()
            if name:
                passed.append(name)
    return passed


def _normalize_test_name(name: str) -> str:
    no_params = name.split("(", 1)[0].strip()
    parts = [part for part in no_params.split(".") if part]
    if len(parts) >= 2:
        canonical = f"{parts[-2]}.{parts[-1]}"
        aliases = {
            "SunatSoapClientFixtureMatrixTests.SoapCdrMatrix_CoversSuccessWarningFaultPendingAndCorruptCdr": "SunatSoapClientFixtureMatrixTests.SoapCdrMatrix_CoversSuccessPendingWarningFaultCorruptHttpAndTimeout",
        }
        return aliases.get(canonical, canonical)
    return no_params


def main() -> int:
    args = _parse_args()
    trx_path = Path(args.trx)
    baseline_path = Path(args.baseline)
    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)

    baseline_tests = sorted({_normalize_test_name(x) for x in _load_baseline(baseline_path)})
    passed_tests = sorted({_normalize_test_name(x) for x in _load_passed_tests_from_trx(trx_path)})

    baseline_set = set(baseline_tests)
    passed_set = set(passed_tests)

    missing_from_current = sorted(baseline_set - passed_set)
    new_in_current = sorted(passed_set - baseline_set)
    matched = sorted(baseline_set & passed_set)

    comparison = {
        "schema_version": "1.0.0",
        "generated_at_utc": _iso_utc_now(),
        "baseline_path": str(baseline_path).replace("\\", "/"),
        "trx_path": str(trx_path).replace("\\", "/"),
        "baseline_count": len(baseline_tests),
        "current_passed_count": len(passed_tests),
        "matched_count": len(matched),
        "missing_from_current": missing_from_current,
        "new_in_current": new_in_current,
        "status": "failed" if missing_from_current else "passed",
    }

    output_path.write_text(json.dumps(comparison, indent=2) + "\n", encoding="utf-8")
    if missing_from_current:
        print("P6 regression comparator detected missing baseline pass-set items:")
        for item in missing_from_current:
            print(f" - {item}")
        return 2

    print(
        f"P6 comparator passed: matched {comparison['matched_count']}/{comparison['baseline_count']} baseline tests"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
