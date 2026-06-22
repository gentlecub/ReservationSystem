import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import resourceService from '../services/resourceService'
import reservationService from '../services/reservationService'
import userService from '../services/userService'

function AdminDashboard() {
  const { user } = useAuth()

  // Estados para tabs
  const [activeTab, setActiveTab] = useState('reservas')

  // Estados para reservas
  const [reservations, setReservations] = useState([])
  const [loadingReservations, setLoadingReservations] = useState(true)

  // Estados para recursos
  const [resources, setResources] = useState([])
  const [loadingResources, setLoadingResources] = useState(true)

  // Estados para usuarios
  const [users, setUsers] = useState([])
  const [loadingUsers, setLoadingUsers] = useState(true)

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
    loadReservations()
    loadResources()
    loadUsers()
  }, [])

  const loadReservations = async () => {
    try {
      setLoadingReservations(true)
      const response = await reservationService.getAll()
      if (response.success) {
        setReservations(response.data || [])
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

  const showAlert = (type, message) => {
    setAlert({ show: true, type, message })
    setTimeout(() => setAlert({ show: false, type: '', message: '' }), 5000)
  }

  // Confirmar reserva
  const handleConfirmReservation = async (id) => {
    try {
      const response = await reservationService.confirm(id)
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
      const response = await reservationService.cancel(id)
      if (response.success) {
        showAlert('success', 'Reserva cancelada')
        loadReservations()
      } else {
        showAlert('danger', response.message || 'Error al cancelar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al cancelar')
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
      default:
        return 'secondary'
    }
  }

  // Formatear fecha
  const formatDate = (dateString) => {
    if (!dateString) return '-'
    return new Date(dateString).toLocaleDateString('es-ES')
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

      {/* Tabs */}
      <ul className="nav nav-tabs mb-4">
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'reservas' ? 'active' : ''}`}
            onClick={() => setActiveTab('reservas')}
          >
            Reservas
            {reservations.length > 0 && (
              <span className="badge bg-primary ms-2">{reservations.length}</span>
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
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">Todas las Reservas</h5>
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
              <div className="alert alert-info">No hay reservas registradas</div>
            ) : (
              <div className="table-responsive">
                <table className="table table-striped table-hover">
                  <thead className="table-dark">
                    <tr>
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
                        <td>{reservation.reservationId}</td>
                        <td>{reservation.resourceName || reservation.resource?.name || '-'}</td>
                        <td>{reservation.userName || reservation.user?.fullName || '-'}</td>
                        <td>{formatDate(reservation.reservationDate)}</td>
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
                                className="btn btn-danger btn-sm"
                                onClick={() => handleCancelReservation(reservation.reservationId)}
                                title="Cancelar"
                              >
                                Cancelar
                              </button>
                            </>
                          )}
                          {reservation.status !== 'Pending' && (
                            <span className="text-muted">-</span>
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
