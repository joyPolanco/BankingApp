# Artemis Banking System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Architecture](https://img.shields.io/badge/Architecture-Onion-purple)
![Database](https://img.shields.io/badge/Database-SQL%20Server-red)
![Authentication](https://img.shields.io/badge/Auth-JWT%20%7C%20Identity-green)

Plataforma bancaria desarrollada con ASP.NET Core 8 utilizando Arquitectura Onion, autenticación basada en roles y servicios RESTful.

</div>

---

# Tabla de Contenido

* [Descripción General](#descripción-general)
* [Características Principales](#características-principales)
* [Tecnologías Implementadas](#tecnologías-implementadas)
* [Arquitectura Onion](#arquitectura-onion)
* [Estructura del Proyecto](#estructura-del-proyecto)
* [Seguridad y Autenticación](#seguridad-y-autenticación)
* [Roles del Sistema](#roles-del-sistema)
* [Funcionalidades del Sistema](#funcionalidades-del-sistema)
* [Web API](#web-api)
* [Hermes Pay](#hermes-pay)
* [Modelo de Datos](#modelo-de-datos)
* [Reglas de Negocio](#reglas-de-negocio)
* [Despliegue y Ejecución](#despliegue-y-ejecución)
* [Capturas de Pantalla](#capturas-de-pantalla)

---

# Descripción General

Artemis Banking es una plataforma de banca digital desarrollada con ASP.NET Core 8, diseñada para gestionar operaciones financieras de forma segura, escalable y organizada.

La solución está compuesta por dos sistemas principales:

## Web App (MVC)

Aplicación para:

* Administradores
* Cajeros
* Clientes

## Web API (REST)

API protegida con JWT para:

* Administración
* Integración con comercios
* Procesamiento de pagos mediante Hermes Pay

---

# Características Principales

* Gestión de cuentas de ahorro
* Administración de préstamos
* Sistema de tarjetas de crédito
* Transferencias bancarias
* Pagos y avances de efectivo
* Dashboard con KPIs financieros
* Seguridad basada en roles
* API RESTful documentada con Swagger
* Validaciones de negocio avanzadas
* Arquitectura Onion
* Automatización con Azure Functions

---

# Tecnologías Implementadas

| Tecnología            | Propósito                         |
| --------------------- | --------------------------------- |
| ASP.NET Core 8        | Framework principal               |
| MVC + Web API         | Aplicaciones web y servicios REST |
| Entity Framework Core | ORM y persistencia                |
| SQL Server            | Base de datos                     |
| ASP.NET Core Identity | Gestión de usuarios y roles       |
| JWT Bearer            | Autenticación API                 |
| AutoMapper            | Mapeo DTO/ViewModels              |
| Swagger               | Documentación API                 |
| Bootstrap 5           | Interfaz de usuario               |
| Azure Functions       | Automatización de procesos        |
| HTML/CSS              | Diseño frontend                   |

---

# Arquitectura Onion

El proyecto implementa Onion Architecture, garantizando:

* Separación de responsabilidades
* Bajo acoplamiento
* Escalabilidad
* Independencia de infraestructura
* Mantenimiento simplificado

## Capas del Sistema

### Domain

Contiene:

* Entidades
* Interfaces
* Enums
* Reglas centrales

### Application

Contiene:

* Servicios
* DTOs
* Casos de uso
* Lógica de aplicación

### Infrastructure

Contiene:

* Persistencia
* Identity
* Repositorios
* Configuración de base de datos

### Presentation

Contiene:

* MVC Web App
* API REST
* Controladores
* ViewModels

---

# Estructura del Proyecto

```bash
/src
├── Domain/
│   ├── Entities/
│   └── Interfaces/
│
├── Application/
│   ├── DTOs/
│   ├── Services/
│   └── Mappings/
│
├── Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── Identity/
│
└── Presentation/
    ├── Artemis.WebApp/
    └── Artemis.WebAPI/
```

---

# Seguridad y Autenticación

## Web App

* ASP.NET Core Identity
* Cookies Authentication
* Roles y autorización

## Web API

* JWT Bearer Token
* Endpoints protegidos
* Validación por roles

## Medidas de Seguridad

* Control de acceso
* Restricción por roles
* Protección de endpoints
* Hashing de datos sensibles
* Validación de solicitudes
* Manejo seguro de sesiones

---

# Roles del Sistema

| Rol           | Descripción                  |
| ------------- | ---------------------------- |
| Administrador | Gestión completa del sistema |
| Cajero        | Operaciones de ventanilla    |
| Cliente       | Gestión financiera personal  |
| Comercio      | Integración con Hermes Pay   |

---

# Funcionalidades del Sistema

# Administrador

## Dashboard Financiero

* KPIs en tiempo real
* Clientes activos/inactivos
* Transacciones diarias
* Productos asignados
* Deuda promedio

## Gestión de Usuarios

* CRUD completo
* Activación/Inactivación
* Filtros por roles
* Cuenta principal automática

## Gestión de Préstamos

* Validación de riesgo
* Sistema Francés
* Tabla de amortización
* Recalculo de cuotas
* Mora automática

## Gestión de Tarjetas

* Generación automática
* CVC cifrado SHA-256
* Control de límites
* Cancelación validada

---

# Cliente

## Operaciones Disponibles

* Transferencias
* Pago de tarjetas
* Pago de préstamos
* Avances de efectivo
* Transferencias internas
* Gestión de beneficiarios

## Productos

* Cuentas de ahorro
* Tarjetas de crédito
* Préstamos activos

---

# Cajero

## Operaciones de Ventanilla

* Depósitos
* Retiros
* Transferencias
* Pago de préstamos
* Pago de tarjetas

---

# Web API

La API REST está protegida mediante JWT y documentada con Swagger.

## Autenticación

| Código | Descripción              |
| ------ | ------------------------ |
| 401    | Token inválido o ausente |
| 403    | Usuario sin permisos     |

---

# Endpoints Principales

## Account

| Método | Endpoint           | Descripción      |
| ------ | ------------------ | ---------------- |
| POST   | `/account/login`   | Iniciar sesión   |
| POST   | `/account/confirm` | Confirmar cuenta |

## Usuarios

| Método | Endpoint     |
| ------ | ------------ |
| GET    | `/api/users` |
| POST   | `/api/users` |

## Préstamos

| Método | Endpoint              |
| ------ | --------------------- |
| POST   | `/api/loan`           |
| PATCH  | `/api/loan/{id}/rate` |

## Tarjetas

| Método | Endpoint                       |
| ------ | ------------------------------ |
| POST   | `/api/credit-card`             |
| PATCH  | `/api/credit-card/{id}/cancel` |

## Comercios

| Método | Endpoint             |
| ------ | -------------------- |
| POST   | `/api/commerce`      |
| PATCH  | `/api/commerce/{id}` |

---

# Hermes Pay

Sistema de procesamiento de pagos integrado para comercios.

## Ejemplo de Solicitud

```json
POST /pay/process-payment/1

{
  "cardNumber": "1589963258467598",
  "monthExpirationCard": "02",
  "yearExpirationCard": "2028",
  "CVC": "859",
  "transactionAmount": "689.25"
}
```

---

# Modelo de Datos

## Entidades Principales

### Usuario

* Nombre
* Apellido
* Cédula
* Estado

### CuentaAhorro

* Número de cuenta
* Balance
* Tipo
* Estado

### Prestamo

* Monto
* Interés
* Plazo
* Estado

### Cuota

* Fecha de pago
* Estado
* Mora

### TarjetaCredito

* Número de tarjeta
* Límite
* Deuda actual
* Fecha expiración

### Transaccion

* Monto
* Tipo
* Beneficiario
* Fecha

---

# Reglas de Negocio

* Un cliente solo puede tener un préstamo activo.
* Los pagos de préstamos se aplican secuencialmente.
* No se puede cancelar una tarjeta con deuda pendiente.
* El avance de efectivo aplica 6.25% de interés.
* Seguridad por roles en todas las rutas.
* Las cuotas vencidas se marcan automáticamente como atrasadas.

---

# Despliegue y Ejecución

## Ejecutar Web API

```bash
cd Presentation/Artemis.WebAPI
dotnet run
```

## Swagger

```bash
https://localhost:7082/swagger
```

---

# Capturas de Pantalla

# Administrador

| Dashboard                                                                                                 | Gestión de Usuarios                                                                                       |
| --------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| <img width="400" src="https://github.com/user-attachments/assets/c7370677-3825-4216-972d-6472bfe2b2fb" /> | <img width="400" src="https://github.com/user-attachments/assets/3893ce5f-9155-4236-aeaa-d44b19bc0a2c" /> |

| Gestión de Préstamos                                                                                      | Asignar Préstamo                                                                                          |
| --------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| <img width="400" src="https://github.com/user-attachments/assets/077d41a3-041e-4af2-9303-b52fa18d50ae" /> | <img width="400" src="https://github.com/user-attachments/assets/ea3b1f85-65dd-4090-a484-087233010feb" /> |

---

# Cliente

| Home Cliente                                                                                              | Detalle Cuenta                                                                                            |
| --------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| <img width="400" src="https://github.com/user-attachments/assets/9298aca1-203f-4be8-9cc1-ca74fe929a80" /> | <img width="400" src="https://github.com/user-attachments/assets/d12ca405-2bf0-422d-8a12-66546f9c2889" /> |

---

# Cajero

| Dashboard Cajero                                                                                          | Depósito                                                                                                  |
| --------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| <img width="400" src="https://github.com/user-attachments/assets/c5a18a3d-1afd-4a45-bfcf-8d9b43d6f79d" /> | <img width="400" src="https://github.com/user-attachments/assets/d6119243-a1b7-4057-9d2e-781415419bc9" /> |

---

<div align="center">

Autores
Johaly Concepción Polanco
Homer Osiris Portés Duran
Kelvin Diaz Ramirez
</div>
