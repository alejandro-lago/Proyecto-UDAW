import { useState, useEffect, useRef } from 'react'
import { createPortal } from 'react-dom'
import { X, Dumbbell, Upload } from 'lucide-react'
import { getProfile, updateProfile } from '../../api/profile'
import { useAuth } from '../../context/AuthContext'
import '../../styles/Routines.css'

export default function ProfileModal({ onClose }) {
  const { updateUser } = useAuth()
  const [profile, setProfile] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [fullName, setFullName] = useState('')
  const [bio, setBio] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [profilePictureUrl, setProfilePictureUrl] = useState('')
  const fileRef = useRef(null)

  useEffect(() => {
    let cancelled = false
    getProfile().then(data => {
      if (cancelled) return
      setProfile(data)
      setFullName(data.fullName || '')
      setBio(data.bio || '')
      setProfilePictureUrl(data.profilePictureUrl || '')
      setPhoneNumber(data.phoneNumber || '')
      if (data.dateOfBirth) setDateOfBirth(data.dateOfBirth.slice(0, 10))
    }).catch(() => {}).finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [])

  function handleFileSelect(e) {
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = (ev) => setProfilePictureUrl(ev.target?.result || '')
    reader.readAsDataURL(file)
  }

  async function handleSave(e) {
    e.preventDefault()
    setSaving(true)
    try {
      const updated = await updateProfile({
        fullName: fullName.trim() || undefined,
        bio: bio || undefined,
        dateOfBirth: dateOfBirth ? `${dateOfBirth}T00:00:00` : undefined,
        phoneNumber: phoneNumber || undefined,
        profilePictureUrl: profilePictureUrl || undefined
      })
      updateUser({ fullName: updated.fullName, firstName: updated.firstName, phoneNumber: updated.phoneNumber, profilePictureUrl: updated.profilePictureUrl, dateOfBirth: updated.dateOfBirth })
      onClose()
    } catch {}
    finally { setSaving(false); onClose() }
  }

  return createPortal(
    <div className="modal-overlay" onClick={onClose}>
      <div className="exercise-modal profile-modal" onClick={e => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}><X size={20} /></button>

        {loading ? (
          <p style={{ textAlign: 'center', padding: '40px 0', color: 'var(--text-muted)' }}>Loading...</p>
        ) : (
          <form onSubmit={handleSave}>
            <div className="profile-avatar-section">
              <div className="profile-avatar" onClick={() => fileRef.current?.click()}>
                {profilePictureUrl ? (
                  <img src={profilePictureUrl} alt="avatar" />
                ) : (
                  <Dumbbell size={32} />
                )}
                <div className="profile-avatar-overlay"><Upload size={18} /></div>
              </div>
              <input ref={fileRef} type="file" accept="image/*" onChange={handleFileSelect} style={{ display: 'none' }} />
              <span className="profile-email">{profile?.email}</span>
            </div>

            <div className="form-group">
              <label>Full Name</label>
              <input value={fullName} onChange={e => setFullName(e.target.value)} className="form-input" placeholder="Your name" />
            </div>

            <div className="form-group">
              <label>Bio</label>
              <textarea value={bio} onChange={e => setBio(e.target.value)} className="form-input form-textarea" rows={2} placeholder="Tell us about yourself" />
            </div>

            <div className="form-group">
              <label>Date of Birth</label>
              <input type="date" value={dateOfBirth} onChange={e => setDateOfBirth(e.target.value)} className="form-input" />
            </div>

            <div className="form-group">
              <label>Phone Number</label>
              <input type="tel" value={phoneNumber} onChange={e => setPhoneNumber(e.target.value)} className="form-input" placeholder="+1 (555) 123-4567" />
            </div>

            <div className="profile-actions">
              <button type="button" className="cancel-btn" onClick={onClose}>Cancel</button>
              <button type="submit" className="save-btn" disabled={saving}>{saving ? 'Saving...' : 'Save Changes'}</button>
            </div>
          </form>
        )}
      </div>
    </div>,
    document.body
  )
}
