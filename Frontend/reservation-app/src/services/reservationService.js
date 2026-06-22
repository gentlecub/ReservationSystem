import api from './api'

const reservationService = {
  // Obtener reservas (Admin: todas, Client: solo las suyas)
  async getAll() {
    const response = await api.get('/reservations')
    return response.data
  },

  // Alias para Client - usa el mismo endpoint
  async getMyReservations() {
    const response = await api.get('/reservations')
    return response.data
  },

  // Obtener una reserva por ID
  async getById(id) {
    const response = await api.get(`/reservations/${id}`)
    return response.data
  },

  // Crear nueva reserva
  async create(reservationData) {
    const response = await api.post('/reservations', reservationData)
    return response.data
  },

  // Actualizar estado de reserva (Admin)
  async updateStatus(id, status) {
    const response = await api.put(`/reservations/${id}`, { status })
    return response.data
  },

  // Confirmar reserva (Admin) - cambia estado a Confirmed
  async confirm(id) {
    const response = await api.put(`/reservations/${id}`, { status: 'Confirmed' })
    return response.data
  },

  // Cancelar reserva - usa DELETE
  async cancel(id) {
    const response = await api.delete(`/reservations/${id}`)
    return response.data
  },

  // Eliminar reserva
  async delete(id) {
    const response = await api.delete(`/reservations/${id}`)
    return response.data
  }
}

export default reservationService
