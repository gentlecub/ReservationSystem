import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './context/AuthContext'

// Pages
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ClientDashboard from './pages/ClientDashboard'
import AdminDashboard from './pages/AdminDashboard'
import UnauthorizedPage from './pages/UnauthorizedPage'

// Components
import Navbar from './components/Navbar'
import Footer from './components/Footer'
import PrivateRoute from './components/PrivateRoute'

function App() {
  const { user } = useAuth()

  return (
    <div className="d-flex flex-column min-vh-100">
      {/* Navbar solo si hay usuario logueado */}
      {user && <Navbar />}

      {/* Contenido principal */}
      <main className="flex-grow-1">
        <Routes>
          {/* Rutas públicas */}
          <Route
            path="/"
            element={
              user
                ? <Navigate to={user.role === 'Admin' ? '/admin/dashboard' : '/client/dashboard'} />
                : <Navigate to="/login" />
            }
          />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/unauthorized" element={<UnauthorizedPage />} />

          {/* Rutas protegidas - Client */}
          <Route
            path="/client/dashboard"
            element={
              <PrivateRoute allowedRoles={['Client']}>
                <ClientDashboard />
              </PrivateRoute>
            }
          />

          {/* Rutas protegidas - Admin */}
          <Route
            path="/admin/dashboard"
            element={
              <PrivateRoute allowedRoles={['Admin']}>
                <AdminDashboard />
              </PrivateRoute>
            }
          />

          {/* Ruta 404 */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </main>

      {/* Footer solo si hay usuario logueado */}
      {user && <Footer />}
    </div>
  )
}

export default App
