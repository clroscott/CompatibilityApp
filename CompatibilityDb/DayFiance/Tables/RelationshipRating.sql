CREATE TABLE [DayFiance].[RelationshipRating] (
    [RelationshipID] INT            NOT NULL,
    [Season]         SMALLINT       NOT NULL,
    [RatingTypeID]   INT            NOT NULL,
    [RatingValue]    DECIMAL (5, 2) NOT NULL,
    CONSTRAINT [PK_RelationshipRating] PRIMARY KEY CLUSTERED ([RelationshipID] ASC, [RatingTypeID] ASC, [Season] ASC),
    CONSTRAINT [FK_RelationshipRating_RatingType] FOREIGN KEY ([RatingTypeID]) REFERENCES [DayFiance].[RatingType] ([RatingTypeID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_RelationshipRating_RelationshipSeason] FOREIGN KEY ([Season], [RelationshipID]) REFERENCES [DayFiance].[RelationshipSeason] ([Season], [RelationshipID])
);

