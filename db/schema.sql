/* ================================================================
   База данных: DB_Rusakov_RealEstate
   Тема: Система управления агентством недвижимости
   Учебная практика УП.01.01, студент: Русаков Артём Сергеевич
   СУБД: Microsoft SQL Server
   ================================================================ */

USE master;
GO

IF DB_ID(N'DB_Rusakov_RealEstate') IS NOT NULL
BEGIN
    ALTER DATABASE DB_Rusakov_RealEstate SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DB_Rusakov_RealEstate;
END
GO

CREATE DATABASE DB_Rusakov_RealEstate;
GO

USE DB_Rusakov_RealEstate;
GO

/* ---------- Таблица пользователей (сотрудники агентства) ---------- */
CREATE TABLE [dbo].[User]
(
    [ID]       INT IDENTITY(1,1) NOT NULL,
    [Login]    NVARCHAR(50)  NOT NULL,
    [Password] NVARCHAR(64)  NOT NULL,   -- хэш SHA-256 (64 hex-символа)
    [Role]     NVARCHAR(20)  NOT NULL,   -- 'Admin' или 'User'
    [FIO]      NVARCHAR(150) NOT NULL,
    [Photo]    NVARCHAR(260) NULL,       -- путь к файлу фотографии
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_User_Login] UNIQUE ([Login]),
    CONSTRAINT [CK_User_Role] CHECK ([Role] IN (N'Admin', N'User'))
);
GO

/* ---------- Справочник типов недвижимости ---------- */
CREATE TABLE [dbo].[PropertyType]
(
    [ID]   INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_PropertyType] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_PropertyType_Name] UNIQUE ([Name])
);
GO

/* ---------- Объекты недвижимости ---------- */
CREATE TABLE [dbo].[Property]
(
    [ID]      INT IDENTITY(1,1) NOT NULL,
    [TypeID]  INT NOT NULL,              -- тип объекта
    [UserID]  INT NOT NULL,              -- ответственный агент
    [Date]    DATE NOT NULL,             -- дата размещения
    [Address] NVARCHAR(250) NOT NULL,
    [Area]    DECIMAL(10,2) NOT NULL,    -- площадь, м2
    [Rooms]   INT NULL,                  -- количество комнат
    [Price]   DECIMAL(18,2) NOT NULL,    -- цена, руб.
    [Photo]   NVARCHAR(260) NULL,
    CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Property_PropertyType] FOREIGN KEY ([TypeID])
        REFERENCES [dbo].[PropertyType] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Property_User] FOREIGN KEY ([UserID])
        REFERENCES [dbo].[User] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [CK_Property_Area]  CHECK ([Area] > 0 AND [Area] <= 100000),
    CONSTRAINT [CK_Property_Rooms] CHECK ([Rooms] IS NULL OR ([Rooms] >= 0 AND [Rooms] <= 50)),
    CONSTRAINT [CK_Property_Price] CHECK ([Price] > 0)
);
GO

/* ================================================================
   Тестовые данные
   Пароли (до хэширования):
     admin       / Admin2026  (Admin)
     manager     / Manager1   (Admin)
     petrov      / Agent111   (User)
     kuznetsova  / Agent222   (User)
     sokolov     / Agent333   (User)
     user        / User123    (User)
   ================================================================ */

SET IDENTITY_INSERT [dbo].[User] ON;
INSERT INTO [dbo].[User] ([ID], [Login], [Password], [Role], [FIO], [Photo]) VALUES
(1, N'admin', '059A50CE956B7EC61527C7ECC0C55B5A009DC54AB4ACDDCE8852B46BAA2ABA30', N'Admin', N'Русаков Артём Сергеевич', NULL),
(2, N'manager', '67E06322C104F451A8CE3D2E1A40DB0C0B91DDB4CD8C5FDACE6425ED6E624A97', N'Admin', N'Смирнова Елена Викторовна', NULL),
(3, N'petrov', '2E7C36183A3641215A7E6795FF829A9AF08126438EA9F26DDF554CD11DD0D475', N'User', N'Петров Иван Алексеевич', NULL),
(4, N'kuznetsova', '9A70B72FF0C30C148C526EA286AD663DB7916713D9E68C34808AAC8FA4350EDC', N'User', N'Кузнецова Мария Олеговна', NULL),
(5, N'sokolov', '024BA8BCC687EF2B374B2EB1D6318407E3FA08EE46EDF71329D40F553DC04B3F', N'User', N'Соколов Дмитрий Игоревич', NULL),
(6, N'user', 'A61A8ADF60038792A2CB88E670B20540A9D6C2CA204AB754FC768950E79E7D36', N'User', N'Иванова Анна Павловна', NULL);
SET IDENTITY_INSERT [dbo].[User] OFF;
GO

SET IDENTITY_INSERT [dbo].[PropertyType] ON;
INSERT INTO [dbo].[PropertyType] ([ID], [Name]) VALUES
(1, N'Квартира'),
(2, N'Комната'),
(3, N'Частный дом'),
(4, N'Коммерческая недвижимость'),
(5, N'Земельный участок'),
(6, N'Гараж');
SET IDENTITY_INSERT [dbo].[PropertyType] OFF;
GO

INSERT INTO [dbo].[Property] ([TypeID], [UserID], [Date], [Address], [Area], [Rooms], [Price], [Photo]) VALUES
(1, 3, '2026-09-01', N'г. Москва, ул. Профсоюзная, д. 45, кв. 12', 54.30, 2, 14500000.00, NULL),
(1, 3, '2026-09-03', N'г. Москва, Ленинский пр-т, д. 98, кв. 77', 38.00, 1, 10900000.00, NULL),
(1, 4, '2026-09-05', N'г. Москва, ул. Бутлерова, д. 7, кв. 140', 76.50, 3, 21800000.00, NULL),
(2, 4, '2026-09-06', N'г. Москва, ул. Вавилова, д. 13, кв. 5', 14.20, 1, 3900000.00, NULL),
(3, 5, '2026-09-08', N'Московская обл., г. Истра, ул. Садовая, д. 21', 142.00, 5, 18700000.00, NULL),
(4, 3, '2026-09-10', N'г. Москва, ул. Тверская, д. 18, пом. 1', 120.00, NULL, 64000000.00, NULL),
(5, 5, '2026-09-11', N'Московская обл., Рузский р-н, д. Лидино, уч. 34', 1200.00, NULL, 2600000.00, NULL),
(6, 4, '2026-09-12', N'г. Москва, ул. Обручева, д. 30, гараж 211', 18.00, NULL, 1450000.00, NULL),
(1, 5, '2026-09-14', N'г. Москва, Мичуринский пр-т, д. 6, кв. 88', 64.80, 2, 19300000.00, NULL),
(1, 6, '2026-09-15', N'г. Москва, ул. Новаторов, д. 4, кв. 31', 45.10, 2, 12700000.00, NULL),
(2, 6, '2026-09-17', N'г. Москва, ул. Гарибальди, д. 23, кв. 9', 17.60, 1, 4200000.00, NULL),
(3, 3, '2026-09-18', N'Московская обл., г. Одинцово, ул. Лесная, д. 3', 210.00, 6, 32500000.00, NULL),
(4, 5, '2026-09-20', N'г. Москва, Варшавское ш., д. 87, пом. 4', 85.00, NULL, 27900000.00, NULL),
(5, 4, '2026-09-22', N'Московская обл., г. Звенигород, СНТ «Ромашка», уч. 12', 800.00, NULL, 1900000.00, NULL),
(6, 3, '2026-09-23', N'г. Москва, ул. Академика Янгеля, д. 2, гараж 18', 20.50, NULL, 1250000.00, NULL),
(1, 4, '2026-09-25', N'г. Москва, ул. Островитянова, д. 11, кв. 56', 33.40, 1, 9600000.00, NULL),
(1, 6, '2026-09-27', N'г. Москва, Севастопольский пр-т, д. 52, кв. 101', 92.00, 4, 26400000.00, NULL),
(3, 6, '2026-09-29', N'Московская обл., г. Красногорск, ул. Речная, д. 16', 175.00, 5, 24800000.00, NULL),
(4, 4, '2026-10-01', N'г. Москва, Кутузовский пр-т, д. 35, пом. 2', 240.00, NULL, 118000000.00, NULL),
(2, 5, '2026-10-03', N'г. Москва, ул. Удальцова, д. 19, кв. 47', 12.80, 1, 3600000.00, NULL),
(1, 3, '2026-10-05', N'г. Москва, Нахимовский пр-т, д. 61, кв. 3', 58.70, 3, 16900000.00, NULL),
(5, 6, '2026-10-07', N'Московская обл., Наро-Фоминский р-н, д. Каменское, уч. 7', 1500.00, NULL, 3100000.00, NULL);
GO

/* Проверка содержимого таблиц */
SELECT * FROM [dbo].[User];
SELECT * FROM [dbo].[PropertyType];
SELECT * FROM [dbo].[Property];
GO
