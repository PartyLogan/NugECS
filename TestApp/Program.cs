using System.Numerics;
using System.Runtime.Intrinsics.X86;

using Raylib_cs;
using TestApp;

namespace NugEcsTestMark;

using nugecs;

public class Program
{
    public static World World;
    public static Texture2D BunnySprite;
    public static Random rng = new Random();

    public static bool ecs = false;
    
    public static void Main(string[] args)
    {
        World = new World(200_000);
        if (args.Length > 0)
        {
            var input = args[0];
            if (input == "ecs")
            {
                ecs = true;
            }
                
            if(input == "fixed")
                World.SetFixedUpdate(true);
        }
        
        World.RegisterComponent<NullComponent>();
        World.RegisterComponent<MoverComponent>();
        World.RegisterComponent<RenderComponent>();
        
        if (World.IsFixedUpdate())
        {
            World.RegisterComponent<FixedMoverComponent>();
        }
        
        if (ecs)
        {
            World.RegisterComponent<ECSMoverComponent>();
            World.RegisterComponent<ECSRenderComponent>();
            World.RegisterComponent<ECSRescaleComponent>();
        }
        
        World.Init();
        Raylib.InitWindow(1280, 720, "Test App");

        var entity = World.CreateEntity();
        World.AddComponent(entity, new NullComponent());
        World.TagEntity(entity, "NULL");
        var nullEnt = World.GetTaggedEntity("NULL");
        Console.WriteLine($"fetch-----------------{nullEnt}-----------------");
        
        World.UntagEntity("NULL");
        nullEnt = World.GetTaggedEntity("NULL");
        Console.WriteLine($"un tag -----------------{nullEnt}-----------------");
        
        World.TagEntity(entity, "NULL");
        nullEnt = World.GetTaggedEntity("NULL");
        Console.WriteLine($"retag-----------------{nullEnt}-----------------");
        
        World.DeleteEntity(entity);
        nullEnt = World.GetTaggedEntity("NULL");
        Console.WriteLine($"delete -----------------{nullEnt}-----------------");

        TimeResource timeR = (TimeResource) World.GetTaggedResource("Time");
        Console.WriteLine($"1-----{timeR}-----");
        timeR = World.GetTaggedResource<TimeResource>("Time");
        Console.WriteLine($"fetch -----{timeR}-----");
        World.UntagResource("Time");
        timeR = World.GetTaggedResource<TimeResource>("Time");
        Console.WriteLine($"untag -----{timeR}-----");
        
        World.TagResource<TimeResource>("Time");
        timeR = World.GetTaggedResource<TimeResource>("Time");
        Console.WriteLine($"retag -----{timeR}-----");
        
        World.UnregisterResource<TimeResource>();
        timeR = World.GetTaggedResource<TimeResource>("Time");
        Console.WriteLine($"unreg -----{timeR}-----");
        
        //BunnySprite = Raylib.LoadTexture("../../../resources/wabbit_alpha.png");
        BunnySprite = Raylib.LoadTexture("../../../resources/bunnys.png");
        Console.WriteLine($"Sprite: {BunnySprite.Width}, {BunnySprite.Height}");
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            
            
            BunnyCheck();
            Update();
            if (ecs)
            {
                EcsUpdate();
            }
            
            World.Render();
            
            //Raylib.DrawFPS(10, 10);
            DrawOutlinedText($"FPS: {Raylib.GetFPS()}",10, 10, 20, Color.Green, 1, Color.Black);
            
            var entCount = World.ActiveEntities();
            result = $"Entities: {entCount}";
            DrawOutlinedText($"{result}", 10, 30, 20, Color.Green, 1, Color.Black);
            //Raylib.DrawText(result, 10, 30, 20, Color.Green);
            var time = World.Time.ToString();
            DrawOutlinedText($"{time}", 10, 50, 20, Color.Green, 1, Color.Black);
            //Raylib.DrawText($"Time Res: {time}", 10, 60, 20, Color.Green);

            if (ecs)
            {
                DrawOutlinedText("Using ECS", 10, 70, 20, Color.Green, 1, Color.Black);
            }
            
            if (World.IsFixedUpdate())
            {
                DrawOutlinedText(World.DebugFixedUpdateString(), 10, 70, 20, Color.Green, 1, Color.Black);
                //Raylib.DrawText(World.DebugFixedUpdateString(), 10, 90, 20, Color.Green);
            }

            Raylib.EndDrawing();
            
            World.Maintain();
        }
        
        Raylib.CloseWindow();
    }
    
    public static void DrawOutlinedText(string text, int posX, int posY, int fontSize, Color color, int outlineSize, Color outlineColor) {
        Raylib.DrawText(text, posX - outlineSize, posY - outlineSize, fontSize, outlineColor);
        Raylib.DrawText(text, posX + outlineSize, posY - outlineSize, fontSize, outlineColor);
        Raylib.DrawText(text, posX - outlineSize, posY + outlineSize, fontSize, outlineColor);
        Raylib.DrawText(text, posX + outlineSize, posY + outlineSize, fontSize, outlineColor);
        Raylib.DrawText(text, posX, posY, fontSize, color);
    }
    
    public static String result;
    public static String fastStr;
    public static bool spawn = false;
    public static void BunnyCheck()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            spawn = !spawn;
        if (Raylib.IsMouseButtonDown(MouseButton.Left) || spawn)
        {
            if(World.ActiveEntities() < World.MaxEntities())
                SpawnBunny();
        }
        else if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            if(World.ActiveEntities() > 0)
                World.DeleteEntity(World.GetLastActive());
        }
    }

    public static void EcsUpdate()
    {
        //var delta = Raylib.GetFrameTime();
        var time = World.Time;
        var delta = time.Delta;
        
        Dictionary<Type, Component[]> movers;

        movers = World.Query([typeof(ECSMoverComponent)]);

        foreach (var m in movers[typeof(ECSMoverComponent)])
        { 
            var mover = (ECSMoverComponent)m;
            mover.Velocity.Y += mover.Gravity * delta;

            mover.Transform.Position.X += mover.Velocity.X  * delta;
            mover.Transform.Position.Y += mover.Velocity.Y  * delta;
            if (mover.Transform.Position.X > mover.WIDTH || mover.Transform.Position.X < 16)
            {
                mover.Velocity.X *= -1;
                mover.Transform.Position.X = Math.Clamp(mover.Transform.Position.X, 16, mover.WIDTH);
            }

            if (mover.Transform.Position.Y > mover.HEIGHT)
            {
                mover.Velocity.Y = -mover.JSpeed;
                mover.Transform.Position.Y = Math.Clamp(mover.Transform.Position.Y, 16, mover.HEIGHT);
            }

            if (mover.Transform.Position.Y < 16)
            {
                mover.Velocity.Y = mover.Gravity;
                mover.Transform.Position.Y = Math.Clamp(mover.Transform.Position.Y, 16, mover.HEIGHT);
            }
        
            float rotationDegrees = (float)(mover.FastAtan2(mover.Velocity.Y, mover.Velocity.X) * (180 / Math.PI)) + 90;
            if (mover.Velocity.Y > 0f)
            {
                rotationDegrees += 180f;
            }
            mover.Transform.Rotation = rotationDegrees;
        }

        Dictionary<Type, Component[]> scalers;
        scalers = World.Query([typeof(ECSRescaleComponent)], [typeof(NullComponent)]);
        var transforms = World.GetTransforms();

        foreach (var s in (scalers[typeof(ECSRescaleComponent)]))
        {
            var scaler = (ECSRescaleComponent)s;
            var t = transforms[s.Owner.Index];
            if (scaler.Increasing)
            {
                if (t.Scale.X < 4.9f)
                {
                    t.Scale += new Vector2(1f, 1f) * delta;
                }
                else
                {
                    scaler.Increasing = false;
                }
            }
            else
            {
                if (t.Scale.X > 0.2f)
                {
                    t.Scale -= new Vector2(1f, 1f)  * delta;
                }
                else
                {
                    scaler.Increasing = true;
                }
            }
        }
        
        Dictionary<Type, Component[]> renderers;
        renderers = World.Query([typeof(ECSRenderComponent)]);
        
        foreach (var r in renderers[typeof(ECSRenderComponent)])
        {
            var render = (ECSRenderComponent)r;
            render.Dest.X = render.Transform.Position.X;
            render.Dest.Y = render.Transform.Position.Y;
            render.Dest.Width = render.Source.Width * render.Transform.Scale.X;
            render.Dest.Height = render.Source.Height * render.Transform.Scale.Y;
            render.Origin.X = render.Dest.Width / 2f;
            render.Origin.Y = render.Dest.Height / 2f;
            Raylib.DrawTexturePro(render.Sprite, render.Source, render.Dest, render.Origin, render.Transform.Rotation, render.Color);
        }
    }
    
    public static void Update()
    {
        if (Raylib.GetMouseWheelMove() != 0)
        {
            if (Raylib.GetMouseWheelMove() > 0)
            {
                 var time = World.Time;
                 time.TimeScale += 0.1f;
            }
            else
            {
                var time = World.Time;
                time.TimeScale -= 0.1f;
            }
        }
        var delta = Raylib.GetFrameTime();
        World.Update(delta);
    }

    public static void SpawnBunny()
    {
        var mousePos = Raylib.GetMousePosition();
        var entity = World.CreateEntity(mousePos.X, mousePos.Y);
        if (World.IsFixedUpdate())
        {
            World.AddComponent(entity, new FixedMoverComponent(rng));
        }
        else if(ecs)
        {
            World.AddComponent(entity, new ECSMoverComponent(rng));
        }
        else
        {
            World.AddComponent(entity, new MoverComponent(rng));
        }

        if (ecs && rng.Next(100) > 95)
        {
            World.AddComponent(entity, new ECSRescaleComponent());
            if (ecs && rng.Next(100) > 90)
            {
                World.AddComponent(entity, new NullComponent());
            }
        }
            
        
        //var color = new Color(rng.Next(255), rng.Next(255), rng.Next(255), 255);
        var spriteIndex = rng.Next(5);
        if (ecs)
        {
            World.AddComponent(entity, new ECSRenderComponent(BunnySprite, Color.White, spriteIndex));
        }
        else
        {
            World.AddComponent(entity, new RenderComponent(BunnySprite, Color.White, spriteIndex));
        }
    }
}