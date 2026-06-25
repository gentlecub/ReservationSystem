import { createContext, useContext, useState, useEffect, useCallback } from 'react'
import { useAuth } from './AuthContext'
import { signalRService } from '../services/signalRService'

const NotificationContext = createContext(null)

export function NotificationProvider({ children }) {
  const { token, user } = useAuth()
  const [notifications, setNotifications] = useState([])
  const [isConnected, setIsConnected] = useState(false)

  // Conectar a SignalR cuando hay token y usuario
  useEffect(() => {
    if (token && user) {
      signalRService.connect(token)
        .then(() => setIsConnected(true))
        .catch(err => {
          console.error('Error conectando a SignalR:', err)
          setIsConnected(false)
        })

      // Cleanup al desmontar o cuando cambia token/user
      return () => {
        signalRService.disconnect()
        setIsConnected(false)
      }
    } else {
      // Sin token, asegurar que esta desconectado
      signalRService.disconnect()
      setIsConnected(false)
    }
  }, [token, user])

  // Escuchar notificaciones de SignalR
  useEffect(() => {
    const unsubscribe = signalRService.onNotification((notification) => {
      const newNotification = {
        id: Date.now() + Math.random(), // ID unico
        type: notification.Type,
        title: notification.Payload?.Title || 'Notificacion',
        message: notification.Payload?.Message || '',
        details: notification.Payload?.Details || null,
        changes: notification.Payload?.Changes || null,
        cancelledBy: notification.Payload?.CancelledBy || null,
        timestamp: notification.Timestamp ? new Date(notification.Timestamp) : new Date(),
        read: false
      }

      setNotifications(prev => [newNotification, ...prev])

      // Auto-remover despues de 8 segundos
      setTimeout(() => {
        removeNotification(newNotification.id)
      }, 8000)
    })

    return unsubscribe
  }, [])

  // Remover una notificacion por ID
  const removeNotification = useCallback((id) => {
    setNotifications(prev => prev.filter(n => n.id !== id))
  }, [])

  // Marcar notificacion como leida
  const markAsRead = useCallback((id) => {
    setNotifications(prev =>
      prev.map(n => n.id === id ? { ...n, read: true } : n)
    )
  }, [])

  // Limpiar todas las notificaciones
  const clearAll = useCallback(() => {
    setNotifications([])
  }, [])

  // Agregar notificacion manualmente (para testing o notificaciones locales)
  const addNotification = useCallback((type, title, message, details = null) => {
    const newNotification = {
      id: Date.now() + Math.random(),
      type,
      title,
      message,
      details,
      timestamp: new Date(),
      read: false
    }

    setNotifications(prev => [newNotification, ...prev])

    // Auto-remover despues de 8 segundos
    setTimeout(() => {
      removeNotification(newNotification.id)
    }, 8000)

    return newNotification.id
  }, [removeNotification])

  const value = {
    notifications,
    isConnected,
    removeNotification,
    markAsRead,
    clearAll,
    addNotification,
    unreadCount: notifications.filter(n => !n.read).length
  }

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  )
}

// Hook personalizado para usar el contexto
export function useNotifications() {
  const context = useContext(NotificationContext)
  if (!context) {
    throw new Error('useNotifications debe usarse dentro de NotificationProvider')
  }
  return context
}
