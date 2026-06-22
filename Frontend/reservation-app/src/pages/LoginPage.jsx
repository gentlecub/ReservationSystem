import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import authService from '../services/authService'

function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const response = await authService.login(email, password)

      if (response.success) {
        // Guardar en contexto
        login(response.data)

        // Redirigir según rol
        if (response.data.role === 'Admin') {
          navigate('/admin/dashboard')
        } else {
          navigate('/client/dashboard')
        }
      } else {
        setError(response.message || 'Error al iniciar sesion')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error de conexion. Verifica que el servidor este activo.'
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container">
      <div className="row justify-content-center align-items-center min-vh-100">
        <div className="col-md-6 col-lg-4">
          <div className="card shadow">
            <div className="card-body p-4">
              {/* Header */}
              <div className="text-center mb-4">
                <h2 className="fw-bold text-primary">SmartBook</h2>
                <p className="text-muted">Sistema de Reservas</p>
              </div>

              {/* Alerta de error */}
              {error && (
                <div className="alert alert-danger alert-dismissible fade show" role="alert">
                  {error}
                  <button
                    type="button"
                    className="btn-close"
                    onClick={() => setError('')}
                  ></button>
                </div>
              )}

              {/* Formulario */}
              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label htmlFor="email" className="form-label">
                    Correo electronico
                  </label>
                  <input
                    type="email"
                    className="form-control"
                    id="email"
                    placeholder="correo@ejemplo.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                    disabled={loading}
                  />
                </div>

                <div className="mb-3">
                  <label htmlFor="password" className="form-label">
                    Contrasena
                  </label>
                  <input
                    type="password"
                    className="form-control"
                    id="password"
                    placeholder="Tu contrasena"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    disabled={loading}
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-primary w-100"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                      Iniciando sesion...
                    </>
                  ) : (
                    'Iniciar Sesion'
                  )}
                </button>
              </form>

              {/* Link a registro */}
              <div className="text-center mt-3">
                <span className="text-muted">No tienes cuenta? </span>
                <Link to="/register" className="text-decoration-none">
                  Registrate aqui
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default LoginPage
