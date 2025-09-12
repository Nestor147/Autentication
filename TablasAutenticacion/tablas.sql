-- Crear esquema si no existe
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Autorizacion')
BEGIN
    EXEC('CREATE SCHEMA Autorizacion');
END;
GO

-- Tabla: Aplicaciones
CREATE TABLE Autorizacion.Aplicaciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Sigla NVARCHAR(25) NOT NULL,
    Descripcion NVARCHAR(250) NOT NULL,
    Enlace NVARCHAR(250),
    Icono NVARCHAR(50),
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER
);
GO

-- Tabla: Roles
CREATE TABLE Autorizacion.Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdAplicacion INT NOT NULL,
    Nombre NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(100) NOT NULL,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_Roles_Aplicaciones FOREIGN KEY (IdAplicacion)
        REFERENCES Autorizacion.Aplicaciones(Id)
);
GO

-- Tabla: UsuariosSistema (ya vinculada a General.Usuarios)
CREATE TABLE Autorizacion.UsuariosSistema (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuarioGeneral INT NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(300) NOT NULL,
    UltimoCambio DATETIME NULL,
    Locked BIT NOT NULL DEFAULT 0,
    LockDate DATETIME NULL,
    NuevoUsuario BIT NOT NULL DEFAULT 0,
    LoginPerpetuo BIT NOT NULL DEFAULT 1,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    LoginClave BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_UsuariosSistema_Usuarios FOREIGN KEY (IdUsuarioGeneral)
        REFERENCES General.Usuarios(Id)
);
GO

-- Tabla: RolesUsuarios
CREATE TABLE Autorizacion.RolesUsuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    IdUsuarioSistema INT NOT NULL,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_RolesUsuarios_Rol FOREIGN KEY (IdRol)
        REFERENCES Autorizacion.Roles(Id),
    CONSTRAINT FK_RolesUsuarios_Usuario FOREIGN KEY (IdUsuarioSistema)
        REFERENCES Autorizacion.UsuariosSistema(Id)
);
GO

-- Tabla: Objetos
CREATE TABLE Autorizacion.Objetos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdAplicacion INT NOT NULL,
    Pagina NVARCHAR(50),
    Descripcion NVARCHAR(100) NOT NULL,
    Detalle NVARCHAR(350) NOT NULL,
    TipoObjeto INT NOT NULL, -- 1: Nodo, 2: Página
    NombreIcono NVARCHAR(100) NOT NULL,
    Nuevo BIT NOT NULL DEFAULT 0,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_Objetos_Aplicaciones FOREIGN KEY (IdAplicacion)
        REFERENCES Autorizacion.Aplicaciones(Id)
);
GO

-- Tabla: ObjetosMenus
CREATE TABLE Autorizacion.ObjetosMenus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdObjeto INT NOT NULL UNIQUE,
    Nivel INT NOT NULL,
    Identacion INT NOT NULL,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_ObjetosMenus_Objeto FOREIGN KEY (IdObjeto)
        REFERENCES Autorizacion.Objetos(Id)
);
GO

-- Tabla: ObjetosPuntos
CREATE TABLE Autorizacion.ObjetosPuntos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdObjeto INT NOT NULL,
    TipoServicio INT NOT NULL,
    Punto NVARCHAR(350) NOT NULL,
    Descripcion NVARCHAR(500) NOT NULL,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_ObjetosPuntos_Objeto FOREIGN KEY (IdObjeto)
        REFERENCES Autorizacion.Objetos(Id)
);
GO

-- Tabla: RolesObjetosMenus
CREATE TABLE Autorizacion.RolesObjetosMenus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    IdObjeto INT NOT NULL,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_RolesObjetosMenus_Rol FOREIGN KEY (IdRol)
        REFERENCES Autorizacion.Roles(Id),
    CONSTRAINT FK_RolesObjetosMenus_Objeto FOREIGN KEY (IdObjeto)
        REFERENCES Autorizacion.Objetos(Id)
);
GO

-- Tabla: RolesPuntos
CREATE TABLE Autorizacion.RolesPuntos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    TipoServicio INT NOT NULL,
    Punto NVARCHAR(500) NOT NULL,
    PageName NVARCHAR(350) NOT NULL,
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_RolesPuntos_Rol FOREIGN KEY (IdRol)
        REFERENCES Autorizacion.Roles(Id)
);
GO

---
