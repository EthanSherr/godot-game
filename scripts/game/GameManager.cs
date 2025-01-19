using Godot;

public partial class GameManager : Node
{
    private Player currentCharacter;

    public static GameManager Instance =>
        NodeUtils.CreateFromScene<GameManager>("res://scripts/game/GameManager.tscn");

    public void PossessCharacter(Player character)
    {
        character.Camera.MakeCurrent();
        currentCharacter = character;
    }
}
