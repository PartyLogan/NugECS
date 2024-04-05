
using System.Numerics;
using nugecs;
using Raylib_cs;

namespace NugEcsTestMark;

public class MoverComponent : Component, IUpdater
{
    public float X = 0;
    public float Y = 0;
    public Vector2 Velocity;
    public int HSpeed = 100;
    public int JSpeed = 400;
    public float Gravity = 98f;

    private const int WIDTH = 1280 - 16;
    private const int HEIGHT = 720 - 16;

    public MoverComponent(float x, float y)
    {
        X = x;
        Y = y;
        var rng = new Random();
        Velocity.X = rng.Next(-HSpeed, HSpeed);
    }
    
    public override void Init()
    {
        Console.WriteLine($"Inited!");
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
            Velocity.Y -= JSpeed;
        }

        if (Y < 16)
        {
            Velocity.Y = Gravity;
        }
        //Console.WriteLine("I updated!");
    }

}
