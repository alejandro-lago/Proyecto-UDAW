import api from './axios'

export const getProfile = () =>
  api.get('/profile').then(r => r.data)

export const updateProfile = (data) =>
  api.put('/profile', data).then(r => r.data)
