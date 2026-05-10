using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergy",
                columns: table => new
                {
                    AllergyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllergyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AllergyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AllergyCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergy", x => x.AllergyID);
                });

            migrationBuilder.CreateTable(
                name: "ER_Room",
                columns: table => new
                {
                    Room_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Room_Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Availability_Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Current_Visit_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ER_Room", x => x.Room_ID);
                });

            migrationBuilder.CreateTable(
                name: "ER_Visit",
                columns: table => new
                {
                    Visit_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Patient_ID = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Arrival_date_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Chief_Complaint = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ER_Visit", x => x.Visit_ID);
                });

            migrationBuilder.CreateTable(
                name: "Examination",
                columns: table => new
                {
                    Exam_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Visit_ID = table.Column<int>(type: "int", nullable: false),
                    Doctor_ID = table.Column<int>(type: "int", nullable: false),
                    Exam_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Room_ID = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examination", x => x.Exam_ID);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CNP = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfDeath = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmergencyContact = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    IsDonor = table.Column<bool>(type: "bit", nullable: false),
                    Transferred = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.PatientID);
                });

            migrationBuilder.CreateTable(
                name: "Transfer_Log",
                columns: table => new
                {
                    Transfer_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Visit_ID = table.Column<int>(type: "int", nullable: false),
                    Transfer_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Target_System = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfer_Log", x => x.Transfer_ID);
                });

            migrationBuilder.CreateTable(
                name: "Transplants",
                columns: table => new
                {
                    TransplantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiverID = table.Column<int>(type: "int", nullable: false),
                    DonorID = table.Column<int>(type: "int", nullable: true),
                    OrganType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransplantDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompatibilityScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transplants", x => x.TransplantID);
                });

            migrationBuilder.CreateTable(
                name: "Triage",
                columns: table => new
                {
                    Triage_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Visit_ID = table.Column<int>(type: "int", nullable: false),
                    Triage_Level = table.Column<int>(type: "int", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nurse_ID = table.Column<int>(type: "int", nullable: false),
                    Triage_Time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triage", x => x.Triage_ID);
                });

            migrationBuilder.CreateTable(
                name: "Triage_Parameters",
                columns: table => new
                {
                    Triage_ID = table.Column<int>(type: "int", nullable: false),
                    Consciousness = table.Column<int>(type: "int", nullable: false),
                    Breathing = table.Column<int>(type: "int", nullable: false),
                    Bleeding = table.Column<int>(type: "int", nullable: false),
                    Injury_Type = table.Column<int>(type: "int", nullable: false),
                    Pain_Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triage_Parameters", x => x.Triage_ID);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistory",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChronicConditions = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistory", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_MedicalHistory_Patient_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecord",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HistoryID = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceID = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    Symptoms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConsultationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountApplied = table.Column<int>(type: "int", nullable: true),
                    PoliceNotified = table.Column<bool>(type: "bit", nullable: false),
                    TransplantID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecord", x => x.RecordID);
                    table.ForeignKey(
                        name: "FK_MedicalRecord_MedicalHistory_HistoryID",
                        column: x => x.HistoryID,
                        principalTable: "MedicalHistory",
                        principalColumn: "HistoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientAllergies",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "int", nullable: false),
                    AllergyID = table.Column<int>(type: "int", nullable: false),
                    SeverityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAllergies", x => new { x.AllergyID, x.HistoryID });
                    table.ForeignKey(
                        name: "FK_PatientAllergies_Allergy_AllergyID",
                        column: x => x.AllergyID,
                        principalTable: "Allergy",
                        principalColumn: "AllergyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientAllergies_MedicalHistory_HistoryID",
                        column: x => x.HistoryID,
                        principalTable: "MedicalHistory",
                        principalColumn: "HistoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                columns: table => new
                {
                    PrescriptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    DoctorNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescription", x => x.PrescriptionID);
                    table.ForeignKey(
                        name: "FK_Prescription_MedicalRecord_RecordID",
                        column: x => x.RecordID,
                        principalTable: "MedicalRecord",
                        principalColumn: "RecordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    PrescrItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionID = table.Column<int>(type: "int", nullable: false),
                    MedName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Quantity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.PrescrItemID);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescription_PrescriptionID",
                        column: x => x.PrescriptionID,
                        principalTable: "Prescription",
                        principalColumn: "PrescriptionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistory_PatientID",
                table: "MedicalHistory",
                column: "PatientID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecord_HistoryID",
                table: "MedicalRecord",
                column: "HistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_CNP",
                table: "Patient",
                column: "CNP",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_HistoryID",
                table: "PatientAllergies",
                column: "HistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_RecordID",
                table: "Prescription",
                column: "RecordID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionID",
                table: "PrescriptionItems",
                column: "PrescriptionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ER_Room");

            migrationBuilder.DropTable(
                name: "ER_Visit");

            migrationBuilder.DropTable(
                name: "Examination");

            migrationBuilder.DropTable(
                name: "PatientAllergies");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "Transfer_Log");

            migrationBuilder.DropTable(
                name: "Transplants");

            migrationBuilder.DropTable(
                name: "Triage");

            migrationBuilder.DropTable(
                name: "Triage_Parameters");

            migrationBuilder.DropTable(
                name: "Allergy");

            migrationBuilder.DropTable(
                name: "Prescription");

            migrationBuilder.DropTable(
                name: "MedicalRecord");

            migrationBuilder.DropTable(
                name: "MedicalHistory");

            migrationBuilder.DropTable(
                name: "Patient");
        }
    }
}
