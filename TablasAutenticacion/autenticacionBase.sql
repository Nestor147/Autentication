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


CREATE TABLE Autorizacion.RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuarioSistema INT NOT NULL,
    TokenHash NVARCHAR(500) NOT NULL,         -- Cifrado o Hash
    FechaExpiracion DATETIME NOT NULL,
    Usado BIT DEFAULT 0,
    Revocado BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    IP NVARCHAR(100),
    UserAgent NVARCHAR(500),
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_RefreshToken_Usuario FOREIGN KEY (IdUsuarioSistema)
        REFERENCES Autorizacion.UsuariosSistema(Id)
);
GO


CREATE TABLE Autorizacion.IntentosFallidosLogin (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuarioSistema INT NULL,
    Username NVARCHAR(50) NOT NULL,
    IP NVARCHAR(100) NOT NULL,
    UserAgent NVARCHAR(500),
    Exitoso BIT NOT NULL DEFAULT 0,
    FechaIntento DATETIME DEFAULT GETDATE(),
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_Intento_Usuario FOREIGN KEY (IdUsuarioSistema)
        REFERENCES Autorizacion.UsuariosSistema(Id)
);
GO


CREATE TABLE Autorizacion.TokensRevocados (
    Jti UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuarioSistema INT NOT NULL,
    Motivo NVARCHAR(250),
    FechaRevocacion DATETIME DEFAULT GETDATE(),
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_TokenRevocado_Usuario FOREIGN KEY (IdUsuarioSistema)
        REFERENCES Autorizacion.UsuariosSistema(Id)
);
GO


CREATE TABLE Autorizacion.DispositivosConocidos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuarioSistema INT NOT NULL,
    FingerprintHash NVARCHAR(300) NOT NULL,
    NombreDispositivo NVARCHAR(100),
    UserAgent NVARCHAR(500),
    IP NVARCHAR(100),
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_Dispositivo_Usuario FOREIGN KEY (IdUsuarioSistema)
        REFERENCES Autorizacion.UsuariosSistema(Id)
);
GO


CREATE TABLE Autorizacion.AuditoriaLogins (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuarioSistema INT NULL,
    Username NVARCHAR(50),
    IP NVARCHAR(100),
    UserAgent NVARCHAR(500),
    Exitoso BIT,
    Mensaje NVARCHAR(250),
    Fecha DATETIME DEFAULT GETDATE(),
    EstadoRegistro INT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    UsuarioRegistro NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT FK_Auditoria_Usuario FOREIGN KEY (IdUsuarioSistema)
        REFERENCES Autorizacion.UsuariosSistema(Id)
);
GO


----