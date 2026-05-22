using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTriageParametersIdentityMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE [Triage_Parameters_New] (
                    [Triage_ID] int NOT NULL IDENTITY,
                    [TriageId] int NOT NULL,
                    [Consciousness] int NOT NULL,
                    [Breathing] int NOT NULL,
                    [Bleeding] int NOT NULL,
                    [Injury_Type] int NOT NULL,
                    [Pain_Level] int NOT NULL,
                    CONSTRAINT [PK_Triage_Parameters_New] PRIMARY KEY ([Triage_ID])
                );

                INSERT INTO [Triage_Parameters_New] ([TriageId], [Consciousness], [Breathing], [Bleeding], [Injury_Type], [Pain_Level])
                SELECT [Triage_ID], [Consciousness], [Breathing], [Bleeding], [Injury_Type], [Pain_Level]
                FROM [Triage_Parameters];

                DROP TABLE [Triage_Parameters];

                EXEC sp_rename N'[Triage_Parameters_New]', N'Triage_Parameters';

                EXEC sp_rename N'[PK_Triage_Parameters_New]', N'PK_Triage_Parameters', N'OBJECT';

                CREATE UNIQUE INDEX [IX_Triage_Parameters_TriageId]
                ON [Triage_Parameters] ([TriageId]);

                ALTER TABLE [Triage_Parameters]
                ADD CONSTRAINT [FK_Triage_Parameters_Triage_TriageId]
                FOREIGN KEY ([TriageId]) REFERENCES [Triage] ([Triage_ID]) ON DELETE CASCADE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE [Triage_Parameters_Old] (
                    [Triage_ID] int NOT NULL,
                    [Consciousness] int NOT NULL,
                    [Breathing] int NOT NULL,
                    [Bleeding] int NOT NULL,
                    [Injury_Type] int NOT NULL,
                    [Pain_Level] int NOT NULL,
                    CONSTRAINT [PK_Triage_Parameters_Old] PRIMARY KEY ([Triage_ID])
                );

                INSERT INTO [Triage_Parameters_Old] ([Triage_ID], [Consciousness], [Breathing], [Bleeding], [Injury_Type], [Pain_Level])
                SELECT [TriageId], [Consciousness], [Breathing], [Bleeding], [Injury_Type], [Pain_Level]
                FROM [Triage_Parameters];

                DROP TABLE [Triage_Parameters];

                EXEC sp_rename N'[Triage_Parameters_Old]', N'Triage_Parameters';

                EXEC sp_rename N'[PK_Triage_Parameters_Old]', N'PK_Triage_Parameters', N'OBJECT';
                """);
        }
    }
}
