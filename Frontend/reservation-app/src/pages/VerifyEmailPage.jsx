import { useState, useEffect } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import authService from '../services/authService'

function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')

  const [status, setStatus] = useState('verifying') // 'verifying', 'success', 'error'
  const [message, setMessage] = useState('')

  useEffect(() => {
    const verifyEmail = async () => {
      if (!token) {
        setStatus('error')
        setMessage('Token de verificacion no proporcionado')
        return
      }

      try {
        const response = await authService.verifyEmail(token)

        if (response.success) {
          setStatus('success')
          setMessage('Tu email ha sido verificado exitosamente')
        } else {
          setStatus('error')
          setMessage(response.message || 'Error al verificar email')
        }
      } catch (err) {
        setStatus('error')
        setMessage(
          err.response?.data?.message ||
          'El enlace de verificacion es invalido o ha expirado'
        )
      }
    }

    verifyEmail()
  }, [token])

  return (
    <div className="container">
      <div className="row justify-content-center align-items-center min-vh-100">
        <div className="col-md-6 col-lg-4">
          <div className="card shadow">
            <div className="card-body p-4 text-center">
              {status === 'verifying' && (
                <>
                  <div className="spinner-border text-primary mb-3" role="status">
                    <span className="visually-hidden">Verificando...</span>
                  </div>
                  <h4>Verificando email...</h4>
                  <p className="text-muted">Por favor espera un momento</p>
                </>
              )}

              {status === 'success' && (
                <>
                  <div className="text-success mb-3">
                    <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" fill="currentColor" viewBox="0 0 16 16">
                      <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
                    </svg>
                  </div>
                  <h4 className="text-success">Email verificado</h4>
                  <p className="text-muted">{message}</p>
                  <Link to="/login" className="btn btn-primary">
                    Ir a iniciar sesion
                  </Link>
                </>
              )}

              {status === 'error' && (
                <>
                  <div className="text-danger mb-3">
                    <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" fill="currentColor" viewBox="0 0 16 16">
                      <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>
                    </svg>
                  </div>
                  <h4 className="text-danger">Error de verificacion</h4>
                  <p className="text-muted">{message}</p>
                  <Link to="/login" className="btn btn-primary">
                    Ir a iniciar sesion
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default VerifyEmailPage
