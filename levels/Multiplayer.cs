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
    }

    private void OnHost()
    {
        MultiplayerManager.Instance.BecomeHost();
        LoadCharacterSelectionScene();
    }

    private void OnJoin()
    {
        GD.Print("on join");
        MultiplayerManager.Instance.Join();
    }

    // // [Remote]
    // private void RequestCharacterSelection()
    // {
    //     // Only the host should trigger scene loading for everyone
    //     if (MultiplayerManager.Instance.IsHost())
    //     {
    //         Rpc(nameof(LoadCharacterSelectionScene));
    //     }
    // }


    private void LoadCharacterSelectionScene()
    {
        GetTree().ChangeSceneToFile("res://levels/CharacterSelection.tscn");
    }
}
