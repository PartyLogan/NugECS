
using System.Numerics;
using nugecs;
using Raylib_cs;

namespace NugEcsTestMark;

public class MoverComponent : Component, IUpdater
{
    public float X = 0;
    public float Y = 0;
    public Vector2 Velocity;
    public int HSpeed = 50;
    public int JSpeed = 620;
    public float Gravity = 280f;
    private Random _rng;

    private const int WIDTH = 1280 - 16;
    private const int HEIGHT = 720 - 16;

    public MoverComponent(float x, float y, Random rng)
    {
        X = x;
        Y = y;
        _rng = rng;
        Velocity.X = _rng.Next(-HSpeed, HSpeed);
    }

    public void Update()
    {
        var delta = Raylib.GetFrameTime();
        Velocity.Y += Gravity * delta;

        X += Velocity.X  * delta;
        Y += Velocity.Y  * delta;
        if (X > WIDTH || X < 16)
        {
            Velocity.X *= -1;
        }

        if (Y > HEIGHT)
        {
            Velocity.Y = -_rng.Next(JSpeed / 6, JSpeed);
        }

        if (Y < 16)
        {
            Velocity.Y = Gravity;
        }
    }

}
