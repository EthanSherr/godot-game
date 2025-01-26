using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public partial class DamageDisplayManager : Node
{
    public static async Task ShowDamage(Vector2 position, int damage, Node parent)
    {
        // Label damageLabel = NodeUtils.CreateFromScene<Label>("res://scripts/ui/DamageLabel.tscn");
        Label damageLabel = new Label();
        damageLabel.GlobalPosition = position;
        damageLabel.Text = damage.ToString();
        damageLabel.ZIndex = 5;
        damageLabel.LabelSettings = new LabelSettings();

        damageLabel.LabelSettings.FontColor = new Color(1, 1, 1);
        damageLabel.LabelSettings.FontSize = 8;
        damageLabel.LabelSettings.OutlineColor = new Color(0, 0, 0);
        damageLabel.LabelSettings.OutlineSize = 1;

        damageLabel.Text = damage.ToString();
        damageLabel.GlobalPosition = position;
        damageLabel.Modulate = new Color(1, 1, 1);

        damageLabel.Set("theme_override_constants/pixel_snap", false);

        parent.GetTree().Root.CallDeferred("add_child", damageLabel);
        await damageLabel.ToSignal(damageLabel, "resized");

        var damageLabelSize = damageLabel.GetRect().Size;
        GD.Print("damage size", damageLabelSize);
        damageLabel.GlobalPosition -= new Vector2(damageLabelSize.X / 2, damageLabelSize.Y / 2);

        var actualPosition = damageLabel.GlobalPosition;

        var tween = damageLabel.CreateTween();
        tween.SetParallel(true);
        tween
            .TweenProperty(damageLabel, "position:y", actualPosition.Y - 5.0f, 0.35f)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(damageLabel, "modulate:a", 0f, 0.45f);
        // tween.TweenCallback(Callable.From(() => damageLabel.QueueFree()));
    }
}
