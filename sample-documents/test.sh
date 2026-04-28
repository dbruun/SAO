#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# test.sh – smoke-test all three verification scenarios against the backend.
#
# Usage:
#   chmod +x test.sh
#   ./test.sh [API_BASE_URL]          # defaults to http://localhost:5000
#
# Prerequisites: curl, jq (jq is optional – raw JSON is shown if not found)
# ---------------------------------------------------------------------------

set -euo pipefail

API="${1:-http://localhost:5000}"
ENDPOINT="${API}/api/verification"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Demo user data that matches the PASS / REVIEW profiles in MockDocumentExtractionService
USER_NAME="John Michael Smith"
USER_ADDR="123 Main Street, Springfield, IL 62701"
USER_DOB="1985-06-15"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
RESET='\033[0m'

pretty() {
  if command -v jq &>/dev/null; then
    echo "$1" | jq .
  else
    echo "$1"
  fi
}

run_scenario() {
  local label="$1"
  local file="$2"
  local expected="$3"
  local color="$4"

  echo ""
  echo -e "${CYAN}──────────────────────────────────────────${RESET}"
  echo -e "${color}▶  Scenario: ${label}${RESET}"
  echo -e "   File:     ${file}"
  echo -e "   Expected: ${expected}"
  echo -e "${CYAN}──────────────────────────────────────────${RESET}"

  response=$(curl -s -X POST "${ENDPOINT}" \
    -F "fileFront=@${DIR}/${file}" \
    -F "fullName=${USER_NAME}" \
    -F "address=${USER_ADDR}" \
    -F "dateOfBirth=${USER_DOB}")

  status=$(echo "$response" | grep -o '"status":"[^"]*"' | head -1 | cut -d'"' -f4 || true)
  confidence=$(echo "$response" | grep -o '"confidenceScore":[0-9.]*' | head -1 | cut -d: -f2 || true)

  echo ""
  pretty "$response"
  echo ""

  if [ "$status" = "$expected" ]; then
    echo -e "${GREEN}✔  RESULT: ${status}  (confidence: ${confidence})  – matches expected${RESET}"
  else
    echo -e "${RED}✘  RESULT: ${status:-<no status>}  (confidence: ${confidence})  – expected ${expected}${RESET}"
  fi
}

echo ""
echo -e "${CYAN}============================================${RESET}"
echo -e "${CYAN}  Identity Verification – Smoke Test Suite  ${RESET}"
echo -e "${CYAN}  API: ${ENDPOINT}${RESET}"
echo -e "${CYAN}============================================${RESET}"

run_scenario "PASS   – perfect match"          "pass-license.png"   "PASS"   "${GREEN}"
run_scenario "REVIEW – address differs (Apt)"  "review-license.png" "REVIEW" "${YELLOW}"
run_scenario "FAIL   – wrong person + DOB"     "fail-license.png"   "FAIL"   "${RED}"

echo ""
echo -e "${CYAN}============================================${RESET}"
echo -e "${CYAN}  All scenarios executed.${RESET}"
echo -e "${CYAN}============================================${RESET}"
echo ""
