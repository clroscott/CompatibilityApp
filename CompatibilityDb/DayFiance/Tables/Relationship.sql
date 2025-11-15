CREATE TABLE [DayFiance].[Relationship] (
    [RelationshipID]   INT           IDENTITY (1, 1) NOT NULL,
    [Person_id_1]      INT           NOT NULL,
    [Person_id_2]      INT           NOT NULL,
    [RelationshipType] VARCHAR (50)  NOT NULL,
    [ImagePath]        VARCHAR (200) NOT NULL,
    CONSTRAINT [PK_Relationship_1] PRIMARY KEY CLUSTERED ([RelationshipID] ASC),
    CONSTRAINT [FK_Relationship_Person_1] FOREIGN KEY ([Person_id_1]) REFERENCES [DayFiance].[Person] ([PersonID]),
    CONSTRAINT [FK_Relationship_Person_2] FOREIGN KEY ([Person_id_2]) REFERENCES [DayFiance].[Person] ([PersonID])
);

