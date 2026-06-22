# Guia de Despliegue: Backend en Railway + Frontend en Vercel

Esta guia te explica paso a paso como desplegar tu aplicacion en produccion.

---

## Tabla de Contenidos

1. [Conceptos Basicos](#1-conceptos-basicos)
2. [Preparar el Backend para Produccion](#2-preparar-el-backend-para-produccion)
3. [Desplegar Backend en Railway](#3-desplegar-backend-en-railway)
4. [Preparar el Frontend para Produccion](#4-preparar-el-frontend-para-produccion)
5. [Desplegar Frontend en Vercel](#5-desplegar-frontend-en-vercel)
6. [Conectar Frontend con Backend](#6-conectar-frontend-con-backend)
7. [Troubleshooting (Problemas Comunes)](#7-troubleshooting-problemas-comunes)

---

## 1. Conceptos Basicos

### Que es Railway?
Railway es una plataforma que te permite desplegar aplicaciones backend (como tu API de .NET) en la nube. Es como tener un servidor en internet que ejecuta tu codigo 24/7.

### Que es Vercel?
Vercel es una plataforma especializada en desplegar aplicaciones frontend (como tu app de React). Es muy rapida y facil de usar.

### Que es CORS?
CORS (Cross-Origin Resource Sharing) es una medida de seguridad del navegador. Cuando tu frontend (en vercel.app) intenta comunicarse con tu backend (en railway.app), el navegador bloquea la peticion por seguridad. Debes configurar CORS para permitir esta comunicacion.

### Que son las Variables de Entorno?
Son valores de configuracion que cambian segun el ambiente (desarrollo vs produccion). Por ejemplo:
- En desarrollo: `baseURL = https://localhost:7000`
- En produccion: `baseURL = https://tu-api.railway.app`

---

## 2. Preparar el Backend para Produccion

### Paso 2.1: Modificar Program.cs para CORS Dinamico

Actualmente tu CORS solo permite localhost. Necesitas cambiarlo para que acepte la URL de tu frontend en Vercel.

**Abre `Program.cs` y reemplaza la seccion de CORS:**

```csharp
// ============================================
// CONFIGURACION DE CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // Obtener origenes permitidos desde variables de entorno
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Paso 2.2: Modificar Program.cs para usar el puerto de Railway

Railway asigna un puerto dinamico. Agrega esto antes de `builder.Build()`:

```csharp
// Configurar puerto para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
```

### Paso 2.3: Habilitar Swagger en Produccion (Opcional)

Si quieres ver Swagger en produccion para probar tu API, cambia esto:

```csharp
// Swagger en todos los ambientes (quitar el if para produccion)
app.UseSwagger();
app.UseSwaggerUI();
```

### Paso 2.4: Crear archivo appsettings.Production.json

Crea un nuevo archivo `appsettings.Production.json` en la misma carpeta que `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AllowedOrigins": [
    "https://tu-app.vercel.app",
    "https://tu-dominio-personalizado.com"
  ]
}
```

**IMPORTANTE:** Las variables sensibles (ConnectionStrings, JwtSettings) se configuraran en Railway, NO en este archivo.

### Paso 2.5: Crear Dockerfile

Crea un archivo llamado `Dockerfile` (sin extension) en la carpeta `Backend/SmartBookAPI/`:

```dockerfile
# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY ["SmartBookAPI.csproj", "./"]
RUN dotnet restore

# Copiar todo el codigo y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar la aplicacion compilada
COPY --from=build /app/publish .

# Exponer puerto (Railway usa la variable PORT)
EXPOSE 8080

# Comando para ejecutar la aplicacion
ENTRYPOINT ["dotnet", "SmartBookAPI.dll"]
```

---

## 3. Desplegar Backend en Railway

### Paso 3.1: Crear cuenta en Railway

1. Ve a [railway.app](https://railway.app)
2. Haz clic en "Login" o "Start a New Project"
3. Inicia sesion con tu cuenta de GitHub

### Paso 3.2: Crear nuevo proyecto

1. Haz clic en **"New Project"**
2. Selecciona **"Deploy from GitHub repo"**
3. Busca y selecciona tu repositorio `ReservationSystem`
4. Railway detectara automaticamente que es un proyecto .NET

### Paso 3.3: Configurar el Root Directory

Como tu backend esta en una subcarpeta:

1. Ve a **Settings** de tu servicio
2. En **"Root Directory"** escribe: `Backend/SmartBookAPI`
3. Guarda los cambios

### Paso 3.4: Agregar Base de Datos PostgreSQL

Railway no soporta SQL Server gratis, asi que usaremos PostgreSQL:

1. En tu proyecto, haz clic en **"+ New"**
2. Selecciona **"Database"** > **"Add PostgreSQL"**
3. Railway creara la base de datos automaticamente

**NOTA:** Necesitaras modificar tu proyecto para usar PostgreSQL. Agrega el paquete NuGet:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Y cambia en `Program.cs`:

```csharp
// Cambiar de SQL Server a PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Paso 3.5: Configurar Variables de Entorno

En Railway, ve a tu servicio backend y haz clic en **"Variables"**. Agrega las siguientes:

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `${{Postgres.DATABASE_URL}}` (Railway lo llena automaticamente) |
| `JwtSettings__SecretKey` | `TuClaveSecretaSuperSegura_DebeSerLarga32Chars!` |
| `JwtSettings__Issuer` | `SmartBookAPI` |
| `JwtSettings__Audience` | `SmartBookClients` |
| `JwtSettings__ExpirationMinutes` | `60` |
| `AllowedOrigins__0` | `https://tu-app.vercel.app` |

**Explicacion de la sintaxis:**
- `ConnectionStrings__DefaultConnection` = En .NET, `__` representa `:` en JSON
- Esto equivale a `"ConnectionStrings": { "DefaultConnection": "..." }`

### Paso 3.6: Desplegar

1. Haz clic en **"Deploy"** o espera el deploy automatico
2. Railway te dara una URL como: `https://smartbookapi-production.up.railway.app`
3. Guarda esta URL, la necesitaras para el frontend

### Paso 3.7: Probar el Backend

Abre tu navegador y ve a:
- `https://tu-url-railway.app/swagger` - Para ver la documentacion
- `https://tu-url-railway.app/api/resources` - Para probar un endpoint

---

## 4. Preparar el Frontend para Produccion

### Paso 4.1: Configurar Variables de Entorno en el Frontend

Crea un archivo `.env.production` en la carpeta `Frontend/reservation-app/`:

```env
VITE_API_URL=https://tu-url-railway.app/api
```

### Paso 4.2: Modificar api.js para usar Variables de Entorno

**Abre `src/services/api.js` y cambia:**

```javascript
import axios from 'axios'

// Usar variable de entorno, con fallback a localhost
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7000/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// ... resto del codigo igual
```

**Explicacion:**
- `import.meta.env.VITE_API_URL` - Asi se leen variables de entorno en Vite
- Las variables deben empezar con `VITE_` para que Vite las exponga al frontend

### Paso 4.3: Crear archivo .env.local para desarrollo

Para desarrollo local, crea `.env.local`:

```env
VITE_API_URL=https://localhost:7000/api
```

### Paso 4.4: Asegurarte que .env este en .gitignore

Verifica que tu `.gitignore` tenga:

```
.env
.env.local
.env.production
```

**IMPORTANTE:** Los archivos `.env` con secretos NO deben subirse a GitHub.

---

## 5. Desplegar Frontend en Vercel

### Paso 5.1: Crear cuenta en Vercel

1. Ve a [vercel.com](https://vercel.com)
2. Haz clic en "Sign Up"
3. Inicia sesion con tu cuenta de GitHub

### Paso 5.2: Importar Proyecto

1. Haz clic en **"Add New..."** > **"Project"**
2. Selecciona tu repositorio `ReservationSystem`
3. Vercel detectara que es un proyecto con multiples carpetas

### Paso 5.3: Configurar el Proyecto

En la pantalla de configuracion:

| Campo | Valor |
|-------|-------|
| **Framework Preset** | Vite |
| **Root Directory** | `Frontend/reservation-app` |
| **Build Command** | `npm run build` |
| **Output Directory** | `dist` |

### Paso 5.4: Agregar Variables de Entorno

En la seccion **"Environment Variables"**, agrega:

| Nombre | Valor |
|--------|-------|
| `VITE_API_URL` | `https://tu-url-railway.app/api` |

### Paso 5.5: Desplegar

1. Haz clic en **"Deploy"**
2. Espera a que termine el build (1-2 minutos)
3. Vercel te dara una URL como: `https://reservation-app.vercel.app`

---

## 6. Conectar Frontend con Backend

### Paso 6.1: Actualizar CORS en Railway

Ahora que tienes la URL de Vercel, actualiza la variable de entorno en Railway:

1. Ve a tu servicio backend en Railway
2. En **Variables**, actualiza:
   - `AllowedOrigins__0` = `https://tu-app.vercel.app`

3. Railway hara redeploy automaticamente

### Paso 6.2: Probar la Conexion

1. Abre tu frontend en Vercel
2. Intenta hacer login o registrarte
3. Abre las DevTools del navegador (F12) > Network
4. Verifica que las peticiones a tu API funcionen correctamente

### Diagrama de Conexion

```
+-------------------+         +-------------------+         +-------------+
|                   |  HTTPS  |                   |   SQL   |             |
|   Vercel          | ------> |   Railway         | ------> |  PostgreSQL |
|   (Frontend)      |         |   (Backend API)   |         |  (Database) |
|                   |         |                   |         |             |
| reservation-app   |         | SmartBookAPI      |         |             |
| .vercel.app       |         | .railway.app      |         |             |
+-------------------+         +-------------------+         +-------------+
```

---

## 7. Troubleshooting (Problemas Comunes)

### Error: "CORS policy blocked"

**Sintoma:** En la consola del navegador ves:
```
Access to XMLHttpRequest at 'https://api.railway.app' from origin 'https://app.vercel.app' has been blocked by CORS policy
```

**Solucion:**
1. Verifica que `AllowedOrigins__0` en Railway tenga la URL exacta de tu frontend
2. No incluyas `/` al final de la URL
3. Asegurate de usar `https://` no `http://`

### Error: "Connection refused" o "Network Error"

**Sintoma:** El frontend no puede conectarse al backend.

**Solucion:**
1. Verifica que `VITE_API_URL` en Vercel sea correcta
2. Asegurate de incluir `/api` al final: `https://tu-api.railway.app/api`
3. Verifica que el backend este corriendo en Railway (no en "Sleeping")

### Error: "500 Internal Server Error"

**Sintoma:** El backend responde pero con error 500.

**Solucion:**
1. Ve a Railway > Logs para ver el error exacto
2. Probablemente falta una variable de entorno
3. Verifica que todas las variables esten configuradas correctamente

### Error: "Invalid token" o "401 Unauthorized"

**Sintoma:** El login funciona pero otras peticiones fallan.

**Solucion:**
1. Verifica que `JwtSettings__SecretKey` sea la misma en produccion
2. Asegurate de que el token se este enviando en el header Authorization

### La base de datos no tiene datos

**Sintoma:** Los endpoints funcionan pero devuelven arrays vacios.

**Solucion:**
1. El Seeder debe ejecutarse automaticamente al iniciar
2. Revisa los logs de Railway para ver si hubo errores en el seed
3. Puedes conectarte a la DB desde Railway y ejecutar los scripts manualmente

### Railway cobra dinero?

**Respuesta:** Railway tiene un plan gratuito con limitaciones:
- $5 USD de credito gratis al mes
- Tu app puede "dormir" despues de inactividad
- Para mantenerla activa 24/7, necesitas plan de pago (~$5/mes)

### Vercel cobra dinero?

**Respuesta:** Vercel es gratis para proyectos personales con:
- Deployments ilimitados
- HTTPS gratis
- Dominio `.vercel.app` gratis

---

## Resumen de URLs Importantes

| Servicio | URL Local | URL Produccion |
|----------|-----------|----------------|
| Frontend | `http://localhost:5173` | `https://tu-app.vercel.app` |
| Backend | `https://localhost:7000` | `https://tu-api.railway.app` |
| Swagger | `https://localhost:7000/swagger` | `https://tu-api.railway.app/swagger` |

---

## Checklist Final

- [ ] Backend desplegado en Railway
- [ ] Base de datos PostgreSQL creada en Railway
- [ ] Variables de entorno configuradas en Railway
- [ ] Frontend desplegado en Vercel
- [ ] Variable VITE_API_URL configurada en Vercel
- [ ] CORS configurado con la URL de Vercel
- [ ] Login y registro funcionando
- [ ] Peticiones autenticadas funcionando

---

## Contacto y Recursos

- [Documentacion de Railway](https://docs.railway.app/)
- [Documentacion de Vercel](https://vercel.com/docs)
- [Documentacion de .NET en containers](https://docs.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Documentacion de Vite Env Variables](https://vitejs.dev/guide/env-and-mode.html)
