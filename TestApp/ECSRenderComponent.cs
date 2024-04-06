using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace TestApp;

public class ECSRenderComponent : Component
{
    public Texture2D Sprite;
    public Color Color;
    public Rectangle Source = new Rectangle(0, 0, 32, 32);
    public Rectangle Dest = new Rectangle(0, 0, 32, 32);
    public Vector2 Origin = new Vector2(16, 16);
    public Transform Transform;
    
    public ECSRenderComponent(Texture2D sprite, Color color)
    {
        Sprite = sprite;
        Color = color;
    }
    
    public override void Init()
    {
        Transform = _world.GetTransform(_owner);
    }
}