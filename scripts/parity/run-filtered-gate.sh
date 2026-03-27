#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 1 ] || [ "$#" -gt 4 ]; then
  echo "Usage: $0 <phase> [baseline_path] [tests_project] [results_dir]" >&2
  exit 1
fi

phase="$1"
baseline_path="${2:-tools/parity/baselines/p6-pass-set.v1.json}"
tests_project="${3:-tests/Khipu.Tests/Khipu.Tests.csproj}"
results_dir="${4:-artifacts/parity}"

if [ ! -f "$baseline_path" ]; then
  echo "Parity baseline file not found: $baseline_path" >&2
  exit 1
fi

gate_filter="$(jq -r --arg phase "$phase" '.gateFilter[$phase] // ""' "$baseline_path")"
if [ -z "$gate_filter" ]; then
  echo "No gateFilter configured for phase '$phase' in $baseline_path" >&2
  exit 1
fi

mkdir -p "$results_dir"
trx_file="${phase,,}-filtered-gate.trx"

dotnet test "$tests_project" --no-restore \
  --results-directory "$results_dir" \
  --logger "trx;LogFileName=$trx_file" \
  --filter "$gate_filter"

trx_path="$results_dir/$trx_file"
if [ ! -f "$trx_path" ]; then
  echo "TRX results were not generated: $trx_path" >&2
  exit 1
fi

executed_total="$(python3 - "$trx_path" <<'PY'
import sys
import xml.etree.ElementTree as ET

trx_path = sys.argv[1]
root = ET.parse(trx_path).getroot()
namespace = ""
if root.tag.startswith("{"):
    namespace = root.tag.split("}", 1)[0] + "}"
counters = root.find(f".//{namespace}Counters")
print(counters.attrib.get("total", "") if counters is not None else "")
PY
)"

if ! [[ "$executed_total" =~ ^[0-9]+$ ]] || [ "$executed_total" -le 0 ]; then
  echo "Phase $phase filter '$gate_filter' executed zero tests; failing to avoid false-green run." >&2
  exit 1
fi

echo "Phase $phase gate filter '$gate_filter' executed $executed_total tests."
