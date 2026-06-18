import { useState, useEffect, useCallback } from 'react'
import { Plus, MoreVertical, Edit3, Trash2, Dumbbell, Clock, Target, Search, X, Check, Filter, ChevronUp, ChevronDown } from 'lucide-react'
import { getRoutines, createRoutine, updateRoutine, deleteRoutine } from '../api/routines'
import { searchExercises, getAllApiExercises } from '../api/exercises'
import '../styles/Routines.css'

export default function Routines() {
  const [routines, setRoutines] = useState([])
  const [selectedId, setSelectedId] = useState(null)
  const [loading, setLoading] = useState(true)
  const [showDialog, setShowDialog] = useState(false)
  const [editingRoutine, setEditingRoutine] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')

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

  const selected = routines.find(r => r.id === selectedId)

  async function handleDelete(id) {
    if (!confirm('Delete this routine?')) return
    try {
      await deleteRoutine(id)
      if (selectedId === id) setSelectedId(null)
      fetchRoutines()
    } catch {}
  }

  const filtered = routines.filter(r =>
    r.name?.toLowerCase().includes(searchTerm.toLowerCase())
  )

  function openCreate() {
    setEditingRoutine(null)
    setShowDialog(true)
  }

  function openEdit(routine) {
    setEditingRoutine(routine)
    setShowDialog(true)
  }

  function getMuscleColor(name) {
    const colors = ['#f97316', '#10b981', '#3b82f6', '#f97316', '#eab308', '#ec4899', '#ef4444']
    let hash = 0
    for (let i = 0; i < (name || '').length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
    return colors[Math.abs(hash) % colors.length]
  }

  return (
    <div className="routines-page">
      <div className="page-header">
        <div>
          <h1>Routines</h1>
          <p className="page-subtitle">Create and manage your workout routines</p>
        </div>
        <button className="create-btn" onClick={openCreate}>
          <Plus size={18} /> Create Routine
        </button>
      </div>

      <div className="search-bar">
        <Search size={16} className="search-icon-sm" />
        <input
          type="text"
          placeholder="Search routines..."
          value={searchTerm}
          onChange={e => setSearchTerm(e.target.value)}
          className="search-input-sm"
        />
        {searchTerm && (
          <button className="clear-btn-sm" onClick={() => setSearchTerm('')}><X size={14} /></button>
        )}
      </div>

      <div className="routines-layout">
        <div className="routines-list">
          {loading && <div className="loading-state">Loading...</div>}
          {!loading && filtered.length === 0 && (
            <div className="empty-list">
              <Dumbbell size={40} className="empty-icon" />
              <p>{searchTerm ? 'No routines match your search' : 'No routines yet'}</p>
              {!searchTerm && <button className="create-btn-sm" onClick={openCreate}>Create your first routine</button>}
            </div>
          )}
          <div className="routine-cards">
            {filtered.map(r => (
              <div
                key={r.id}
                className={`routine-card ${selectedId === r.id ? 'selected' : ''}`}
                onClick={() => setSelectedId(r.id)}
              >
                <div className="routine-icon" style={{ background: getMuscleColor(r.name) }}>
                  <Dumbbell size={20} />
                </div>
                <div className="routine-card-body">
                  <h4>{r.name}</h4>
                  <div className="routine-meta">
                    <span className="routine-day">{r.dayOfWeekName}</span>
                    <span className="routine-exercises">{r.exercises?.length || 0} exercises</span>
                  </div>
                </div>
                <div className="routine-card-menu">
                  <button className="menu-btn" onClick={e => { e.stopPropagation(); openEdit(r) }}>
                    <MoreVertical size={16} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="routine-detail">
          {!selected ? (
            <div className="no-selection">
              <Dumbbell size={48} className="empty-icon" />
              <p>Select a routine to see its details</p>
            </div>
          ) : (
            <DetailPanel
              routine={selected}
              onUpdate={fetchRoutines}
              onDelete={handleDelete}
              onEdit={() => openEdit(selected)}
            />
          )}
        </div>
      </div>

      {showDialog && (
        <RoutineDialog
          routine={editingRoutine}
          onClose={() => setShowDialog(false)}
          onSave={() => { setShowDialog(false); fetchRoutines() }}
        />
      )}
    </div>
  )
}

function DetailPanel({ routine, onUpdate, onDelete, onEdit }) {
  const [allExercises, setAllExercises] = useState([])
  const [loading, setLoading] = useState(false)
  const [search, setSearch] = useState('')
  const [selectedMuscles, setSelectedMuscles] = useState([])
  const [page, setPage] = useState(1)
  const [failedImages, setFailedImages] = useState(new Set())
  const [selectedExercise, setSelectedExercise] = useState(null)
  const MUSCLE_GROUPS = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Core', 'Cardio']
  const MUSCLE_TO_BODY_PARTS = {
    Chest: ['chest'], Back: ['back'], Shoulders: ['shoulders'],
    Arms: ['upper arms'], Legs: ['upper legs', 'lower legs'],
    Core: ['waist'], Cardio: ['cardio'],
  }
  const ITEMS_PER_PAGE = 40

  const routineExIds = new Set((routine.exercises || []).map(e => e.exerciseId))

  useEffect(() => {
    let cancelled = false
    async function load() {
      setLoading(true)
      try {
        const trimmed = search.trim()
        const res = trimmed
          ? await searchExercises({ name: trimmed })
          : await getAllApiExercises()
        if (!cancelled) {
          setAllExercises(res || [])
          setFailedImages(new Set())
          setPage(1)
        }
      } catch {
        if (!cancelled) setAllExercises([])
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    return () => { cancelled = true }
  }, [search])

  const filtered = selectedMuscles.length
    ? allExercises.filter(ex =>
        ex.bodyParts?.some(bp => selectedMuscles.some(m => MUSCLE_TO_BODY_PARTS[m].includes(bp.toLowerCase())))
      )
    : allExercises

  const totalPages = Math.max(1, Math.ceil(filtered.length / ITEMS_PER_PAGE))
  const safePage = Math.min(page, totalPages)
  const displayExercises = filtered.slice((safePage - 1) * ITEMS_PER_PAGE, safePage * ITEMS_PER_PAGE)

  useEffect(() => { if (page > totalPages) setPage(totalPages) }, [totalPages])

  async function addExercise(ex) {
    try {
      await updateRoutine(routine.id, {
        name: routine.name,
        description: routine.description,
        dayOfWeeks: routine.dayOfWeekOrders?.length ? routine.dayOfWeekOrders : [],
        exercises: [
          ...(routine.exercises || []).map(e => ({
            externalApiId: null,
            exerciseId: e.exerciseId,
            sets: e.sets,
            reps: e.reps,
            order: 1,
            restTimeSeconds: e.restTimeSeconds,
            notes: e.notes || ''
          })),
          {
            externalApiId: ex.exerciseId,
            sets: 3,
            reps: 10,
            order: 1,
            restTimeSeconds: 60,
            notes: ''
          }
        ]
      })
      onUpdate()
    } catch {}
  }

  async function removeExercise(exerciseId) {
    try {
      await updateRoutine(routine.id, {
        name: routine.name,
        description: routine.description,
        dayOfWeeks: routine.dayOfWeekOrders?.length ? routine.dayOfWeekOrders : [],
        exercises: (routine.exercises || [])
          .filter(e => e.exerciseId !== exerciseId)
          .map(e => ({
            externalApiId: null,
            exerciseId: e.exerciseId,
            sets: e.sets,
            reps: e.reps,
            order: 1,
            restTimeSeconds: e.restTimeSeconds,
            notes: e.notes || ''
          }))
      })
      onUpdate()
    } catch {}
  }

  async function updateField(index, field, value) {
    const exercises = [...(routine.exercises || [])]
    exercises[index] = { ...exercises[index], [field]: value }
    try {
      await updateRoutine(routine.id, {
        name: routine.name,
        description: routine.description,
        dayOfWeeks: routine.dayOfWeekOrders?.length ? routine.dayOfWeekOrders : [],
        exercises: exercises.map(e => ({
          externalApiId: null,
          exerciseId: e.exerciseId,
          sets: e.sets,
          reps: e.reps,
          order: 1,
          restTimeSeconds: e.restTimeSeconds,
          notes: e.notes || ''
        }))
      })
      onUpdate()
    } catch {}
  }

  async function moveExercise(index, direction) {
    const exercises = [...(routine.exercises || [])]
    const target = index + direction
    if (target < 0 || target >= exercises.length) return
    ;[exercises[index], exercises[target]] = [exercises[target], exercises[index]]
    try {
      await updateRoutine(routine.id, {
        name: routine.name,
        description: routine.description,
        dayOfWeeks: routine.dayOfWeekOrders?.length ? routine.dayOfWeekOrders : [],
        exercises: exercises.map(e => ({
          externalApiId: null,
          exerciseId: e.exerciseId,
          sets: e.sets,
          reps: e.reps,
          order: 1,
          restTimeSeconds: e.restTimeSeconds,
          notes: e.notes || ''
        }))
      })
      onUpdate()
    } catch {}
  }

  function NumInput({ value, min, onChange }) {
    return (
      <div className="num-input">
        <button type="button" className="num-btn" onClick={() => onChange(Math.max(min, (value || 0) - 1))} disabled={(value || 0) <= min}>-</button>
        <input type="number" min={min} value={value} onChange={e => onChange(Number(e.target.value))} className="num-field" />
        <button type="button" className="num-btn" onClick={() => onChange((value || 0) + 1)}>+</button>
      </div>
    )
  }

  return (
    <div className="detail-content">
      <div className="detail-header">
        <div>
          <h2>{routine.name}</h2>
          <p className="detail-day">{routine.dayOfWeekName || 'Unassigned'}</p>
        </div>
        <div className="detail-actions">
          <span className="detail-ex-count">{routine.exercises?.length || 0} exercises</span>
          <button className="icon-btn" onClick={onEdit} title="Edit"><Edit3 size={16} /></button>
          <button className="icon-btn danger" onClick={() => onDelete(routine.id)} title="Delete"><Trash2 size={16} /></button>
        </div>
      </div>

      {routine.description && <p className="detail-desc">{routine.description}</p>}

      <div className="picker-section">
        <div className="picker-toolbar">
          <div className="picker-search-box">
            <Search size={16} className="search-icon" />
            <input
              type="text" placeholder="Search exercises..."
              value={search} onChange={e => setSearch(e.target.value)}
              className="picker-search-input"
            />
            {search && <button className="clear-btn" onClick={() => setSearch('')}><X size={14} /></button>}
          </div>
          <div className="picker-muscle-pills">
            {MUSCLE_GROUPS.map(m => (
              <button
                key={m}
                className={`picker-pill ${selectedMuscles.includes(m) ? 'active' : ''}`}
                onClick={() => { setSelectedMuscles(prev => prev.includes(m) ? prev.filter(x => x !== m) : [...prev, m]); setPage(1) }}
              >
                {m}
              </button>
            ))}
          </div>
        </div>

        {loading && <div className="loading-hint">Loading exercises...</div>}

        {!loading && displayExercises.length === 0 && (
          <div className="empty-hint">No exercises found. Try a different search.</div>
        )}

        {!loading && displayExercises.length > 0 && (
          <>
            <div className="picker-grid">
              {displayExercises.map(ex => {
                const alreadyAdded = routineExIds.has(ex.exerciseId)
                return (
                  <div
                    key={ex.exerciseId}
                    className={`picker-card ${alreadyAdded ? 'added' : ''}`}
                    onClick={() => setSelectedExercise(ex)}
                  >
                    {alreadyAdded && <div className="picker-added-badge"><Check size={16} /></div>}
                    <div className="exercise-img-wrapper">
                      {ex.gifUrl && !failedImages.has(ex.exerciseId) ? (
                        <img src={ex.gifUrl} alt={ex.name} className="exercise-img" loading="lazy" onError={() => setFailedImages(prev => new Set(prev).add(ex.exerciseId))} />
                      ) : (
                        <div className="exercise-img-placeholder"><Dumbbell size={40} /></div>
                      )}
                    </div>
                    <div className="exercise-info">
                      <h3>{ex.name}</h3>
                      <div className="exercise-tags">
                        {ex.bodyParts?.slice(0, 2).map(bp => <span key={bp} className="tag tag-muscle">{bp}</span>)}
                        {ex.equipments?.slice(0, 1).map(eq => <span key={eq} className="tag tag-equipment">{eq}</span>)}
                      </div>
                      <div className="exercise-targets">
                        {ex.targetMuscles?.slice(0, 2).map(tm => <span key={tm} className="target-label">{tm}</span>)}
                      </div>
                    </div>
                  </div>
                )
              })}
            </div>

            {totalPages > 1 && (
              <div className="picker-pagination">
                <button disabled={safePage <= 1} onClick={() => setPage(p => p - 1)} className="page-btn">Previous</button>
                <span className="page-info">Page {safePage} of {totalPages}</span>
                <button disabled={safePage >= totalPages} onClick={() => setPage(p => p + 1)} className="page-btn">Next</button>
              </div>
            )}
          </>
        )}
      </div>

      {routine.exercises?.length > 0 && (
        <div className="detail-section">
          <label className="detail-section-label">Added Exercises</label>
          <div className="added-table">
            <div className="added-table-header">
              <span className="col-num"></span>
              <span className="col-img"></span>
              <span className="col-name">Exercise</span>
              <span className="col-sets">Sets</span>
              <span className="col-reps">Reps</span>
              <span className="col-rest">Rest (s)</span>
              <span className="col-actions"></span>
            </div>
            {routine.exercises.map((ex, i) => (
              <div key={ex.id || i} className="added-table-row">
                <span className="col-num">
                  <button className="move-btn" onClick={() => moveExercise(i, -1)} disabled={i === 0} title="Move up"><ChevronUp size={14} /></button>
                  <span className="row-idx">{i + 1}</span>
                  <button className="move-btn" onClick={() => moveExercise(i, 1)} disabled={i === routine.exercises.length - 1} title="Move down"><ChevronDown size={14} /></button>
                </span>
                <span className="col-img">
                  {ex.exerciseGifUrl && <img src={ex.exerciseGifUrl} alt="" className="ex-thumb" />}
                </span>
                <span className="col-name">{ex.exerciseName}</span>
                <span className="col-sets">
                  <NumInput value={ex.sets} min={1} onChange={v => updateField(i, 'sets', v)} />
                </span>
                <span className="col-reps">
                  <NumInput value={ex.reps} min={1} onChange={v => updateField(i, 'reps', v)} />
                </span>
                <span className="col-rest">
                  <NumInput value={ex.restTimeSeconds} min={0} onChange={v => updateField(i, 'restTimeSeconds', v)} />
                </span>
                <span className="col-actions">
                  <button className="icon-btn-sm danger" onClick={() => removeExercise(ex.exerciseId)} title="Remove"><Trash2 size={14} /></button>
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {selectedExercise && (
        <div className="modal-overlay" onClick={() => setSelectedExercise(null)}>
          <div className="exercise-modal" onClick={e => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setSelectedExercise(null)}><X size={20} /></button>
            <div className="modal-image">
              {selectedExercise.gifUrl ? (
                <img src={selectedExercise.gifUrl} alt={selectedExercise.name} />
              ) : (
                <Dumbbell size={48} />
              )}
            </div>
            <h2 className="modal-title">{selectedExercise.name}</h2>
            <div className="modal-tags">
              {selectedExercise.bodyParts?.map(bp => <span key={bp} className="tag tag-muscle">{bp}</span>)}
            </div>
            {selectedExercise.targetMuscles?.length > 0 && (
              <div className="modal-section">
                <strong>Target Muscles:</strong>
                <p>{selectedExercise.targetMuscles.join(', ')}</p>
              </div>
            )}
            {selectedExercise.secondaryMuscles?.length > 0 && (
              <div className="modal-section">
                <strong>Secondary Muscles:</strong>
                <p>{selectedExercise.secondaryMuscles.join(', ')}</p>
              </div>
            )}
            {selectedExercise.equipments?.length > 0 && (
              <div className="modal-section">
                <strong>Equipment:</strong>
                <p>{selectedExercise.equipments.join(', ')}</p>
              </div>
            )}
            {selectedExercise.instructions?.length > 0 && (
              <div className="modal-section">
                <strong>Instructions:</strong>
                <ol className="modal-instructions">
                  {selectedExercise.instructions.map((step, i) => <li key={i}>{step}</li>)}
                </ol>
              </div>
            )}
            {!routineExIds.has(selectedExercise.exerciseId) && (
              <button className="modal-add-btn" onClick={() => { addExercise(selectedExercise); setSelectedExercise(null) }}>
                Add to Routine
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

function RoutineDialog({ routine, onClose, onSave }) {
  const isEdit = !!routine
  const [name, setName] = useState(routine?.name || '')
  const [description, setDescription] = useState(routine?.description || '')
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setSaving(true)
    try {
      const payload = { name, description }

      if (isEdit) {
        payload.dayOfWeeks = routine.dayOfWeekOrders?.length ? routine.dayOfWeekOrders : []
        payload.exercises = (routine.exercises || []).map(ex => ({
          externalApiId: null,
          exerciseId: ex.exerciseId,
          sets: ex.sets,
          reps: ex.reps,
          order: 1,
          restTimeSeconds: ex.restTimeSeconds,
          notes: ex.notes || ''
        }))
        await updateRoutine(routine.id, payload)
      } else {
        await createRoutine(payload)
      }
      onSave()
    } catch (err) {
      alert('Failed to save routine: ' + (err.response?.data?.message || err.message))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="dialog-overlay" onClick={onClose}>
      <div className="dialog" onClick={e => e.stopPropagation()}>
        <div className="dialog-header">
          <h2>{isEdit ? 'Edit Routine' : 'Create Routine'}</h2>
          <button className="close-btn" onClick={onClose}><X size={20} /></button>
        </div>

        <form onSubmit={handleSubmit} className="dialog-form">
          <div className="form-group">
            <label>Routine Name</label>
            <input
              required value={name} onChange={e => setName(e.target.value)}
              className="form-input" placeholder="e.g. Push Day"
            />
          </div>

          <div className="form-group">
            <label>Description (optional)</label>
            <textarea
              value={description} onChange={e => setDescription(e.target.value)}
              className="form-input form-textarea" placeholder="Chest, shoulders, triceps"
              rows={2}
            />
          </div>

          <div className="dialog-actions">
            <button type="button" className="cancel-btn" onClick={onClose}>Cancel</button>
            <button type="submit" className="save-btn" disabled={saving}>
              {saving ? 'Saving...' : isEdit ? 'Save Changes' : 'Create Routine'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}