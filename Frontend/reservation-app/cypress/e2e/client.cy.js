/// <reference types="cypress" />

describe('Flujo Client', () => {
  beforeEach(() => {
    cy.clearAuth()
    cy.loginAsClient()
  })

  // ==========================================
  // TESTS DE DASHBOARD
  // ==========================================
  describe('Dashboard Cliente', () => {
    it('TC-015: Debe mostrar tabs Recursos y Mis Reservas', () => {
      cy.contains('Recursos Disponibles').should('be.visible')
      cy.contains('Mis Reservas').should('be.visible')
    })

    it('TC-015: Debe mostrar mensaje de bienvenida', () => {
      cy.contains('Bienvenido').should('be.visible')
    })

    it('TC-016: Tab Recursos muestra cards de recursos', () => {
      cy.contains('Recursos Disponibles').click()
      // Esperar que carguen los recursos
      cy.get('.card').should('have.length.at.least', 1)
    })

    it('TC-064: Debe mostrar spinner mientras carga recursos', () => {
      cy.visit('/client/dashboard')
      // El spinner debe aparecer brevemente
      cy.get('.spinner-border').should('exist')
    })
  })

  // ==========================================
  // TESTS DE RECURSOS
  // ==========================================
  describe('Ver Recursos', () => {
    it('TC-016: Cards de recursos muestran informacion', () => {
      cy.contains('Recursos Disponibles').click()
      cy.get('.card').first().within(() => {
        cy.get('.card-title').should('be.visible')
        cy.contains('button', 'Reservar').should('be.visible')
      })
    })

    it('TC-019: Click en Reservar abre modal', () => {
      cy.contains('Recursos Disponibles').click()
      cy.get('.card').first().find('button').contains('Reservar').click()
      cy.get('.modal').should('be.visible')
      cy.contains('Reservar Recurso').should('be.visible')
    })
  })

  // ==========================================
  // TESTS DE CREAR RESERVA
  // ==========================================
  describe('Crear Reserva', () => {
    beforeEach(() => {
      cy.contains('Recursos Disponibles').click()
      cy.get('.card').first().find('button').contains('Reservar').click()
    })

    it('TC-019: Modal muestra info del recurso', () => {
      cy.get('.modal').should('be.visible')
      cy.get('.alert-info').should('be.visible') // Info del recurso
    })

    it('TC-019: Modal tiene campos de fecha y hora', () => {
      cy.get('#reservationDate').should('be.visible')
      cy.get('#startTime').should('be.visible')
      cy.get('#endTime').should('be.visible')
    })

    it('TC-020: Crear reserva exitosa', () => {
      // Fecha de manana
      const tomorrow = new Date()
      tomorrow.setDate(tomorrow.getDate() + 1)
      const dateStr = tomorrow.toISOString().split('T')[0]

      cy.get('#reservationDate').type(dateStr)
      cy.get('#startTime').type('09:00')
      cy.get('#endTime').type('10:00')
      cy.contains('button', 'Confirmar Reserva').click()

      // Verificar exito
      cy.get('.alert-success').should('be.visible')
      // Modal debe cerrarse
      cy.get('.modal').should('not.exist')
    })

    it('TC-021: Crear reserva con notas', () => {
      const tomorrow = new Date()
      tomorrow.setDate(tomorrow.getDate() + 1)
      const dateStr = tomorrow.toISOString().split('T')[0]

      cy.get('#reservationDate').type(dateStr)
      cy.get('#startTime').type('11:00')
      cy.get('#endTime').type('12:00')
      cy.get('#notes').type('Reunion de prueba')
      cy.contains('button', 'Confirmar Reserva').click()

      cy.get('.alert-success').should('be.visible')
    })

    it('TC-022: No permite fecha pasada', () => {
      // El input type="date" con min no permite seleccionar fechas pasadas
      cy.get('#reservationDate').should('have.attr', 'min')
    })

    it('TC-023: No envia sin campos requeridos', () => {
      cy.contains('button', 'Confirmar Reserva').click()
      // El modal debe seguir abierto (validacion HTML5)
      cy.get('.modal').should('be.visible')
    })

    it('TC-024: Cerrar modal sin guardar', () => {
      cy.get('.modal').find('.btn-close').click()
      cy.get('.modal').should('not.exist')
    })

    it('TC-066: Boton muestra spinner al guardar', () => {
      const tomorrow = new Date()
      tomorrow.setDate(tomorrow.getDate() + 1)
      const dateStr = tomorrow.toISOString().split('T')[0]

      cy.get('#reservationDate').type(dateStr)
      cy.get('#startTime').type('14:00')
      cy.get('#endTime').type('15:00')
      cy.contains('button', 'Confirmar Reserva').click()

      // El boton debe deshabilitarse
      cy.contains('button', 'Reservando').should('be.disabled')
    })
  })

  // ==========================================
  // TESTS DE MIS RESERVAS
  // ==========================================
  describe('Mis Reservas', () => {
    it('TC-018: Tab Mis Reservas muestra reservas', () => {
      cy.contains('Mis Reservas').click()
      // Puede tener reservas o mensaje de vacio
      cy.get('.tab-content').should('be.visible')
    })

    it('TC-071: Badge Pending es amarillo', () => {
      cy.contains('Mis Reservas').click()
      // Si hay reservas pendientes
      cy.get('body').then(($body) => {
        if ($body.find('.badge.bg-warning').length) {
          cy.get('.badge.bg-warning').should('contain', 'Pendiente')
        }
      })
    })

    it('TC-072: Badge Confirmed es verde', () => {
      cy.contains('Mis Reservas').click()
      cy.get('body').then(($body) => {
        if ($body.find('.badge.bg-success').length) {
          cy.get('.badge.bg-success').should('contain', 'Confirmada')
        }
      })
    })
  })

  // ==========================================
  // TESTS DE CANCELAR RESERVA
  // ==========================================
  describe('Cancelar Reserva', () => {
    it('TC-026: Boton cancelar visible en reservas Pending', () => {
      cy.contains('Mis Reservas').click()
      cy.get('body').then(($body) => {
        if ($body.find('.badge.bg-warning').length) {
          // Si hay reserva pendiente, debe haber boton cancelar
          cy.contains('button', 'Cancelar').should('be.visible')
        }
      })
    })

    it('TC-028: Boton cancelar NO visible en Confirmed', () => {
      cy.contains('Mis Reservas').click()
      // Las cards con badge success no deben tener boton cancelar
      cy.get('.card').each(($card) => {
        if ($card.find('.badge.bg-success').length) {
          cy.wrap($card).find('button').contains('Cancelar').should('not.exist')
        }
      })
    })
  })

  // ==========================================
  // TESTS DE UI RESPONSIVE
  // ==========================================
  describe('Responsive Design', () => {
    it('TC-061: Cards en columna en mobile', () => {
      cy.viewport(375, 667) // iPhone
      cy.contains('Recursos Disponibles').click()
      cy.get('.card').first().should('be.visible')
    })

    it('TC-059: Navbar muestra hamburguesa en mobile', () => {
      cy.viewport(375, 667)
      cy.get('.navbar-toggler').should('be.visible')
    })

    it('TC-060: Menu hamburguesa funciona', () => {
      cy.viewport(375, 667)
      cy.get('.navbar-toggler').click()
      cy.get('.navbar-collapse').should('have.class', 'show')
    })
  })
})
