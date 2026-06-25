import { useNotifications } from '../context/NotificationContext'
import ToastNotification from './ToastNotification'

/**
 * Contenedor de notificaciones toast.
 * Se posiciona en la esquina superior derecha de la pantalla.
 * Muestra todas las notificaciones activas.
 */
export default function ToastContainer() {
  const { notifications, removeNotification } = useNotifications()

  // No renderizar nada si no hay notificaciones
  if (notifications.length === 0) {
    return null
  }

  return (
    <div
      className="toast-container position-fixed top-0 end-0 p-3"
      style={{ zIndex: 1100 }}
      aria-live="polite"
      aria-label="Notificaciones"
    >
      {notifications.map(notification => (
        <ToastNotification
          key={notification.id}
          notification={notification}
          onClose={removeNotification}
        />
      ))}
    </div>
  )
}
