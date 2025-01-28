using System;
using Godot;

public partial class Multiplayer : Node2D
{
    private Button Host;
    private Button Join;

    public override void _Ready()
    {
        var Host = GetNode<Button>("Control/Panel/VBoxContainer/Host");
        var Join = GetNode<Button>("Control/Panel/VBoxContainer/Join");

        Host.Pressed += OnHost;
        Join.Pressed += OnJoin;
        MultiplayerManager.Instance.Connect(
            nameof(MultiplayerManager.PeerConnected),
            new Callable(this, nameof(OnPeerConnected))
        );
    }

    public void OnPeerConnected(long peerId)
    {
        // LoadCharacterSelectionScene for the peer that just connected!
        RpcId((int)peerId, nameof(LoadCharacterSelectionScene));
    }

    private void OnHost()
    {
        MultiplayerManager.Instance.BecomeHost();
        LoadCharacterSelectionScene();
    }

    private void OnJoin()
    {
        MultiplayerManager.Instance.Join();
    }

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
    )]
    private void LoadCharacterSelectionScene()
    {
        var charSelectionPath = "res://levels/CharacterSelection.tscn";
        var charSelectionScene = NodeUtils.CreateFromScene<Node2D>(charSelectionPath);
        GetTree().Root.AddChild(charSelectionScene);
        this.Hide();
    }
}
