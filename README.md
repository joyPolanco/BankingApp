
Artemis Banking - Plataforma de Banca en Línea
https://img.shields.io/badge/.NET-8.0-blue
https://img.shields.io/badge/Architecture-Onion-purple

## Descripción del Proyecto
Artemis Banking es una plataforma de banca en línea completa. La solución permite gestionar de manera integral cuentas de ahorro, préstamos y tarjetas de crédito, procesar pagos, realizar avances de efectivo y transferencias, todo bajo un estricto modelo de seguridad basado en roles.

El proyecto se compone de dos subsistemas:

Web App (MVC): Interfaz para Administradores, Cajeros y Clientes.

Web API (REST): Servicios backend y procesador de pagos Hermes Pay para comercios.

## Tecnologías Implementadas
Framework: ASP.NET Core 8 (MVC y Web API)

Arquitectura: Onion Architecture (Capa Domain, Application, Infrastructure, Presentation)

ORM: Entity Framework Core (Code-First con Migraciones)

Autenticación:

Web App: Cookies con ASP.NET Core Identity.

Web API: JWT Bearer Token.

Gestión de Usuarios: ASP.NET Core Identity con Roles (Admin, Cajero, Cliente, Comercio).

Mapeo: AutoMapper (para DTOs y ViewModels).

Base de Datos: SQL Server.

Frontend: Bootstrap 5, CSS, HTML.

Documentación API: Swagger / Swashbuckle.

Automatización: Azure Functions (para control diario de mora en cuotas).

## Arquitectura del Proyecto (Onion)
La solución sigue una estricta Arquitectura Onion, garantizando la independencia de la infraestructura y la separación de responsabilidades.
/src
├── Domain/                      # Capa Núcleo (Entidades, Enums, Interfaces)
│   ├── Entities/                # Usuario, CuentaAhorro, Prestamo, TarjetaCredito, etc.
│   └── Interfaces/              # IRepository<T>, IUnitOfWork
├── Application/                 # Capa de Aplicación (Lógica de negocio)
│   ├── DTOs/                    # Data Transfer Objects
│   ├── Services/                # Servicios específicos y genéricos
│   └── Mappings/                # Perfiles de AutoMapper
├── Infrastructure/              # Capa de Infraestructura (Persistencia, Identity)
│   ├── Data/                    # ApplicationDbContext, Migraciones
│   ├── Repositories/            # Implementación de repositorios
│   └── Identity/                # Configuración extendida de Identity
└── Presentation/                # Capa de Presentación
    ├── Artemis.WebApp/          # Aplicación MVC (Vistas, ViewModels, Controladores)
    └── Artemis.WebAPI/          # API REST (Controladores, JWT, Swagger)
    
## Funcionalidades del Sistema
-- Administrador
Dashboard: KPIs en tiempo real (transacciones totales/día, clientes activos/inactivos, productos asignados, deuda promedio, préstamos y tarjetas vigentes).

Gestión de Usuarios: CRUD completo, activar/inactivar, filtro por rol (Admin/Cajero/Cliente). Los clientes reciben automáticamente una cuenta de ahorro principal.

Gestión de Préstamos:

Asignación con validación de riesgo (deuda promedio).

Cálculo de cuota fija mediante Sistema Francés.

Generación automática de tabla de amortización.

Edición de tasa de interés (recalcula cuotas futuras).

Control automático de mora (Azure Function diaria).

Gestión de Tarjetas de Crédito:

Asignación con generación de número de 16 dígitos y CVC cifrado (SHA-256).

Edición de límite (no menor a la deuda actual).

Cancelación solo si no tiene deuda pendiente.

Gestión de Cuentas de Ahorro:

Creación de cuentas secundarias.

Cancelación de cuentas secundarias (transfiriendo el saldo a la principal).

--Cliente
Dashboard de Productos: Visualización de cuentas de ahorro, préstamos y tarjetas activas.

Beneficiarios: Registro y eliminación de cuentas de terceros para transferencias rápidas.

Transacciones:

Express: Transferencias a cualquier cuenta del sistema.

A Beneficiarios: Transferencias a contactos guardados.

Pago de Tarjeta: Abono a sus propias tarjetas.

Pago de Préstamo: Abono a sus préstamos (aplicado secuencialmente a las cuotas).

Avance de Efectivo: Transferencia de saldo desde tarjeta de crédito a cuenta de ahorro (aplica 6.25% de interés).

Transferencia entre Cuentas: Movimiento de fondos entre sus propias cuentas de ahorro.

-- Cajero
Dashboard: Indicadores de actividad diaria (depósitos, retiros, pagos).

Operaciones de Ventanilla: Depósitos, Retiros, Pagos a Tarjetas, Pagos a Préstamos y Transferencias a cuentas de terceros.

## Web API (Endpoints para Comercios y Administración)
La API está documentada con Swagger y protegida con JWT.

Seguridad de la API
401 Unauthorized: Token no proporcionado o inválido.

403 Forbidden: Usuario autenticado pero sin el rol requerido (Administrador o Comercio).

Endpoints Principales
Módulo	Método	Endpoint	Rol	Descripción
Account	POST	/account/login	-	Autentica y devuelve JWT.
POST	/account/confirm	Admin/Comercio	Activa cuenta mediante token.
Usuarios	GET	/api/users	Admin	Lista paginada de usuarios.
POST	/api/users	Admin	Crea Admin, Cajero o Cliente.
Préstamos	POST	/api/loan	Admin	Asigna nuevo préstamo.
PATCH	/api/loan/{id}/rate	Admin	Modifica tasa de interés.
Tarjetas	POST	/api/credit-card	Admin	Asigna nueva tarjeta.
PATCH	/api/credit-card/{id}/cancel	Admin	Cancela tarjeta sin deuda.
Comercios	POST	/api/commerce	Admin	Crea nuevo comercio.
PATCH	/api/commerce/{id}	Admin	Activa/Desactiva comercio.
HermesPay	POST	/pay/process-payment/{id}	Admin/Comercio	Procesa pago con tarjeta.
Ejemplo de Petición a Hermes Pay
json
POST /pay/process-payment/1
Authorization: Bearer {token_comercio}
Content-Type: application/json

{
  "cardNumber": "1589963258467598",
  "monthExpirationCard": "02",
  "yearExpirationCard": "2028",
  "CVC": "859",
  "transactionAmount": "689.25"
}
## Capturas de Pantalla
Módulo Administrador
Dashboard de Administrador	Gestión de Usuarios
<img width="400" src="https://github.com/user-attachments/assets/c7370677-3825-4216-972d-6472bfe2b2fb" />	<img width="400" src="https://github.com/user-attachments/assets/3893ce5f-9155-4236-aeaa-d44b19bc0a2c" />
Gestión de Préstamos	Asignar Préstamo
<img width="400" src="https://github.com/user-attachments/assets/077d41a3-041e-4af2-9303-b52fa18d50ae" />	<img width="400" src="https://github.com/user-attachments/assets/ea3b1f85-65dd-4090-a484-087233010feb" />
Gestión de Tarjetas	Gestión de Cuentas de Ahorro
<img width="400" src="https://github.com/user-attachments/assets/f84adb56-d4a3-448c-a9e6-9b07ca769c6e" />	<img width="400" src="https://github.com/user-attachments/assets/668e67f2-f45e-4ee4-ac1f-578cb2539679" />
Módulo Cliente
Home del Cliente	Detalle de Cuenta
<img width="400" src="https://github.com/user-attachments/assets/9298aca1-203f-4be8-9cc1-ca74fe929a80" />	<img width="400" src="https://github.com/user-attachments/assets/d12ca405-2bf0-422d-8a12-66546f9c2889" />
Transferencia Express	Confirmación de Transferencia
<img width="400" src="https://github.com/user-attachments/assets/6ebb25bd-8e8c-4693-b274-2e61838c68c6" />	<img width="400" src="https://github.com/user-attachments/assets/085fa72f-ee92-446d-8dd4-08a247e2c1bf" />
Módulo Cajero
Home del Cajero	Depósito en Ventanilla
<img width="400" src="https://github.com/user-attachments/assets/c5a18a3d-1afd-4a45-bfcf-8d9b43d6f79d" />	<img width="400" src="https://github.com/user-attachments/assets/d6119243-a1b7-4057-9d2e-781415419bc9" />
Pago de Préstamo	Transferencia a Terceros
<img width="400" src="https://github.com/user-attachments/assets/84ef867d-570f-452f-bff4-6d0f22c674f5" />	<img width="400" src="https://github.com/user-attachments/assets/9d41e925-b0f3-4f68-9aa9-2d49f43b5fcd" />
📊 Modelo de Datos (Entidades Clave)
Usuario: Hereda de IdentityUser. Datos: Nombre, Apellido, Cédula, Estado (Activo/Inactivo).

CuentaAhorro: Número único (9 dígitos), Balance, Tipo (Principal/Secundaria), Estado.

Prestamo: Monto, TasaInterés, PlazoMeses, Estado (Activo/Completado). Relación 1:N con Cuota.

Cuota: FechaPago, Monto, EstadoPago, IndicadorAtraso.

TarjetaCredito: Número único (16 dígitos), Limite, DeudaActual, FechaExpiracion, CVC (cifrado), Estado.

Transaccion: Fecha, Monto, Tipo (Crédito/Débito), Estado, Beneficiario, Origen.

Comercio: Nombre, Estado (Activo/Inactivo), Relación 1:1 con Usuario.

$ Reglas de Negocio Implementadas
Préstamos: Un cliente solo puede tener un préstamo activo a la vez.

Pago de Préstamos: Se aplica secuencialmente a la cuota pendiente más antigua.

Tarjetas de Crédito: No se puede cancelar una tarjeta con deuda > 0. El nuevo límite no puede ser menor a la deuda actual.

Avance de Efectivo: Aplica un interés del 6.25% automático sobre el monto del avance.

Seguridad por Roles: Redirección a "Acceso Denegado" si un usuario intenta entrar a una sección no autorizada.

Morosidad Automática: Job diario (Azure Functions) que marca cuotas vencidas como "atrasadas".

Comercios: Al desactivar un comercio, su usuario asociado se desactiva automáticamente.



bash
cd Presentation/Artemis.WebAPI
dotnet run
Documentación Swagger: https://localhost:7082/swagger
