#!/usr/bin/env bash
# Lists NuGet packages (direct and indirect) with known security advisories.
# Exit code 1 if any is High or Critical; lower severities only print a warning.
# `dotnet list package --vulnerable` always exits 0, so we read its output ourselves.
set -euo pipefail

output="$(dotnet list package --vulnerable --include-transitive 2>&1)"
echo "$output"

# Table rows look like: "   > MessagePack   2.5.302   Moderate   https://github.com/advisories/..."
serious="$(grep -E '^\s*>.*\s(High|Critical)\s' <<<"$output" || true)"
lower="$(grep -E '^\s*>.*\s(Low|Moderate)\s' <<<"$output" || true)"

summary="${GITHUB_STEP_SUMMARY:-/dev/null}"
{
  echo "### Vulnerable NuGet packages"
  if [[ -z "$serious" && -z "$lower" ]]; then
    echo "None found."
  fi
  if [[ -n "$lower" ]]; then
    echo ""
    echo "Low/Moderate (warning only):"
    echo '```'; echo "$lower"; echo '```'
  fi
  if [[ -n "$serious" ]]; then
    echo ""
    echo "High/Critical (fails the build):"
    echo '```'; echo "$serious"; echo '```'
  fi
} >> "$summary"

if [[ -n "$lower" ]]; then
  echo "::warning title=Vulnerable packages::Low or Moderate advisories found. See the run summary."
fi

if [[ -n "$serious" ]]; then
  echo "::error title=Vulnerable packages::High or Critical advisories found."
  exit 1
fi
