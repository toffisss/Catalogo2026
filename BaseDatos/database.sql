-- =====================================================================
--  Práctica 3 · Catálogo de videojuegos · Acceso a BBDD MySQL
--  Script de creación de la base de datos y de la tabla Usuarios
-- =====================================================================

-- 1. Base de datos
CREATE DATABASE IF NOT EXISTS catalogo2026
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_spanish_ci;

USE catalogo2026;

-- 2. Tabla Usuarios (id, nombre_usuario, password)
--    nombre_usuario es UNIQUE: no admite duplicados
DROP TABLE IF EXISTS Usuarios;

CREATE TABLE Usuarios (
    id              INT          NOT NULL AUTO_INCREMENT,
    nombre_usuario  VARCHAR(50)  NOT NULL,
    password        VARCHAR(100) NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_usuarios_nombre UNIQUE (nombre_usuario)
);

-- 3. Usuarios de prueba (los mismos de la Práctica 2)
INSERT INTO Usuarios (nombre_usuario, password) VALUES
    ('admin',    '1234'),
    ('alumno',   'dam2026'),
    ('profesor', 'prof123');

-- 4. Usuario de MySQL para la aplicación (solo los permisos que necesita)
--    Así la app no se conecta como root.
CREATE USER IF NOT EXISTS 'catalogo_app'@'localhost' IDENTIFIED BY 'Catalogo2026!';
GRANT SELECT, INSERT ON catalogo2026.Usuarios TO 'catalogo_app'@'localhost';
FLUSH PRIVILEGES;

-- 5. Comprobación
SELECT * FROM Usuarios;
