using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace TestApp;

public class ECSRenderComponent : Component
{
    public Texture2D Sprite;
    public Color Color;
    public Rectangle Source = new Rectangle(0, 0, 26, 36);
    public Rectangle Dest = new Rectangle(0, 0, 26, 36);
    public Vector2 Origin = new Vector2(26 / 2, 36 / 2);
    public Transform Transform;
    private int _spriteIndex = 0;
    
   
    public ECSRenderComponent(Texture2D sprite, Color color, int spriteIndex)
    {
        Sprite = sprite;
        Color = color;
        _spriteIndex = spriteIndex;
        Source.Y = _spriteIndex * Source.Height;
    }
    
    public override void Init()
    {
        Transform = _world.GetTransform(_owner);
    }
}