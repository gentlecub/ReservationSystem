import { useState } from 'react'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import authService from '../services/authService'

function VerifyPhonePage() {
  const location = useLocation()
  const navigate = useNavigate()
  const phoneNumber = location.state?.phoneNumber || ''

  const [code, setCode] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)
  const [resending, setResending] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')

    if (code.length !== 6) {
      setError('El codigo debe tener 6 digitos')
      return
    }

    setLoading(true)

    try {
      const response = await authService.verifyPhone(phoneNumber, code)

      if (response.success) {
        setSuccess('Telefono verificado exitosamente. Redirigiendo...')
        setTimeout(() => {
          navigate('/client/dashboard')
        }, 2000)
      } else {
        setError(response.message || 'Codigo incorrecto')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al verificar codigo'
      )
    } finally {
      setLoading(false)
    }
  }

  const handleResend = async () => {
    setError('')
    setSuccess('')
    setResending(true)

    try {
      const response = await authService.resendSms(phoneNumber)

      if (response.success) {
        setSuccess('Codigo reenviado exitosamente')
      } else {
        setError(response.message || 'Error al reenviar codigo')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al reenviar codigo'
      )
    } finally {
      setResending(false)
    }
  }

  if (!phoneNumber) {
    return (
      <div className="container">
        <div className="row justify-content-center align-items-center min-vh-100">
          <div className="col-md-6 col-lg-4">
            <div className="card shadow">
              <div className="card-body p-4 text-center">
                <h4 className="text-danger mb-3">Sesion expirada</h4>
                <p className="text-muted">
                  No se encontro el numero de telefono. Por favor registrate de nuevo.
                </p>
                <Link to="/register" className="btn btn-primary">
                  Ir a registro
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
                <p className="text-muted">Verificar telefono</p>
              </div>

              {/* Descripcion */}
              <p className="text-muted small mb-4 text-center">
                Ingresa el codigo de 6 digitos enviado a<br />
                <strong>{phoneNumber}</strong>
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
                  <label htmlFor="code" className="form-label">
                    Codigo de verificacion
                  </label>
                  <input
                    type="text"
                    className="form-control form-control-lg text-center"
                    id="code"
                    placeholder="000000"
                    value={code}
                    onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                    maxLength={6}
                    required
                    disabled={loading}
                    style={{ letterSpacing: '0.5em', fontSize: '1.5em' }}
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-primary w-100 mb-3"
                  disabled={loading || code.length !== 6}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                      Verificando...
                    </>
                  ) : (
                    'Verificar codigo'
                  )}
                </button>

                <button
                  type="button"
                  className="btn btn-outline-secondary w-100"
                  onClick={handleResend}
                  disabled={resending}
                >
                  {resending ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                      Reenviando...
                    </>
                  ) : (
                    'Reenviar codigo'
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

export default VerifyPhonePage
