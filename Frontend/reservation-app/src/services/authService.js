import api from './api'

const authService = {
  // Registrar nuevo usuario
  async register(fullName, email, password) {
    const response = await api.post('/auth/register', {
      fullName,
      email,
      password
    })
    return response.data
  },

  // Iniciar sesión
  async login(email, password) {
    const response = await api.post('/auth/login', {
      email,
      password
    })
    return response.data
  }
}

export default authService
