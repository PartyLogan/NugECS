using System.Numerics;
using nugecs;

namespace TestApp;

public class ECSMoverComponent : Component
{
    public Vector2 Velocity;
    public int HSpeed = 60;
    public int JSpeed = 820;
    public float Gravity = 480f;

    public int WIDTH = 1280 - 16;
    public int HEIGHT = 720 - 16;
    public Transform Transform;
    
    public ECSMoverComponent(Random rng)
    {
        JSpeed = rng.Next(JSpeed / 8, JSpeed);
        Velocity.X = rng.Next(-HSpeed, HSpeed);
    }
    
    public override void Init()
    {
        Transform = _world.GetTransform(_owner);
    }
    
    public float FastAtan2(float y, float x)
    {
        if (x == 0f)
        {
            if (y > 0f) return (float)(Math.PI / 2);
            if (y == 0f) return 0f;
            return (float)(-Math.PI / 2);
        }
        float atan;
        float z = y / x;
        if (Math.Abs(z) < 1f)
        {
            atan = z / (1f + 0.28f * z * z);
            if (x < 0f)
            {
                if (y < 0f) return atan - (float)Math.PI;
                return atan + (float)Math.PI;
            }
        }
        else
        {
            atan = (float)(Math.PI / 2) - z / (z * z + 0.28f);
            if (y < 0f) return atan - (float)Math.PI;
        }
        return atan;
    }
}