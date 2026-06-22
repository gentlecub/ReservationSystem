import axios from 'axios'

// Crear instancia de Axios con configuración base
const api = axios.create({
  baseURL: 'https://localhost:7000/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// Interceptor de REQUEST: Agregar token automáticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Interceptor de RESPONSE: Manejar errores globalmente
api.interceptors.response.use(
  (response) => {
    // Si la respuesta es exitosa, retornar los datos
    return response
  },
  (error) => {
    // Si hay error 401 (no autorizado), hacer logout
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      // Redirigir a login solo si no estamos ya en login
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default api
