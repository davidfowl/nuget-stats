CREATE TABLE Stats (
    Id int PRIMARY KEY IDENTITY(1,1),
    LogTime datetime NULL,
    Downloads int NOT NULL,
    UniquePackages int NOT NULL,
    TotalPackages int NOT NULL
)