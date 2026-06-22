import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function UnauthorizedPage() {
  const { user } = useAuth()

  return (
    <div className="container">
      <div className="row justify-content-center align-items-center min-vh-100">
        <div className="col-md-6 text-center">
          <div className="card shadow border-0">
            <div className="card-body p-5">
              {/* Icono de error */}
              <div className="mb-4">
                <span className="display-1 text-danger">403</span>
              </div>

              <h2 className="fw-bold mb-3">Acceso Denegado</h2>

              <p className="text-muted mb-4">
                No tienes permisos para acceder a esta pagina.
                Contacta al administrador si crees que esto es un error.
              </p>

              {/* Botón para volver */}
              <Link
                to={user?.role === 'Admin' ? '/admin/dashboard' : '/client/dashboard'}
                className="btn btn-primary"
              >
                Volver al Dashboard
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default UnauthorizedPage
