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
            migrationBuilder.AlterColumn<int>(
                name: "Triage_ID",
                table: "Triage_Parameters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "TriageId",
                table: "Triage_Parameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Triage_Parameters_TriageId",
                table: "Triage_Parameters",
                column: "TriageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Triage_Parameters_TriageId",
                table: "Triage_Parameters");

            migrationBuilder.DropColumn(
                name: "TriageId",
                table: "Triage_Parameters");

            migrationBuilder.AlterColumn<int>(
                name: "Triage_ID",
                table: "Triage_Parameters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
