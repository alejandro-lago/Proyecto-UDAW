using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TFG_Proyect.Migrations
{
    /// <inheritdoc />
    public partial class FixRoutineAndAutoUpdateCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Routines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Routines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }
    }
}
