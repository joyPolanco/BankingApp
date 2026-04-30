
# Sistema de Banca en Línea

## Descripción

Aplicación web desarrollada en **ASP.NET Core 8** para la gestión integral de productos financieros: cuentas de ahorro, préstamos, tarjetas de crédito, transferencias y pagos.

El sistema incorpora control de acceso basado en roles:

* Administrador
* Cajero
* Cliente
* Comercio (en la API)

### Estructura de la solución

* **Web App (MVC):** interfaz de usuario
* **Web API:** servicios backend y procesador de pagos

---

## Tecnologías

* **Framework:** .NET 8
* **Arquitectura:** Onion Architecture
* **ORM:** Entity Framework Core (Code First)
* **Autenticación:**

  * Web App: Cookies
  * API: JWT Bearer Token
* **Gestión de usuarios:** ASP.NET Core Identity
* **Mapeo:** AutoMapper
* **Base de datos:** SQL Server
* **Frontend:** Bootstrap 5 + CSS personalizado
* **Documentación API:** Swagger
* **Automatización:** Azure Functions

---

## Arquitectura

```
/src
├── Domain          → Entidades y contratos
├── Application     → Lógica de negocio y servicios
├── Infrastructure  → Acceso a datos
├── Presentation    → WebApp (MVC) y WebAPI
└── AzureFunctions  → Procesos automatizados
```

---

## Funcionalidades Principales

### Gestión de Usuarios

* Registro y autenticación
* Autorización por roles
* Activación e inactivación de cuentas

### Cuentas de Ahorro

* Cuenta principal obligatoria
* Creación de cuentas secundarias
* Transferencias y movimientos
* Historial de transacciones

### Préstamos

* Método francés (cuotas fijas)
* Un préstamo activo por cliente
* Generación automática de amortización
* Control automático de mora

### Tarjetas de Crédito

* Generación segura (CVC cifrado)
* Control de límite y deuda
* Registro de consumos

### Transacciones

* Transferencias entre cuentas
* Pagos de préstamos y tarjetas
* Avances de efectivo

### Procesador de Pagos (Hermes Pay)

* Validación de tarjetas
* Registro de consumos
* Notificaciones por correo

---

## API - Seguridad

El sistema implementa autenticación y autorización mediante **JWT**.

### Reglas generales

* Todos los endpoints (excepto login) requieren autenticación
* Usuario no autenticado → **401 Unauthorized**
* Usuario sin permisos → **403 Forbidden**
* Uso de `[Authorize(Roles = "...")]`

### Encabezado requerido

```
Authorization: Bearer {token}
```

### Roles en API

* Administrador
* Comercio

---

## API - Módulos y Endpoints

### 1. Account

**POST /account/login**

* Autenticación y generación de JWT

**POST /account/confirm**

* Activación de usuario mediante token

**POST /account/get-reset-token**

* Generación de token para reset de contraseña
* Inactiva usuario temporalmente
* Envía token por correo

**POST /account/reset-password**

* Cambio de contraseña mediante token

---

### 2. Gestión de Usuarios (Administrador)

/api/users

Endpoints:

* GET /api/users → Listado paginado
* GET /api/users/commerce → Usuarios tipo comercio
* POST /api/users → Crear usuario
* POST /api/users/commerce/{id} → Crear usuario comercio
* PUT /api/users/{id} → Actualizar usuario
* PATCH /api/users/{id}/status → Activar/Inactivar
* GET /api/users/{id} → Detalle

**Reglas:**

* Usuario y correo únicos
* Clientes generan cuenta automáticamente
* Comercios solo pueden tener un usuario

---

### 3. Préstamos (Administrador)

/api/loan

Endpoints:

* GET /api/loan
* POST /api/loan
* GET /api/loan/{id}
* PATCH /api/loan/{id}/rate

**Reglas:**

* Un préstamo activo por cliente
* Evaluación de riesgo
* Generación automática de cuotas
* Acreditación automática del monto

---

### 4. Tarjetas de Crédito (Administrador)

/api/credit-card

Endpoints:

* GET /api/credit-card
* POST /api/credit-card
* GET /api/credit-card/{id}
* PATCH /api/credit-card/{id}/limit
* PATCH /api/credit-card/{id}/cancel

**Reglas:**

* No cancelar con deuda pendiente
* Límite no menor a la deuda actual

---

### 5. Cuentas de Ahorro (Administrador)

/api/savings-account

Endpoints:

* GET /api/savings-account
* POST /api/savings-account
* GET /api/savings-account/{accountNumber}/transactions

---

### 6. Comercios (Administrador)

/api/commerce

Endpoints:

* GET /api/commerce
* GET /api/commerce/{id}
* POST /api/commerce
* PUT /api/commerce/{id}
* PATCH /api/commerce/{id}

**Regla clave:**

* Desactivar comercio desactiva sus usuarios asociados

---

### 7. Pagos - Hermes Pay

/payload

**GET /pay/get-transactions/{commerceId}**

* Comercio: obtiene ID desde JWT
* Admin: envía ID por parámetro

**POST /pay/process-payment/{commerceId}**

**Validaciones:**

* Tarjeta válida (número, fecha, CVC)
* Comercio existente
* Límite suficiente

**Acciones:**

* Acredita monto al comercio
* Registra consumo
* Envía notificación por correo

---

## Códigos de Estado

| Código | Significado         |
| ------ | ------------------- |
| 200    | OK                  |
| 201    | Creado              |
| 204    | Sin contenido       |
| 400    | Error de validación |
| 401    | No autenticado      |
| 403    | Sin permisos        |
| 404    | No encontrado       |
| 409    | Conflicto           |

---

## Seguridad

* Autenticación mediante JWT
* Autorización basada en roles
* Validación de acceso (401 / 403)
* Cifrado de datos sensibles
* Uso de transacciones atómicas

---

## Automatización

### Azure Function

* Ejecución diaria (1:00 AM)
* Marca cuotas vencidas automáticamente

---

## Modelo de Datos (Resumen)

### Entidades principales

* Usuario
* Cuenta de ahorro
* Préstamo
* Cuotas
* Tarjeta de crédito
* Transacciones
* Comercios

### Relaciones

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

---

## Configuración

Variables principales:

* Cadena de conexión (SQL Server)
* Clave JWT
* Configuración SMTP
* Azure Storage (Functions)

---

## Pruebas

* Pruebas unitarias en servicios
* Pruebas de integración

---

## Autores

* Johaly Concepción Polanco
* Homer Osiris Portés Duran
* Kelvin Diaz Ramirez


