using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Xml.Linq;

public class MainHub : Hub
{

    private static readonly Dictionary<string, string> AuthorizedUsers = new Dictionary<string, string>();
    private static readonly Dictionary<string, List<string>> UserFriends = new Dictionary<string, List<string>>();
    private static readonly Dictionary<string, List<string>> UserChats = new Dictionary<string, List<string>>();
    private static readonly Dictionary<string, string> CurrentChats = new Dictionary<string, string>();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> Authorization(string username)
    {
        if (AuthorizedUsers.ContainsKey(username))
        {
            AuthorizedUsers[username] = Context.ConnectionId;
            return true;
        }
        else if (!string.IsNullOrEmpty(username))
        {
            AuthorizedUsers.Add(username, Context.ConnectionId);
            return true;
        }
        else return false;
    }

    public async Task<List<string>> GetFriends()
    {
        foreach (var pair in AuthorizedUsers)
        {
            if (pair.Value == Context.ConnectionId)
            {
                if (UserFriends.ContainsKey(pair.Key))
                    return UserFriends[pair.Key];
            }
        }
        return new List<string>();
    }

    public async Task InviteFriends(string friendName)
    {
        string connectionIdFriend = string.Empty;
        string userName = string.Empty;

        foreach (var pair in AuthorizedUsers)
        {
            if (pair.Value == Context.ConnectionId)
            {
                userName = pair.Key;
                break;
            }
        }

        foreach (var pair in AuthorizedUsers)
        {
            if (pair.Key == friendName)
            {
                connectionIdFriend = pair.Value;
                break;
            }
        }
        if (!string.IsNullOrEmpty(connectionIdFriend))
        {
            await Clients.Client(connectionIdFriend).SendAsync("FriendRequest", userName); //
        }
    }

    public async Task ProcessingFriendAdditions(string recipient, string sender/*, string keyInfo*/)
    {
        if (AuthorizedUsers.ContainsKey(sender))
        {
            //if (keyInfo == "N")
            //{
            //    await Clients.Client(AuthorizedUsers[sender]).SendAsync("ResponseFriendshipRequest", nameRecipient, keyInfo);
            //}
            //else if (keyInfo == "Y")
            //{

            if (!UserFriends.ContainsKey(sender))
            {
                UserFriends.Add(sender, new List<string>());
            }
            UserFriends[sender].Add(recipient);

            if (!UserFriends.ContainsKey(recipient))
            {
                UserFriends.Add(recipient, new List<string>());
            }
            UserFriends[recipient].Add(sender);

            if (!UserChats.ContainsKey(sender + recipient))
            {
                UserChats.Add(sender + recipient, new List<string>());
            }

            if (!UserChats.ContainsKey(recipient + sender))
            {
                UserChats.Add(recipient + sender, new List<string>());
            }

            //UsersChatHistory.Add(sender + recipient, new List<string>());

                //await Clients.Client(AuthorizedUsers[sender]).SendAsync("ResponseFriendshipRequest", nameRecipient, keyInfo);
            //}
        }
    }

    public async Task<bool> FindUser(string UserName)
    {
        return AuthorizedUsers.ContainsKey(UserName);
    }

    public async Task<bool> FindFriend(string friendName)
    {
        string userName = string.Empty;

        foreach (var pair in AuthorizedUsers)
        {
            if (pair.Value == Context.ConnectionId)
            {
                userName = pair.Key;
                break;
            }
        }
        if (!string.IsNullOrEmpty(userName))
        {
            if (UserFriends.ContainsKey(userName))
            {
                return UserFriends[userName].Contains(friendName);
            }
        }
        return false; //
    }

    public async Task JoinChat(string friendName)
    {
        string userName = string.Empty;

        foreach (var pair in AuthorizedUsers)
        {
            if (pair.Value == Context.ConnectionId)
            {
                userName = pair.Key;
                break;
            }
        }

        await Clients.Caller.SendAsync("printHistory", string.Join("\n", UserChats[userName + friendName]));
    }

    public async Task SendMessage(string sender, string recipient, string msg)
    {
        UserChats[sender + recipient].Add("Your message: " + msg);
        UserChats[recipient + sender].Add(sender + ": " + msg);

        string connectionIdFriend = string.Empty;

        foreach (var pair in AuthorizedUsers)
        {
            if (pair.Key == recipient)
            {
                connectionIdFriend = pair.Value;
                break;
            }
        }

        await Clients.Client(connectionIdFriend).SendAsync("printMessage", string.Join("\n", UserChats[recipient + sender]), sender, recipient);
    }

    public async Task EnterChat(string sender, string recipient)
    {
        CurrentChats.Add(sender, recipient);
    }
    public async Task LeaveChat(string sender, string recipient)
    {
        CurrentChats.Remove(sender);
    }
    public async Task<bool> CheckChat(string sender, string recipient)
    {
        if (CurrentChats.ContainsKey(sender))
        {
            return CurrentChats[sender] == recipient;
        }
        return false;
    }
}


//using Microsoft.AspNetCore.SignalR;
//using System.Text.RegularExpressions;

//namespace WebApplication1
//{
//    public class MainHub : Hub
//    {
//        private static readonly Dictionary<string, string> ConnectionsGroup = new Dictionary<string, string>();

//        public override async Task OnConnectedAsync()
//        {
//            await base.OnConnectedAsync();
//            await JoinGroup("groupId");
//        }

//        public override async Task OnDisconnectedAsync(Exception exception)
//        {
//            if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
//            {
//                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConnectionsGroup[Context.ConnectionId]);
//                ConnectionsGroup.Remove(Context.ConnectionId);
//            }
//            await base.OnDisconnectedAsync(exception);
//        }

//        public async Task JoinGroup(string group)
//        {
//            if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
//            {
//                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConnectionsGroup[Context.ConnectionId]);
//                ConnectionsGroup.Remove(Context.ConnectionId);
//            }
//            ConnectionsGroup.Add(Context.ConnectionId, group);
//            await Groups.AddToGroupAsync(Context.ConnectionId, group);
//        }

//        public async Task SendMessage(string message)
//        {
//            await Clients.Group("groupId")
//                         .SendAsync("onMessage", message);  
//        }
//    }
//}