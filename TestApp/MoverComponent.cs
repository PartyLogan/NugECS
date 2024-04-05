﻿
using System.Numerics;
using nugecs;
using Raylib_cs;
using Transform = nugecs.Transform;

namespace NugEcsTestMark;

public class MoverComponent : Component, IUpdater
{
    public Vector2 Velocity;
    public int HSpeed = 60;
    public int JSpeed = 820;
    public float Gravity = 480f;

    private const int WIDTH = 1280 - 16;
    private const int HEIGHT = 720 - 16;
    private Transform _transform;

    public MoverComponent(Random rng)
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

        _transform.Position.X += Velocity.X  * delta;
        _transform.Position.Y += Velocity.Y  * delta;
        if (_transform.Position.X > WIDTH || _transform.Position.X < 16)
        {
            Velocity.X *= -1;
            _transform.Position.X = Math.Clamp(_transform.Position.X, 16, WIDTH);
        }

        if (_transform.Position.Y > HEIGHT)
        {
            Velocity.Y = -JSpeed;
            _transform.Position.Y = Math.Clamp(_transform.Position.Y, 16, HEIGHT);
        }

        if (_transform.Position.Y < 16)
        {
            Velocity.Y = Gravity;
            _transform.Position.Y = Math.Clamp(_transform.Position.Y, 16, HEIGHT);
        }
        
        //double rotationDegrees = Math.Atan2(Velocity.Y, Velocity.X) * (180 / Math.PI);
        _transform.Rotation = (float)(Math.Atan2(Velocity.Y, Velocity.X) * (180 / Math.PI)) + 90;
    }

}
