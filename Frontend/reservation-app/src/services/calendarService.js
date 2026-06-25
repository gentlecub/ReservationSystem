import api from './api'

const calendarService = {
  // Obtener conexiones de calendario del usuario
  async getConnections() {
    const response = await api.get('/calendar/connections')
    return response.data
  },

  // Obtener URL de autorizacion de Google Calendar
  async getGoogleAuthUrl() {
    const response = await api.get('/calendar/google/auth')
    return response.data
  },

  // Obtener URL de autorizacion de Microsoft Calendar
  async getMicrosoftAuthUrl() {
    const response = await api.get('/calendar/microsoft/auth')
    return response.data
  },

  // Desconectar calendario
  async disconnect(provider) {
    const response = await api.delete(`/calendar/${provider}`)
    return response.data
  }
}

export default calendarService
