import api from './api'

const waitlistService = {
  // Obtener lista de espera (Admin: todas, Client: solo las suyas)
  async getAll() {
    const response = await api.get('/waitlist')
    return response.data
  },

  // Obtener mis entradas en lista de espera
  async getMyWaitlist() {
    const response = await api.get('/waitlist')
    return response.data
  },

  // Obtener lista de espera por recurso (Admin)
  async getByResource(resourceId) {
    const response = await api.get(`/waitlist/resource/${resourceId}`)
    return response.data
  },

  // Obtener una entrada por ID
  async getById(id) {
    const response = await api.get(`/waitlist/${id}`)
    return response.data
  },

  // Obtener posicion en la cola
  async getPosition(id) {
    const response = await api.get(`/waitlist/${id}/position`)
    return response.data
  },

  // Agregar a lista de espera
  async addToWaitlist(waitlistData) {
    const response = await api.post('/waitlist', waitlistData)
    return response.data
  },

  // Cancelar mi entrada en lista de espera
  async cancel(id) {
    const response = await api.put(`/waitlist/${id}/cancel`)
    return response.data
  },

  // Eliminar de lista de espera
  async remove(id) {
    const response = await api.delete(`/waitlist/${id}`)
    return response.data
  },

  // Notificar al siguiente en la cola (Admin)
  async notifyNext(resourceId, date) {
    const response = await api.post(`/waitlist/notify/${resourceId}/${date}`)
    return response.data
  },

  // Expirar entradas antiguas (Admin)
  async expireOldEntries() {
    const response = await api.post('/waitlist/expire')
    return response.data
  }
}

export default waitlistService
