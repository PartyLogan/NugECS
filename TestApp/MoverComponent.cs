
using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace TestApp;

public class MoverComponent : Component, IUpdater
{
    public Vector2 Velocity;
    public int HSpeed = 200;
    public int JSpeed = 820;
    public float Gravity = 480f;

    private const int WIDTH = 1280 - 16;
    private const int HEIGHT = 720 - 16;
    public Transform Transform { get; set; }

    public MoverComponent(Random rng)
    {
        JSpeed = rng.Next(JSpeed / 8, JSpeed);
        Velocity.X = rng.Next(-HSpeed, HSpeed);
    }

    public override void Init()
    {
        Transform = _world.GetTransform(_owner);
    }

    public void Update(float delta)
    {
        Velocity.Y += Gravity * delta;

        Transform.Position.X += Velocity.X  * delta;
        Transform.Position.Y += Velocity.Y  * delta;
        if (Transform.Position.X > WIDTH || Transform.Position.X < 16)
        {
            Velocity.X *= -1;
            Transform.Position.X = Math.Clamp(Transform.Position.X, 16, WIDTH);
        }

        if (Transform.Position.Y > HEIGHT)
        {
            Velocity.Y = -JSpeed;
            Transform.Position.Y = Math.Clamp(Transform.Position.Y, 16, HEIGHT);
        }

        if (Transform.Position.Y < 16)
        {
            Velocity.Y = Gravity;
            Transform.Position.Y = Math.Clamp(Transform.Position.Y, 16, HEIGHT);
        }
        
        float rotationDegrees = (float)(FastAtan2(Velocity.Y, Velocity.X) * (180 / Math.PI)) + 90;
        if (Velocity.Y > 0f)
        {
            rotationDegrees += 180f;
        }
        Transform.Rotation = rotationDegrees;
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

