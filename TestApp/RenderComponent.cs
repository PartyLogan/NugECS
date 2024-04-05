using System.Numerics;
using nugecs;
using Raylib_cs;

namespace NugEcsTestMark;

public class RenderComponent : Component, IRenderer
{
    private Texture2D _sprite;
    private Color _color;
    private Rectangle _source = new Rectangle(0, 0, 32, 32);
    private Rectangle _dest = new Rectangle(0, 0, 32, 32);
    private float _rotation = 0;
    private Vector2 _origin = new Vector2(16, 16);
    
    public RenderComponent(Texture2D sprite, Color color)
    {
        _sprite = sprite;
        _color = color;
    }
    
    public void Render()
    {
        var pos = _world.GetComponent<MoverComponent>(_owner);
        _dest.X = pos.X;
        _dest.Y = pos.Y;
        double rotationRadians = Math.Atan2(pos.Velocity.Y, pos.Velocity.X);
        double rotationDegrees = rotationRadians * (180 / Math.PI);
        _rotation = (float)rotationDegrees + 90;
        Raylib.DrawTexturePro(_sprite, _source, _dest, _origin, _rotation, _color);
        //Raylib.DrawTexture(_sprite, (int)pos.X, (int)pos.Y, _color);
    }
}