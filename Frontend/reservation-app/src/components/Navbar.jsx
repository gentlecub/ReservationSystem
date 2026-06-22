import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function Navbar() {
  const { user, logout, isAdmin } = useAuth()
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

        {/* Botón hamburguesa para mobile */}
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

        {/* Links de navegación */}
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

          {/* Usuario y Logout */}
          <div className="d-flex align-items-center">
            <span className="navbar-text me-3">
              <i className="bi bi-person-circle me-1"></i>
              {user?.name || user?.email}
              <span className="badge bg-secondary ms-2">{user?.role}</span>
            </span>
            <button
              className="btn btn-outline-light btn-sm"
              onClick={handleLogout}
            >
              Cerrar Sesion
            </button>
          </div>
        </div>
      </div>
    </nav>
  )
}

export default Navbar
