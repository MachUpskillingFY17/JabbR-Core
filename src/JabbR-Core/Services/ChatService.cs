﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JabbR_Core.Configuration;
using JabbR_Core.Models;
using Microsoft.AspNetCore.SignalR;
//using JabbR_Core.UploadHandlers;
//using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace JabbR_Core.Services
{
    public class ChatService : IChatService
    {
        private readonly IJabbrRepository _repository;
        private readonly ICache _cache;
        private readonly IRecentMessageCache _recentMessageCache;
        private readonly ApplicationSettings _settings;

        private const int NoteMaximumLength = 140;
        private const int TopicMaximumLength = 80;
        private const int WelcomeMaximumLength = 200;

        // To migrate from previous versions of Jabbr
        private static readonly IDictionary<string, string> LegacyCountryConversion = new Dictionary<string, string>
                                                                                          {
                                                                                              {"g1", "england"},
                                                                                              {"g2", "wales"},
                                                                                              {"g3", "scotland"}
                                                            };

        // Iso reference: http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2
        private static readonly IDictionary<string, string> Countries = new Dictionary<string, string>
                                                                            {
                                                                                {"ad", "Andorra"},
                                                                                {"ae", "United Arab Emirates"},
                                                                                {"af", "Afghanistan"},
                                                                                {"ag", "Antigua and Barbuda"},
                                                                                {"ai", "Anguilla"},
                                                                                {"al", "Albania"},
                                                                                {"am", "Armenia"},
                                                                                {"ao", "Angola"},
                                                                                {"aq", "Antarctica"},
                                                                                {"ar", "Argentina"},
                                                                                {"as", "American Samoa"},
                                                                                {"at", "Austria"},
                                                                                {"au", "Australia"},
                                                                                {"aw", "Aruba"},
                                                                                {"ax", "Åland Islands"},
                                                                                {"az", "Azerbaijan"},
                                                                                {"ba", "Bosnia and Herzegovina"},
                                                                                {"bb", "Barbados"},
                                                                                {"bd", "Bangladesh"},
                                                                                {"be", "Belgium"},
                                                                                {"bf", "Burkina Faso"},
                                                                                {"bg", "Bulgaria"},
                                                                                {"bh", "Bahrain"},
                                                                                {"bi", "Burundi"},
                                                                                {"bj", "Benin"},
                                                                                {"bl", "Saint Barthélemy"},
                                                                                {"bm", "Bermuda"},
                                                                                {"bn", "Brunei Darussalam"},
                                                                                {"bo", "Bolivia"},
                                                                                {"bq", "Bonaire, Sint Eustatius and Saba"},
                                                                                {"br", "Brazil"},
                                                                                {"bs", "Bahamas"},
                                                                                {"bt", "Bhutan"},
                                                                                {"bv", "Bouvet Island"},
                                                                                {"bw", "Botswana"},
                                                                                {"by", "Belarus"},
                                                                                {"bz", "Belize"},
                                                                                {"ca", "Canada"},
                                                                                {"cc", "Cocos (Keeling) Islands"},
                                                                                {"cd", "Congo, the Democratic Republic of the"},
                                                                                {"cf", "Central African Republic"},
                                                                                {"cg", "Congo"},
                                                                                {"ch", "Switzerland"},
                                                                                {"ci", "Côte d'Ivoire"},
                                                                                {"ck", "Cook Islands"},
                                                                                {"cl", "Chile"},
                                                                                {"cm", "Cameroon"},
                                                                                {"cn", "China"},
                                                                                {"co", "Colombia"},
                                                                                {"cr", "Costa Rica"},
                                                                                {"cu", "Cuba"},
                                                                                {"cv", "Cape Verde"},
                                                                                {"cw", "Curaçao"},
                                                                                {"cx", "Christmas Island"},
                                                                                {"cy", "Cyprus"},
                                                                                {"cz", "Czech Republic"},
                                                                                {"de", "Germany"},
                                                                                {"dj", "Djibouti"},
                                                                                {"dk", "Denmark"},
                                                                                {"dm", "Dominica"},
                                                                                {"do", "Dominican Republic"},
                                                                                {"dz", "Algeria"},
                                                                                {"ec", "Ecuador"},
                                                                                {"ee", "Estonia"},
                                                                                {"eg", "Egypt"},
                                                                                {"eh", "Western Sahara"},
                                                                                {"er", "Eritrea"},
                                                                                {"es", "Spain"},
                                                                                {"et", "Ethiopia"},
                                                                                {"fi", "Finland"},
                                                                                {"fj", "Fiji"},
                                                                                {"fk", "Falkland Islands (Malvinas)"},
                                                                                {"fm", "Micronesia, Federated States of"},
                                                                                {"fo", "Faroe Islands"},
                                                                                {"fr", "France"},
                                                                                {"ga", "Gabon"},
                                                                                {"gb", "United Kingdom"},
                                                                                {"gd", "Grenada"},
                                                                                {"ge", "Georgia"},
                                                                                {"gf", "French Guiana"},
                                                                                {"gg", "Guernsey"},
                                                                                {"gh", "Ghana"},
                                                                                {"gi", "Gibraltar"},
                                                                                {"gl", "Greenland"},
                                                                                {"gm", "Gambia"},
                                                                                {"gn", "Guinea"},
                                                                                {"gp", "Guadeloupe"},
                                                                                {"gq", "Equatorial Guinea"},
                                                                                {"gr", "Greece"},
                                                                                {"gs", "South Georgia and the South Sandwich Islands"},
                                                                                {"gt", "Guatemala"},
                                                                                {"gu", "Guam"},
                                                                                {"gw", "Guinea-Bissau"},
                                                                                {"gy", "Guyana"},
                                                                                {"hk", "Hong Kong"},
                                                                                {"hm", "Heard Island and McDonald Islands"},
                                                                                {"hn", "Honduras"},
                                                                                {"hr", "Croatia"},
                                                                                {"ht", "Haiti"},
                                                                                {"hu", "Hungary"},
                                                                                {"ic", "Canary Islands"},
                                                                                {"id", "Indonesia"},
                                                                                {"ie", "Ireland"},
                                                                                {"il", "Israel"},
                                                                                {"im", "Isle of Man"},
                                                                                {"in", "India"},
                                                                                {"io", "British Indian Ocean Territory"},
                                                                                {"iq", "Iraq"},
                                                                                {"ir", "Iran, Islamic Republic of"},
                                                                                {"is", "Iceland"},
                                                                                {"it", "Italy"},
                                                                                {"je", "Jersey"},
                                                                                {"jm", "Jamaica"},
                                                                                {"jo", "Jordan"},
                                                                                {"jp", "Japan"},
                                                                                {"ke", "Kenya"},
                                                                                {"kg", "Kyrgyzstan"},
                                                                                {"kh", "Cambodia"},
                                                                                {"ki", "Kiribati"},
                                                                                {"km", "Comoros"},
                                                                                {"kn", "Saint Kitts and Nevis"},
                                                                                {"kp", "Korea, Democratic People's Republic of"},
                                                                                {"kr", "Korea, Republic of"},
                                                                                {"kw", "Kuwait"},
                                                                                {"ky", "Cayman Islands"},
                                                                                {"kz", "Kazakhstan"},
                                                                                {"la", "Lao People's Democratic Republic"},
                                                                                {"lb", "Lebanon"},
                                                                                {"lc", "Saint Lucia"},
                                                                                {"li", "Liechtenstein"},
                                                                                {"lk", "Sri Lanka"},
                                                                                {"lr", "Liberia"},
                                                                                {"ls", "Lesotho"},
                                                                                {"lt", "Lithuania"},
                                                                                {"lu", "Luxembourg"},
                                                                                {"lv", "Latvia"},
                                                                                {"ly", "Libya"},
                                                                                {"ma", "Morocco"},
                                                                                {"mc", "Monaco"},
                                                                                {"md", "Moldova, Republic of"},
                                                                                {"me", "Montenegro"},
                                                                                {"mf", "Saint Martin (French part)"},
                                                                                {"mg", "Madagascar"},
                                                                                {"mh", "Marshall Islands"},
                                                                                {"mk", "Macedonia, the former Yugoslav Republic of"},
                                                                                {"ml", "Mali"},
                                                                                {"mm", "Myanmar"},
                                                                                {"mn", "Mongolia"},
                                                                                {"mo", "Macao"},
                                                                                {"mp", "Northern Mariana Islands"},
                                                                                {"mq", "Martinique"},
                                                                                {"mr", "Mauritania"},
                                                                                {"ms", "Montserrat"},
                                                                                {"mt", "Malta"},
                                                                                {"mu", "Mauritius"},
                                                                                {"mv", "Maldives"},
                                                                                {"mw", "Malawi"},
                                                                                {"mx", "Mexico"},
                                                                                {"my", "Malaysia"},
                                                                                {"mz", "Mozambique"},
                                                                                {"na", "Namibia"},
                                                                                {"nc", "New Caledonia"},
                                                                                {"ne", "Niger"},
                                                                                {"nf", "Norfolk Island"},
                                                                                {"ng", "Nigeria"},
                                                                                {"ni", "Nicaragua"},
                                                                                {"nl", "Netherlands"},
                                                                                {"no", "Norway"},
                                                                                {"np", "Nepal"},
                                                                                {"nr", "Nauru"},
                                                                                {"nu", "Niue"},
                                                                                {"nz", "New Zealand"},
                                                                                {"om", "Oman"},
                                                                                {"pa", "Panama"},
                                                                                {"pe", "Peru"},
                                                                                {"pf", "French Polynesia"},
                                                                                {"pg", "Papua New Guinea"},
                                                                                {"ph", "Philippines"},
                                                                                {"pk", "Pakistan"},
                                                                                {"pl", "Poland"},
                                                                                {"pm", "Saint Pierre and Miquelon"},
                                                                                {"pn", "Pitcairn"},
                                                                                {"pr", "Puerto Rico"},
                                                                                {"ps", "Palestinian Territory, Occupied"},
                                                                                {"pt", "Portugal"},
                                                                                {"pw", "Palau"},
                                                                                {"py", "Paraguay"},
                                                                                {"qa", "Qatar"},
                                                                                {"re", "Réunion"},
                                                                                {"ro", "Romania"},
                                                                                {"rs", "Serbia"},
                                                                                {"ru", "Russian Federation"},
                                                                                {"rw", "Rwanda"},
                                                                                {"sa", "Saudi Arabia"},
                                                                                {"sb", "Solomon Islands"},
                                                                                {"sc", "Seychelles"},
                                                                                {"sd", "Seychelles"},
                                                                                {"se", "Sweden"},
                                                                                {"sg", "Singapore"},
                                                                                {"sh", "Saint Helena, Ascension and Tristan da Cunha"},
                                                                                {"si", "Slovenia"},
                                                                                {"sj", "Svalbard and Jan Mayen"},
                                                                                {"sk", "Slovakia"},
                                                                                {"sl", "Sierra Leone"},
                                                                                {"sm", "San Marino"},
                                                                                {"sn", "Senegal"},
                                                                                {"so", "Somalia"},
                                                                                {"sr", "Suriname"},
                                                                                {"ss", "South Sudan"},
                                                                                {"st", "Sao Tome and Principe"},
                                                                                {"sv", "El Salvador"},
                                                                                {"sx", "Sint Maarten (Dutch part)"},
                                                                                {"sy", "Syrian Arab Republic"},
                                                                                {"sz", "Swaziland"},
                                                                                {"tc", "Turks and Caicos Islands"},
                                                                                {"td", "Chad"},
                                                                                {"tf", "French Southern Territories"},
                                                                                {"tg", "Togo"},
                                                                                {"th", "Thailand"},
                                                                                {"tj", "Tajikistan"},
                                                                                {"tk", "Tokelau"},
                                                                                {"tl", "Timor-Leste"},
                                                                                {"tm", "Turkmenistan"},
                                                                                {"tn", "Tunisia"},
                                                                                {"to", "Tonga"},
                                                                                {"tr", "Turkey"},
                                                                                {"tt", "Trinidad and Tobago"},
                                                                                {"tv", "Tuvalu"},
                                                                                {"tw", "Taiwan, Province of China"},
                                                                                {"tz", "Tanzania, United Republic of"},
                                                                                {"ua", "Ukraine"},
                                                                                {"ug", "Uganda"},
                                                                                {"um", "United States Minor Outlying Islands"},
                                                                                {"us", "United States"},
                                                                                {"uy", "Uruguay"},
                                                                                {"uz", "Uzbekistan"},
                                                                                {"va", "Holy See (Vatican City State)"},
                                                                                {"vc", "Saint Vincent and the Grenadines"},
                                                                                {"ve", "Venezuela, Bolivarian Republic of"},
                                                                                {"vg", "Virgin Islands, British"},
                                                                                {"vi", "Virgin Islands, U.S."},
                                                                                {"vn", "Viet Nam"},
                                                                                {"vu", "Vanuatu"},
                                                                                {"wf", "Wallis and Futuna"},
                                                                                {"ws", "Samoa"},
                                                                                {"ye", "Yemen"},
                                                                                {"yt", "Mayotte"},
                                                                                {"za", "South Africa"},
                                                                                {"zm", "Zambia"},
                                                                                {"zw", "Zimbabwe"},
                                                                                /* Not country codes */
                                                                                {"england", "England"},
                                                                                {"wales", "Wales"},
                                                                                {"scotland", "Scotland"},
                                                                                {"kurdistan","Kurdistan"},
                                                                                {"somaliland","Somaliland"},
                                                                                {"zanzibar","Zanzibar"}
                                                  };

        public ChatService(ICache cache, IRecentMessageCache recentMessageCache, IJabbrRepository repository)
            : this(cache,
                   recentMessageCache,
                   repository,
                   ApplicationSettings.GetDefaultSettings())
        {
        }

        public ChatService(ICache cache,
                           IRecentMessageCache recentMessageCache,
                           IJabbrRepository repository,
                           ApplicationSettings settings)
        {
            _cache = cache;
            _recentMessageCache = recentMessageCache;
            _repository = repository;
            _settings = settings;
        }
        
        //Added to have empty constructor to get openroom to work
        //TODO: implement
        //Delete after cache/repository set up
        public ChatService()
        {
        }

        public ChatRoom AddRoom(ChatUser user, string name)
        {
            if (!_settings.AllowRoomCreation && !user.IsAdmin)
            {
                throw new HubException(LanguageResources.RoomCreationDisabled);
            }

            if (name.Equals("Lobby", StringComparison.OrdinalIgnoreCase))
            {
                throw new HubException(LanguageResources.RoomCannotBeNamedLobby);
            }

            if (!IsValidRoomName(name))
            {
                throw new HubException(String.Format(LanguageResources.RoomInvalidName, name));
            }

            var room = new ChatRoom
            {
                Name = name,
                Creator = user
            };

            room.Owners.Add(user);

            _repository.Add(room);

            user.OwnedRooms.Add(room);

            return room;
        }

        public void JoinRoom(ChatUser user, ChatRoom room, string inviteCode)
        {
            // Throw if the room is private but the user isn't allowed
            if (room.Private)
            {
                // First, check if the invite code is correct
                if (!String.IsNullOrEmpty(inviteCode) && String.Equals(inviteCode, room.InviteCode, StringComparison.OrdinalIgnoreCase))
                {
                    // It is, add the user to the allowed users so that future joins will work
                    room.AllowedUsers.Add(user);
                }
                if (!room.IsUserAllowed(user))
                {
                    throw new HubException(String.Format(LanguageResources.Join_LockedAccessPermission, room.Name));
                }
            }

            
            // Add this user to the room

            //REMOVE _openroom
            var _repository = new InMemoryRepository();
            _repository.AddUserRoom(user, room);

            ChatUserPreferences userPreferences = user.Preferences;
            userPreferences.TabOrder.Add(room.Name);
            user.Preferences = userPreferences;

            // Clear the cache
            
            //REMOVE _openroom
            //_cache.RemoveUserInRoom(user, room);
        }

        public void SetInviteCode(ChatUser user, ChatRoom room, string inviteCode)
        {
            EnsureOwnerOrAdmin(user, room);
            if (!room.Private)
            {
                throw new HubException(LanguageResources.InviteCode_PrivateRoomRequired);
            }

            // Set the invite code and save
            room.InviteCode = inviteCode;
            _repository.CommitChanges();
        }

        public void UpdateActivity(ChatUser user, string clientId, string userAgent)
        {
            user.Status = (int)UserStatus.Active;
            user.LastActivity = DateTime.UtcNow;

            ChatClient client = AddClient(user, clientId, userAgent);
            client.UserAgent = userAgent;
            client.LastActivity = DateTimeOffset.UtcNow;
            client.LastClientActivity = DateTimeOffset.UtcNow;

            // Remove any Afk notes.
            if (user.IsAfk)
            {
                user.AfkNote = null;
                user.IsAfk = false;
            }
        }

        public void LeaveRoom(ChatUser user, ChatRoom room)
        {
            // Update the cache
            _cache.RemoveUserInRoom(user, room);

            // Remove the user from this room
            _repository.RemoveUserRoom(user, room);

            ChatUserPreferences userPreferences = user.Preferences;
            userPreferences.TabOrder.Remove(room.Name);
            user.Preferences = userPreferences;

            _repository.CommitChanges();
        }

        //public void AddAttachment(ChatMessage message, string fileName, string contentType, long size, UploadResult result)
        //{
        //    var attachment = new Attachment
        //    {
        //        Id = result.Identifier,
        //        Url = result.Url,
        //        FileName = fileName,
        //        ContentType = contentType,
        //        Size = size,
        //        Room = message.Room,
        //        Owner = message.User,
        //        When = DateTimeOffset.UtcNow
        //    };

        //    _repository.Add(attachment);
        //}

        public ChatMessage AddMessage(ChatUser user, ChatRoom room, string id, string content)
        {
            var chatMessage = new ChatMessage
            {
                Id = id,
                User = user,
                Content = content,
                When = DateTimeOffset.UtcNow,
                Room = room,
                HtmlEncoded = false
            };

            _recentMessageCache.Add(chatMessage);

            _repository.Add(chatMessage);

            return chatMessage;
        }

        public ChatMessage AddMessage(string userId, string roomName, string content)
        {
            ChatUser user = _repository.VerifyUserId(userId);
            ChatRoom room = _repository.VerifyUserRoom(_cache, user, roomName);

            // REVIEW: Is it better to use room.EnsureOpen() here?
            if (room.Closed)
            {
                throw new HubException(String.Format(LanguageResources.SendMessageRoomClosed, roomName));
            }

            var message = AddMessage(user, room, Guid.NewGuid().ToString("d"), content);

            _repository.CommitChanges();

            return message;
        }

        public void AddNotification(ChatUser mentionedUser, ChatMessage message, ChatRoom room, bool markAsRead)
        {
            // We need to use the key here since messages might be a new entity
            var notification = new Notification
            {
                User = mentionedUser,
                Message = message,
                Read = markAsRead,
                Room = room
            };

            _repository.Add(notification);
        }

        public void AppendMessage(string id, string content)
        {
            ChatMessage message = _repository.GetMessageById(id);

            message.Content += content;

            _repository.CommitChanges();
        }

        public void AddOwner(ChatUser ownerOrCreator, ChatUser targetUser, ChatRoom targetRoom)
        {
            // Ensure the user is owner of the target room
            EnsureOwnerOrAdmin(ownerOrCreator, targetRoom);

            if (targetRoom.Owners.Contains(targetUser))
            {
                // If the target user is already an owner, then throw
                throw new HubException(String.Format(LanguageResources.RoomUserAlreadyOwner, targetUser.Name, targetRoom.Name));
            }

            // Make the user an owner
            targetRoom.Owners.Add(targetUser);
            targetUser.OwnedRooms.Add(targetRoom);

            if (targetRoom.Private)
            {
                if (!targetRoom.AllowedUsers.Contains(targetUser))
                {
                    // If the room is private make this user allowed
                    targetRoom.AllowedUsers.Add(targetUser);
                    targetUser.AllowedRooms.Add(targetRoom);
                }
            }
        }

        public void RemoveOwner(ChatUser creator, ChatUser targetUser, ChatRoom targetRoom)
        {
            // must be admin OR creator
            EnsureCreatorOrAdmin(creator, targetRoom);

            // ensure acting user is owner
            EnsureOwnerOrAdmin(creator, targetRoom);

            if (!targetRoom.Owners.Contains(targetUser))
            {
                // If the target user is not an owner, then throw
                throw new HubException(String.Format(LanguageResources.UserNotRoomOwner, targetUser.Name, targetRoom.Name));
            }

            // Remove user as owner of room
            targetRoom.Owners.Remove(targetUser);
            targetUser.OwnedRooms.Remove(targetRoom);
        }

        public void KickUser(ChatUser callingUser, ChatUser targetUser, ChatRoom targetRoom)
        {
            EnsureOwnerOrAdmin(callingUser, targetRoom);

            if (targetUser == callingUser)
            {
                throw new HubException(LanguageResources.Kick_CannotKickSelf);
            }

            if (!_repository.IsUserInRoom(_cache, targetUser, targetRoom))
            {
                throw new HubException(String.Format(LanguageResources.UserNotInRoom, targetUser.Name, targetRoom.Name));
            }

            // only admin can kick admin
            if (!callingUser.IsAdmin && targetUser.IsAdmin)
            {
                throw new HubException(LanguageResources.Kick_AdminRequiredToKickAdmin);
            }

            // If this user isn't the creator/admin AND the target user is an owner then throw
            if (targetRoom.Creator != callingUser && targetRoom.Owners.Contains(targetUser) && !callingUser.IsAdmin)
            {
                throw new HubException(LanguageResources.Kick_CreatorRequiredToKickOwner);
            }

            LeaveRoom(targetUser, targetRoom);
        }

        public ChatClient AddClient(ChatUser user, string clientId, string userAgent)
        {
            //ChatClient client = _repository.GetClientById(clientId);
            var client = new ChatClient();
            if (client != null)
            {
                return client;
            }

            client = new ChatClient
            {
                Id = clientId,
                User = user,
                UserAgent = userAgent,
                LastActivity = DateTimeOffset.UtcNow,
                LastClientActivity = user.LastActivity
            };

            _repository.Add(client);
            _repository.CommitChanges();

            return client;
        }

        public string DisconnectClient(string clientId)
        {
            // Remove this client from the list of user's clients
            ChatClient client = _repository.GetClientById(clientId, includeUser: true);

            // No client tracking this user
            if (client == null)
            {
                return null;
            }

            // Get the user for this client
            ChatUser user = client.User;

            if (user != null)
            {
                user.ConnectedClients.Remove(client);

                if (!user.ConnectedClients.Any())
                {
                    // If no more clients mark the user as offline
                    user.Status = (int)UserStatus.Offline;
                }

                _repository.Remove(client);
                _repository.CommitChanges();
            }

            return user.Id;
        }

        internal static string NormalizeRoomName(string roomName)
        {
            return roomName.StartsWith("#") ? roomName.Substring(1) : roomName;
        }

        private static bool IsValidRoomName(string name)
        {
            return !String.IsNullOrEmpty(name) && Regex.IsMatch(name, "^[\\w-_]{1,30}$");
        }

        private static void EnsureAdmin(ChatUser user)
        {
            if (!user.IsAdmin)
            {
                throw new HubException(LanguageResources.AdminRequired);
            }
        }

        private static void EnsureOwnerOrAdmin(ChatUser user, ChatRoom room)
        {
            if (!room.Owners.Contains(user) && !user.IsAdmin)
            {
                throw new HubException(String.Format(LanguageResources.RoomOwnerRequired, room.Name));
            }
        }

        private static void EnsureOwner(ChatUser user, ChatRoom room)
        {
            if (!room.Owners.Contains(user))
            {
                throw new HubException(String.Format(LanguageResources.RoomOwnerRequired, room.Name));
            }
        }

        private static void EnsureCreator(ChatUser user, ChatRoom room)
        {
            if (user != room.Creator)
            {
                throw new HubException(String.Format(LanguageResources.RoomCreatorRequired, room.Name));
            }
        }

        private static void EnsureCreatorOrAdmin(ChatUser user, ChatRoom room)
        {
            if (user != room.Creator && !user.IsAdmin)
            {
                throw new HubException(String.Format(LanguageResources.RoomCreatorRequired, room.Name));
            }
        }

        public void AllowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            EnsureOwnerOrAdmin(user, targetRoom);

            if (!targetRoom.Private)
            {
                throw new HubException(String.Format(LanguageResources.RoomNotPrivate, targetRoom.Name));
            }

            if (targetUser.AllowedRooms.Contains(targetRoom))
            {
                throw new HubException(String.Format(LanguageResources.RoomUserAlreadyAllowed, targetUser.Name, targetRoom.Name));
            }

            targetRoom.AllowedUsers.Add(targetUser);
            targetUser.AllowedRooms.Add(targetRoom);

            _repository.CommitChanges();
        }

        public void UnallowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            EnsureOwnerOrAdmin(user, targetRoom);

            if (targetUser == user)
            {
                throw new HubException(LanguageResources.UnAllow_CannotUnallowSelf);
            }

            if (!targetRoom.Private)
            {
                throw new HubException(String.Format(LanguageResources.RoomNotPrivate, targetRoom.Name));
            }

            if (!targetUser.AllowedRooms.Contains(targetRoom))
            {
                throw new HubException(String.Format(LanguageResources.RoomAccessPermissionUser, targetUser.Name, targetRoom.Name));
            }

            // only admin can unallow admin
            if (!user.IsAdmin && targetUser.IsAdmin)
            {
                throw new HubException(LanguageResources.UnAllow_AdminRequired);
            }

            // If this user isn't the creator and the target user is an owner then throw
            if (targetRoom.Creator != user && targetRoom.Owners.Contains(targetUser) && !user.IsAdmin)
            {
                throw new HubException(LanguageResources.UnAllow_CreatorRequiredToUnallowOwner);
            }

            targetRoom.AllowedUsers.Remove(targetUser);
            targetUser.AllowedRooms.Remove(targetRoom);

            // Make the user leave the room
            LeaveRoom(targetUser, targetRoom);

            _repository.CommitChanges();
        }

        public void LockRoom(ChatUser user, ChatRoom targetRoom)
        {
            EnsureOwnerOrAdmin(user, targetRoom);

            if (targetRoom.Private)
            {
                throw new HubException(String.Format(LanguageResources.RoomAlreadyLocked, targetRoom.Name));
            }

            // Make the room private
            targetRoom.Private = true;

            // Add the creator to the allowed list
            targetRoom.AllowedUsers.Add(user);

            // Add the room to the users' list
            user.AllowedRooms.Add(targetRoom);

            // Make all users in the current room allowed
            foreach (var u in targetRoom.Users.Online())
            {
                u.AllowedRooms.Add(targetRoom);
                targetRoom.AllowedUsers.Add(u);
            }

            _repository.CommitChanges();
        }

        public void CloseRoom(ChatUser user, ChatRoom targetRoom)
        {
            EnsureOwnerOrAdmin(user, targetRoom);

            if (targetRoom.Closed)
            {
                throw new HubException(String.Format(LanguageResources.RoomAlreadyClosed, targetRoom.Name));
            }

            // Make the room closed.
            targetRoom.Closed = true;

            _repository.CommitChanges();
        }

        public void OpenRoom(ChatUser user, ChatRoom targetRoom)
        {
            EnsureOwnerOrAdmin(user, targetRoom);

            if (!targetRoom.Closed)
            {
                throw new HubException(String.Format(LanguageResources.RoomAlreadyOpen, targetRoom.Name));
            }

            // Open the room
            targetRoom.Closed = false;
            _repository.CommitChanges();
        }

        public void ChangeTopic(ChatUser user, ChatRoom room, string newTopic)
        {
            EnsureOwnerOrAdmin(user, room);
            room.Topic = newTopic;
            _repository.CommitChanges();
        }

        public void ChangeWelcome(ChatUser user, ChatRoom room, string newWelcome)
        {
            EnsureOwnerOrAdmin(user, room);
            room.Welcome = newWelcome;
            _repository.CommitChanges();
        }

        public void AddAdmin(ChatUser admin, ChatUser targetUser)
        {
            // Ensure the user is admin
            EnsureAdmin(admin);

            if (targetUser.IsAdmin)
            {
                // If the target user is already an admin, then throw
                throw new HubException(String.Format(LanguageResources.UserAlreadyAdmin, targetUser.Name));
            }

            // Make the user an admin
            targetUser.IsAdmin = true;
            _repository.CommitChanges();
        }

        public void RemoveAdmin(ChatUser admin, ChatUser targetUser)
        {
            // Ensure the user is admin
            EnsureAdmin(admin);

            if (!targetUser.IsAdmin)
            {
                // If the target user is NOT an admin, then throw
                throw new HubException(String.Format(LanguageResources.UserNotAdmin, targetUser.Name));
            }

            // Make the user an admin
            targetUser.IsAdmin = false;
            _repository.CommitChanges();
        }

        public void BanUser(ChatUser admin, ChatUser targetUser)
        {
            EnsureAdmin(admin);

            if (targetUser == admin)
            {
                throw new HubException(LanguageResources.Ban_CannotBanSelf);
            }

            if (targetUser.IsAdmin)
            {
                throw new HubException(LanguageResources.Ban_CannotBanAdmin);
            }

            targetUser.IsBanned = true;

            _repository.CommitChanges();
        }

        public void UnbanUser(ChatUser admin, ChatUser targetUser)
        {
            // Ensure the user is admin
            EnsureAdmin(admin);

            if (targetUser.IsAdmin)
            {
                // If the target user is an admin, then throw
                throw new HubException(LanguageResources.Unban_CannotUnbanAdmin);
            }

            //Unban the user
            targetUser.IsBanned = false;

            _repository.CommitChanges();
        }

        internal static void ValidateNote(string note, string noteTypeName = "note", int? maxLength = null)
        {
            var lengthToValidateFor = (maxLength ?? NoteMaximumLength);
            if (!String.IsNullOrWhiteSpace(note) &&
                note.Length > lengthToValidateFor)
            {
                throw new HubException(
                    String.Format(LanguageResources.NoteTooLong,
                        lengthToValidateFor, noteTypeName));
            }
        }

        internal static void ValidateTopic(string topic)
        {
            ValidateNote(topic, noteTypeName: "topic", maxLength: TopicMaximumLength);
        }

        internal static void ValidateWelcome(string message)
        {
            ValidateNote(message, noteTypeName: "welcome", maxLength: WelcomeMaximumLength);
        }

        internal static void ValidateIsoCode(string isoCode)
        {
            string country = GetCountry(isoCode);
            if (String.IsNullOrWhiteSpace(country))
            {
                throw new HubException(
                    LanguageResources.CountryISOInvalid);
            }
        }

        internal static string GetCountry(string isoCode)
        {
            if (String.IsNullOrEmpty(isoCode))
            {
                return null;
            }

            string country;
            if (Countries.TryGetValue(isoCode, out country))
                return country;

            string newIsoCode;
            if (LegacyCountryConversion.TryGetValue(isoCode, out newIsoCode))
            {
                Countries.TryGetValue(newIsoCode, out country);
            }

            return country;
        }

        internal static string GetUserRoomPresence(ChatUser user, ChatRoom room)
        {
            return user.Rooms.Contains(room) ? "present" : "absent";
        }
    }
}
