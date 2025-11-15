CREATE TABLE [DayFiance].[RatingType] (
    [RatingTypeID]   INT             IDENTITY (1, 1) NOT NULL,
    [RatingName]     VARCHAR (50)    NOT NULL,
    [RatingWeight]   DECIMAL (10, 2) NOT NULL,
    [RatingCategory] VARCHAR (50)    NOT NULL,
    CONSTRAINT [PK_RatingType] PRIMARY KEY CLUSTERED ([RatingTypeID] ASC)
);

