import api from './axios'

export const searchExercises = (params) =>
  api.get('/exercise/search', { params }).then(r => r.data)

export const getAllApiExercises = () =>
  api.get('/exercise/all').then(r => r.data)

export const getExerciseById = (id) =>
  api.get(`/exercise/${id}`).then(r => r.data)

export const getAllLocalExercises = () =>
  api.get('/exercise').then(r => r.data)
