import api from './axios'

export const getRoutines = () =>
  api.get('/routine').then(r => r.data)

export const getRoutineById = (id) =>
  api.get(`/routine/${id}`).then(r => r.data)

export const createRoutine = (data) =>
  api.post('/routine', data).then(r => r.data)

export const updateRoutine = (id, data) =>
  api.put(`/routine/${id}`, data).then(r => r.data)

export const deleteRoutine = (id) =>
  api.delete(`/routine/${id}`).then(r => r.data)
