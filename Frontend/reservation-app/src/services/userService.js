import api from './api'

const userService = {
  // Obtener todos los usuarios (Admin)
  async getAll() {
    const response = await api.get('/users')
    return response.data
  },

  // Obtener un usuario por ID (Admin)
  async getById(id) {
    const response = await api.get(`/users/${id}`)
    return response.data
  }
}

export default userService
