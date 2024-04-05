using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace NugEcsTestMark;

public class RenderComponent : Component, IRenderer
{
    private Texture2D _sprite;
    private Color _color;
    private Rectangle _source = new Rectangle(0, 0, 32, 32);
    private Rectangle _dest = new Rectangle(0, 0, 32, 32);
    private Vector2 _origin = new Vector2(16, 16);
    private Transform _transform;
    
    public RenderComponent(Texture2D sprite, Color color)
    {
        _sprite = sprite;
        _color = color;
    }

    public override void Init()
    {
        _transform = _world.GetTransform(_owner);
    }
    
    public void Render()
    {
        _dest.X = _transform.Position.X;
        _dest.Y = _transform.Position.Y;

        Raylib.DrawTexturePro(_sprite, _source, _dest, _origin, _transform.Rotation, _color);
    }
}