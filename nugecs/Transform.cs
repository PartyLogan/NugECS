using System.Numerics;

namespace nugecs;

public class Transform
{
    public Vector2 Position;
    public Vector2 Scale = Vector2.One;
    public float Rotation = 0f;

    public Transform()
    {
        Position = Vector2.Zero;
        Scale = Vector2.One;
        Rotation = 0f;
    }

    public Transform(Vector2 pos)
    {
        Position = pos;
    }
    
    public Transform(Vector2 pos, Vector2 scale)
    {
        Position = pos;
        Scale = scale;
    }
    
    public Transform(Vector2 pos, Vector2 scale, float rot)
    {
        Position = pos;
        Scale = scale;
        Rotation = rot;
    }
    
}