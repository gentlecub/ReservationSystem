import { useState } from 'react'
import { Link } from 'react-router-dom'
import authService from '../services/authService'

function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setLoading(true)

    try {
      const response = await authService.forgotPassword(email)

      if (response.success) {
        setSuccess('Si el email existe, recibiras instrucciones para recuperar tu contrasena.')
        setEmail('')
      } else {
        setError(response.message || 'Error al procesar la solicitud')
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

  return (
    <div className="container">
      <div className="row justify-content-center align-items-center min-vh-100">
        <div className="col-md-6 col-lg-4">
          <div className="card shadow">
            <div className="card-body p-4">
              {/* Header */}
              <div className="text-center mb-4">
                <h2 className="fw-bold text-primary">SmartBook</h2>
                <p className="text-muted">Recuperar contrasena</p>
              </div>

              {/* Descripcion */}
              <p className="text-muted small mb-4">
                Ingresa tu correo electronico y te enviaremos un enlace para restablecer tu contrasena.
              </p>

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

                <button
                  type="submit"
                  className="btn btn-primary w-100"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                      Enviando...
                    </>
                  ) : (
                    'Enviar instrucciones'
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

export default ForgotPasswordPage
