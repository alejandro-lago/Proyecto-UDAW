using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TFG_Proyect.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutineDaysJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routines_DayOfWeeks_DayOfWeekId",
                table: "Routines");

            migrationBuilder.DropIndex(
                name: "IX_Routines_DayOfWeekId",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "DayOfWeekId",
                table: "Routines");

            migrationBuilder.CreateTable(
                name: "RoutineDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoutineId = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeekId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutineDays_DayOfWeeks_DayOfWeekId",
                        column: x => x.DayOfWeekId,
                        principalTable: "DayOfWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoutineDays_Routines_RoutineId",
                        column: x => x.RoutineId,
                        principalTable: "Routines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoutineDays_DayOfWeekId",
                table: "RoutineDays",
                column: "DayOfWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineDays_RoutineId_DayOfWeekId",
                table: "RoutineDays",
                columns: new[] { "RoutineId", "DayOfWeekId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoutineDays");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeekId",
                table: "Routines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routines_DayOfWeekId",
                table: "Routines",
                column: "DayOfWeekId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routines_DayOfWeeks_DayOfWeekId",
                table: "Routines",
                column: "DayOfWeekId",
                principalTable: "DayOfWeeks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
