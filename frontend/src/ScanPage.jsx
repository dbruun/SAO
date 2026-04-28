import { useState, useRef } from 'react'

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

function FileUpload({ id, label, required, file, onChange }) {
  const inputRef = useRef(null)
  return (
    <div className="form-group">
      <label>{label}{required && ' *'}</label>
      <div className="upload-zone" onClick={() => inputRef.current?.click()}>
        <input
          ref={inputRef}
          id={id}
          type="file"
          accept="image/*,.pdf"
          onChange={e => onChange(e.target.files?.[0] || null)}
        />
        <div className="upload-text">📁 Click to choose file</div>
        {file && <div className="file-name">✓ {file.name}</div>}
      </div>
      {required && <small>Accepted: JPEG, PNG, PDF — required</small>}
      {!required && <small>Optional</small>}
    </div>
  )
}

function CheckItem({ label, value }) {
  const isString = typeof value === 'string'
  let icon, color
  if (isString) {
    icon  = value === 'Passed' ? '✅' : value === 'Failed' ? '❌' : '⏸'
    color = value === 'Passed' ? '#2e7d32' : value === 'Failed' ? '#c62828' : '#888'
  } else {
    icon  = value ? '✅' : '❌'
    color = value ? '#2e7d32' : '#c62828'
  }
  return (
    <div className="check-item">
      <span>{icon}</span>
      <span style={{ color }}>{label}</span>
    </div>
  )
}

function ScoreBar({ label, score }) {
  const pct = Math.round(score * 1000) / 10
  return (
    <div className="score-item">
      <div className="score-label">{label}</div>
      <div className="score-value">{pct.toFixed(1)}%</div>
      <div className="score-track">
        <div className="score-fill" style={{ width: `${pct}%` }} />
      </div>
    </div>
  )
}

export default function ScanPage() {
  const [fileFront, setFileFront] = useState(null)
  const [fileBack, setFileBack]   = useState(null)
  const [loading, setLoading]     = useState(false)
  const [error, setError]         = useState(null)
  const [result, setResult]       = useState(null)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!fileFront) { setError('Please upload the front of the ID document.'); return }

    setError(null)
    setLoading(true)
    setResult(null)

    try {
      const form = new FormData()
      form.append('fileFront', fileFront)
      if (fileBack) form.append('fileBack', fileBack)

      const res = await fetch(`${API_BASE}/api/scan`, { method: 'POST', body: form })

      if (!res.ok) {
        const err = await res.json().catch(() => ({}))
        throw new Error(err.error || `Server error: HTTP ${res.status}`)
      }

      setResult(await res.json())
    } catch (err) {
      setError(err.message || 'An unexpected error occurred.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container">
      <h1>📤 Document Scan</h1>
      <p className="subtitle">Upload a document — the system extracts your details and checks them against backend records automatically</p>

      <div className="hint">
        <strong>Demo tip:</strong> Name your file with <strong>"pass"</strong> for a PASS result,{' '}
        <strong>"review"</strong> for REVIEW, or any other name for FAIL.{' '}
        No manual data entry needed — the document is read and verified automatically.
      </div>

      <div className="card">
        <h2>Upload Document</h2>
        <form onSubmit={handleSubmit}>
          <FileUpload id="fileFront" label="ID Front" required file={fileFront} onChange={setFileFront} />
          <FileUpload id="fileBack"  label="ID Back"  file={fileBack}  onChange={setFileBack}  />

          {error && <div className="error-msg">⚠️ {error}</div>}
          <button type="submit" className="btn" disabled={loading}>
            {loading ? '⏳ Scanning…' : 'Scan Document'}
          </button>
        </form>
      </div>

      {loading && (
        <div className="card loading-card">⏳ Extracting and verifying your document…</div>
      )}

      {result && (
        <>
          <div className="card">
            <h2>Scan Result</h2>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', flexWrap: 'wrap' }}>
              <span className={`status-badge status-${result.status}`}>{result.status}</span>
              <div>
                <div className="confidence-score">{result.confidenceScore}</div>
                <div className="confidence-label">Confidence Score (0–100)</div>
              </div>
            </div>
            <div className="checks-grid">
              <CheckItem label="Name Match"        value={result.checks.nameMatch} />
              <CheckItem label="Address Match"     value={result.checks.addressMatch} />
              <CheckItem label="DOB Match"         value={result.checks.dobMatch} />
              <CheckItem label="Addr Validated"    value={result.checks.addressValidated} />
              <CheckItem label={`Liveness: ${result.checks.liveness}`} value={result.checks.liveness} />
            </div>
          </div>

          <div className="card">
            <h2>Similarity Scores</h2>
            <div className="scores-grid">
              <ScoreBar label="Name Similarity"    score={result.similarity.nameScore} />
              <ScoreBar label="Address Similarity" score={result.similarity.addressScore} />
            </div>
          </div>

          <div className="card">
            <h2>Extracted Document Fields</h2>
            <div className="extracted-grid">
              <div className="extracted-item">
                <div className="extracted-label">Name</div>
                <div className="extracted-value">{result.extracted.name || '—'}</div>
              </div>
              <div className="extracted-item">
                <div className="extracted-label">Address</div>
                <div className="extracted-value">{result.extracted.address || '—'}</div>
              </div>
              <div className="extracted-item">
                <div className="extracted-label">Date of Birth</div>
                <div className="extracted-value">{result.extracted.dateOfBirth || '—'}</div>
              </div>
              {result.extracted.licenseNumber && (
                <div className="extracted-item">
                  <div className="extracted-label">License #</div>
                  <div className="extracted-value">{result.extracted.licenseNumber}</div>
                </div>
              )}
            </div>
          </div>

          <div className="card">
            <h2>Details</h2>
            <ul className="details-list">
              {result.details.map((d, i) => (
                <li key={i}><span>•</span>{d}</li>
              ))}
            </ul>
          </div>
        </>
      )}
    </div>
  )
}
