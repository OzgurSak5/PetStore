using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetReservedUntil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedUntil",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedUntil",
                table: "Pets");
        }
    }
}
