using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganixMessenger.ServerData.Migrations
{
    /// <inheritdoc />
    public partial class userprofilepictureandlastonline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastOnline",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePictureId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfilePictureId",
                table: "Users",
                column: "ProfilePictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Files_ProfilePictureId",
                table: "Users",
                column: "ProfilePictureId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Files_ProfilePictureId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProfilePictureId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastOnline",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePictureId",
                table: "Users");
        }
    }
}
