using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Minimal.Services.api.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusCodeToAiInteractionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "AiInteractionLogs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "AiInteractionLogs");
        }
    }
}
