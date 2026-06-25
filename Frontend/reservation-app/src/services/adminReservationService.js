import api from './api'

const adminReservationService = {
  // Obtener reservas con filtros avanzados
  async getWithFilters(filters = {}) {
    const params = new URLSearchParams()

    if (filters.status) params.append('status', filters.status)
    if (filters.resourceId) params.append('resourceId', filters.resourceId)
    if (filters.userId) params.append('userId', filters.userId)
    if (filters.fromDate) params.append('fromDate', filters.fromDate)
    if (filters.toDate) params.append('toDate', filters.toDate)
    if (filters.activeOnly !== undefined) params.append('activeOnly', filters.activeOnly)
    if (filters.historyOnly !== undefined) params.append('historyOnly', filters.historyOnly)
    if (filters.page) params.append('page', filters.page)
    if (filters.pageSize) params.append('pageSize', filters.pageSize)
    if (filters.sortBy) params.append('sortBy', filters.sortBy)
    if (filters.sortDescending !== undefined) params.append('sortDescending', filters.sortDescending)

    const response = await api.get(`/admin/reservations?${params.toString()}`)
    return response.data
  },

  // Obtener solo reservas activas
  async getActive() {
    const response = await api.get('/admin/reservations/active')
    return response.data
  },

  // Obtener historial de reservas
  async getHistory() {
    const response = await api.get('/admin/reservations/history')
    return response.data
  },

  // Obtener resumen estadistico
  async getSummary() {
    const response = await api.get('/admin/reservations/summary')
    return response.data
  },

  // Obtener detalle de una reserva
  async getById(id) {
    const response = await api.get(`/admin/reservations/${id}`)
    return response.data
  },

  // Modificar una reserva
  async update(id, reservationData) {
    const response = await api.put(`/admin/reservations/${id}`, reservationData)
    return response.data
  },

  // Actualizar estado de una reserva
  async updateStatus(id, status) {
    const response = await api.put(`/admin/reservations/${id}/status`, { status })
    return response.data
  },

  // Cancelar una reserva
  async cancel(id) {
    const response = await api.delete(`/admin/reservations/${id}`)
    return response.data
  },

  // Actualizar estado de multiples reservas
  async bulkUpdateStatus(reservationIds, status) {
    const response = await api.put('/admin/reservations/bulk-status', {
      reservationIds,
      status
    })
    return response.data
  }
}

export default adminReservationService
