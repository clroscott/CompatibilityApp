CREATE TABLE [DayFiance].[Person] (
    [PersonID]    INT           IDENTITY (1, 1) NOT NULL,
    [FirstName]   VARCHAR (50)  NOT NULL,
    [LastName]    VARCHAR (50)  NOT NULL,
    [HomeCity]    VARCHAR (100) NOT NULL,
    [HomeCountry] VARCHAR (100) NOT NULL,
    [DateOfBirth] DATETIME      NOT NULL,
    [PersonType]  VARCHAR (50)  NOT NULL,
    [Gender]      NVARCHAR (10) NOT NULL,
    [ImagePath]   VARCHAR (200) NOT NULL,
    CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ([PersonID] ASC)
);

