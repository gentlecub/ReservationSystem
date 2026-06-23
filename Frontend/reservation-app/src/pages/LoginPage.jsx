import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import authService from '../services/authService'
import { GoogleLogin } from '@react-oauth/google'

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
        login(response.data)

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

  const handleGoogleSuccess = async (credentialResponse) => {
    setError('')
    setLoading(true)

    try {
      // Google devuelve un credential (ID token), lo enviamos al backend
      const response = await authService.googleAuth(credentialResponse.credential)

      if (response.success) {
        login(response.data)

        if (response.data.role === 'Admin') {
          navigate('/admin/dashboard')
        } else {
          navigate('/client/dashboard')
        }
      } else {
        setError(response.message || 'Error al iniciar sesion con Google')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al autenticar con Google'
      )
    } finally {
      setLoading(false)
    }
  }

  const handleGoogleError = () => {
    setError('Error al iniciar sesion con Google')
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

              {/* Boton de Google */}
              <div className="d-flex justify-content-center mb-3">
                <GoogleLogin
                  onSuccess={handleGoogleSuccess}
                  onError={handleGoogleError}
                  useOneTap
                  text="signin_with"
                  shape="rectangular"
                  locale="es"
                />
              </div>

              {/* Separador */}
              <div className="d-flex align-items-center mb-3">
                <hr className="flex-grow-1" />
                <span className="px-3 text-muted">o</span>
                <hr className="flex-grow-1" />
              </div>

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

                {/* Link Olvide contrasena */}
                <div className="mb-3 text-end">
                  <Link to="/forgot-password" className="text-decoration-none small">
                    Olvidaste tu contrasena?
                  </Link>
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
