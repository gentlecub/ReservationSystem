import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import resourceService from '../services/resourceService'
import reservationService from '../services/reservationService'
import waitlistService from '../services/waitlistService'
import ResourceCard from '../components/ResourceCard'
import ReservationCard from '../components/ReservationCard'

function ClientDashboard() {
  const { user } = useAuth()

  // Estados para tabs
  const [activeTab, setActiveTab] = useState('recursos')

  // Estados para recursos
  const [resources, setResources] = useState([])
  const [loadingResources, setLoadingResources] = useState(true)

  // Estados para reservas
  const [reservations, setReservations] = useState([])
  const [loadingReservations, setLoadingReservations] = useState(true)

  // Estados para lista de espera
  const [waitlist, setWaitlist] = useState([])
  const [loadingWaitlist, setLoadingWaitlist] = useState(true)

  // Estados para el modal de reserva
  const [showModal, setShowModal] = useState(false)
  const [selectedResource, setSelectedResource] = useState(null)
  const [reservationDate, setReservationDate] = useState('')
  const [startTime, setStartTime] = useState('')
  const [endTime, setEndTime] = useState('')
  const [reservationNotes, setReservationNotes] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [modalError, setModalError] = useState('')

  // Estados para opcion de lista de espera
  const [showWaitlistOption, setShowWaitlistOption] = useState(false)
  const [waitlistData, setWaitlistData] = useState(null)

  // Estados para alertas
  const [alert, setAlert] = useState({ show: false, type: '', message: '' })

  // Cargar datos al montar
  useEffect(() => {
    loadResources()
    loadReservations()
    loadWaitlist()
  }, [])

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

  const loadReservations = async () => {
    try {
      setLoadingReservations(true)
      const response = await reservationService.getMyReservations()
      if (response.success) {
        setReservations(response.data || [])
      }
    } catch (error) {
      showAlert('danger', 'Error al cargar reservas')
    } finally {
      setLoadingReservations(false)
    }
  }

  const loadWaitlist = async () => {
    try {
      setLoadingWaitlist(true)
      const response = await waitlistService.getMyWaitlist()
      if (response.success) {
        setWaitlist(response.data || [])
      }
    } catch (error) {
      // No mostrar error si no hay lista de espera
    } finally {
      setLoadingWaitlist(false)
    }
  }

  const showAlert = (type, message) => {
    setAlert({ show: true, type, message })
    setTimeout(() => setAlert({ show: false, type: '', message: '' }), 5000)
  }

  // Abrir modal para reservar
  const handleOpenReservationModal = (resource) => {
    setSelectedResource(resource)
    setReservationDate('')
    setStartTime('')
    setEndTime('')
    setReservationNotes('')
    setModalError('')
    setShowWaitlistOption(false)
    setWaitlistData(null)
    setShowModal(true)
  }

  // Cerrar modal
  const handleCloseModal = () => {
    setShowModal(false)
    setSelectedResource(null)
    setModalError('')
    setShowWaitlistOption(false)
    setWaitlistData(null)
  }

  // Crear reserva
  const handleCreateReservation = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    setShowWaitlistOption(false)

    try {
      const reservationData = {
        resourceId: selectedResource.resourceId,
        date: reservationDate,
        startTime: `${startTime}:00`,
        endTime: `${endTime}:00`
      }

      const response = await reservationService.create(reservationData)

      if (response.success) {
        showAlert('success', 'Reserva creada exitosamente')
        handleCloseModal()
        loadReservations()
        setActiveTab('reservas')
      } else {
        setModalError(response.message || 'Error al crear reserva')
        // Si el error es por conflicto de horario, ofrecer lista de espera
        if (response.message?.toLowerCase().includes('reservado') ||
            response.message?.toLowerCase().includes('horario') ||
            response.message?.toLowerCase().includes('conflicto')) {
          setShowWaitlistOption(true)
          setWaitlistData({
            resourceId: selectedResource.resourceId,
            preferredDate: reservationDate,
            preferredStartTime: `${startTime}:00`,
            preferredEndTime: `${endTime}:00`
          })
        }
      }
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Error al crear reserva'
      setModalError(errorMessage)
      // Si el error es por conflicto, ofrecer lista de espera
      if (errorMessage?.toLowerCase().includes('reservado') ||
          errorMessage?.toLowerCase().includes('horario') ||
          errorMessage?.toLowerCase().includes('conflicto')) {
        setShowWaitlistOption(true)
        setWaitlistData({
          resourceId: selectedResource.resourceId,
          preferredDate: reservationDate,
          preferredStartTime: `${startTime}:00`,
          preferredEndTime: `${endTime}:00`
        })
      }
    } finally {
      setSubmitting(false)
    }
  }

  // Agregar a lista de espera
  const handleAddToWaitlist = async () => {
    if (!waitlistData) return
    setSubmitting(true)

    try {
      const response = await waitlistService.addToWaitlist(waitlistData)
      if (response.success) {
        showAlert('success', response.message || 'Agregado a la lista de espera')
        handleCloseModal()
        loadWaitlist()
        setActiveTab('waitlist')
      } else {
        setModalError(response.message || 'Error al agregar a lista de espera')
      }
    } catch (error) {
      setModalError(error.response?.data?.message || 'Error al agregar a lista de espera')
    } finally {
      setSubmitting(false)
    }
  }

  // Cancelar reserva
  const handleCancelReservation = async (reservationId) => {
    if (!window.confirm('Estas seguro de cancelar esta reserva?')) {
      return
    }

    try {
      const response = await reservationService.cancel(reservationId)
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

  // Cancelar entrada de lista de espera
  const handleCancelWaitlistEntry = async (waitlistId) => {
    if (!window.confirm('Estas seguro de salir de la lista de espera?')) {
      return
    }

    try {
      const response = await waitlistService.cancel(waitlistId)
      if (response.success) {
        showAlert('success', 'Eliminado de la lista de espera')
        loadWaitlist()
      } else {
        showAlert('danger', response.message || 'Error al cancelar')
      }
    } catch (error) {
      showAlert('danger', error.response?.data?.message || 'Error al cancelar')
    }
  }

  // Obtener fecha mínima (hoy)
  const getMinDate = () => {
    return new Date().toISOString().split('T')[0]
  }

  // Formatear fecha
  const formatDate = (dateString) => {
    if (!dateString) return '-'
    return new Date(dateString).toLocaleDateString('es-ES')
  }

  // Obtener color del badge según estado
  const getStatusBadge = (status) => {
    switch (status) {
      case 'Active':
        return 'primary'
      case 'Notified':
        return 'info'
      case 'Fulfilled':
        return 'success'
      case 'Cancelled':
        return 'danger'
      case 'Expired':
        return 'secondary'
      default:
        return 'secondary'
    }
  }

  // Obtener texto del estado
  const getStatusText = (status) => {
    switch (status) {
      case 'Active':
        return 'En espera'
      case 'Notified':
        return 'Notificado'
      case 'Fulfilled':
        return 'Completado'
      case 'Cancelled':
        return 'Cancelado'
      case 'Expired':
        return 'Expirado'
      default:
        return status
    }
  }

  return (
    <div className="container py-4">
      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">Bienvenido, {user?.name || 'Cliente'}</h2>
          <p className="text-muted mb-0">Gestiona tus reservas de recursos</p>
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
            className={`nav-link ${activeTab === 'recursos' ? 'active' : ''}`}
            onClick={() => setActiveTab('recursos')}
          >
            Recursos Disponibles
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'reservas' ? 'active' : ''}`}
            onClick={() => setActiveTab('reservas')}
          >
            Mis Reservas
            {reservations.length > 0 && (
              <span className="badge bg-primary ms-2">{reservations.length}</span>
            )}
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'waitlist' ? 'active' : ''}`}
            onClick={() => setActiveTab('waitlist')}
          >
            Lista de Espera
            {waitlist.filter(w => w.status === 'Active' || w.status === 'Notified').length > 0 && (
              <span className="badge bg-info ms-2">
                {waitlist.filter(w => w.status === 'Active' || w.status === 'Notified').length}
              </span>
            )}
          </button>
        </li>
      </ul>

      {/* Contenido de Tabs */}
      <div className="tab-content">
        {/* Tab Recursos */}
        {activeTab === 'recursos' && (
          <div>
            {loadingResources ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
                <p className="mt-2 text-muted">Cargando recursos...</p>
              </div>
            ) : resources.length === 0 ? (
              <div className="text-center py-5">
                <p className="text-muted">No hay recursos disponibles</p>
              </div>
            ) : (
              <div className="row">
                {resources.map((resource) => (
                  <div key={resource.resourceId} className="col-md-6 col-lg-4 mb-4">
                    <ResourceCard
                      resource={resource}
                      onReserve={() => handleOpenReservationModal(resource)}
                      showReserveButton={true}
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Tab Reservas */}
        {activeTab === 'reservas' && (
          <div>
            {loadingReservations ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
                <p className="mt-2 text-muted">Cargando reservas...</p>
              </div>
            ) : reservations.length === 0 ? (
              <div className="text-center py-5">
                <p className="text-muted">No tienes reservas</p>
                <button
                  className="btn btn-primary"
                  onClick={() => setActiveTab('recursos')}
                >
                  Ver Recursos Disponibles
                </button>
              </div>
            ) : (
              <div className="row">
                {reservations.map((reservation) => (
                  <div key={reservation.reservationId} className="col-md-6 col-lg-4 mb-4">
                    <ReservationCard
                      reservation={reservation}
                      onCancel={() => handleCancelReservation(reservation.reservationId)}
                      showCancelButton={reservation.status === 'Pending'}
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Tab Lista de Espera */}
        {activeTab === 'waitlist' && (
          <div>
            <div className="mb-3">
              <div className="alert alert-info">
                <strong>Lista de Espera:</strong> Cuando un recurso no esta disponible en el horario deseado,
                puedes agregarte a la lista de espera. Te notificaremos cuando haya disponibilidad.
              </div>
            </div>

            {loadingWaitlist ? (
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Cargando...</span>
                </div>
                <p className="mt-2 text-muted">Cargando lista de espera...</p>
              </div>
            ) : waitlist.length === 0 ? (
              <div className="text-center py-5">
                <p className="text-muted">No tienes entradas en lista de espera</p>
                <button
                  className="btn btn-primary"
                  onClick={() => setActiveTab('recursos')}
                >
                  Ver Recursos Disponibles
                </button>
              </div>
            ) : (
              <div className="row">
                {waitlist.map((entry) => (
                  <div key={entry.waitlistId} className="col-md-6 col-lg-4 mb-4">
                    <div className="card h-100">
                      <div className="card-body">
                        <div className="d-flex justify-content-between align-items-start mb-2">
                          <h5 className="card-title mb-0">{entry.resourceName}</h5>
                          <span className={`badge bg-${getStatusBadge(entry.status)}`}>
                            {getStatusText(entry.status)}
                          </span>
                        </div>
                        {entry.resourceLocation && (
                          <p className="text-muted small mb-2">{entry.resourceLocation}</p>
                        )}
                        <hr />
                        <p className="mb-1">
                          <strong>Fecha:</strong> {formatDate(entry.preferredDate)}
                        </p>
                        <p className="mb-1">
                          <strong>Horario:</strong>{' '}
                          {entry.preferredStartTime && entry.preferredEndTime
                            ? `${entry.preferredStartTime} - ${entry.preferredEndTime}`
                            : 'Cualquier horario'}
                        </p>
                        <p className="mb-1">
                          <strong>Posicion en cola:</strong>{' '}
                          <span className="badge bg-dark">{entry.position}</span>
                        </p>
                        {entry.notifiedAt && (
                          <p className="mb-1 text-info">
                            <small>Notificado: {formatDate(entry.notifiedAt)}</small>
                          </p>
                        )}
                        {entry.expiresAt && entry.status === 'Active' && (
                          <p className="mb-0 text-muted">
                            <small>Expira: {formatDate(entry.expiresAt)}</small>
                          </p>
                        )}
                      </div>
                      {(entry.status === 'Active' || entry.status === 'Notified') && (
                        <div className="card-footer bg-transparent">
                          {entry.status === 'Notified' && (
                            <button
                              className="btn btn-success btn-sm me-2"
                              onClick={() => {
                                setSelectedResource({
                                  resourceId: entry.resourceId,
                                  name: entry.resourceName,
                                  location: entry.resourceLocation
                                })
                                setReservationDate(entry.preferredDate)
                                setStartTime(entry.preferredStartTime?.substring(0, 5) || '')
                                setEndTime(entry.preferredEndTime?.substring(0, 5) || '')
                                setShowModal(true)
                              }}
                            >
                              Reservar ahora
                            </button>
                          )}
                          <button
                            className="btn btn-outline-danger btn-sm"
                            onClick={() => handleCancelWaitlistEntry(entry.waitlistId)}
                          >
                            {entry.status === 'Notified' ? 'Rechazar' : 'Cancelar'}
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      {/* Modal de Reserva */}
      {showModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Reservar Recurso</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={handleCloseModal}
                  disabled={submitting}
                ></button>
              </div>
              <form onSubmit={handleCreateReservation}>
                <div className="modal-body">
                  {/* Info del recurso */}
                  <div className="alert alert-info">
                    <strong>{selectedResource?.name}</strong>
                    <br />
                    <small>{selectedResource?.location}</small>
                  </div>

                  {/* Error de reserva */}
                  {modalError && (
                    <div className="alert alert-danger" role="alert">
                      {modalError}
                    </div>
                  )}

                  {/* Opcion de lista de espera */}
                  {showWaitlistOption && (
                    <div className="alert alert-warning">
                      <strong>No hay disponibilidad</strong>
                      <p className="mb-2">El recurso ya esta reservado en ese horario.</p>
                      <button
                        type="button"
                        className="btn btn-warning btn-sm"
                        onClick={handleAddToWaitlist}
                        disabled={submitting}
                      >
                        {submitting ? (
                          <>
                            <span className="spinner-border spinner-border-sm me-2"></span>
                            Agregando...
                          </>
                        ) : (
                          'Agregarme a la lista de espera'
                        )}
                      </button>
                    </div>
                  )}

                  {/* Fecha */}
                  <div className="mb-3">
                    <label htmlFor="reservationDate" className="form-label">
                      Fecha de Reserva
                    </label>
                    <input
                      type="date"
                      className="form-control"
                      id="reservationDate"
                      value={reservationDate}
                      onChange={(e) => {
                        setReservationDate(e.target.value)
                        setShowWaitlistOption(false)
                        setModalError('')
                      }}
                      min={getMinDate()}
                      required
                      disabled={submitting}
                    />
                  </div>

                  {/* Hora inicio */}
                  <div className="mb-3">
                    <label htmlFor="startTime" className="form-label">
                      Hora de Inicio
                    </label>
                    <input
                      type="time"
                      className="form-control"
                      id="startTime"
                      value={startTime}
                      onChange={(e) => {
                        setStartTime(e.target.value)
                        setShowWaitlistOption(false)
                        setModalError('')
                      }}
                      required
                      disabled={submitting}
                    />
                  </div>

                  {/* Hora fin */}
                  <div className="mb-3">
                    <label htmlFor="endTime" className="form-label">
                      Hora de Fin
                    </label>
                    <input
                      type="time"
                      className="form-control"
                      id="endTime"
                      value={endTime}
                      onChange={(e) => {
                        setEndTime(e.target.value)
                        setShowWaitlistOption(false)
                        setModalError('')
                      }}
                      required
                      disabled={submitting}
                    />
                  </div>

                  {/* Notas */}
                  <div className="mb-3">
                    <label htmlFor="notes" className="form-label">
                      Notas (opcional)
                    </label>
                    <textarea
                      className="form-control"
                      id="notes"
                      rows="2"
                      value={reservationNotes}
                      onChange={(e) => setReservationNotes(e.target.value)}
                      placeholder="Informacion adicional..."
                      disabled={submitting}
                    ></textarea>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={handleCloseModal}
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
                        Reservando...
                      </>
                    ) : (
                      'Confirmar Reserva'
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

export default ClientDashboard
