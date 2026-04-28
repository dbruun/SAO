# Identity Verification POC

A full-stack identity verification proof-of-concept:
- **Backend**: .NET 8 ASP.NET Core Web API
- **Frontend**: React + Vite SPA

Users upload a driver's license image, enter their name/address/date-of-birth, and receive a **PASS / REVIEW / FAIL** decision with a confidence score and per-field match details.

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.x |
| [Node.js](https://nodejs.org/) | 20+ |
| [Docker + Compose](https://www.docker.com/) | any recent |

---

## Project Structure

```
/backend
  /Controllers   VerificationController.cs
  /Services      Extraction, AddressValidation, Liveness, Verification
  /Models        Request/Response models
  /Helpers       Normalization, Matching, Scoring
  Program.cs
  appsettings.json
  Dockerfile

/frontend
  /src           App.jsx, main.jsx, index.css
  Dockerfile
  nginx.conf

docker-compose.yml
README.md
```

---

## Running Locally

### Backend

```bash
cd backend
dotnet run
# API:    http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# http://localhost:5173
```

> The frontend reads `VITE_API_BASE_URL` (defaults to `http://localhost:5000`).
> Copy `.env.example` to `.env` and adjust if needed.

---

## Running via Docker Compose

```bash
docker compose up --build
```

| Service  | URL                          |
|----------|------------------------------|
| Frontend | http://localhost:3000        |
| Backend  | http://localhost:5000        |
| Swagger  | http://localhost:5000/swagger |

---

## Sample Test Documents

Ready-made fake driver's licence images are in the `sample-documents/` folder:

| File | Scenario | Expected status |
|------|----------|-----------------|
| `sample-documents/pass-license.png` | Perfect match | **PASS** |
| `sample-documents/review-license.png` | Address has extra `Apt 2B` | **REVIEW** |
| `sample-documents/fail-license.png` | Different person + DOB mismatch | **FAIL** |

### Run all three scenarios at once

```bash
chmod +x sample-documents/test.sh
./sample-documents/test.sh          # default: http://localhost:5000
```

See [`sample-documents/README.md`](sample-documents/README.md) for full details
and how to regenerate the images.

---

## Example curl Requests

### PASS scenario (filename contains "pass")
```bash
curl -X POST http://localhost:5000/api/verification \
  -F "fileFront=@sample-documents/pass-license.png" \
  -F "fullName=John Michael Smith" \
  -F "address=123 Main Street, Springfield, IL 62701" \
  -F "dateOfBirth=1985-06-15"
```

### REVIEW scenario (filename contains "review")
```bash
curl -X POST http://localhost:5000/api/verification \
  -F "fileFront=@sample-documents/review-license.png" \
  -F "fullName=John Michael Smith" \
  -F "address=123 Main Street, Springfield, IL 62701" \
  -F "dateOfBirth=1985-06-15"
```

### FAIL scenario (filename contains "fail")
```bash
curl -X POST http://localhost:5000/api/verification \
  -F "fileFront=@sample-documents/fail-license.png" \
  -F "fullName=John Michael Smith" \
  -F "address=123 Main Street, Springfield, IL 62701" \
  -F "dateOfBirth=1985-06-15"
```

---

## Smoke Test Checklist

| # | Scenario | File pattern | Name | Address | DOB | Expected status | Expected confidence |
|---|----------|-------------|------|---------|-----|-----------------|---------------------|
| 1 | All match | `*pass*` | John Michael Smith | 123 Main Street, Springfield, IL 62701 | 1985-06-15 | **PASS** | ~99 |
| 2 | Address differs slightly | `*review*` | John Michael Smith | 123 Main Street, Springfield, IL 62701 | 1985-06-15 | **REVIEW** | ~89 |
| 3 | Wrong person + DOB | _(any)_ | John Michael Smith | 123 Main Street, Springfield, IL 62701 | 1985-06-15 | **FAIL** | ~29 |

---

## Architecture

```
React (Vite)
    │  POST /api/verification  (multipart/form-data)
    ▼
ASP.NET Core
    ├─ MockDocumentExtractionService  (or Azure Document Intelligence)
    │   ↳ PASS / REVIEW / FAIL profiles selected by filename
    ├─ NormalizationHelper            (abbrev expansion, casing)
    ├─ MatchingHelper                 (Levenshtein similarity 0–1)
    ├─ StubAddressValidationService   (plausibility checks)
    ├─ StubLivenessService            (NotPerformed stub)
    └─ ScoringHelper                  (weighted score → PASS/REVIEW/FAIL)
```

### Weights

| Field | Weight |
|-------|--------|
| Name match | 37% |
| Address match | 37% |
| Date of birth | 17% |
| Address validation | 7% |
| Liveness | 2% |

### Decision thresholds

| Status | Rule |
|--------|------|
| **PASS** | confidence ≥ 90 **and** name/address/DOB all pass |
| **REVIEW** | confidence 70–89 or minor mismatch |
| **FAIL** | confidence < 70 or DOB mismatch or very low scores |

---

## Azure Document Intelligence (optional)

Set in `appsettings.json` or via environment variables:

```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://your-resource.cognitiveservices.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

When configured, the backend uses the `prebuilt-idDocument` model for real OCR. When missing, it automatically falls back to the mock service.