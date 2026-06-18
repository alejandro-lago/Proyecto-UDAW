import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import Layout from './components/layout/Layout'
import Sidebar from './components/layout/Sidebar'
import Body from './components/layout/Body'
import Login from './pages/Login'
import Register from './pages/Register'
import Planning from './pages/Planning'
import Routines from './pages/Routines'
import Exercises from './pages/Exercises'

function ProtectedLayout() {
  const token = localStorage.getItem('token')
  if (!token) return <Navigate to="/login" replace />
  return (
    <Layout>
      <Sidebar />
      <Body />
    </Layout>
  )
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route element={<ProtectedLayout />}>
            <Route path="/" element={<Navigate to="/planning" replace />} />
            <Route path="/planning" element={<Planning />} />
            <Route path="/routines" element={<Routines />} />
            <Route path="/exercises" element={<Exercises />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
