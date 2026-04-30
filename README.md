

# Sistema de Banca en Línea

## Descripción

Aplicación web desarrollada en **ASP.NET Core 8** que permite la gestión integral de productos financieros: cuentas de ahorro, préstamos, tarjetas de crédito, transferencias y pagos. Incluye control de acceso basado en roles: **Administrador, Cajero y Cliente**.

La solución está dividida en:

* **Web App (MVC):** interfaz de usuario
* **Web API:** servicios backend y procesador de pagos

---

## Tecnologías

* **Framework:** .NET 8
* **Arquitectura:** Onion Architecture
* **ORM:** Entity Framework Core (Code First)
* **Autenticación:**

  * Web App: Cookies
  * API: JWT
* **Usuarios:** ASP.NET Identity
* **Mapeo:** AutoMapper
* **Base de datos:** SQL Server
* **Frontend:** Bootstrap 5 + CSS
* **Documentación API:** Swagger
* **Tareas programadas:** Azure Functions

---

## Arquitectura

```
/src
├── Domain          → Entidades y contratos
├── Application     → Servicios y lógica de negocio
├── Infrastructure  → Acceso a datos e implementación
├── Presentation    → WebApp (MVC) y WebAPI
└── AzureFunctions  → Tareas programadas
```

---

## Funcionalidades Principales

### Usuarios

* Registro, autenticación y autorización por roles
* Activación/desactivación de cuentas

### Cuentas de Ahorro

* Cuenta principal obligatoria
* Cuentas secundarias transferibles
* Historial de transacciones

### Préstamos

* Método francés (cuotas fijas)
* Un préstamo activo por cliente
* Cálculo automático de cuotas
* Control de mora automatizado

### Tarjetas de Crédito

* Generación segura (CVC cifrado)
* Control de límite y deuda
* Registro de consumos

### Transacciones

* Transferencias entre cuentas
* Pagos de préstamos y tarjetas
* Avances de efectivo

### Procesador de Pagos

* Validación completa de tarjetas
* Registro de transacciones
* Notificaciones por correo

---

API - Funcionalidades
Seguridad

El sistema implementa autenticación y autorización mediante JWT (JSON Web Token).

Roles definidos:
Administrador
Comercio
Reglas:
Todos los endpoints (excepto login) requieren JWT
Usuarios no autenticados → 401 Unauthorized
Usuarios sin permisos → 403 Forbidden
Uso de [Authorize] con roles

Encabezado requerido:

Authorization: Bearer {token}
Módulo: Account
Endpoints
Login
POST /account/login

Permite autenticarse y obtener el token JWT.

Respuestas:

200 → Token generado
400 → Datos inválidos
401 → Credenciales incorrectas
Confirmar cuenta
POST /account/confirm

Activa un usuario mediante token enviado por correo.

204 → Usuario activado
400 → Token inválido
401 → JWT inválido
Obtener token de reseteo
POST /account/get-reset-token

Genera token para cambio de contraseña.

Inactiva usuario temporalmente
Envía token por correo
Resetear contraseña
POST /account/reset-password

Actualiza contraseña usando token.

204 → Contraseña actualizada
400 → Error de validación
401 → No autorizado
Módulo: Usuarios (Admin)
/api/users
Endpoints
GET /api/users → Listado paginado
GET /api/users/commerce → Usuarios tipo comercio
POST /api/users → Crear usuario
POST /api/users/commerce/{id} → Crear usuario comercio
PUT /api/users/{id} → Actualizar
PATCH /api/users/{id}/status → Activar/Inactivar
GET /api/users/{id} → Detalle

Reglas clave:

Usuario y correo únicos
Clientes crean cuenta automáticamente
Comercio solo puede tener un usuario
Módulo: Préstamos (Admin)
/api/loan
Endpoints
GET /api/loan → Listado
POST /api/loan → Crear préstamo
GET /api/loan/{id} → Detalle + amortización
PATCH /api/loan/{id}/rate → Editar tasa

Reglas:

Un préstamo activo por cliente
Evaluación de riesgo
Genera tabla de amortización
Acredita monto automáticamente
Módulo: Tarjetas de Crédito (Admin)
/api/credit-card
Endpoints
GET /api/credit-card → Listado
POST /api/credit-card → Crear
GET /api/credit-card/{id} → Detalle
PATCH /api/credit-card/{id}/limit → Editar límite
PATCH /api/credit-card/{id}/cancel → Cancelar

Reglas:

No cancelar con deuda
Límite ≥ deuda actual
Módulo: Cuentas de Ahorro (Admin)
/api/savings-account
Endpoints
GET /api/savings-account → Listado
POST /api/savings-account → Crear secundaria
GET /api/savings-account/{accountNumber}/transactions → Movimientos
Módulo: Comercios (Admin)
/api/commerce
Endpoints
GET /api/commerce → Listado
GET /api/commerce/{id} → Detalle
POST /api/commerce → Crear
PUT /api/commerce/{id} → Actualizar
PATCH /api/commerce/{id} → Estado

Regla clave:

Desactivar comercio → desactiva usuarios asociados
Módulo: Pagos (Hermes Pay)
/pay
Endpoints
Obtener transacciones
GET /pay/get-transactions/{commerceId}
Comercio → ID desde token
Admin → ID por parámetro
Procesar pago
POST /pay/process-payment/{commerceId}

Validaciones:

Tarjeta válida (número, fecha, CVC)
Comercio existente
Límite disponible suficiente

Acciones:

Acredita monto al comercio
Registra consumo
Envía notificación por correo
Códigos de Estado
Código	Significado
200	OK
201	Creado
204	Sin contenido
400	Error de validación
401	No autenticado
403	Sin permisos
404	No encontrado
409	Conflicto
---

## Seguridad

* Autenticación con JWT
* Autorización por roles
* Validación de acceso (401 / 403)
* Cifrado de datos sensibles
* Transacciones atómicas en operaciones financieras

---

## Automatización

**Azure Function:**

* Ejecuta diariamente (1:00 AM)
* Marca cuotas vencidas automáticamente

---

## Modelo de Datos (Resumen)

Principales entidades:

* Usuario
* Cuenta de ahorro
* Préstamo
* Cuotas
* Tarjeta de crédito
* Transacciones
* Comercios

Relaciones:

* Usuario → múltiples cuentas, préstamos y tarjetas
* Cuenta → múltiples transacciones
* Préstamo → múltiples cuotas

---

## Reglas de Negocio

* Un préstamo activo por cliente
* Límite de tarjeta ≥ deuda actual
* Cuenta principal no eliminable
* Pagos no exceden deuda
* Evaluación de riesgo basada en deuda promedio


## Configuración

Variables principales:

* Cadena de conexión (SQL Server)
* Clave JWT
* Configuración de correo SMTP
* Azure Storage (Functions)

---

## Pruebas

* Pruebas unitarias en servicios
* Pruebas de integración

---

## Autor

Johaly Concepción Polanco
Homer Osiris Portés Duran
Kelvin Diaz Ramirez

---


