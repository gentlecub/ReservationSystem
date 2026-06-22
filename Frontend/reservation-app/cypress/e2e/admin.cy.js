/// <reference types="cypress" />

describe('Flujo Admin', () => {
  beforeEach(() => {
    cy.clearAuth()
    cy.loginAsAdmin()
  })

  // ==========================================
  // TESTS DE DASHBOARD ADMIN
  // ==========================================
  describe('Dashboard Admin', () => {
    it('TC-030: Debe mostrar 3 tabs', () => {
      cy.contains('Reservas').should('be.visible')
      cy.contains('Recursos').should('be.visible')
      cy.contains('Usuarios').should('be.visible')
    })

    it('TC-030: Debe mostrar titulo Panel de Administracion', () => {
      cy.contains('Panel de Administracion').should('be.visible')
    })
  })

  // ==========================================
  // TESTS DE GESTION DE RESERVAS
  // ==========================================
  describe('Gestion de Reservas', () => {
    beforeEach(() => {
      cy.contains('button', 'Reservas').click()
    })

    it('TC-031: Muestra tabla de todas las reservas', () => {
      cy.get('table').should('be.visible')
      cy.get('thead').contains('ID').should('be.visible')
      cy.get('thead').contains('Recurso').should('be.visible')
      cy.get('thead').contains('Usuario').should('be.visible')
      cy.get('thead').contains('Estado').should('be.visible')
    })

    it('TC-037: Boton Actualizar recarga lista', () => {
      cy.contains('button', 'Actualizar').click()
      // Debe mostrar spinner brevemente
      cy.get('.spinner-border').should('exist')
    })

    it('TC-034: Boton Confirmar visible en reservas Pending', () => {
      cy.get('tbody tr').each(($row) => {
        if ($row.find('.badge.bg-warning').length) {
          cy.wrap($row).contains('button', 'Confirmar').should('be.visible')
        }
      })
    })

    it('TC-035: Boton Cancelar visible en reservas Pending', () => {
      cy.get('tbody tr').each(($row) => {
        if ($row.find('.badge.bg-warning').length) {
          cy.wrap($row).contains('button', 'Cancelar').should('be.visible')
        }
      })
    })

    it('TC-036: Sin botones en reservas Confirmed', () => {
      cy.get('tbody tr').each(($row) => {
        if ($row.find('.badge.bg-success').length) {
          cy.wrap($row).find('td').last().should('contain', '-')
        }
      })
    })

    it('TC-034: Confirmar reserva cambia estado', () => {
      // Buscar una reserva pendiente
      cy.get('tbody tr').then(($rows) => {
        const pendingRow = $rows.filter((i, row) =>
          Cypress.$(row).find('.badge.bg-warning').length > 0
        )
        if (pendingRow.length) {
          cy.wrap(pendingRow.first()).contains('button', 'Confirmar').click()
          cy.get('.alert-success').should('contain', 'confirmada')
        }
      })
    })
  })

  // ==========================================
  // TESTS DE CRUD RECURSOS
  // ==========================================
  describe('Gestion de Recursos', () => {
    beforeEach(() => {
      cy.contains('button', 'Recursos').click()
    })

    it('TC-032: Muestra tabla de recursos', () => {
      cy.get('table').should('be.visible')
      cy.get('thead').contains('Nombre').should('be.visible')
      cy.get('thead').contains('Ubicacion').should('be.visible')
      cy.get('thead').contains('Capacidad').should('be.visible')
    })

    it('TC-032: Cada recurso tiene botones Editar/Eliminar', () => {
      cy.get('tbody tr').first().within(() => {
        cy.contains('button', 'Editar').should('be.visible')
        cy.contains('button', 'Eliminar').should('be.visible')
      })
    })

    it('TC-038: Click en Nuevo Recurso abre modal', () => {
      cy.contains('button', 'Nuevo Recurso').click()
      cy.get('.modal').should('be.visible')
      cy.contains('Nuevo Recurso').should('be.visible')
    })

    it('TC-038: Crear recurso exitoso', () => {
      cy.contains('button', 'Nuevo Recurso').click()

      const randomName = `Sala Test ${Date.now()}`
      cy.get('#resourceName').type(randomName)
      cy.get('#resourceDescription').type('Sala de prueba automatizada')
      cy.get('#resourceLocation').type('Edificio Test, Piso 1')
      cy.get('#resourceCapacity').clear().type('20')

      cy.contains('button', 'Guardar').click()

      cy.get('.alert-success').should('contain', 'creado')
      cy.get('.modal').should('not.exist')
      // Verificar que aparece en la tabla
      cy.contains(randomName).should('be.visible')
    })

    it('TC-039: No permite crear sin nombre', () => {
      cy.contains('button', 'Nuevo Recurso').click()
      cy.get('#resourceDescription').type('Sin nombre')
      cy.contains('button', 'Guardar').click()
      // Modal debe seguir abierto (validacion HTML5)
      cy.get('.modal').should('be.visible')
    })

    it('TC-040: Editar recurso', () => {
      cy.get('tbody tr').first().contains('button', 'Editar').click()
      cy.get('.modal').should('be.visible')
      cy.contains('Editar Recurso').should('be.visible')

      // Modificar descripcion
      cy.get('#resourceDescription').clear().type('Descripcion modificada ' + Date.now())
      cy.contains('button', 'Guardar').click()

      cy.get('.alert-success').should('contain', 'actualizado')
    })

    it('TC-043: Desactivar recurso', () => {
      cy.get('tbody tr').first().contains('button', 'Editar').click()
      cy.get('#resourceIsActive').uncheck()
      cy.contains('button', 'Guardar').click()

      cy.get('.alert-success').should('be.visible')
    })

    it('TC-044: Cerrar modal sin guardar', () => {
      cy.contains('button', 'Nuevo Recurso').click()
      cy.get('#resourceName').type('No guardar')
      cy.get('.modal').find('.btn-secondary').click()
      cy.get('.modal').should('not.exist')
    })
  })

  // ==========================================
  // TESTS DE VISTA USUARIOS
  // ==========================================
  describe('Vista de Usuarios', () => {
    beforeEach(() => {
      cy.contains('button', 'Usuarios').click()
    })

    it('TC-045: Muestra tabla de usuarios', () => {
      cy.get('table').should('be.visible')
      cy.get('thead').contains('ID').should('be.visible')
      cy.get('thead').contains('Nombre').should('be.visible')
      cy.get('thead').contains('Email').should('be.visible')
      cy.get('thead').contains('Rol').should('be.visible')
    })

    it('TC-046: Boton Actualizar funciona', () => {
      cy.contains('button', 'Actualizar').click()
      cy.get('.spinner-border').should('exist')
    })

    it('TC-047: Badge Admin es rojo', () => {
      cy.get('.badge.bg-danger').should('contain', 'Admin')
    })

    it('TC-048: Badge Client es azul', () => {
      cy.get('body').then(($body) => {
        if ($body.find('.badge.bg-primary:contains("Client")').length) {
          cy.get('.badge.bg-primary').should('contain', 'Client')
        }
      })
    })
  })

  // ==========================================
  // TESTS DE UI/UX
  // ==========================================
  describe('UI/UX Admin', () => {
    it('TC-062: Tablas tienen scroll horizontal en mobile', () => {
      cy.viewport(375, 667)
      cy.contains('button', 'Reservas').click()
      cy.get('.table-responsive').should('be.visible')
    })

    it('TC-070: Alertas desaparecen automaticamente', () => {
      cy.contains('button', 'Recursos').click()
      cy.contains('button', 'Nuevo Recurso').click()

      const randomName = `AutoHide Test ${Date.now()}`
      cy.get('#resourceName').type(randomName)
      cy.contains('button', 'Guardar').click()

      cy.get('.alert-success').should('be.visible')
      // Esperar 6 segundos para que desaparezca
      cy.wait(6000)
      cy.get('.alert-success').should('not.exist')
    })

    it('TC-069: Alerta se puede cerrar manualmente', () => {
      cy.contains('button', 'Recursos').click()
      cy.contains('button', 'Nuevo Recurso').click()

      const randomName = `CloseTest ${Date.now()}`
      cy.get('#resourceName').type(randomName)
      cy.contains('button', 'Guardar').click()

      cy.get('.alert-success').should('be.visible')
      cy.get('.alert-success .btn-close').click()
      cy.get('.alert-success').should('not.exist')
    })
  })
})
