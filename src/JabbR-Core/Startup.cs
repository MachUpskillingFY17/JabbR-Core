using System;
using JabbR_Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JabbR_Core
{
    public class Startup
    {           
        // Temporary, likely will add this to Localization/ClientResourceManager.cs when logic is implemented
        private const string _clientResources = @"'Chat_YouBannedReason': 'You have been banned by \x7b0\x7d for \x22\x7b1\x7d\x22.','Chat_UserKickedFromRoomReason': '\x7b0\x7d was kicked from \x7b1\x7d by \x7b2\x7d for \x22\x7b3\x7d\x22.','Chat_UserAdminAllowed': '\x7b0\x7d is now an admin.','Chat_RoomOwnersResults': 'The following users are owners of \x7b0\x7d\x3a','Client_Confirm': 'Confirm','Chat_UserPerformsAction': '\x2a\x7b0\x7d \x7b1\x7d','Client_NoUsersInList': 'No users','Chat_CollapseHiddenMessages': '\x28click to collapse\x29','Chat_UserSetRoomTopic': '\x7b0\x7d has set the room topic to \x22\x7b1\x7d\x22.','RoomAccessPermissionUser': '\x7b0\x7d isn\x27t allowed to access \x7b1\x7d.','Client_ShortcutSpecificTab': 'Go to specific Tab.','Chat_UserBannedReason': '\x7b0\x7d was banned by \x7b1\x7d for \x22\x7b2\x7d\x22.','Client_OccupantsOne': '1 occupant','Client_JabbrErrorMessage': 'There was an error contacting the server, please refresh in a few minutes.','Chat_YouClearedRoomWelcome': 'You have cleared the room welcome.','Chat_UserGrantedRoomAccess': '\x7b0\x7d now has access to \x7b1\x7d.','Client_DateRangeLastHour': 'Last hour','Client_DateRangeLastWeek': 'Last week','RoomAlreadyOpen': '\x7b0\x7d is already open.','Chat_YouKickedFromRoom': 'You were kicked from \x7b0\x7d by \x7b1\x7d.','Chat_YouInvitedUserToRoom': 'Invitation to \x7b0\x7d to join \x23\x7b1\x7d has been sent.','Chat_YouAdminAllowed': 'You are now an admin.','Chat_RoomNowLocked': '\x7b0\x7d is now locked.','Client_JabbrSettings': 'JabbR Settings','Chat_YouClearedFlag': 'You have cleared your flag.','RoomAlreadyLocked': '\x7b0\x7d is already locked.','Client_DownloadMessages': 'Download Messages','Chat_CannotSendLobby': 'You cannot send messages within the lobby.','Client_Disconnected': 'The connection to JabbR has been lost, trying to reconnect.','Client_LoadMissingMessages': 'Load missing messages','Client_PopupNotifications': 'Popup Notifications','Client_Upload': 'Upload','Chat_UserIsAfkNote': '\x7b0\x7d has gone AFK, with the message \x22\x7b1\x7d\x22.','Chat_RoomNotPrivateAllowed': 'Anyone is allowed in \x7b0\x7d as it is not private.','Client_ShortcutLobby': 'Go to the Lobby.','Chat_YouKickedFromRoomReason': 'You were kicked from \x7b0\x7d by \x7b1\x7d for \x22\x7b2\x7d\x22.','Client_UploadNoPreview': 'No preview available for this file type.','Chat_ExpandHiddenMessages': '\x28plus \x7b0\x7d hidden... click to expand\x29','Content_HeaderAndToggle': '\x7b0\x7d \x28click to show\x2fhide\x29','Chat_UserKickedFromRoom': '\x7b0\x7d was kicked from \x7b1\x7d by \x7b2\x7d.','Chat_YouEnteredRoom': 'You just entered \x7b0\x7d.','Chat_RoomUsersEmpty': 'Room is empty.','Chat_RoomNowClosed': '\x7b0\x7d is now closed.','Chat_YourRoomAccessRevoked': 'Your access to \x7b0\x7d has been revoked.','LoadingMessage': 'Loading...','Chat_YouAreAfkNote': 'You have gone AFK, with the message \x22\x7b0\x7d\x22.','Chat_UserClearedRoomTopic': '\x7b0\x7d has cleared the room topic.','RoomOwnerRequired': 'You are not an owner of \x7b0\x7d.','RoomMemberButNotExists': 'You\x27re in \x7b0\x7d, but it doesn\x27t exist.','RoomAccessPermission': 'You do not have access to \x7b0\x7d.','Client_Uploading': 'Uploading \x7b0\x7d.','Client_UploadingFromClipboard': 'Uploading from clipboard','Chat_UserNoteCleared': '\x7b0\x7d has cleared their note.','Client_LoadMore': 'Load more...','Create_CommandInfo': 'Create a room with the given name.','RoomAlreadyClosed': '\x7b0\x7d is already closed.','Chat_RoomNowOpen': '\x7b0\x7d is now open.','Client_ShortcutTabs': 'Go to the next open room tab or Go to the previous open room tab.','RoomCannotBeNamedLobby': 'Lobby is not a valid chat room name.','Chat_UserEnteredRoom': '\x7b0\x7d just entered \x7b1\x7d.','Chat_DefaultTopic': 'You\x27re chatting in \x7b0\x7d.','Client_YourPrivateRooms': 'Your Private Rooms','Client_Reconnecting': 'The connection to JabbR has been temporarily lost, trying to reconnect.','Client_SiteWideShortcuts': 'Site-wide shortcuts','Chat_UserGrantedRoomOwnership': '\x7b0\x7d is now an owner of \x7b1\x7d.','Client_UserCommands': 'User commands','Chat_YouSetFlag': 'You have set your flag to \x7b0\x7d.','Client_Help': 'JabbR Help','Client_Send': 'Send','Chat_UserOwnsRooms': '\x7b0\x7d owns the following rooms\x3a','Client_AudibleNotifications': 'Audible Notifications','Chat_YouAreAfk': 'You have gone AFK.','Chat_YouBanned': 'You have been banned by \x7b0\x7d.','Client_OtherRooms': 'Other Rooms','Client_DisplayHelp': 'Display Help','Client_OwnerTag': 'owner','Chat_YouBannedTitle': 'Banned','Chat_UserLeftRoom': '\x7b0\x7d left \x7b1\x7d.','Client_DeploymentInfo': 'Deployed from \x3ca target\x3d\x22_blank\x22 href\x3d\x22https\x3a\x2f\x2fgithub.com\x2fJabbR\x2fJabbR\x2fcommit\x2f\x7b0\x7d\x22 title\x3d\x22View the commit\x22\x3e\x7b1\x7d\x3c\x2fa\x3e on \x3ca target\x3d\x22_blank\x22 href\x3d\x22https\x3a\x2f\x2fgithub.com\x2fJabbR\x2fJabbR\x2fbranches\x2f\x7b2\x7d\x22 title\x3d\x22View the branch\x22\x3e\x7b2\x7d\x3c\x2fa\x3e at \x7b3\x7d.','Chat_YouSetRoomTopic': 'You have set the room topic to \x22\x7b0\x7d\x22.','Chat_YourGravatarChanged': 'Your gravatar has been set.','Chat_YourNameChanged': 'Your name is now \x7b0\x7d.','RoomJoinMessage': 'Use \x27\x2fjoin room\x27 to join a room.','Chat_UserIsAfk': '\x7b0\x7d has gone AFK.','RoomUserAlreadyAllowed': '\x7b0\x7d is already allowed into \x7b1\x7d.','Client_UploadDropTarget': 'Drop files here','Client_ToggleRichContent': 'Toggle Rich Content','Chat_YourRoomOwnershipRevoked': 'You are no longer an owner of \x7b0\x7d.','Chat_RoomSearchResults': 'The following users match your search\x3a','Chat_UserInvitedYouToRoom': '\x7b0\x7d has invited you to \x23\x7b1\x7d. Click the room name to join.','Client_DownloadMessagesNotOpen': '\x7b0\x7d is not an open room, messages cannot be downloaded.','Client_NoMatchingRooms': 'No matching rooms','RoomNotFound': 'Unable to find \x7b0\x7d.','Client_RoomCommands': 'Room commands','RoomInvalidNameSpaces': 'Room names cannot contain spaces.','Client_LoadingPreviousMessages': 'Loading previous messages...','Chat_RoomOwnersEmpty': 'No users are owners of \x7b0\x7d.','Chat_UserBanned': '\x7b0\x7d was banned by \x7b1\x7d.','Client_OccupantsMany': '\x7b0\x7d occupants','Client_OccupantsZero': 'Unoccupied','Client_Close': 'Close','Chat_UserNotInRooms': '\x7b0\x7d \x28Currently \x7b1\x7d\x29 is not in any rooms.','Client_Rooms': 'Rooms','Client_Lobby': 'Lobby','RoomClosed': '\x7b0\x7d is closed.','Chat_UserNudgedUser': '\x2a\x7b0\x7d nudged \x7b1\x7d.','Chat_UserNudgedRoom': '\x2a\x7b0\x7d nudged room \x7b1\x7d.','Client_RoomFilterInstruction': 'Start typing to filter room list...','Chat_UserGravatarChanged': '\x7b0\x7d\x27s gravatar changed.','Chat_UserClearedFlag': '\x7b0\x7d has cleared their flag.','Client_ConnectedStatus': 'Status\x3a Connected','Chat_UserOwnsNoRooms': '\x7b0\x7d does not own any rooms.','RoomExistsButClosed': '\x7b0\x7d already exists, but it\x27s closed.','Client_RefreshRequiredHeader': 'JabbR Update','Chat_UserNoteSet': '\x7b0\x7d has set their note to \x22\x7b1\x7d\x22.','Chat_RoomUsersHeader': 'Users in \x7b0\x7d','Client_CreatorTag': 'creator','Chat_UserInRooms': '\x7b0\x7d \x28Currently \x7b1\x7d\x29 is in the following rooms\x3a','Chat_UserHeader': 'Users','RoomExists': '\x7b0\x7d already exists.','Chat_YourNoteSet': 'Your note has been set to \x22\x7b0\x7d\x22.','RoomUserAlreadyOwner': '\x7b0\x7d is already an owner of \x7b1\x7d.','RoomNotPrivate': '\x7b0\x7d is not a private room.','Client_Connected': 'You\x27re connected.','Chat_YouAdminRevoked': 'You are no longer an admin.','Client_RoomSettings': 'Room Settings','Client_FAQ': 'FAQ','Chat_YouKickedTitle': 'Kicked','Client_Notifications': 'Notifications','Client_RefreshRequiredNotification': 'Refresh your browser to get the latest version.  Auto update will take place in 15 seconds.','Chat_YouRevokedUserRoomAccess': 'You have revoked \x7b0\x7d\x27s access to \x7b1\x7d.','Chat_UserNudgedYou': '\x2a\x7b0\x7d nudged you.','Chat_UserOwnerHeader': 'Room Owners','Chat_YouGrantedRoomAccess': 'You have been granted access to \x7b0\x7d.','RoomNameCannotContainSpaces': 'Room name cannot contain spaces\x21','Client_SignOut': 'Sign Out','Chat_InitialMessages': 'Welcome to JabbR\x0d\x0aUse \x3f or type \x2f\x3f to display the FAQ and list of commands.','Client_DateRangeLastDay': 'Last day','RoomCreatorRequired': 'You are not the creator of \x7b0\x7d.','Client_FAQMessage': '\x3cp\x3eClick on a user to send message.\x3c\x2fp\x3e\x0d\x0a\x3cp\x3eType \x23roomname to create a link to a room.\x3c\x2fp\x3e\x0d\x0a\x3cp\x3eUse \x23test for testing.\x3c\x2fp\x3e\x0d\x0a\x3cp\x3eTo use a command, enter it into the chat message textbox.\x3c\x2fp\x3e','Chat_UserAdminRevoked': '\x7b0\x7d is no longer an admin.','Chat_AdminBroadcast': 'ADMIN\x3a \x7b0\x7d','Chat_RoomSearchEmpty': 'No users matched your search.','Chat_UserNameChanged': '\x7b0\x7d\x27s name has changed to \x7b1\x7d.','Chat_UserLockedRoom': '\x7b0\x7d has locked \x7b1\x7d.','Chat_YouGrantedRoomOwnership': 'You are now an owner of \x7b0\x7d.','Client_OnlineTag': 'online','Chat_UserRoomOwnershipRevoked': '\x7b0\x7d is no longer an owner of \x7b1\x7d.','RoomInvalidName': '\x7b0\x7d is not a valid room name.','Client_AdminTag': 'admin','Chat_PrivateMessage': '\x2a\x7b0\x7d\x2a \x2a\x7b1\x7d\x2a \x7b2\x7d','Chat_RoomPrivateNoUsersAllowed': 'No users are allowed in \x7b0\x7d.','Client_DownloadMessagesDateRange': 'Select date range for messages\x3a','Chat_YourNoteCleared': 'Your note has been cleared.','Client_Transport': 'Transport\x3a \x7b0\x7d','Chat_YouClearedRoomTopic': 'You have cleared the room topic.','Client_AccountSettings': 'Account Settings','Client_ShowClosedRooms': 'Show Closed Rooms\x3f','Chat_RoomPrivateUsersAllowedResults': 'The following users are allowed in \x7b0\x7d\x3a','Content_PasteHeaderAndToggle': 'Paste \x28click to show\x2fhide\x29','Chat_YouSetRoomWelcome': 'You have set the room welcome to \x22\x7b0\x7d\x22.','RoomCreationDisabled': 'Room creation is disabled.','RoomNameCannotBeBlank': 'Room name cannot be blank\x21','Client_Download': 'Download','Client_Cancel': 'Cancel','Client_DateRangeAll': 'All','Chat_UserSetFlag': '\x7b0\x7d has set their flag to \x7b1\x7d.','Content_DisabledMessage': 'Content collapsed because you have rich content disabled.','Client_SiteWideCommands': 'Site-wide commands','RoomRequired': 'No room specified.','Client_JabbrError': 'Jabbr Error','Client_DateRangeLastMonth': 'Last month','RoomNotMember': 'You\x27re not in \x7b0\x7d. Use \x27\x2fjoin \x7b0\x7d\x27 to join it.'";

        private IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if(env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            
            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Use `dotnet user-secrets set key value` to save as an env variable
            // on your machine.
            //
            // Store the connection string using the CLI tool. Include your actual username and password
            // >dotnet user-secrets set "connectionString" "Server=MYAPPNAME.database.windows.net,1433;Initial Catalog=MYCATALOG;Persist Security Info=False;User ID={plaintext user};Password={plaintext pass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
            // 
            // Reference the Configuration API with the key you defined, and your env variable will be referenced.
            // var connectionString = Configuration["connectionString"];
            // services.AddDbContext<MyContext>(options => options.UseSqlServer(connectionString));

            services.AddMvc();
            services.AddSignalR();

            // Establish default settings from appsettings.json
            services.Configure<ApplicationSettings>(_configuration.GetSection("ApplicationSettings"));

            // Programmatically add other options that cannot be taken from static strings
            services.Configure<ApplicationSettings>(settings => 
            {
                settings.Version = Version.Parse("0.1");
                settings.Time = DateTimeOffset.UtcNow.ToString();
                settings.ClientLanguageResources = _clientResources;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseStaticFiles();
            app.UseSignalR();
        }
    }
}
