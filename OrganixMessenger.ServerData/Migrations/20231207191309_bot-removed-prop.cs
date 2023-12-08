using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganixMessenger.ServerData.Migrations
{
    /// <inheritdoc />
    public partial class botremovedprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "MessengerEntities",
                newName: "Name");

            migrationBuilder.AddColumn<bool>(
                name: "Removed",
                table: "Bots",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Removed",
                table: "Bots");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MessengerEntities",
                newName: "Username");
        }
    }
}
