import React, { useState } from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import ScanPage from './ScanPage'
import './index.css'

function Root() {
  const [page, setPage] = useState('manual')
  return (
    <>
      <nav className="page-nav">
        <button
          className={`nav-tab${page === 'manual' ? ' active' : ''}`}
          onClick={() => setPage('manual')}
        >
          📝 Manual Verification
        </button>
        <button
          className={`nav-tab${page === 'scan' ? ' active' : ''}`}
          onClick={() => setPage('scan')}
        >
          📤 Document Scan
        </button>
      </nav>
      {page === 'manual' ? <App /> : <ScanPage />}
    </>
  )
}

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <Root />
  </React.StrictMode>
)
