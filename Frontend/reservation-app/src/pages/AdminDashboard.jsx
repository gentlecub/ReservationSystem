import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import resourceService from '../services/resourceService'
import adminReservationService from '../services/adminReservationService'
import waitlistService from '../services/waitlistService'
import userService from '../services/userService'

function AdminDashboard() {
  const { user } = useAuth()

  // Estados para tabs
  const [activeTab, setActiveTab] = useState('reservas')

  // Estados para reservas
  const [reservations, setReservations] = useState([])
  const [reservationSummary, setReservationSummary] = useState(null)
  const [loadingReservations, setLoadingReservations] = useState(true)
  const [reservationView, setReservationView] = useState('active') // 'active', 'history', 'all'
  const [selectedReservations, setSelectedReservations] = useState([])

  // Estados para filtros de reservas
  const [filters, setFilters] = useState({
    status: '',
    resourceId: '',
    fromDate: '',
    toDate: '',
    page: 1,
    pageSize: 20
  })
  const [pagination, setPagination] = useState({ totalPages: 1, totalCount: 0 })

  // Estados para editar reserva
  const [showEditReservationModal, setShowEditReservationModal] = useState(false)
  const [editingReservation, setEditingReservation] = useState(null)
  const [reservationForm, setReservationForm] = useState({
    resourceId: '',
    date: '',
    startTime: '',
    endTime: ''
  })

  // Estados para recursos
  const [resources, setResources] = useState([])
  const [loadingResources, setLoadingResources] = useState(true)

  // Estados para usuarios
  const [users, setUsers] = useState([])
  const [loadingUsers, setLoadingUsers] = useState(true)

  // Estados para lista de espera
  const [waitlist, setWaitlist] = useState([])
  const [loadingWaitlist, setLoadingWaitlist] = useState(true)

  // Estados para modal de recurso
  const [showResourceModal, setShowResourceModal] = useState(false)
  const [editingResource, setEditingResource] = useState(null)
  const [resourceForm, setResourceForm] = useState({
    name: '',
    description: '',
    location: '',
    capacity: 1,
    isActive: true
  })
  const [submitting, setSubmitting] = useState(false)

  // Estados para alertas
  const [alert, setAlert] = useState({ show: false, type: '', message: '' })

  // Cargar datos al montar
  useEffect(() => {
    loadResources()
    loadUsers()
    loadWaitlist()
  }, [])

  // Cargar reservas cuando cambia la vista o filtros
  useEffect(() => {
    loadReservations()
  }, [reservationView, filters.page])

  const loadReservations = async () => {
    try {
      setLoadingReservations(true)
      let response

      if (reservationView === 'active') {
        response = await adminReservationService.getActive()
        if (response.success) {
          setReservations(response.data || [])
        }
      } else if (reservationView === 'history') {
        response = await adminReservationService.getHistory()
        if (response.success) {
          setReservations(response.data || [])
        }
      } else {
        response = await adminReservationService.getWithFilters({
          ...filters,
          status: filters.status || undefined,
          resourceId: filters.resourceId || undefined
        })
        if (response.success) {
          setReservations(response.data?.reservations || [])
          setPagination({
            totalPages: response.data?.totalPages || 1,
            totalCount: response.data?.totalCount || 0
          })
          setReservationSummary(response.data?.summary || null)
        }
      }

      // Cargar resumen
      const summaryResponse = await adminReservationService.getSummary()
      if (summaryResponse.success) {
        setReservationSummary(summaryResponse.data)
      }
    } catch (error) {
      showAlert('danger', 'Error al cargar reservas')
    } finally {
      setLoadingReservations(false)
    }
  }

  const loadResources = async () => {
    try {
      setLoadingResources(true)
      const response = await resourceService.getAll()
      if (response.success) {
        setResources(response.data || [])
      }
    } catch (error) {
      showAlert('danger', 'Error al cargar recursos')
    } finally {
      setLoadingResources(false)
    }
  }

  const loadUsers = async () => {
    try {
      setLoadingUsers(true)
      const response = await userService.getAll()
      if (response.success) {
        setUsers(response.data || [])
      }
    } catch (error) {
      showAlert('danger', 'Error al cargar usuarios')
    } finally {
      setLoadingUsers(false)
    }
  }

  const loadWaitlist = async () => {
    try {
      setLoadingWaitlist(true)
      const response = await waitlistService.getAll()
      if (response.success) {
        setWaitlist(response.data || [])
      }
    } catch (error) {
      showAlert('danger', 'Error al cargar lista de espera')
    } finally {
      setLoadingWaitlist(false)
    }
  }

  const showAlert = (type, message) => {
    setAlert({ show: true, type, message })
    setTimeout(() => setAlert({ show: false, type: '', message: '' }), 5000)
  }

  // Confirmar reserva
  const handleConfirmReservation = async (id) => {
    try {
      const response = await adminReservationService.updateStatus(id, 'Confirmed')
      if (response.success) {
        showAlert('success', 'Reserva confirmada')
        loadReservations()
      } else {
        showAlert('danger', response.message || 'Error al confirmar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al confirmar')
    }
  }

  // Cancelar reserva
  const handleCancelReservation = async (id) => {
    if (!window.confirm('Estas seguro de cancelar esta reserva?')) return
    try {
      const response = await adminReservationService.cancel(id)
      if (response.success) {
        showAlert('success', 'Reserva cancelada')
        loadReservations()
        loadWaitlist() // Recargar lista de espera por si hay cambios
      } else {
        showAlert('danger', response.message || 'Error al cancelar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al cancelar')
    }
  }

  // Abrir modal para editar reserva
  const handleOpenEditReservation = (reservation) => {
    setEditingReservation(reservation)
    setReservationForm({
      resourceId: reservation.resourceId,
      date: reservation.date || reservation.reservationDate?.split('T')[0],
      startTime: reservation.startTime,
      endTime: reservation.endTime
    })
    setShowEditReservationModal(true)
  }

  // Cerrar modal de editar reserva
  const handleCloseEditReservationModal = () => {
    setShowEditReservationModal(false)
    setEditingReservation(null)
  }

  // Guardar cambios de reserva
  const handleSaveReservation = async (e) => {
    e.preventDefault()
    setSubmitting(true)

    try {
      const response = await adminReservationService.update(editingReservation.reservationId, reservationForm)
      if (response.success) {
        showAlert('success', 'Reserva actualizada')
        handleCloseEditReservationModal()
        loadReservations()
      } else {
        showAlert('danger', response.message || 'Error al actualizar reserva')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al actualizar reserva')
    } finally {
      setSubmitting(false)
    }
  }

  // Acciones masivas
  const handleBulkAction = async (action) => {
    if (selectedReservations.length === 0) {
      showAlert('warning', 'Selecciona al menos una reserva')
      return
    }

    const statusMap = {
      confirm: 'Confirmed',
      cancel: 'Cancelled'
    }

    if (!window.confirm(`Estas seguro de ${action === 'confirm' ? 'confirmar' : 'cancelar'} ${selectedReservations.length} reservas?`)) return

    try {
      const response = await adminReservationService.bulkUpdateStatus(selectedReservations, statusMap[action])
      if (response.success) {
        showAlert('success', `${response.data} reservas actualizadas`)
        setSelectedReservations([])
        loadReservations()
      } else {
        showAlert('danger', response.message || 'Error en accion masiva')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error en accion masiva')
    }
  }

  // Toggle seleccion de reserva
  const toggleReservationSelection = (id) => {
    setSelectedReservations(prev =>
      prev.includes(id) ? prev.filter(r => r !== id) : [...prev, id]
    )
  }

  // Seleccionar todas las reservas
  const toggleSelectAll = () => {
    if (selectedReservations.length === reservations.length) {
      setSelectedReservations([])
    } else {
      setSelectedReservations(reservations.map(r => r.reservationId))
    }
  }

  // Eliminar entrada de lista de espera
  const handleRemoveFromWaitlist = async (id) => {
    if (!window.confirm('Estas seguro de eliminar esta entrada de la lista de espera?')) return
    try {
      const response = await waitlistService.remove(id)
      if (response.success) {
        showAlert('success', 'Entrada eliminada de lista de espera')
        loadWaitlist()
      } else {
        showAlert('danger', response.message || 'Error al eliminar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al eliminar')
    }
  }

  // Notificar al siguiente en cola
  const handleNotifyWaitlist = async (resourceId, date) => {
    try {
      const response = await waitlistService.notifyNext(resourceId, date)
      if (response.success) {
        showAlert('success', response.message || 'Usuario notificado')
        loadWaitlist()
      } else {
        showAlert('danger', response.message || 'Error al notificar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al notificar')
    }
  }

  // Abrir modal para crear recurso
  const handleOpenCreateResource = () => {
    setEditingResource(null)
    setResourceForm({
      name: '',
      description: '',
      location: '',
      capacity: 1,
      isActive: true
    })
    setShowResourceModal(true)
  }

  // Abrir modal para editar recurso
  const handleOpenEditResource = (resource) => {
    setEditingResource(resource)
    setResourceForm({
      name: resource.name,
      description: resource.description || '',
      location: resource.location || '',
      capacity: resource.capacity || 1,
      isActive: resource.isActive !== false
    })
    setShowResourceModal(true)
  }

  // Cerrar modal de recurso
  const handleCloseResourceModal = () => {
    setShowResourceModal(false)
    setEditingResource(null)
  }

  // Guardar recurso (crear o editar)
  const handleSaveResource = async (e) => {
    e.preventDefault()
    setSubmitting(true)

    try {
      let response
      if (editingResource) {
        response = await resourceService.update(editingResource.resourceId, resourceForm)
      } else {
        response = await resourceService.create(resourceForm)
      }

      if (response.success) {
        showAlert('success', editingResource ? 'Recurso actualizado' : 'Recurso creado')
        handleCloseResourceModal()
        loadResources()
      } else {
        showAlert('danger', response.message || 'Error al guardar recurso')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al guardar recurso')
    } finally {
      setSubmitting(false)
    }
  }

  // Eliminar recurso
  const handleDeleteResource = async (id) => {
    if (!window.confirm('Estas seguro de eliminar este recurso?')) return
    try {
      const response = await resourceService.delete(id)
      if (response.success) {
        showAlert('success', 'Recurso eliminado')
        loadResources()
      } else {
        showAlert('danger', response.message || 'Error al eliminar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al eliminar')
    }
  }

  // Obtener color del badge según estado
  const getStatusBadge = (status) => {
    switch (status) {
      case 'Confirmed':
        return 'success'
      case 'Pending':
        return 'warning'
      case 'Cancelled':
        return 'danger'
      case 'Active':
        return 'primary'
      case 'Notified':
        return 'info'
      case 'Expired':
        return 'secondary'
      default:
        return 'secondary'
    }
  }

  // Formatear fecha
  const formatDate = (dateString) => {
    if (!dateString) return '-'
    return new Date(dateString).toLocaleDateString('es-ES')
  }

  // Aplicar filtros
  const handleApplyFilters = () => {
    setFilters({ ...filters, page: 1 })
    loadReservations()
  }

  // Limpiar filtros
  const handleClearFilters = () => {
    setFilters({
      status: '',
      resourceId: '',
      fromDate: '',
      toDate: '',
      page: 1,
      pageSize: 20
    })
  }

  return (
    <div className="container py-4">
      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">Panel de Administracion</h2>
          <p className="text-muted mb-0">Bienvenido, {user?.name || 'Admin'}</p>
        </div>
      </div>

      {/* Alerta */}
      {alert.show && (
        <div className={`alert alert-${alert.type} alert-dismissible fade show`} role="alert">
          {alert.message}
          <button
            type="button"
            className="btn-close"
            onClick={() => setAlert({ show: false, type: '', message: '' })}
          ></button>
        </div>
      )}

      {/* Resumen de reservas */}
      {reservationSummary && (
        <div className="row mb-4">
          <div className="col-md-3 col-6 mb-2">
            <div className="card bg-warning bg-opacity-10 border-warning">
              <div className="card-body py-2">
                <h6 className="card-title text-warning mb-0">Pendientes</h6>
                <h3 className="mb-0">{reservationSummary.totalPending}</h3>
              </div>
            </div>
          </div>
          <div className="col-md-3 col-6 mb-2">
            <div className="card bg-success bg-opacity-10 border-success">
              <div className="card-body py-2">
                <h6 className="card-title text-success mb-0">Confirmadas</h6>
                <h3 className="mb-0">{reservationSummary.totalConfirmed}</h3>
              </div>
            </div>
          </div>
          <div className="col-md-3 col-6 mb-2">
            <div className="card bg-primary bg-opacity-10 border-primary">
              <div className="card-body py-2">
                <h6 className="card-title text-primary mb-0">Activas</h6>
                <h3 className="mb-0">{reservationSummary.totalActive}</h3>
              </div>
            </div>
          </div>
          <div className="col-md-3 col-6 mb-2">
            <div className="card bg-secondary bg-opacity-10 border-secondary">
              <div className="card-body py-2">
                <h6 className="card-title text-secondary mb-0">Historial</h6>
                <h3 className="mb-0">{reservationSummary.totalHistory}</h3>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <ul className="nav nav-tabs mb-4">
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'reservas' ? 'active' : ''}`}
            onClick={() => setActiveTab('reservas')}
          >
            Reservas
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'waitlist' ? 'active' : ''}`}
            onClick={() => setActiveTab('waitlist')}
          >
            Lista de Espera
            {waitlist.filter(w => w.status === 'Active').length > 0 && (
              <span className="badge bg-info ms-2">{waitlist.filter(w => w.status === 'Active').length}</span>
            )}
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'recursos' ? 'active' : ''}`}
            onClick={() => setActiveTab('recursos')}
          >
            Recursos
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'usuarios' ? 'active' : ''}`}
            onClick={() => setActiveTab('usuarios')}
          >
            Usuarios
          </button>
        </li>
      </ul>

      {/* Contenido de Tabs */}
      <div className="tab-content">
        {/* Tab Reservas */}
        {activeTab === 'reservas' && (
          <div>
            {/* Sub-tabs para vista de reservas */}
            <div className="btn-group mb-3">
              <button
                className={`btn btn-sm ${reservationView === 'active' ? 'btn-primary' : 'btn-outline-primary'}`}
                onClick={() => setReservationView('active')}
              >
                Activas
              </button>
              <button
                className={`btn btn-sm ${reservationView === 'history' ? 'btn-primary' : 'btn-outline-primary'}`}
                onClick={() => setReservationView('history')}
              >
                Historial
              </button>
              <button
                className={`btn btn-sm ${reservationView === 'all' ? 'btn-primary' : 'btn-outline-primary'}`}
                onClick={() => setReservationView('all')}
              >
                Todas
              </button>
            </div>

            {/* Filtros (solo en vista "Todas") */}
            {reservationView === 'all' && (
              <div className="card mb-3">
                <div className="card-body">
                  <div className="row g-2">
                    <div className="col-md-2">
                      <select
                        className="form-select form-select-sm"
                        value={filters.status}
                        onChange={(e) => setFilters({ ...filters, status: e.target.value })}
                      >
                        <option value="">Todos los estados</option>
                        <option value="Pending">Pendiente</option>
                        <option value="Confirmed">Confirmada</option>
                        <option value="Cancelled">Cancelada</option>
                      </select>
                    </div>
                    <div className="col-md-2">
                      <select
                        className="form-select form-select-sm"
                        value={filters.resourceId}
                        onChange={(e) => setFilters({ ...filters, resourceId: e.target.value })}
                      >
                        <option value="">Todos los recursos</option>
                        {resources.map(r => (
                          <option key={r.resourceId} value={r.resourceId}>{r.name}</option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-2">
                      <input
                        type="date"
                        className="form-control form-control-sm"
                        placeholder="Desde"
                        value={filters.fromDate}
                        onChange={(e) => setFilters({ ...filters, fromDate: e.target.value })}
                      />
                    </div>
                    <div className="col-md-2">
                      <input
                        type="date"
                        className="form-control form-control-sm"
                        placeholder="Hasta"
                        value={filters.toDate}
                        onChange={(e) => setFilters({ ...filters, toDate: e.target.value })}
                      />
                    </div>
                    <div className="col-md-4">
                      <button className="btn btn-sm btn-primary me-1" onClick={handleApplyFilters}>
                        Filtrar
                      </button>
                      <button className="btn btn-sm btn-outline-secondary" onClick={handleClearFilters}>
                        Limpiar
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Acciones masivas */}
            {selectedReservations.length > 0 && (
              <div className="alert alert-info d-flex justify-content-between align-items-center">
                <span>{selectedReservations.length} reservas seleccionadas</span>
                <div>
                  <button className="btn btn-sm btn-success me-1" onClick={() => handleBulkAction('confirm')}>
                    Confirmar todas
                  </button>
                  <button className="btn btn-sm btn-danger" onClick={() => handleBulkAction('cancel')}>
                    Cancelar todas
                  </button>
                </div>
              </div>
            )}

            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">
                {reservationView === 'active' && 'Reservas Activas'}
                {reservationView === 'history' && 'Historial de Reservas'}
                {reservationView === 'all' && `Todas las Reservas (${pagination.totalCount})`}
              </h5>
              <button className="btn btn-outline-primary btn-sm" onClick={loadReservations}>
                Actualizar
              </button>
            </div>

            {loadingReservations ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
              </div>
            ) : reservations.length === 0 ? (
              <div className="alert alert-info">No hay reservas en esta vista</div>
            ) : (
              <>
                <div className="table-responsive">
                  <table className="table table-striped table-hover">
                    <thead className="table-dark">
                      <tr>
                        <th>
                          <input
                            type="checkbox"
                            checked={selectedReservations.length === reservations.length && reservations.length > 0}
                            onChange={toggleSelectAll}
                          />
                        </th>
                        <th>ID</th>
                        <th>Recurso</th>
                        <th>Usuario</th>
                        <th>Fecha</th>
                        <th>Horario</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                      </tr>
                    </thead>
                    <tbody>
                      {reservations.map((reservation) => (
                        <tr key={reservation.reservationId}>
                          <td>
                            <input
                              type="checkbox"
                              checked={selectedReservations.includes(reservation.reservationId)}
                              onChange={() => toggleReservationSelection(reservation.reservationId)}
                            />
                          </td>
                          <td>{reservation.reservationId}</td>
                          <td>{reservation.resourceName || '-'}</td>
                          <td>
                            <div>{reservation.userName || '-'}</div>
                            <small className="text-muted">{reservation.userEmail}</small>
                          </td>
                          <td>{formatDate(reservation.date || reservation.reservationDate)}</td>
                          <td>{reservation.startTime} - {reservation.endTime}</td>
                          <td>
                            <span className={`badge bg-${getStatusBadge(reservation.status)}`}>
                              {reservation.status}
                            </span>
                          </td>
                          <td>
                            {reservation.status === 'Pending' && (
                              <>
                                <button
                                  className="btn btn-success btn-sm me-1"
                                  onClick={() => handleConfirmReservation(reservation.reservationId)}
                                  title="Confirmar"
                                >
                                  Confirmar
                                </button>
                                <button
                                  className="btn btn-outline-primary btn-sm me-1"
                                  onClick={() => handleOpenEditReservation(reservation)}
                                  title="Editar"
                                >
                                  Editar
                                </button>
                                <button
                                  className="btn btn-danger btn-sm"
                                  onClick={() => handleCancelReservation(reservation.reservationId)}
                                  title="Cancelar"
                                >
                                  Cancelar
                                </button>
                              </>
                            )}
                            {reservation.status === 'Confirmed' && (
                              <>
                                <button
                                  className="btn btn-outline-primary btn-sm me-1"
                                  onClick={() => handleOpenEditReservation(reservation)}
                                  title="Editar"
                                >
                                  Editar
                                </button>
                                <button
                                  className="btn btn-danger btn-sm"
                                  onClick={() => handleCancelReservation(reservation.reservationId)}
                                  title="Cancelar"
                                >
                                  Cancelar
                                </button>
                              </>
                            )}
                            {reservation.status === 'Cancelled' && (
                              <span className="text-muted">-</span>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* Paginacion (solo en vista "Todas") */}
                {reservationView === 'all' && pagination.totalPages > 1 && (
                  <nav className="mt-3">
                    <ul className="pagination justify-content-center">
                      <li className={`page-item ${filters.page === 1 ? 'disabled' : ''}`}>
                        <button className="page-link" onClick={() => setFilters({ ...filters, page: filters.page - 1 })}>
                          Anterior
                        </button>
                      </li>
                      {[...Array(pagination.totalPages)].map((_, i) => (
                        <li key={i} className={`page-item ${filters.page === i + 1 ? 'active' : ''}`}>
                          <button className="page-link" onClick={() => setFilters({ ...filters, page: i + 1 })}>
                            {i + 1}
                          </button>
                        </li>
                      ))}
                      <li className={`page-item ${filters.page === pagination.totalPages ? 'disabled' : ''}`}>
                        <button className="page-link" onClick={() => setFilters({ ...filters, page: filters.page + 1 })}>
                          Siguiente
                        </button>
                      </li>
                    </ul>
                  </nav>
                )}
              </>
            )}
          </div>
        )}

        {/* Tab Lista de Espera */}
        {activeTab === 'waitlist' && (
          <div>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">Lista de Espera</h5>
              <button className="btn btn-outline-primary btn-sm" onClick={loadWaitlist}>
                Actualizar
              </button>
            </div>

            {loadingWaitlist ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
              </div>
            ) : waitlist.length === 0 ? (
              <div className="alert alert-info">No hay entradas en lista de espera</div>
            ) : (
              <div className="table-responsive">
                <table className="table table-striped table-hover">
                  <thead className="table-dark">
                    <tr>
                      <th>Pos.</th>
                      <th>Recurso</th>
                      <th>Usuario</th>
                      <th>Fecha Preferida</th>
                      <th>Horario Preferido</th>
                      <th>Estado</th>
                      <th>Creado</th>
                      <th>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {waitlist.map((entry) => (
                      <tr key={entry.waitlistId}>
                        <td>
                          <span className="badge bg-dark">{entry.position}</span>
                        </td>
                        <td>{entry.resourceName || '-'}</td>
                        <td>
                          <div>{entry.userName || '-'}</div>
                          <small className="text-muted">{entry.userEmail}</small>
                        </td>
                        <td>{formatDate(entry.preferredDate)}</td>
                        <td>
                          {entry.preferredStartTime && entry.preferredEndTime
                            ? `${entry.preferredStartTime} - ${entry.preferredEndTime}`
                            : 'Cualquier horario'}
                        </td>
                        <td>
                          <span className={`badge bg-${getStatusBadge(entry.status)}`}>
                            {entry.status}
                          </span>
                        </td>
                        <td>{formatDate(entry.createdAt)}</td>
                        <td>
                          {entry.status === 'Active' && (
                            <>
                              <button
                                className="btn btn-info btn-sm me-1"
                                onClick={() => handleNotifyWaitlist(entry.resourceId, entry.preferredDate)}
                                title="Notificar disponibilidad"
                              >
                                Notificar
                              </button>
                              <button
                                className="btn btn-danger btn-sm"
                                onClick={() => handleRemoveFromWaitlist(entry.waitlistId)}
                                title="Eliminar"
                              >
                                Eliminar
                              </button>
                            </>
                          )}
                          {entry.status !== 'Active' && (
                            <button
                              className="btn btn-outline-danger btn-sm"
                              onClick={() => handleRemoveFromWaitlist(entry.waitlistId)}
                              title="Eliminar"
                            >
                              Eliminar
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* Tab Recursos */}
        {activeTab === 'recursos' && (
          <div>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">Gestion de Recursos</h5>
              <button className="btn btn-primary" onClick={handleOpenCreateResource}>
                + Nuevo Recurso
              </button>
            </div>

            {loadingResources ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
              </div>
            ) : resources.length === 0 ? (
              <div className="alert alert-info">No hay recursos registrados</div>
            ) : (
              <div className="table-responsive">
                <table className="table table-striped table-hover">
                  <thead className="table-dark">
                    <tr>
                      <th>ID</th>
                      <th>Nombre</th>
                      <th>Descripcion</th>
                      <th>Ubicacion</th>
                      <th>Capacidad</th>
                      <th>Estado</th>
                      <th>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {resources.map((resource) => (
                      <tr key={resource.resourceId}>
                        <td>{resource.resourceId}</td>
                        <td>{resource.name}</td>
                        <td>{resource.description || '-'}</td>
                        <td>{resource.location || '-'}</td>
                        <td>{resource.capacity || '-'}</td>
                        <td>
                          <span className={`badge bg-${resource.isActive !== false ? 'success' : 'secondary'}`}>
                            {resource.isActive !== false ? 'Activo' : 'Inactivo'}
                          </span>
                        </td>
                        <td>
                          <button
                            className="btn btn-outline-primary btn-sm me-1"
                            onClick={() => handleOpenEditResource(resource)}
                            title="Editar"
                          >
                            Editar
                          </button>
                          <button
                            className="btn btn-outline-danger btn-sm"
                            onClick={() => handleDeleteResource(resource.resourceId)}
                            title="Eliminar"
                          >
                            Eliminar
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* Tab Usuarios */}
        {activeTab === 'usuarios' && (
          <div>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">Usuarios Registrados</h5>
              <button className="btn btn-outline-primary btn-sm" onClick={loadUsers}>
                Actualizar
              </button>
            </div>

            {loadingUsers ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
              </div>
            ) : users.length === 0 ? (
              <div className="alert alert-info">No hay usuarios registrados</div>
            ) : (
              <div className="table-responsive">
                <table className="table table-striped table-hover">
                  <thead className="table-dark">
                    <tr>
                      <th>ID</th>
                      <th>Nombre</th>
                      <th>Email</th>
                      <th>Rol</th>
                      <th>Fecha Registro</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((u) => (
                      <tr key={u.userId}>
                        <td>{u.userId}</td>
                        <td>{u.fullName}</td>
                        <td>{u.email}</td>
                        <td>
                          <span className={`badge bg-${u.role === 'Admin' ? 'danger' : 'primary'}`}>
                            {u.role}
                          </span>
                        </td>
                        <td>{formatDate(u.createdAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Modal de Editar Reserva */}
      {showEditReservationModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Editar Reserva #{editingReservation?.reservationId}</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={handleCloseEditReservationModal}
                  disabled={submitting}
                ></button>
              </div>
              <form onSubmit={handleSaveReservation}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label htmlFor="editResourceId" className="form-label">Recurso *</label>
                    <select
                      className="form-select"
                      id="editResourceId"
                      value={reservationForm.resourceId}
                      onChange={(e) => setReservationForm({ ...reservationForm, resourceId: parseInt(e.target.value) })}
                      required
                      disabled={submitting}
                    >
                      <option value="">Seleccionar recurso</option>
                      {resources.filter(r => r.isActive !== false).map(r => (
                        <option key={r.resourceId} value={r.resourceId}>{r.name}</option>
                      ))}
                    </select>
                  </div>

                  <div className="mb-3">
                    <label htmlFor="editDate" className="form-label">Fecha *</label>
                    <input
                      type="date"
                      className="form-control"
                      id="editDate"
                      value={reservationForm.date}
                      onChange={(e) => setReservationForm({ ...reservationForm, date: e.target.value })}
                      min={new Date().toISOString().split('T')[0]}
                      required
                      disabled={submitting}
                    />
                  </div>

                  <div className="row">
                    <div className="col-6">
                      <div className="mb-3">
                        <label htmlFor="editStartTime" className="form-label">Hora Inicio *</label>
                        <input
                          type="time"
                          className="form-control"
                          id="editStartTime"
                          value={reservationForm.startTime}
                          onChange={(e) => setReservationForm({ ...reservationForm, startTime: e.target.value })}
                          required
                          disabled={submitting}
                        />
                      </div>
                    </div>
                    <div className="col-6">
                      <div className="mb-3">
                        <label htmlFor="editEndTime" className="form-label">Hora Fin *</label>
                        <input
                          type="time"
                          className="form-control"
                          id="editEndTime"
                          value={reservationForm.endTime}
                          onChange={(e) => setReservationForm({ ...reservationForm, endTime: e.target.value })}
                          required
                          disabled={submitting}
                        />
                      </div>
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={handleCloseEditReservationModal}
                    disabled={submitting}
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={submitting}
                  >
                    {submitting ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2"></span>
                        Guardando...
                      </>
                    ) : (
                      'Guardar Cambios'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Modal de Recurso */}
      {showResourceModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingResource ? 'Editar Recurso' : 'Nuevo Recurso'}
                </h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={handleCloseResourceModal}
                  disabled={submitting}
                ></button>
              </div>
              <form onSubmit={handleSaveResource}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label htmlFor="resourceName" className="form-label">
                      Nombre *
                    </label>
                    <input
                      type="text"
                      className="form-control"
                      id="resourceName"
                      value={resourceForm.name}
                      onChange={(e) => setResourceForm({ ...resourceForm, name: e.target.value })}
                      required
                      disabled={submitting}
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="resourceDescription" className="form-label">
                      Descripcion
                    </label>
                    <textarea
                      className="form-control"
                      id="resourceDescription"
                      rows="2"
                      value={resourceForm.description}
                      onChange={(e) => setResourceForm({ ...resourceForm, description: e.target.value })}
                      disabled={submitting}
                    ></textarea>
                  </div>

                  <div className="mb-3">
                    <label htmlFor="resourceLocation" className="form-label">
                      Ubicacion
                    </label>
                    <input
                      type="text"
                      className="form-control"
                      id="resourceLocation"
                      value={resourceForm.location}
                      onChange={(e) => setResourceForm({ ...resourceForm, location: e.target.value })}
                      disabled={submitting}
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="resourceCapacity" className="form-label">
                      Capacidad
                    </label>
                    <input
                      type="number"
                      className="form-control"
                      id="resourceCapacity"
                      min="1"
                      value={resourceForm.capacity}
                      onChange={(e) => setResourceForm({ ...resourceForm, capacity: parseInt(e.target.value) || 1 })}
                      disabled={submitting}
                    />
                  </div>

                  <div className="form-check">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      id="resourceIsActive"
                      checked={resourceForm.isActive}
                      onChange={(e) => setResourceForm({ ...resourceForm, isActive: e.target.checked })}
                      disabled={submitting}
                    />
                    <label className="form-check-label" htmlFor="resourceIsActive">
                      Recurso activo
                    </label>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={handleCloseResourceModal}
                    disabled={submitting}
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={submitting}
                  >
                    {submitting ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2"></span>
                        Guardando...
                      </>
                    ) : (
                      'Guardar'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default AdminDashboard
