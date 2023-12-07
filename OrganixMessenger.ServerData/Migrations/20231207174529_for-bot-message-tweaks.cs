using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganixMessenger.ServerData.Migrations
{
    /// <inheritdoc />
    public partial class forbotmessagetweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomProfilePictureId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomUsername",
                table: "Messages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CustomProfilePictureId",
                table: "Messages",
                column: "CustomProfilePictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Files_CustomProfilePictureId",
                table: "Messages",
                column: "CustomProfilePictureId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Files_CustomProfilePictureId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CustomProfilePictureId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CustomProfilePictureId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CustomUsername",
                table: "Messages");
        }
    }
}
