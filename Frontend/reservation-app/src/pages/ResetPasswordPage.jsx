import { useState } from 'react'
import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import authService from '../services/authService'

function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const navigate = useNavigate()

  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')

    if (newPassword !== confirmPassword) {
      setError('Las contrasenas no coinciden')
      return
    }

    if (newPassword.length < 6) {
      setError('La contrasena debe tener al menos 6 caracteres')
      return
    }

    if (!token) {
      setError('Token de recuperacion no valido')
      return
    }

    setLoading(true)

    try {
      const response = await authService.resetPassword(token, newPassword, confirmPassword)

      if (response.success) {
        setSuccess('Contrasena restablecida exitosamente. Redirigiendo al login...')
        setTimeout(() => {
          navigate('/login')
        }, 2000)
      } else {
        setError(response.message || 'Error al restablecer contrasena')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error de conexion. Intenta de nuevo.'
      )
    } finally {
      setLoading(false)
    }
  }

  if (!token) {
    return (
      <div className="container">
        <div className="row justify-content-center align-items-center min-vh-100">
          <div className="col-md-6 col-lg-4">
            <div className="card shadow">
              <div className="card-body p-4 text-center">
                <h4 className="text-danger mb-3">Enlace invalido</h4>
                <p className="text-muted">
                  El enlace de recuperacion no es valido o ha expirado.
                </p>
                <Link to="/forgot-password" className="btn btn-primary">
                  Solicitar nuevo enlace
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    )
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
                <p className="text-muted">Nueva contrasena</p>
              </div>

              {/* Alertas */}
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

              {success && (
                <div className="alert alert-success" role="alert">
                  {success}
                </div>
              )}

              {/* Formulario */}
              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label htmlFor="newPassword" className="form-label">
                    Nueva contrasena
                  </label>
                  <input
                    type="password"
                    className="form-control"
                    id="newPassword"
                    placeholder="Minimo 6 caracteres"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
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
                      Guardando...
                    </>
                  ) : (
                    'Restablecer contrasena'
                  )}
                </button>
              </form>

              {/* Link a login */}
              <div className="text-center mt-3">
                <Link to="/login" className="text-decoration-none">
                  Volver al inicio de sesion
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ResetPasswordPage
