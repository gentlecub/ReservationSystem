import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import resourceService from '../services/resourceService'
import reservationService from '../services/reservationService'
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

  // Estados para el modal de reserva
  const [showModal, setShowModal] = useState(false)
  const [selectedResource, setSelectedResource] = useState(null)
  const [reservationDate, setReservationDate] = useState('')
  const [startTime, setStartTime] = useState('')
  const [endTime, setEndTime] = useState('')
  const [reservationNotes, setReservationNotes] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [modalError, setModalError] = useState('')

  // Estados para alertas
  const [alert, setAlert] = useState({ show: false, type: '', message: '' })

  // Cargar recursos al montar
  useEffect(() => {
    loadResources()
    loadReservations()
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
    setShowModal(true)
  }

  // Cerrar modal
  const handleCloseModal = () => {
    setShowModal(false)
    setSelectedResource(null)
    setModalError('')
  }

  // Crear reserva
  const handleCreateReservation = async (e) => {
    e.preventDefault()
    setSubmitting(true)

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
      }
    } catch (error) {
      setModalError(error.response?.data?.message || 'Error al crear reserva')
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

  // Obtener fecha mínima (hoy)
  const getMinDate = () => {
    return new Date().toISOString().split('T')[0]
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
                      onChange={(e) => setReservationDate(e.target.value)}
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
                      onChange={(e) => setStartTime(e.target.value)}
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
                      onChange={(e) => setEndTime(e.target.value)}
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
