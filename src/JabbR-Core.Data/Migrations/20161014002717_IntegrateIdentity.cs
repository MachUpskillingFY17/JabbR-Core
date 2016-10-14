using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JabbRCore.Data.Migrations
{
    public partial class IntegrateIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatPrivateRoomUsers");

            migrationBuilder.DropTable(
                name: "ChatRoomOwners");

            migrationBuilder.DropTable(
                name: "ChatRoomUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_ChatUsers_OwnerKey",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatClients_ChatUsers_User_Key",
                table: "ChatClients");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatUsers_User_Key",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_ChatUsers_CreatorKey",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatUserIdentities_ChatUsers_UserKey",
                table: "ChatUserIdentities");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Notifications_dbo.ChatUsers_UserKey",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_UserKey",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_UserKey",
                table: "ChatUserIdentities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers");

            migrationBuilder.DropIndex(
                name: "IX_Id",
                table: "ChatUsers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_CreatorKey",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_OwnerKey",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_User_Key",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatClients_User_Key",
                table: "ChatClients");

            migrationBuilder.DropColumn(
                name: "UserKey",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserKey",
                table: "ChatUserIdentities");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "CreatorKey",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "OwnerKey",
                table: "Attachments");

            migrationBuilder.RenameTable(
                name: "ChatUsers",
                newName: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Salt",
                table: "AspNetUsers",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "HashedPassword",
                table: "AspNetUsers",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ChatUserIdentities",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "ChatRooms",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "User_Key",
                table: "ChatMessages",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "User_Key",
                table: "ChatClients",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Attachments",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Id",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomUsers",
                columns: table => new
                {
                    ChatUser_Key = table.Column<int>(nullable: false),
                    ChatRoom_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                table.PrimaryKey("PK_ChatRoomUsers", x => new { x.ChatUser_Key, x.ChatRoom_Key });
                table.ForeignKey(
                    name: "FK_ChatRoomUsers_ChatRooms_ChatRoom_Key",
                    column: x => x.ChatRoom_Key,
                    principalTable: "ChatRooms",
                    principalColumn: "Key",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ChatRoomUsers_AspNetUsers_ChatUser_Key",
                    column: x => x.ChatUser_Key,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatPrivateRoomUsers",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPrivateRoomUsers", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_ChatPrivateRoomUsers_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatPrivateRoomUsers_AspNetUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomOwners",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomOwners", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_ChatRoomOwners_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomOwners_AspNetUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserKey",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserKey",
                table: "ChatUserIdentities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_User_Key",
                table: "ChatMessages",
                column: "User_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatClients_User_Key",
                table: "ChatClients",
                column: "User_Key");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatorId",
                table: "ChatRooms",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerKey",
                table: "Attachments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Attachments_dbo.AspNetUsers_OwnerKey",
                table: "Attachments",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatClients_AspNetUsers_User_Key",
                table: "ChatClients",
                column: "User_Key",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_User_Key",
                table: "ChatMessages",
                column: "User_Key",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_AspNetUsers_CreatorId",
                table: "ChatRooms",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ChatUserIdentities_dbo.AspNetUsers_UserKey",
                table: "ChatUserIdentities",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Notifications_dbo.AspNetUsers_UserKey",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Attachments_dbo.AspNetUsers_OwnerKey",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatClients_AspNetUsers_User_Key",
                table: "ChatClients");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_User_Key",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatPrivateRoomUsers_AspNetUsers_ChatUser_Key",
                table: "ChatPrivateRoomUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_AspNetUsers_CreatorId",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomOwners_AspNetUsers_ChatUser_Key",
                table: "ChatRoomOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomUsers_AspNetUsers_ChatUser_Key",
                table: "ChatRoomUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ChatUserIdentities_dbo.AspNetUsers_UserKey",
                table: "ChatUserIdentities");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Notifications_dbo.AspNetUsers_UserKey",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserKey",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_UserKey",
                table: "ChatUserIdentities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Id",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_CreatorId",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_OwnerKey",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ChatUserIdentities");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Attachments");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "ChatUsers");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "ChatUsers",
                newName: "Salt");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "ChatUsers",
                newName: "HashedPassword");

            migrationBuilder.AddColumn<int>(
                name: "UserKey",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserKey",
                table: "ChatUserIdentities",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ChatUsers",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ChatUsers",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Key",
                table: "ChatUsers",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ChatUser_Key",
                table: "ChatRoomUsers",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "ChatUser_Key",
                table: "ChatRoomOwners",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "CreatorKey",
                table: "ChatRooms",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChatUser_Key",
                table: "ChatPrivateRoomUsers",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "User_Key",
                table: "ChatMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "User_Key",
                table: "ChatClients",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerKey",
                table: "Attachments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_UserKey",
                table: "Notifications",
                column: "UserKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserKey",
                table: "ChatUserIdentities",
                column: "UserKey");

            migrationBuilder.CreateIndex(
                name: "IX_Id",
                table: "ChatUsers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatorKey",
                table: "ChatRooms",
                column: "CreatorKey");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerKey",
                table: "Attachments",
                column: "OwnerKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_ChatUsers_OwnerKey",
                table: "Attachments",
                column: "OwnerKey",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatClients_ChatUsers_User_Key",
                table: "ChatClients",
                column: "User_Key",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatUsers_User_Key",
                table: "ChatMessages",
                column: "User_Key",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatPrivateRoomUsers_ChatUsers_ChatUser_Key",
                table: "ChatPrivateRoomUsers",
                column: "ChatUser_Key",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_ChatUsers_CreatorKey",
                table: "ChatRooms",
                column: "CreatorKey",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomOwners_ChatUsers_ChatUser_Key",
                table: "ChatRoomOwners",
                column: "ChatUser_Key",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomUsers_ChatUsers_ChatUser_Key",
                table: "ChatRoomUsers",
                column: "ChatUser_Key",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUserIdentities_ChatUsers_UserKey",
                table: "ChatUserIdentities",
                column: "UserKey",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Notifications_dbo.ChatUsers_UserKey",
                table: "Notifications",
                column: "UserKey",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
