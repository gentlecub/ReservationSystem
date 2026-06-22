import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function PrivateRoute({ children, allowedRoles }) {
  const { user, token } = useAuth()

  // Si no hay token, redirigir a login
  if (!token) {
    return <Navigate to="/login" replace />
  }

  // Si hay roles permitidos y el usuario no tiene el rol correcto
  if (allowedRoles && !allowedRoles.includes(user?.role)) {
    return <Navigate to="/unauthorized" replace />
  }

  // Si todo está bien, renderizar el componente hijo
  return children
}

export default PrivateRoute
