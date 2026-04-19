using DiscordRPC;
using DiscordRPC.Logging;
using System;

namespace RecoilHelper.Core;

public class DiscordUserArgs : EventArgs
{
    public string Name { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}

public class DiscordRpcManager : IDisposable
{
    private DiscordRpcClient? _client;
    public event EventHandler<DiscordUserArgs>? UserReady;
    public DiscordUserArgs? LastUser { get; private set; }

    // Client ID for "Republic Mabar" from Discord Developer Portal
    private const string ClientId = "1486665318355964005"; 

    public void Initialize()
    {
        _client = new DiscordRpcClient(ClientId);
        _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

        _client.OnReady += (sender, e) =>
        {
            var user = e.User;
            var avatarUrl = user.GetAvatarURL(User.AvatarFormat.PNG);
            
            LastUser = new DiscordUserArgs 
            { 
                Name = user.Username, 
                AvatarUrl = avatarUrl 
            };

            UserReady?.Invoke(this, LastUser);
        };

        _client.OnPresenceUpdate += (sender, e) => { /* Optional */ };

        _client.Initialize();

        // Set a default presence
        UpdatePresence("Menjelajahi Menu", "Join discord.gg/k4Gue7NRzD");
    }

    public void UpdatePresence(string details, string state)
    {
        if (_client == null || !_client.IsInitialized) return;

        _client.SetPresence(new RichPresence()
        {
            Details = details,
            State = state,
            Assets = new Assets()
            {
                LargeImageKey = "cover_image",
                LargeImageText = "Republic Mabar",
                SmallImageKey = "logo"
            },
            Buttons = new DiscordRPC.Button[]
            {
                new DiscordRPC.Button() 
                { 
                    Label = "Join Community", 
                    Url = "https://discord.gg/k4Gue7NRzD" 
                }
            }
        });
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
