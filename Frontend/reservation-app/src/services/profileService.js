import api from './api'

const profileService = {
  // Obtener perfil del usuario autenticado
  async getProfile() {
    const response = await api.get('/profile')
    return response.data
  },

  // Actualizar perfil
  async updateProfile(data) {
    const response = await api.put('/profile', data)
    return response.data
  },

  // Cambiar contrasena
  async changePassword(currentPassword, newPassword, confirmPassword) {
    const response = await api.post('/profile/change-password', {
      currentPassword,
      newPassword,
      confirmPassword
    })
    return response.data
  },

  // Eliminar cuenta
  async deleteAccount() {
    const response = await api.delete('/profile')
    return response.data
  }
}

export default profileService
