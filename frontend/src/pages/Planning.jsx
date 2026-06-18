import { useState, useEffect, useCallback } from 'react'
import { ChevronLeft, ChevronRight, Dumbbell, CalendarDays, Plus, X } from 'lucide-react'
import { getRoutines, updateRoutine } from '../api/routines'
import '../styles/Planning.css'

const DAYS = ['', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
const DAY_COLORS = ['#f97316', '#10b981', '#3b82f6', '#f97316', '#eab308', '#ec4899', '#ef4444']

export default function Planning() {
  const [routines, setRoutines] = useState([])
  const [loading, setLoading] = useState(true)
  const [weekOffset, setWeekOffset] = useState(0)
  const [expandedDay, setExpandedDay] = useState(null)
  const [dragId, setDragId] = useState(null)
  const [dropTarget, setDropTarget] = useState(null)
  const [saving, setSaving] = useState(false)

  const fetchRoutines = useCallback(async () => {
    setLoading(true)
    try {
      const data = await getRoutines()
      setRoutines(data || [])
    } catch {
      setRoutines([])
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchRoutines() }, [fetchRoutines])

  function getWeekDates(offset) {
    const today = new Date()
    const currentDay = today.getDay()
    const diff = currentDay === 0 ? -6 : 1 - currentDay
    const monday = new Date(today)
    monday.setDate(today.getDate() + diff + offset * 7)
    const days = []
    for (let i = 0; i < 7; i++) {
      const d = new Date(monday)
      d.setDate(monday.getDate() + i)
      days.push(d)
    }
    return days
  }

  const weekDays = getWeekDates(weekOffset)
  const weekStart = weekDays[0]
  const weekEnd = weekDays[6]
  const weekDayNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

  function getDayId(date) {
    const d = date.getDay()
    return d === 0 ? 7 : d
  }

  function getRoutinesForDay(dayOfWeekId) {
    return routines.filter(r => (r.dayOfWeekOrders || []).includes(dayOfWeekId))
  }

  function isToday(date) {
    return date.toDateString() === new Date().toDateString()
  }

  function getMuscleColor(name) {
    const colors = ['#f97316', '#10b981', '#3b82f6', '#f97316', '#eab308', '#ec4899', '#ef4444']
    let hash = 0
    for (let i = 0; i < (name || '').length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
    return colors[Math.abs(hash) % colors.length]
  }

  async function handleDrop(routineId, targetDay) {
    if (saving) return
    setSaving(true)
    try {
      const r = routines.find(x => x.id === routineId)
      if (!r) return

      const currentDays = r.dayOfWeekOrders || []
      if (!currentDays.includes(targetDay)) {
        await updateRoutine(routineId, {
          name: r.name,
          description: r.description,
          dayOfWeeks: [...currentDays, targetDay],
          exercises: (r.exercises || []).map(e => ({
            externalApiId: null,
            exerciseId: e.exerciseId,
            sets: e.sets,
            reps: e.reps,
            order: 1,
            restTimeSeconds: e.restTimeSeconds,
            notes: e.notes || ''
          }))
        })
      }

      setExpandedDay(targetDay)
      await fetchRoutines()
    } catch {}
    finally { setSaving(false); setDropTarget(null) }
  }

  const unassigned = routines.filter(r => !r.dayOfWeekOrders?.length)
  const assigned = routines.filter(r => r.dayOfWeekOrders?.length)
  const expandedRoutines = expandedDay ? getRoutinesForDay(expandedDay) : []

  return (
    <div className="planning-page">
      <div className="page-header">
        <div>
          <h1>Planning</h1>
          <p className="page-subtitle">Drag routines onto a day to assign them</p>
        </div>
        <button className="create-btn" onClick={() => window.location.href = '/routines'}>
          <Plus size={18} /> Manage Routines
        </button>
      </div>

      <div className="week-nav">
        <button className="week-nav-btn" onClick={() => setWeekOffset(o => o - 1)}>
          <ChevronLeft size={18} />
        </button>
        <div className="week-range">
          <CalendarDays size={16} />
          <span>
            {weekStart.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} – {weekEnd.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
          </span>
        </div>
        <button className="week-nav-btn" onClick={() => setWeekOffset(o => o + 1)}>
          <ChevronRight size={18} />
        </button>
        <button className="today-btn" onClick={() => setWeekOffset(0)}>
          Today
        </button>
      </div>

      <div className="week-grid">
        {weekDays.map((date, i) => {
          const dayId = getDayId(date)
          const dayRoutines = getRoutinesForDay(dayId)
          const today = isToday(date)
          const isExpanded = expandedDay === dayId
          const isOver = dropTarget === dayId
          const hasRoutines = dayRoutines.length > 0

          return (
            <div
              key={i}
              className={`day-card ${today ? 'today' : ''} ${isExpanded ? 'expanded' : ''} ${isOver ? 'drag-over' : ''} ${dragId ? 'can-drop' : ''}`}
              onClick={() => (hasRoutines || isExpanded) && !dragId && setExpandedDay(isExpanded ? null : dayId)}
              onDragOver={e => { e.preventDefault(); setDropTarget(dayId) }}
              onDragEnter={e => { e.preventDefault(); setDropTarget(dayId) }}
              onDragLeave={() => setDropTarget(null)}
              onDrop={e => { e.preventDefault(); if (dragId) handleDrop(dragId, dayId) }}
            >
              <div className="day-header">
                <span className="day-name">{weekDayNames[i]}</span>
                <span className="day-number">{date.getDate()}</span>
              </div>
              {hasRoutines ? (
                <div className="day-routines">
                  {dayRoutines.slice(0, 3).map(r => (
                    <div key={r.id} className="day-routine">
                      <div className="day-routine-icon" style={{ background: DAY_COLORS[dayId % DAY_COLORS.length] }}>
                        <Dumbbell size={16} />
                      </div>
                      <span className="day-routine-name">{r.name}</span>
                    </div>
                  ))}
                  {dayRoutines.length > 3 && (
                    <span className="day-routine-more">+{dayRoutines.length - 3} more</span>
                  )}
                </div>
              ) : (
                <div className="day-rest"><span>{dragId ? 'Drop to add' : 'Rest Day'}</span></div>
              )}
            </div>
          )
        })}
      </div>

      {expandedRoutines.length > 0 && (
        <div className="expanded-day-detail">
          <div className="edd-header">
            <div className="edd-title">
              <h3>{DAYS[expandedDay]}</h3>
              <span className="edd-count">{expandedRoutines.length} routine{expandedRoutines.length > 1 ? 's' : ''}</span>
            </div>
            <button className="close-btn" onClick={() => setExpandedDay(null)}><X size={18} /></button>
          </div>
          {expandedRoutines.map(r => (
            <div key={r.id} className="edd-routine-block">
              <div className="edd-routine-header">
                <span className="edd-routine-name">{r.name}</span>
                <span className="edd-count">{r.exercises?.length || 0} exercises</span>
              </div>
              {r.description && <p className="edd-desc">{r.description}</p>}
              {r.exercises?.length > 0 ? (
                <div className="edd-table">
                  <div className="edd-table-header">
                    <span className="col-num">#</span>
                    <span className="col-img"></span>
                    <span className="col-name">Exercise</span>
                    <span className="col-sets">Sets</span>
                    <span className="col-reps">Reps</span>
                    <span className="col-rest">Rest</span>
                  </div>
                  {r.exercises.map((ex, i) => (
                    <div key={ex.id || i} className="edd-table-row">
                      <span className="col-num">{i + 1}</span>
                      <span className="col-img">
                        {ex.exerciseGifUrl && <img src={ex.exerciseGifUrl} alt="" className="ex-thumb" />}
                      </span>
                      <span className="col-name">{ex.exerciseName}</span>
                      <span className="col-sets">{ex.sets}</span>
                      <span className="col-reps">{ex.reps}</span>
                      <span className="col-rest">{ex.restTimeSeconds}s</span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="edd-empty">No exercises in this routine.</p>
              )}
            </div>
          ))}
        </div>
      )}

      <div className="routines-section">
        <h2 className="section-title">
          All Routines
          <span className="drag-hint">Drag a routine onto a day above</span>
        </h2>
        {loading && <p className="loading-hint">Loading...</p>}
        {!loading && routines.length === 0 && (
          <div className="empty-state">
            <Dumbbell size={32} className="empty-icon" />
            <p>No routines yet</p>
            <button className="create-btn-sm" onClick={() => window.location.href = '/routines'}>
              Create your first routine
            </button>
          </div>
        )}

        {assigned.concat(unassigned).map(r => (
          <div
            key={r.id}
            className={`plan-routine-card ${dragId === r.id ? 'dragging' : ''}`}
            draggable={!saving}
            onDragStart={() => setDragId(r.id)}
            onDragEnd={() => setDragId(null)}
          >
            <div className="prc-icon" style={{ background: getMuscleColor(r.name) }}>
              <Dumbbell size={20} />
            </div>
            <div className="prc-body">
              <span className="prc-name">{r.name}</span>
              {r.description && <span className="prc-desc">{r.description}</span>}
              <span className="prc-ex-count">{r.exercises?.length || 0} exercises</span>
            </div>
            {r.dayOfWeekOrders?.length ? (
              <div className="prc-day-badges">
                {r.dayOfWeekOrders.map(d => <span key={d} className="prc-day-badge">{DAYS[d]}</span>)}
              </div>
            ) : (
              <span className="prc-day-badge unassigned">Unassigned</span>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
