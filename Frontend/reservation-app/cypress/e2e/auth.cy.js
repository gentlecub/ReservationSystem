/// <reference types="cypress" />

describe('Autenticacion', () => {
  beforeEach(() => {
    cy.clearAuth()
  })

  // ==========================================
  // TESTS DE REGISTRO
  // ==========================================
  describe('Registro de Usuario', () => {
    beforeEach(() => {
      cy.visit('/register')
    })

    it('TC-001: Debe mostrar formulario de registro', () => {
      cy.get('#fullName').should('be.visible')
      cy.get('#email').should('be.visible')
      cy.get('#password').should('be.visible')
      cy.get('#confirmPassword').should('be.visible')
      cy.get('button[type="submit"]').should('contain', 'Crear Cuenta')
    })

    it('TC-003: Debe mostrar error con password corta', () => {
      cy.get('#fullName').type('Test User')
      cy.get('#email').type('test@test.com')
      cy.get('#password').type('123')
      cy.get('#confirmPassword').type('123')
      cy.get('button[type="submit"]').click()
      cy.get('.alert-danger').should('contain', 'al menos 6 caracteres')
    })

    it('TC-004: Debe mostrar error si passwords no coinciden', () => {
      cy.get('#fullName').type('Test User')
      cy.get('#email').type('test@test.com')
      cy.get('#password').type('Test123!')
      cy.get('#confirmPassword').type('Test456!')
      cy.get('button[type="submit"]').click()
      cy.get('.alert-danger').should('contain', 'no coinciden')
    })

    it('TC-005: No debe enviar con campos vacios', () => {
      cy.get('button[type="submit"]').click()
      // El formulario no se envia (validacion HTML5)
      cy.url().should('include', '/register')
    })

    it('TC-012: Link a login debe funcionar', () => {
      cy.contains('Inicia sesion').click()
      cy.url().should('include', '/login')
    })
  })

  // ==========================================
  // TESTS DE LOGIN
  // ==========================================
  describe('Login de Usuario', () => {
    beforeEach(() => {
      cy.visit('/login')
    })

    it('TC-007: Debe mostrar formulario de login', () => {
      cy.get('#email').should('be.visible')
      cy.get('#password').should('be.visible')
      cy.get('button[type="submit"]').should('contain', 'Iniciar Sesion')
    })

    it('TC-007: Login exitoso como Client', () => {
      cy.get('#email').type('cliente@test.com')
      cy.get('#password').type('Test123!')
      cy.get('button[type="submit"]').click()

      // Debe redirigir al dashboard de cliente
      cy.url().should('include', '/client/dashboard')

      // Token debe estar en localStorage
      cy.window().then((win) => {
        expect(win.localStorage.getItem('token')).to.not.be.null
      })
    })

    it('TC-008: Login exitoso como Admin', () => {
      cy.get('#email').type('admin@smartbook.com')
      cy.get('#password').type('Admin123!')
      cy.get('button[type="submit"]').click()

      // Debe redirigir al dashboard de admin
      cy.url().should('include', '/admin/dashboard')
    })

    it('TC-009: Login con credenciales incorrectas', () => {
      cy.get('#email').type('cliente@test.com')
      cy.get('#password').type('wrongpassword')
      cy.get('button[type="submit"]').click()

      // Debe mostrar error
      cy.get('.alert-danger').should('be.visible')
      cy.url().should('include', '/login')
    })

    it('TC-011: No debe enviar con campos vacios', () => {
      cy.get('button[type="submit"]').click()
      cy.url().should('include', '/login')
    })

    it('TC-063: Debe mostrar spinner durante login', () => {
      cy.get('#email').type('cliente@test.com')
      cy.get('#password').type('Test123!')
      cy.get('button[type="submit"]').click()

      // El boton debe estar deshabilitado mientras carga
      cy.get('button[type="submit"]').should('be.disabled')
    })
  })

  // ==========================================
  // TESTS DE LOGOUT
  // ==========================================
  describe('Logout', () => {
    it('TC-013: Logout exitoso', () => {
      // Login primero
      cy.loginAsClient()

      // Hacer logout
      cy.logout()

      // Verificar que token fue eliminado
      cy.window().then((win) => {
        expect(win.localStorage.getItem('token')).to.be.null
      })
    })

    it('TC-014: No debe acceder a dashboard despues de logout', () => {
      cy.loginAsClient()
      cy.logout()

      // Intentar acceder al dashboard
      cy.visit('/client/dashboard')
      cy.url().should('include', '/login')
    })
  })

  // ==========================================
  // TESTS DE RUTAS PROTEGIDAS
  // ==========================================
  describe('Rutas Protegidas', () => {
    it('TC-049: Redirige a login si no autenticado (/client)', () => {
      cy.visit('/client/dashboard')
      cy.url().should('include', '/login')
    })

    it('TC-050: Redirige a login si no autenticado (/admin)', () => {
      cy.visit('/admin/dashboard')
      cy.url().should('include', '/login')
    })

    it('TC-051: Client no puede acceder a /admin', () => {
      cy.loginAsClient()
      cy.visit('/admin/dashboard')
      cy.url().should('include', '/unauthorized')
    })

    it('TC-052: Admin no puede acceder a /client', () => {
      cy.loginAsAdmin()
      cy.visit('/client/dashboard')
      cy.url().should('include', '/unauthorized')
    })

    it('TC-053: Pagina unauthorized muestra mensaje', () => {
      cy.loginAsClient()
      cy.visit('/admin/dashboard')
      cy.contains('Acceso Denegado').should('be.visible')
      cy.contains('403').should('be.visible')
    })
  })

  // ==========================================
  // TESTS DE PERSISTENCIA
  // ==========================================
  describe('Persistencia de Sesion', () => {
    it('TC-054: Token se guarda en localStorage', () => {
      cy.loginAsClient()
      cy.window().then((win) => {
        const token = win.localStorage.getItem('token')
        expect(token).to.not.be.null
        expect(token).to.include('eyJ') // JWT siempre empieza con eyJ
      })
    })

    it('TC-079: Sesion persiste al refrescar pagina', () => {
      cy.loginAsClient()
      cy.reload()
      cy.url().should('include', '/client/dashboard')
    })
  })
})
