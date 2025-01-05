using System;
using System.Collections.Generic;
using Godot;

public partial class Generation : Node2D
{
    public override void _Ready()
    {
        RoomGenerator generator = new RoomGenerator();
        AddChild(generator);
    }
}
