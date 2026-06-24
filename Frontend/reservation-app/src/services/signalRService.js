import * as signalR from '@microsoft/signalr';

/**
 * Servicio para manejar la conexion WebSocket con SignalR.
 * Permite recibir notificaciones en tiempo real del backend.
 */
class SignalRService {
    constructor() {
        this.connection = null;
        this.callbacks = [];
        this.connectionPromise = null;
    }

    /**
     * Conecta al hub de notificaciones del backend.
     * @param {string} token - JWT del usuario autenticado
     */
    async connect(token) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        // Construir URL del hub (quitar /api de la URL base)
        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
        const hubUrl = apiUrl.replace('/api', '') + '/hubs/notifications';

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Escuchar notificaciones del servidor
        this.connection.on('ReceiveNotification', (notification) => {
            console.log('Notificacion recibida:', notification);
            this.callbacks.forEach(cb => cb(notification));
        });

        // Eventos de conexion
        this.connection.onreconnecting((error) => {
            console.log('SignalR: Reconectando...', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('SignalR: Reconectado con ID:', connectionId);
        });

        this.connection.onclose((error) => {
            console.log('SignalR: Conexion cerrada', error);
        });

        try {
            await this.connection.start();
            console.log('SignalR: Conectado exitosamente');
        } catch (error) {
            console.error('SignalR: Error al conectar:', error);
            throw error;
        }
    }

    /**
     * Desconecta del hub de notificaciones.
     */
    async disconnect() {
        if (this.connection) {
            try {
                await this.connection.stop();
                console.log('SignalR: Desconectado');
            } catch (error) {
                console.error('SignalR: Error al desconectar:', error);
            }
            this.connection = null;
        }
    }

    /**
     * Suscribirse a notificaciones.
     * @param {Function} callback - Funcion que se ejecuta cuando llega una notificacion
     * @returns {Function} - Funcion para cancelar la suscripcion
     */
    onNotification(callback) {
        this.callbacks.push(callback);

        // Retornar funcion para cancelar suscripcion
        return () => {
            this.callbacks = this.callbacks.filter(cb => cb !== callback);
        };
    }

    /**
     * Verifica si esta conectado al hub.
     * @returns {boolean}
     */
    isConnected() {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }

    /**
     * Obtiene el estado actual de la conexion.
     * @returns {string}
     */
    getState() {
        if (!this.connection) return 'Disconnected';
        return this.connection.state;
    }
}

// Exportar instancia singleton
export const signalRService = new SignalRService();
