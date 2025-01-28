using Godot;

public partial class MultiplayerManager : Node
{
    public static int SERVER_PORT = 8080;
    public static string SERVER_IP = "127.0.0.1";

    private static MultiplayerManager _instance;
    public static MultiplayerManager Instance => _instance;

    public override void _Ready()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            GD.PrintErr("Attempted to create a second instance of MultiplayerManager");
            QueueFree();
        }
    }

    public void BecomeHost()
    {
        var serverPeer = new ENetMultiplayerPeer();
        serverPeer.CreateServer(SERVER_PORT);
        GD.Print($"Server running on :{SERVER_PORT}");
        Multiplayer.MultiplayerPeer = serverPeer;
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    public void Join()
    {
        var clientPeer = new ENetMultiplayerPeer();
        clientPeer.CreateClient(SERVER_IP, SERVER_PORT);
        Multiplayer.MultiplayerPeer = clientPeer;
    }

    private void OnPeerConnected(long peerId)
    {
        GD.Print($"OnPeerConnected({peerId})");
        var player = Player.Create();
        player.PlayerId = peerId;
        player.Name = peerId.ToString();
    }

    private void OnPeerDisconnected(long peerId)
    {
        GD.Print($"OnPeerDisconnected({peerId})");
    }
}
