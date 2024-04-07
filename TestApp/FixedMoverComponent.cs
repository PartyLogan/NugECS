using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace NugEcsTestMark;

public class FixedMoverComponent : Component, IUpdater
{
    public Vector2 Velocity;
    public int HSpeed = 200;
    public int JSpeed = 600;
    public float Gravity = 180f;

    private const int WIDTH = 1280 - 16;
    private const int HEIGHT = 720 - 16;
    private Transform _transform;

    public FixedMoverComponent(Random rng)
    {
        JSpeed = rng.Next(JSpeed / 8, JSpeed);
        Velocity.X = rng.Next(-HSpeed, HSpeed);
    }

    public override void Init()
    {
        _transform = _world.GetTransform(_owner);
    }

    public void Update(float delta)
    {
        Velocity.Y += Gravity * delta;

        _transform.Position.X += Velocity.X * delta;
        _transform.Position.Y += Velocity.Y * delta;
        if (_transform.Position.X > WIDTH || _transform.Position.X < 16)
        {
            Velocity.X *= -1;
        }

        if (_transform.Position.Y > HEIGHT)
        {
            Velocity.Y = -JSpeed;
        }

        if (_transform.Position.Y < 16)
        {
            Velocity.Y = Gravity * delta;
        }
        
        float rotationDegrees = (float)(FastAtan2(Velocity.Y, Velocity.X) * (180 / Math.PI)) + 90;
        if (Velocity.Y > 0f)
        {
            rotationDegrees += 180f;
        }
        _transform.Rotation = rotationDegrees;
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
