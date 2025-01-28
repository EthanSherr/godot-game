using System;
using Godot;

public partial class CharacterSelection : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Multiplayer.GetUniqueId()", Multiplayer.GetUniqueId());
        //  if (MultiplayerManager.Instance.IsHost())
        // 	{
        // 			SpawnCharacter(1); // Host is always peer ID 1
        // 	}
        // 	else
        // 	{
        // 			SpawnCharacter(Multiplayer.GetUniqueId());
        // 	}
    }
}
