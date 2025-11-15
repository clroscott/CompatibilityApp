CREATE TABLE [DayFiance].[PersonSeason] (
    [PersonID] INT      NOT NULL,
    [Season]   SMALLINT NOT NULL,
    CONSTRAINT [PK_PersonSeason] PRIMARY KEY CLUSTERED ([PersonID] ASC, [Season] ASC),
    CONSTRAINT [FK_PersonSeason_Person] FOREIGN KEY ([PersonID]) REFERENCES [DayFiance].[Person] ([PersonID]),
    CONSTRAINT [FK_PersonSeason_Season] FOREIGN KEY ([Season]) REFERENCES [DayFiance].[Season] ([SeasonNum]),
    CONSTRAINT [FK_PersonSeason_Season1] FOREIGN KEY ([Season]) REFERENCES [DayFiance].[Season] ([SeasonNum])
);

