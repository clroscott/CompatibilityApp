CREATE VIEW [DayFiance].Relationship_view as

select

      person1.firstname [Person1FirstName]
      ,person1.LastName [Person1LastName]
      ,person2.FirstName [Person2FirstName]
      ,person2.LastName [Person2LastName]
      ,relationship.RelationshipType
  FROM [CompatibiltyDB].[DayFiance].[Relationship] relationship 
  join [CompatibiltyDB].[DayFiance].[Person] person1 on relationship.Person_id_1 = person1.PersonID
  join [CompatibiltyDB].[DayFiance].[Person] person2 on relationship.Person_id_2 = person2.PersonID


