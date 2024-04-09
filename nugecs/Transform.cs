using System.Numerics;

namespace nugecs;

public class Transform
{
    public Vector3 Position;
    public Vector3 Scale = Vector3.One;
    public float Rotation = 0f;

    public Transform()
    {
        Position = Vector3.Zero;
        Scale = Vector3.One;
        Rotation = 0f;
    }

    public Transform(Vector3 pos)
    {
        Position = pos;
    }
    
    public Transform(Vector3 pos, Vector3 scale)
    {
        Position = pos;
        Scale = scale;
    }
    
    public Transform(Vector3 pos, Vector3 scale, float rot)
    {
        Position = pos;
        Scale = scale;
        Rotation = rot;
    }
    
}