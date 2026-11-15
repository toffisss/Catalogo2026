# Catalogo2026



**Autor:** Ever Osmar Toffoletti Allende



Aplicación WPF (.NET 10) con login contra MySQL. Práctica 3 de Desarrollo de Interfaces (2º DAM, IES Barajas).



## Requisitos

- Visual Studio 2026 con la carga de trabajo «Desarrollo de escritorio de .NET»

- MySQL Server (probado con 26.10) en localhost:3306



## Puesta en marcha

1. Ejecutar `BaseDatos/database.sql` en MySQL Workbench (crea la BD, la tabla, los usuarios de prueba y el usuario `catalogo_app`).

2. Abrir `Catalogo2026.slnx` y ejecutar.



## Usuarios de prueba

admin / 1234 · alumno / dam2026 · profesor / prof123



## Funcionalidad

- Login contra la tabla `Usuarios` de MySQL

- Alta de usuarios con control de duplicados

- Bloqueo temporal tras 3 intentos fallidos

- Gestión de errores de conexión con try/catch

- Home con secciones (Destacados con carrusel, Mis juegos, Tienda, Comunidad, Soporte, Perfil, Configuración con tema oscuro/claro) y cierre de sesión



## Tecnologías

- C# / WPF (.NET 10)

- MySQL Server 26.10 y MySQL Workbench

- MySql.Data 9.7.0 (NuGet)



## Estructura

- `BaseDatos/database.sql` – script de creación de la BD

- `Catalogo2026/DatabaseHelper.cs` – cadena de conexión y apertura/cierre

- `Catalogo2026/LoginWindow.xaml(.cs)` – login y alta de usuarios

- `Catalogo2026/MainWindow.xaml(.cs)` – Home con tienda, comunidad, soporte y perfil

- `Catalogo2026/Imagenes/` – recursos del carrusel



## Seguridad

- Consultas parametrizadas (`@usuario`, `@password`): el intento `' OR '1'='1` no permite acceder.

- Usuario MySQL `catalogo_app` con privilegios mínimos (solo SELECT e INSERT sobre `Usuarios`).

- Validación de campos vacíos antes de abrir conexión.

- Timeout de conexión de 5 s y mensaje controlado si MySQL no está disponible.



## Configuración

La cadena de conexión está en `DatabaseHelper.cs`. Si tu MySQL usa otro puerto o usuario, modifícala ahí.



## Limitaciones

- Las contraseñas se guardan en texto plano (en producción se usaría hash con sal, p. ej. BCrypt).

- La cadena de conexión está en el código; en producción iría en un archivo de configuración o en variables de entorno.






