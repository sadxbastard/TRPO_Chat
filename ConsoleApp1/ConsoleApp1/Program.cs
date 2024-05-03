using Microsoft.AspNetCore.SignalR.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var hubConnectionBuilder = new HubConnectionBuilder();

        var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:5193/hub")
                                                .Build();

        await hubConnection.StartAsync();

        hubConnection.On("printHistory", (string history) =>
        {
            Console.Clear();
            Console.WriteLine(history);
        });

        hubConnection.On("printMessage", async (string msg, string userName, string friendName) =>
        {
            if (await hubConnection.InvokeAsync<bool>("CheckChat", friendName, userName))
            {
                Console.Clear();
                Console.WriteLine(msg);
            }
        });

        hubConnection.On("ResponseFriendshipRequest", (string nameRecipient, ConsoleKeyInfo keyInfo) =>
        {
            if (keyInfo.Key == ConsoleKey.N)
            {
                //
            }
            else if (keyInfo.Key == ConsoleKey.Y)
            {
                //
            }
        });
        
        Console.Write("Please enter your username: ");
        var name = Console.ReadLine();

        hubConnection.On("FriendRequest", async (string sender) =>
        {
            //Console.WriteLine($"You have a friend request from {sender}\nClick \"Y\" to accept the request, or \"N\" to reject");
            //ConsoleKeyInfo keyInfo;
            //var input = string.Empty;
            //while (true)
            //{
            //    input = Console.ReadLine();
            //    if (input == "Y" || input == "N")
            //        break;
            //}

            //Console.WriteLine($"{sender} has been added to your friends");

            //await hubConnection.InvokeAsync("ProcessingFriendAdditions", name, sender, input);
            await hubConnection.InvokeAsync("ProcessingFriendAdditions", name, sender);
        });

        while (true)
        {   
            if (await hubConnection.InvokeAsync<bool>("Authorization", name))
            {
                break;
            }
            else
            {
                Console.Clear();
                Console.WriteLine("The username cannot be empty\n");
                Console.WriteLine("Please enter your username:");
                name = Console.ReadLine();
            }
        }

        bool inChat = false;

        var inputName = string.Empty;

        Console.Clear();

        while (true)
        {

            if (!inChat)
            {
                Console.WriteLine($"User: {name}\n");

                var friends = await hubConnection.InvokeAsync<List<string>>("GetFriends");

                if (friends.Count > 0)
                {
                    Console.WriteLine("Your friends:");
                    for (int i = 0; i < friends.Count; i++)
                    {
                        Console.WriteLine($"{friends[i]}\n");
                    }
                    Console.Write("\nWrite a friend's name to write to him or press ENTER to add a new one\n\nInput: ");
                }
                else
                {
                    Console.Write("You don't have any friends yet(\n\nPress ENTER to add a friend..");
                }

                inputName = Console.ReadLine();

                if (string.IsNullOrEmpty(inputName))
                {
                    Console.Clear();

                    Console.WriteLine($"User: {name}\n");

                    Console.Write("Write the username of a new friend: ");

                    inputName = Console.ReadLine();

                    if (await hubConnection.InvokeAsync<bool>("FindUser", inputName))
                    {
                        if (!await hubConnection.InvokeAsync<bool>("FindFriend", inputName))
                        {
                            await hubConnection.InvokeAsync("InviteFriends", inputName);
                            Console.WriteLine($"\n{inputName}'s friend request has been sent!\n\nPress ENTER to exit the menu..");
                        }
                        else Console.WriteLine($"A user named {inputName} is already your friend\n\nPress ENTER to exit the menu.");
                    }
                    else
                    {
                        Console.Write($"\nThere is no user named {inputName}\n\nPress ENTER to exit the menu..");
                    }

                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        keyInfo = Console.ReadKey(true);
                    }
                    while (keyInfo.Key != ConsoleKey.Enter);

                    Console.Clear();
                    continue;
                }
                else
                {
                    Console.Clear();

                    Console.WriteLine($"User: {name}");

                    var joinResult = await hubConnection.InvokeAsync<bool>("FindFriend", inputName);

                    if (joinResult)
                    {
                        inChat = true;
                        await hubConnection.InvokeAsync("EnterChat", name, inputName);
                    }
                    else
                    {
                        Console.Write($"\nYou have entered {inputName}\n\nSuch a user is not on your friends list\n\n" +
                                      $"Click ENTER to exit..");

                        ConsoleKeyInfo keyInfo;
                        do
                        {
                            keyInfo = Console.ReadKey(true);
                        }
                        while (keyInfo.Key != ConsoleKey.Enter);
                        Console.Clear();
                        continue;
                    }

                }
            }
            else
            {
                Console.WriteLine($"Chat with the user: {inputName}\n");
                await hubConnection.InvokeAsync("JoinChat", inputName);
                var inputMessage = Console.ReadLine();
                if (string.IsNullOrEmpty(inputMessage))
                {
                    inChat = false;
                    await hubConnection.InvokeAsync("LeaveChat", name, inputName);
                    Console.Clear();
                }
                else
                {
                    await hubConnection.InvokeAsync("SendMessage", name, inputName, inputMessage); //
                }
            }
        }

    }
}

//using Microsoft.AspNetCore.SignalR.Client;

//namespace ConsoleApp1
//{
//    internal class Program
//    {
//        static async Task Main(string[] args)
//        {
//            var hubConnectionBuilder = new HubConnectionBuilder();

//            var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:5193/hub")
//                                                    .Build();

//            await hubConnection.StartAsync();

//            hubConnection.On("onMessage", (string x) =>
//            {
//                Console.WriteLine(x);
//            });

//            while (true)
//            {
//                var line = Console.ReadLine();

//                if (string.IsNullOrEmpty(line))
//                    break;

//                await hubConnection.InvokeAsync("SendMessage", line);
//            }   
//        }
//    }
//}