import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useNotifications } from '../context/NotificationContext'

function Navbar() {
  const { user, logout, isAdmin } = useAuth()
  const { isConnected, unreadCount } = useNotifications()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container">
        {/* Logo */}
        <Link className="navbar-brand fw-bold" to="/">
          SmartBook
        </Link>

        {/* Boton hamburguesa para mobile */}
        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
          aria-controls="navbarNav"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        {/* Links de navegacion */}
        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav me-auto">
            {isAdmin() ? (
              // Links para Admin
              <>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/dashboard">
                    Dashboard
                  </Link>
                </li>
              </>
            ) : (
              // Links para Client
              <>
                <li className="nav-item">
                  <Link className="nav-link" to="/client/dashboard">
                    Dashboard
                  </Link>
                </li>
              </>
            )}
          </ul>

          {/* Usuario y dropdown */}
          <div className="d-flex align-items-center">
            {/* Indicador de conexion SignalR */}
            <span
              className={`badge me-3 ${isConnected ? 'bg-success' : 'bg-secondary'}`}
              title={isConnected ? 'Notificaciones activas' : 'Conectando...'}
              style={{ fontSize: '0.7rem' }}
            >
              {isConnected ? (
                <>
                  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" fill="currentColor" className="me-1" viewBox="0 0 16 16">
                    <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2m.995-14.901a1 1 0 1 0-1.99 0A5 5 0 0 0 3 6c0 1.098-.5 6-2 7h14c-1.5-1-2-5.902-2-7 0-2.42-1.72-4.44-4.005-4.901"/>
                  </svg>
                  {unreadCount > 0 ? unreadCount : 'Live'}
                </>
              ) : (
                'Offline'
              )}
            </span>

            <div className="dropdown">
              <button
                className="btn btn-outline-light btn-sm dropdown-toggle"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
              >
                {user?.name || user?.email}
                <span className="badge bg-secondary ms-2">{user?.role}</span>
              </button>
              <ul className="dropdown-menu dropdown-menu-end">
                <li>
                  <Link className="dropdown-item" to="/profile">
                    Mi Perfil
                  </Link>
                </li>
                <li><hr className="dropdown-divider" /></li>
                <li>
                  <button className="dropdown-item text-danger" onClick={handleLogout}>
                    Cerrar Sesion
                  </button>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </nav>
  )
}

export default Navbar
