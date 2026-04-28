# Sample Test Documents

Three ready-made fake driver's licence images for smoke-testing the identity
verification API without needing a real document or a live OCR service.

The backend's `MockDocumentExtractionService` selects the extraction profile
purely from the **filename**, so the images are intentionally styled to make
that obvious at a glance.

---

## Files

| File | Badge colour | Scenario | Expected API status | Expected confidence |
|------|-------------|----------|---------------------|---------------------|
| `pass-license.png` | 🟢 Green | Extracted data **exactly matches** user input | **PASS** | ~99 |
| `review-license.png` | 🟡 Orange | Address has extra `Apt 2B` → moderate mismatch | **REVIEW** | ~89 |
| `fail-license.png` | 🔴 Red | Different person (`Jane Doe`) + DOB mismatch | **FAIL** | ~29 |

---

## Demo user input (use for all three scenarios)

| Field | Value |
|-------|-------|
| Full name | `John Michael Smith` |
| Address | `123 Main Street, Springfield, IL 62701` |
| Date of birth | `1985-06-15` |

---

## Quick test with the shell script

```bash
# Start the backend first (in another terminal):
#   cd backend && dotnet run

chmod +x sample-documents/test.sh
./sample-documents/test.sh                      # default: http://localhost:5000
./sample-documents/test.sh http://localhost:5000
```

The script runs all three scenarios, pretty-prints the JSON response (requires
`jq` for coloured output), and prints a ✔ / ✘ next to each result.

---

## Manual curl examples

### PASS
```bash
curl -X POST http://localhost:5000/api/verification \
  -F "fileFront=@sample-documents/pass-license.png" \
  -F "fullName=John Michael Smith" \
  -F "address=123 Main Street, Springfield, IL 62701" \
  -F "dateOfBirth=1985-06-15"
```

### REVIEW
```bash
curl -X POST http://localhost:5000/api/verification \
  -F "fileFront=@sample-documents/review-license.png" \
  -F "fullName=John Michael Smith" \
  -F "address=123 Main Street, Springfield, IL 62701" \
  -F "dateOfBirth=1985-06-15"
```

### FAIL
```bash
curl -X POST http://localhost:5000/api/verification \
  -F "fileFront=@sample-documents/fail-license.png" \
  -F "fullName=John Michael Smith" \
  -F "address=123 Main Street, Springfield, IL 62701" \
  -F "dateOfBirth=1985-06-15"
```

---

## How the mock service works

`MockDocumentExtractionService` inspects the **filename** (case-insensitive):

| Filename contains | Returned profile | Effect |
|-------------------|-----------------|--------|
| `pass` | `Name="John Michael Smith"`, exact address, exact DOB | PASS |
| `review` | Same name/DOB, address has `Apt 2B` appended | REVIEW |
| _(anything else)_ | `Name="Jane Doe"`, different address, different DOB | FAIL |

To regenerate the PNG files (e.g. after changing the demo data), run:

```bash
python3 sample-documents/generate.py
```
