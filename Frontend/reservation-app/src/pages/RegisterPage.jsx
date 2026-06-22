import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import authService from '../services/authService'

function RegisterPage() {
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')

    // Validar que las contraseñas coincidan
    if (password !== confirmPassword) {
      setError('Las contrasenas no coinciden')
      return
    }

    // Validar longitud de contraseña
    if (password.length < 6) {
      setError('La contrasena debe tener al menos 6 caracteres')
      return
    }

    setLoading(true)

    try {
      // Registrar usuario
      const registerResponse = await authService.register(fullName, email, password)

      if (registerResponse.success) {
        // Auto-login después de registrar
        const loginResponse = await authService.login(email, password)

        if (loginResponse.success) {
          login(loginResponse.data)
          navigate('/client/dashboard')
        } else {
          // Si el login falla, redirigir a login page
          navigate('/login')
        }
      } else {
        setError(registerResponse.message || 'Error al registrar usuario')
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
                <p className="text-muted">Crear cuenta nueva</p>
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
                  <label htmlFor="fullName" className="form-label">
                    Nombre completo
                  </label>
                  <input
                    type="text"
                    className="form-control"
                    id="fullName"
                    placeholder="Juan Perez"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    required
                    disabled={loading}
                  />
                </div>

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
                    placeholder="Minimo 6 caracteres"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    disabled={loading}
                  />
                </div>

                <div className="mb-3">
                  <label htmlFor="confirmPassword" className="form-label">
                    Confirmar contrasena
                  </label>
                  <input
                    type="password"
                    className="form-control"
                    id="confirmPassword"
                    placeholder="Repite tu contrasena"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
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
                      Registrando...
                    </>
                  ) : (
                    'Crear Cuenta'
                  )}
                </button>
              </form>

              {/* Link a login */}
              <div className="text-center mt-3">
                <span className="text-muted">Ya tienes cuenta? </span>
                <Link to="/login" className="text-decoration-none">
                  Inicia sesion
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default RegisterPage
