import { useState, useEffect, useMemo } from 'react'
import { Search, Dumbbell, Filter, X } from 'lucide-react'
import { searchExercises, getAllApiExercises } from '../api/exercises'
import '../styles/Exercises.css'
import '../styles/Routines.css'

const MUSCLE_GROUPS = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Core', 'Cardio']

const MUSCLE_TO_BODY_PARTS = {
  Chest: ['chest'],
  Back: ['back'],
  Shoulders: ['shoulders'],
  Arms: ['upper arms'],
  Legs: ['upper legs', 'lower legs'],
  Core: ['waist'],
  Cardio: ['cardio'],
}

const ITEMS_PER_PAGE = 40

export default function Exercises() {
  const [allExercises, setAllExercises] = useState([])
  const [loading, setLoading] = useState(false)
  const [search, setSearch] = useState('')
  const [selectedMuscle, setSelectedMuscle] = useState('')
  const [page, setPage] = useState(1)
  const [error, setError] = useState('')
  const [failedImages, setFailedImages] = useState(new Set())
  const [selectedExercise, setSelectedExercise] = useState(null)

  useEffect(() => {
    let cancelled = false
    async function load() {
      setLoading(true)
      setError('')
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
      } catch (err) {
        if (!cancelled) {
          setError('Failed to load exercises.')
          setAllExercises([])
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    return () => { cancelled = true }
  }, [search])

  const filtered = useMemo(() => {
    if (!selectedMuscle) return allExercises
    const allowed = MUSCLE_TO_BODY_PARTS[selectedMuscle]
    return allExercises.filter(ex =>
      ex.bodyParts?.some(bp => allowed.includes(bp.toLowerCase()))
    )
  }, [allExercises, selectedMuscle])

  const totalPages = Math.max(1, Math.ceil(filtered.length / ITEMS_PER_PAGE))
  const safePage = Math.min(page, totalPages)
  const displayExercises = filtered.slice((safePage - 1) * ITEMS_PER_PAGE, safePage * ITEMS_PER_PAGE)

  useEffect(() => {
    if (page > totalPages) setPage(totalPages)
  }, [totalPages])

  function handleSearch(e) {
    e.preventDefault()
  }

  function handleMuscleClick(muscle) {
    setSelectedMuscle(prev => prev === muscle ? '' : muscle)
    setPage(1)
  }

  function clearFilters() {
    setSelectedMuscle('')
    setSearch('')
    setPage(1)
  }

  return (
    <div className="exercises-page">
      <div className="page-header">
        <div>
          <h1>Exercises</h1>
          <p className="page-subtitle">
            {!loading && `${filtered.length} exercises`}
            {selectedMuscle && ` \u00B7 ${selectedMuscle}`}
          </p>
        </div>
      </div>

      <div className="filters-bar">
        <form onSubmit={handleSearch} className="search-box">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Search exercises..."
            value={search}
            onChange={e => setSearch(e.target.value)}
            className="search-input"
          />
          {search && (
            <button type="button" className="clear-btn" onClick={() => { setSearch(''); setPage(1) }}>
              <X size={16} />
            </button>
          )}
        </form>

        <div className="filter-select-wrapper">
          <Filter size={16} className="filter-icon" />
          <select
            value={selectedMuscle}
            onChange={e => { setSelectedMuscle(e.target.value); setPage(1) }}
            className="filter-select"
          >
            <option value="">All Muscle Groups</option>
            {MUSCLE_GROUPS.map(m => (
              <option key={m} value={m}>{m}</option>
            ))}
          </select>
        </div>

        {(selectedMuscle || search) && (
          <button className="reset-btn" onClick={clearFilters}>
            Reset Filters
          </button>
        )}
      </div>

      <div className="muscle-pills">
        {MUSCLE_GROUPS.map(m => (
          <button
            key={m}
            className={`muscle-pill ${selectedMuscle === m ? 'active' : ''}`}
            onClick={() => handleMuscleClick(m)}
          >
            {m}
          </button>
        ))}
      </div>

      {loading && <div className="loading"><Dumbbell className="spin" /> Loading exercises...</div>}

      {error && <div className="error-msg">{error}</div>}

      {!loading && !error && displayExercises.length === 0 && (
        <div className="empty-state">No exercises found. Try a different search.</div>
      )}

      {!loading && displayExercises.length > 0 && (
        <>
          <div className="exercises-grid" key={`${selectedMuscle}-${safePage}`}>
            {displayExercises.map((ex, i) => (
              <div key={ex.exerciseId} className="exercise-card" onClick={() => setSelectedExercise(ex)}>
                <div className="exercise-img-wrapper">
                  {ex.gifUrl && !failedImages.has(ex.exerciseId) ? (
                    <img src={ex.gifUrl} alt={ex.name} className="exercise-img" loading="lazy" onError={() => setFailedImages(prev => new Set(prev).add(ex.exerciseId))} />
                  ) : (
                    <div className="exercise-img-placeholder">
                      <Dumbbell size={32} />
                    </div>
                  )}
                </div>
                <div className="exercise-info">
                  <h3>{ex.name}</h3>
                  <div className="exercise-tags">
                    {ex.bodyParts?.slice(0, 2).map(bp => (
                      <span key={bp} className="tag tag-muscle">{bp}</span>
                    ))}
                    {ex.equipments?.slice(0, 1).map(eq => (
                      <span key={eq} className="tag tag-equipment">{eq}</span>
                    ))}
                  </div>
                  <div className="exercise-targets">
                    {ex.targetMuscles?.slice(0, 2).map(tm => (
                      <span key={tm} className="target-label">{tm}</span>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {totalPages > 1 && (
            <div className="pagination">
              <button
                disabled={safePage <= 1}
                onClick={() => setPage(p => Math.max(1, p - 1))}
                className="page-btn"
              >
                Previous
              </button>
              <span className="page-info">
                Page {safePage} of {totalPages}
                <span className="page-count"> ({filtered.length} total)</span>
              </span>
              <button
                disabled={safePage >= totalPages}
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                className="page-btn"
              >
                Next
              </button>
            </div>
          )}
        </>
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
          </div>
        </div>
      )}
    </div>
  )
}