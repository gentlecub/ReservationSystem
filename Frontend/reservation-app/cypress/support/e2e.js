// Comandos globales de Cypress para SmartBook

// Comando para login
Cypress.Commands.add('login', (email, password) => {
  cy.visit('/login')
  cy.get('#email').type(email)
  cy.get('#password').type(password)
  cy.get('button[type="submit"]').click()
})

// Comando para login como Client
Cypress.Commands.add('loginAsClient', () => {
  cy.login('cliente@test.com', 'Test123!')
  cy.url().should('include', '/client/dashboard')
})

// Comando para login como Admin
Cypress.Commands.add('loginAsAdmin', () => {
  cy.login('admin@smartbook.com', 'Admin123!')
  cy.url().should('include', '/admin/dashboard')
})

// Comando para logout
Cypress.Commands.add('logout', () => {
  cy.contains('button', 'Cerrar Sesion').click()
  cy.url().should('include', '/login')
})

// Comando para limpiar localStorage
Cypress.Commands.add('clearAuth', () => {
  cy.clearLocalStorage('token')
})

// Comando para verificar alerta
Cypress.Commands.add('checkAlert', (type, message) => {
  cy.get(`.alert-${type}`).should('be.visible').and('contain', message)
})

// Ignorar errores de certificado HTTPS en desarrollo
Cypress.on('uncaught:exception', (err, runnable) => {
  // Ignorar errores de red
  if (err.message.includes('NetworkError') || err.message.includes('fetch')) {
    return false
  }
  return true
})
