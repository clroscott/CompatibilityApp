CREATE TABLE [DayFiance].[RelationshipSeason] (
    [Season]         SMALLINT NOT NULL,
    [RelationshipID] INT      NOT NULL,
    CONSTRAINT [PK_RelationshipSeason] PRIMARY KEY CLUSTERED ([Season] ASC, [RelationshipID] ASC),
    CONSTRAINT [FK_RelationshipSeason_Relationship] FOREIGN KEY ([RelationshipID]) REFERENCES [DayFiance].[Relationship] ([RelationshipID]),
    CONSTRAINT [FK_RelationshipSeason_Season] FOREIGN KEY ([Season]) REFERENCES [DayFiance].[Season] ([SeasonNum])
);

