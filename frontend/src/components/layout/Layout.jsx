import { useState, cloneElement } from 'react'
import { Menu, X } from 'lucide-react'
import '../../styles/Layout.css'

function Layout({ children }) {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const close = () => setSidebarOpen(false)

  return (
    <div className="layout">
      <button className="hamburger" onClick={() => setSidebarOpen(o => !o)}>
        {sidebarOpen ? <X size={22} /> : <Menu size={22} />}
      </button>

      {sidebarOpen && <div className="sidebar-overlay" onClick={close} />}

      {children[0] && (
        <div className={`sidebar-wrapper ${sidebarOpen ? 'open' : ''}`}>
          {cloneElement(children[0], { onNavClick: close })}
        </div>
      )}

      {children[1]}
    </div>
  )
}

export default Layout