using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace NugEcsTestMark;

public class FixedMoverComponent : Component, IUpdater
{
    public Vector2 Velocity;
    public int HSpeed = 5;
    public int JSpeed = 40;
    public float Gravity = 0.98f;

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
        Velocity.Y += Gravity;

        _transform.Position.X += Velocity.X;
        _transform.Position.Y += Velocity.Y;
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
            Velocity.Y = Gravity;
        }
        
        double rotationRadians = Math.Atan2(Velocity.Y, Velocity.X);
        double rotationDegrees = rotationRadians * (180 / Math.PI);
        _transform.Rotation = (float)rotationDegrees + 90;
    }

}
