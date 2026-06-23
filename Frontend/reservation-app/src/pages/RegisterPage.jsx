import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import authService from '../services/authService'
import { GoogleLogin } from '@react-oauth/google'

function RegisterPage() {
  const [registerType, setRegisterType] = useState('email') // 'email' o 'phone'
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')

    // Validar contrasenas
    if (password !== confirmPassword) {
      setError('Las contrasenas no coinciden')
      return
    }

    if (password.length < 6) {
      setError('La contrasena debe tener al menos 6 caracteres')
      return
    }

    setLoading(true)

    try {
      let response

      if (registerType === 'email') {
        response = await authService.register(fullName, email, password)

        if (response.success) {
          login(response.data)
          setSuccess('Cuenta creada. Revisa tu email para verificar tu cuenta.')
          // Redirigir despues de 2 segundos
          setTimeout(() => {
            navigate('/client/dashboard')
          }, 2000)
        }
      } else {
        // Registro con telefono
        response = await authService.registerWithPhone(fullName, phoneNumber, password)

        if (response.success) {
          login(response.data)
          // Redirigir a verificacion de telefono
          navigate('/verify-phone', { state: { phoneNumber } })
        }
      }

      if (!response.success) {
        setError(response.message || 'Error al registrar usuario')
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
      const response = await authService.googleAuth(credentialResponse.credential)

      if (response.success) {
        login(response.data)
        navigate('/client/dashboard')
      } else {
        setError(response.message || 'Error al registrar con Google')
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
    setError('Error al registrar con Google')
  }

  return (
    <div className="container">
      <div className="row justify-content-center align-items-center min-vh-100">
        <div className="col-md-6 col-lg-5">
          <div className="card shadow">
            <div className="card-body p-4">
              {/* Header */}
              <div className="text-center mb-4">
                <h2 className="fw-bold text-primary">SmartBook</h2>
                <p className="text-muted">Crear cuenta nueva</p>
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

              {/* Boton de Google */}
              <div className="d-flex justify-content-center mb-3">
                <GoogleLogin
                  onSuccess={handleGoogleSuccess}
                  onError={handleGoogleError}
                  text="signup_with"
                  shape="rectangular"
                  locale="es"
                />
              </div>

              {/* Separador */}
              <div className="d-flex align-items-center mb-3">
                <hr className="flex-grow-1" />
                <span className="px-3 text-muted">o registrate con</span>
                <hr className="flex-grow-1" />
              </div>

              {/* Tabs para tipo de registro */}
              <ul className="nav nav-pills nav-fill mb-3">
                <li className="nav-item">
                  <button
                    className={`nav-link ${registerType === 'email' ? 'active' : ''}`}
                    onClick={() => setRegisterType('email')}
                    type="button"
                  >
                    Email
                  </button>
                </li>
                <li className="nav-item">
                  <button
                    className={`nav-link ${registerType === 'phone' ? 'active' : ''}`}
                    onClick={() => setRegisterType('phone')}
                    type="button"
                  >
                    Telefono
                  </button>
                </li>
              </ul>

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

                {registerType === 'email' ? (
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
                ) : (
                  <div className="mb-3">
                    <label htmlFor="phoneNumber" className="form-label">
                      Numero de telefono
                    </label>
                    <input
                      type="tel"
                      className="form-control"
                      id="phoneNumber"
                      placeholder="+573001234567"
                      value={phoneNumber}
                      onChange={(e) => setPhoneNumber(e.target.value)}
                      required
                      disabled={loading}
                    />
                    <div className="form-text">
                      Incluye el codigo de pais (ej: +57 para Colombia)
                    </div>
                  </div>
                )}

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
