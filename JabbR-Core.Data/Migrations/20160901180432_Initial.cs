using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JabbRCore.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatUsers",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AfkNote = table.Column<string>(maxLength: 200, nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Flag = table.Column<string>(maxLength: 255, nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    HashedPassword = table.Column<string>(nullable: true),
                    Id = table.Column<string>(maxLength: 200, nullable: false),
                    Identity = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    IsAfk = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    IsBanned = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    LastActivity = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastNudged = table.Column<DateTime>(type: "datetime", nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Note = table.Column<string>(maxLength: 200, nullable: true),
                    RawPreferences = table.Column<string>(nullable: true),
                    RequestPasswordResetId = table.Column<string>(nullable: true),
                    RequestPasswordResetValidThrough = table.Column<DateTimeOffset>(nullable: true),
                    Salt = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUsers", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "__MigrationHistory",
                columns: table => new
                {
                    MigrationId = table.Column<string>(maxLength: 150, nullable: false),
                    ContextKey = table.Column<string>(maxLength: 300, nullable: false),
                    Model = table.Column<byte[]>(nullable: false),
                    ProductVersion = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.__MigrationHistory", x => new { x.MigrationId, x.ContextKey });
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RawSettings = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "ChatClients",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Id = table.Column<string>(nullable: true),
                    LastActivity = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "'0001-01-01T00:00:00.000+00:00'"),
                    LastClientActivity = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "'0001-01-01T00:00:00.000+00:00'"),
                    Name = table.Column<string>(nullable: true),
                    UserAgent = table.Column<string>(nullable: true),
                    User_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatClients", x => x.Key);
                    table.ForeignKey(
                        name: "FK_ChatClients_ChatUsers_User_Key",
                        column: x => x.User_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Closed = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    Creator_Key = table.Column<int>(nullable: true),
                    InviteCode = table.Column<string>(type: "nchar(6)", nullable: true),
                    LastNudged = table.Column<DateTime>(type: "datetime", nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Private = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    Topic = table.Column<string>(maxLength: 80, nullable: true),
                    Welcome = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Key);
                    table.ForeignKey(
                        name: "FK_ChatRooms_ChatUsers_Creator_Key",
                        column: x => x.Creator_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatUserIdentities",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: true),
                    Identity = table.Column<string>(nullable: true),
                    ProviderName = table.Column<string>(nullable: true),
                    UserKey = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChatUserIdentities", x => x.Key);
                    table.ForeignKey(
                        name: "FK_ChatUserIdentities_ChatUsers_UserKey",
                        column: x => x.UserKey,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentType = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Id = table.Column<string>(nullable: true),
                    OwnerKey = table.Column<int>(nullable: false),
                    RoomKey = table.Column<int>(nullable: false),
                    Size = table.Column<long>(nullable: false, defaultValueSql: "0"),
                    Url = table.Column<string>(nullable: true),
                    When = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Attachments", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Attachments_ChatUsers_OwnerKey",
                        column: x => x.OwnerKey,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_ChatRooms_RoomKey",
                        column: x => x.RoomKey,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    HtmlContent = table.Column<string>(nullable: true),
                    HtmlEncoded = table.Column<bool>(nullable: false, defaultValueSql: "1"),
                    Id = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    MessageType = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    Room_Key = table.Column<int>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    User_Key = table.Column<int>(nullable: true),
                    When = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Key);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatRooms_Room_Key",
                        column: x => x.Room_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatUsers_User_Key",
                        column: x => x.User_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomChatUser1",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomChatUser1", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUser1_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUser1_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomChatUsers",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomChatUsers", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUsers_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUsers_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatUserChatRooms",
                columns: table => new
                {
                    ChatUser_Key = table.Column<int>(nullable: false),
                    ChatRoom_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUserChatRooms", x => new { x.ChatUser_Key, x.ChatRoom_Key });
                    table.ForeignKey(
                        name: "FK_ChatUserChatRooms_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatUserChatRooms_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MessageKey = table.Column<int>(nullable: false),
                    Read = table.Column<bool>(nullable: false),
                    RoomKey = table.Column<int>(nullable: false),
                    UserKey = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Notifications", x => x.Key);
                    table.ForeignKey(
                        name: "FK_dbo.Notifications_dbo.ChatMessages_MessageKey",
                        column: x => x.MessageKey,
                        principalTable: "ChatMessages",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.Notifications_dbo.ChatRooms_RoomKey",
                        column: x => x.RoomKey,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.Notifications_dbo.ChatUsers_UserKey",
                        column: x => x.UserKey,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerKey",
                table: "Attachments",
                column: "OwnerKey");

            migrationBuilder.CreateIndex(
                name: "IX_RoomKey",
                table: "Attachments",
                column: "RoomKey");

            migrationBuilder.CreateIndex(
                name: "IX_ChatClients_User_Key",
                table: "ChatClients",
                column: "User_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Room_Key",
                table: "ChatMessages",
                column: "Room_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_User_Key",
                table: "ChatMessages",
                column: "User_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomChatUser1_ChatUser_Key",
                table: "ChatRoomChatUser1",
                column: "ChatUser_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomChatUsers_ChatUser_Key",
                table: "ChatRoomChatUsers",
                column: "ChatUser_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_Creator_Key",
                table: "ChatRooms",
                column: "Creator_Key");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "ChatRooms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatUserChatRooms_ChatRoom_Key",
                table: "ChatUserChatRooms",
                column: "ChatRoom_Key");

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
                name: "IX_MessageKey",
                table: "Notifications",
                column: "MessageKey");

            migrationBuilder.CreateIndex(
                name: "IX_RoomKey",
                table: "Notifications",
                column: "RoomKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserKey",
                table: "Notifications",
                column: "UserKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ChatClients");

            migrationBuilder.DropTable(
                name: "ChatRoomChatUser1");

            migrationBuilder.DropTable(
                name: "ChatRoomChatUsers");

            migrationBuilder.DropTable(
                name: "ChatUserChatRooms");

            migrationBuilder.DropTable(
                name: "ChatUserIdentities");

            migrationBuilder.DropTable(
                name: "__MigrationHistory");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatRooms");

            migrationBuilder.DropTable(
                name: "ChatUsers");
        }
    }
}
