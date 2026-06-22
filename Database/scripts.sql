-- =============================================
-- Crear Base de Datos si no existe
-- =============================================
IF DB_ID('ResourceReservationD') IS NULL
BEGIN
  CREATE DATABASE ResourceReservationD;
END
GO

-- =============================================
-- Seleccionar Base de Datos
-- =============================================
USE ResourceReservationD;
GO

-- =============================================
-- Tabla Roles
-- =============================================
CREATE TABLE Roles
(
  RoleId INT IDENTITY(1,1) PRIMARY KEY,
  RoleName NVARCHAR(50) NOT NULL UNIQUE,
  CONSTRAINT CK_Roles_RoleName
        CHECK (RoleName IN ('Admin', 'Client'))
);
GO

-- =============================================
-- Tabla Users
-- =============================================
CREATE TABLE Users
(
  UserId INT IDENTITY(1,1) PRIMARY KEY,
  FullName NVARCHAR(150) NOT NULL,
  Email NVARCHAR(255) NOT NULL,
  PasswordHash NVARCHAR(500) NOT NULL,
  RoleId INT NOT NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

  CONSTRAINT UQ_Users_Email UNIQUE (Email),

  CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId)
);
GO

-- =============================================
-- Tabla Resources
-- =============================================
CREATE TABLE Resources
(
  ResourceId INT IDENTITY(1,1) PRIMARY KEY,
  Name NVARCHAR(150) NOT NULL,
  Description NVARCHAR(1000) NULL,
  Location NVARCHAR(255) NOT NULL,
  IsActive BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- Tabla Reservations
-- =============================================
CREATE TABLE Reservations
(
  ReservationId INT IDENTITY(1,1) PRIMARY KEY,
  UserId INT NOT NULL,
  ResourceId INT NOT NULL,
  [Date] DATE NOT NULL,
  StartTime TIME NOT NULL,
  EndTime TIME NOT NULL,
  Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

  CONSTRAINT FK_Reservations_Users
        FOREIGN KEY (UserId)
        REFERENCES Users(UserId),

  CONSTRAINT FK_Reservations_Resources
        FOREIGN KEY (ResourceId)
        REFERENCES Resources(ResourceId),

  CONSTRAINT CK_Reservations_Status
        CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),

  CONSTRAINT CK_Reservations_TimeRange
        CHECK (EndTime > StartTime)
);
GO

-- =============================================
-- Datos Iniciales
-- =============================================
INSERT INTO Roles
  (RoleName)
VALUES
  ('Admin'),
  ('Client');
GO

-- =============================================
-- Índices Recomendados
-- =============================================
CREATE INDEX IX_Reservations_UserId
ON Reservations(UserId);

CREATE INDEX IX_Reservations_ResourceId
ON Reservations(ResourceId);

CREATE INDEX IX_Reservations_Date
ON Reservations([Date]);
GO