import { createContext, useContext, useState, useEffect } from 'react'
import { jwtDecode } from 'jwt-decode'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(localStorage.getItem('token'))
  const [loading, setLoading] = useState(true)

  // Al cargar la app, verificar si hay token guardado
  useEffect(() => {
    if (token) {
      try {
        const decoded = jwtDecode(token)
        // Verificar si el token no ha expirado
        if (decoded.exp * 1000 > Date.now()) {
          setUser({
            userId: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
            email: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
            name: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
            role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          })
        } else {
          // Token expirado
          localStorage.removeItem('token')
          setToken(null)
        }
      } catch (error) {
        console.error('Error al decodificar token:', error)
        localStorage.removeItem('token')
        setToken(null)
      }
    }
    setLoading(false)
  }, [token])

  // Función para hacer login
  const login = (authResponse) => {
    const { token: newToken, userId, fullName, email, role } = authResponse
    localStorage.setItem('token', newToken)
    setToken(newToken)
    setUser({
      userId,
      email,
      name: fullName,
      role
    })
  }

  // Función para hacer logout
  const logout = () => {
    localStorage.removeItem('token')
    setToken(null)
    setUser(null)
  }

  // Verificar si es Admin
  const isAdmin = () => {
    return user?.role === 'Admin'
  }

  // Verificar si es Client
  const isClient = () => {
    return user?.role === 'Client'
  }

  // Actualizar datos del usuario
  const updateUser = (updatedData) => {
    setUser(prev => ({ ...prev, ...updatedData }))
  }

  const value = {
    user,
    token,
    loading,
    login,
    logout,
    isAdmin,
    isClient,
    updateUser
  }

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  )
}

// Hook personalizado para usar el contexto
export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth debe usarse dentro de un AuthProvider')
  }
  return context
}
