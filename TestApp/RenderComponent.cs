using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace NugEcsTestMark;

public class RenderComponent : Component, IRenderer
{
    private Texture2D _sprite;
    private Color _color;
    private Rectangle _source = new Rectangle(0, 0, 26, 36);
    private Rectangle _dest = new Rectangle(0, 0, 26, 36);
    private Vector2 _origin = new Vector2(26 / 2, 36 / 2);
    private Transform _transform;
    private int _spriteIndex = 0;
    
    public RenderComponent(Texture2D sprite, Color color, int spriteIndex)
    {
        _sprite = sprite;
        _color = color;
        _spriteIndex = spriteIndex;
        _source.Y = _spriteIndex * _source.Height;
    }

    public override void Init()
    {
        _transform = _world.GetTransform(_owner);
    }
    
    public void Render()
    {
        _dest.X = _transform.Position.X;
        _dest.Y = _transform.Position.Y;
        _dest.Width = _source.Width * _transform.Scale.X;
        _dest.Height = _source.Height * _transform.Scale.Y;
        Raylib.DrawTexturePro(_sprite, _source, _dest, _origin, _transform.Rotation, _color);
    }
}