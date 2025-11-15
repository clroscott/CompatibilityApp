CREATE TABLE [DayFiance].[PersonRating] (
    [PersonID]     INT            NOT NULL,
    [Season]       SMALLINT       NOT NULL,
    [RatingTypeID] INT            NOT NULL,
    [RatingValue]  DECIMAL (5, 2) NOT NULL,
    CONSTRAINT [PK_PersonRating] PRIMARY KEY CLUSTERED ([PersonID] ASC, [Season] ASC, [RatingTypeID] ASC),
    CONSTRAINT [FK_PersonRating_PersonSeason] FOREIGN KEY ([PersonID], [Season]) REFERENCES [DayFiance].[PersonSeason] ([PersonID], [Season]),
    CONSTRAINT [FK_PersonRating_RatingType] FOREIGN KEY ([RatingTypeID]) REFERENCES [DayFiance].[RatingType] ([RatingTypeID]) ON DELETE CASCADE ON UPDATE CASCADE
);

