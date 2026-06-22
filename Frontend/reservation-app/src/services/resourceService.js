import api from './api'

const resourceService = {
  // Obtener todos los recursos
  async getAll() {
    const response = await api.get('/resources')
    return response.data
  },

  // Obtener un recurso por ID
  async getById(id) {
    const response = await api.get(`/resources/${id}`)
    return response.data
  },

  // Crear nuevo recurso (Admin)
  async create(resourceData) {
    const response = await api.post('/resources', resourceData)
    return response.data
  },

  // Actualizar recurso (Admin)
  async update(id, resourceData) {
    const response = await api.put(`/resources/${id}`, resourceData)
    return response.data
  },

  // Eliminar recurso (Admin)
  async delete(id) {
    const response = await api.delete(`/resources/${id}`)
    return response.data
  }
}

export default resourceService
