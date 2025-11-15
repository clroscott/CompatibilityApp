CREATE TABLE [DayFiance].[Season] (
    [SeasonNum]   SMALLINT NOT NULL,
    [AirDate]     DATETIME NOT NULL,
    [FilmingDate] DATETIME NOT NULL,
    CONSTRAINT [PK_Season] PRIMARY KEY CLUSTERED ([SeasonNum] ASC)
);

