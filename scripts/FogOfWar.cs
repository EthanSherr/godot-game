using System;
using Godot;

public partial class FogOfWar : Sprite2D
{
    [Export]
    public Vector2I TextureSize = new Vector2I(1024, 1024);

    [Export]
    public Color FogColor = new Color(0, 0, 0, 1);

    private Image fogImage;
    private ImageTexture fogTexture;

    [Export]
    private bool enabled = true;

    public override void _Ready()
    {
        fogImage = Image.CreateEmpty(
            (int)TextureSize.X,
            (int)TextureSize.Y,
            false,
            Image.Format.Rgba8
        );
        fogImage.Fill(FogColor);
        fogTexture = ImageTexture.CreateFromImage(fogImage);

        if (enabled)
        {
            Texture = fogTexture;
        }
    }

    public void Reveal(Vector2 revealWorldPosition, int RevealRadius)
    {
        if (!enabled)
        {
            return;
        }
        // global position into texture space, (upper left corner of the image)
        Vector2 revealTexturePosition = revealWorldPosition - GlobalPosition + TextureSize / 2;

        // TODO is this performant enough?  Should I consider using a shader?
        for (int y = -RevealRadius; y <= RevealRadius; y++)
        {
            for (int x = -RevealRadius; x <= RevealRadius; x++)
            {
                Vector2I pixel = new Vector2I(
                    x + (int)revealTexturePosition.X,
                    y + (int)revealTexturePosition.Y
                );
                pixel.X = Math.Clamp(pixel.X, 0, TextureSize.X - 1);
                pixel.Y = Math.Clamp(pixel.Y, 0, TextureSize.Y - 1);

                fogImage.SetPixelv(pixel, new Color(0, 0, 0, 0));
            }
        }

        fogTexture.Update(fogImage);
    }
}
