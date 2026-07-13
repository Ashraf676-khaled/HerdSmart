using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastrucre.Migrations
{
    /// <inheritdoc />
    public partial class addiscontagious : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsContagious",
                table: "HealthLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsContagious",
                table: "HealthLogs");
        }
    }
}
