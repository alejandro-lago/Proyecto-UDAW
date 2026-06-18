import { useState, useEffect } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { Dumbbell, Calendar, ListChecks, Search, User, LogOut, Sun, Moon } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import ProfileModal from './ProfileModal'
import '../../styles/Routines.css'
import '../../styles/Sidebar.css'

const navItems = [
  { to: '/planning', label: 'Planning', icon: Calendar },
  { to: '/routines', label: 'Routines', icon: ListChecks },
  { to: '/exercises', label: 'Exercises', icon: Search }
]

function Sidebar({ onNavClick }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [showProfile, setShowProfile] = useState(false)
  const [isDark, setIsDark] = useState(() => localStorage.getItem('theme') !== 'light')

  useEffect(() => {
    document.body.classList.toggle('light-theme', !isDark)
    localStorage.setItem('theme', isDark ? 'dark' : 'light')
  }, [isDark])

  return (
    <aside className="sidebar">
      <div className="sidebar-logo">
        <Dumbbell className="sidebar-logo-icon" />
        <h1>FitPlanner</h1>
      </div>

      <nav className="sidebar-nav">
        {navItems.map(({ to, label, icon: Icon }) => (
          <NavLink key={to} to={to} onClick={onNavClick} className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
            <Icon size={20} />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-bottom">
        <button className="sidebar-link" onClick={() => setIsDark(d => !d)}>
          {isDark ? <Sun size={20} /> : <Moon size={20} />}<span>{isDark ? 'Light Mode' : 'Dark Mode'}</span>
        </button>
        <button className="sidebar-link" onClick={() => setShowProfile(true)}>
          <User size={20} /><span>Profile</span>
        </button>
        <button className="sidebar-link" onClick={() => { logout(); navigate('/login') }}>
          <LogOut size={20} /><span>Sign out</span>
        </button>

        {user && (
          <div className="sidebar-user">
            <div className="sidebar-avatar">
              {user.profilePictureUrl ? (
                <img src={user.profilePictureUrl} alt="avatar" />
              ) : (
                user.firstName?.[0] ?? 'U'
              )}
            </div>
            <div className="sidebar-user-info">
              <p className="sidebar-user-name">{user.firstName}</p>
              <p className="sidebar-user-email">{user.email}</p>
            </div>
          </div>
        )}
      </div>

      {showProfile && <ProfileModal onClose={() => setShowProfile(false)} />}
    </aside>
  )
}

export default Sidebar
