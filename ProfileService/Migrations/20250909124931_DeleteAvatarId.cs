using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Migrations
{
    /// <inheritdoc />
    public partial class DeleteAvatarId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Avatars_AvatarId",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_AvatarId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Profiles");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileId",
                table: "Avatars",
                type: "character varying(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Avatars_ProfileId",
                table: "Avatars",
                column: "ProfileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Avatars_Profiles_ProfileId",
                table: "Avatars",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avatars_Profiles_ProfileId",
                table: "Avatars");

            migrationBuilder.DropIndex(
                name: "IX_Avatars_ProfileId",
                table: "Avatars");

            migrationBuilder.AddColumn<string>(
                name: "AvatarId",
                table: "Profiles",
                type: "character varying(195)",
                maxLength: 195,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileId",
                table: "Avatars",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_AvatarId",
                table: "Profiles",
                column: "AvatarId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Avatars_AvatarId",
                table: "Profiles",
                column: "AvatarId",
                principalTable: "Avatars",
                principalColumn: "Id");
        }
    }
}
