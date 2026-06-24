import { useState } from 'react'

// Iconos SVG simples para cada tipo de notificacion
const ICONS = {
  ReservationCreated: (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
      <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5M1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4z"/>
      <path d="M8 5.5a.5.5 0 0 1 .5.5v1.5H10a.5.5 0 0 1 0 1H8.5V10a.5.5 0 0 1-1 0V8.5H6a.5.5 0 0 1 0-1h1.5V6a.5.5 0 0 1 .5-.5"/>
    </svg>
  ),
  ReservationConfirmed: (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
      <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0m-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
    </svg>
  ),
  ReservationCancelled: (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
      <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0M5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293z"/>
    </svg>
  ),
  ReservationModified: (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
      <path d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"/>
      <path fillRule="evenodd" d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5z"/>
    </svg>
  ),
  ReservationReminder: (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
      <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2m.995-14.901a1 1 0 1 0-1.99 0A5 5 0 0 0 3 6c0 1.098-.5 6-2 7h14c-1.5-1-2-5.902-2-7 0-2.42-1.72-4.44-4.005-4.901"/>
    </svg>
  )
}

// Colores Bootstrap para cada tipo
const COLORS = {
  ReservationCreated: 'primary',
  ReservationConfirmed: 'success',
  ReservationCancelled: 'danger',
  ReservationModified: 'warning',
  ReservationReminder: 'info'
}

// Formatear hora
function formatTime(date) {
  return new Date(date).toLocaleTimeString('es', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

export default function ToastNotification({ notification, onClose }) {
  const [isExiting, setIsExiting] = useState(false)

  const handleClose = () => {
    setIsExiting(true)
    setTimeout(() => onClose(notification.id), 300)
  }

  const icon = ICONS[notification.type] || ICONS.ReservationReminder
  const color = COLORS[notification.type] || 'secondary'

  return (
    <div
      className={`toast show mb-2`}
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
      style={{
        minWidth: '320px',
        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
        animation: isExiting ? 'slideOut 0.3s ease-out forwards' : 'slideIn 0.3s ease-out',
        borderLeft: `4px solid var(--bs-${color})`
      }}
    >
      <div className={`toast-header bg-${color} bg-opacity-10`}>
        <span className={`text-${color} me-2`}>{icon}</span>
        <strong className={`me-auto text-${color}`}>{notification.title}</strong>
        <small className="text-muted">{formatTime(notification.timestamp)}</small>
        <button
          type="button"
          className="btn-close"
          aria-label="Cerrar"
          onClick={handleClose}
        />
      </div>
      <div className="toast-body">
        <p className="mb-1">{notification.message}</p>

        {/* Mostrar detalles del recurso si existen */}
        {notification.details && (
          <div className="small text-muted mt-2 pt-2 border-top">
            <div className="d-flex align-items-center">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" className="me-1" viewBox="0 0 16 16">
                <path d="M8 16s6-5.686 6-10A6 6 0 0 0 2 6c0 4.314 6 10 6 10m0-7a3 3 0 1 1 0-6 3 3 0 0 1 0 6"/>
              </svg>
              <span>
                {notification.details.ResourceName}
                {notification.details.ResourceLocation && ` - ${notification.details.ResourceLocation}`}
              </span>
            </div>
            {notification.details.Date && (
              <div className="d-flex align-items-center mt-1">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" className="me-1" viewBox="0 0 16 16">
                  <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5M1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4z"/>
                </svg>
                <span>
                  {notification.details.Date} {notification.details.StartTime && `${notification.details.StartTime} - ${notification.details.EndTime}`}
                </span>
              </div>
            )}
          </div>
        )}

        {/* Mostrar cambios si es modificacion */}
        {notification.changes && (
          <div className="small text-warning mt-2 pt-2 border-top">
            <strong>Cambios:</strong> {notification.changes}
          </div>
        )}
      </div>

      {/* Estilos de animacion inline */}
      <style>{`
        @keyframes slideIn {
          from {
            opacity: 0;
            transform: translateX(100%);
          }
          to {
            opacity: 1;
            transform: translateX(0);
          }
        }
        @keyframes slideOut {
          from {
            opacity: 1;
            transform: translateX(0);
          }
          to {
            opacity: 0;
            transform: translateX(100%);
          }
        }
      `}</style>
    </div>
  )
}
