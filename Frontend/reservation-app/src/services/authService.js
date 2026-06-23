import api from './api'

const authService = {
  // Registrar nuevo usuario con email
  async register(fullName, email, password) {
    const response = await api.post('/auth/register', {
      fullName,
      email,
      password
    })
    return response.data
  },

  // Registrar nuevo usuario con telefono
  async registerWithPhone(fullName, phoneNumber, password) {
    const response = await api.post('/auth/register/phone', {
      fullName,
      phoneNumber,
      password
    })
    return response.data
  },

  // Iniciar sesion con email
  async login(email, password) {
    const response = await api.post('/auth/login', {
      email,
      password
    })
    return response.data
  },

  // Autenticacion con Google
  async googleAuth(accessToken) {
    const response = await api.post('/auth/google', {
      accessToken,
      provider: 'Google'
    })
    return response.data
  },

  // Verificar codigo SMS
  async verifyPhone(phoneNumber, code) {
    const response = await api.post('/auth/verify-phone', {
      phoneNumber,
      code
    })
    return response.data
  },

  // Reenviar codigo SMS
  async resendSms(phoneNumber) {
    const response = await api.post('/auth/resend-sms', {
      phoneNumber
    })
    return response.data
  },

  // Verificar email con token
  async verifyEmail(token) {
    const response = await api.post('/auth/verify-email', {
      token
    })
    return response.data
  },

  // Solicitar recuperacion de contrasena
  async forgotPassword(email) {
    const response = await api.post('/auth/forgot-password', {
      email
    })
    return response.data
  },

  // Restablecer contrasena
  async resetPassword(token, newPassword, confirmPassword) {
    const response = await api.post('/auth/reset-password', {
      token,
      newPassword,
      confirmPassword
    })
    return response.data
  }
}

export default authService
