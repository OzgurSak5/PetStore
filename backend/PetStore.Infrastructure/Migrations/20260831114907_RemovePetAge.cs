using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePetAge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Pets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Pets",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
