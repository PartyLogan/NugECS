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

    public static int ToSpawn = 0;
    public static int ToDespawn = 0;

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
        }
        
        World.Init();
        Raylib.InitWindow(1280, 720, "Test App");
        
        
        
        BunnySprite = Raylib.LoadTexture("../../../resources/wabbit_alpha.png");
        ToSpawn = 100;
        
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
            
            Raylib.DrawFPS(10, 10);
            
            var entCount = World.ActiveEntities();
            result = $"Entities: {entCount}";
            Raylib.DrawText(result, 10, 30, 20, Color.Green);
            var time = World.Time.ToString();
            Raylib.DrawText($"Time Res: {time}", 10, 60, 20, Color.Green);

            if (ecs)
            {
                Raylib.DrawText($"Using ECS", 10, 90, 20, Color.Green);
            }
            
            if (World.IsFixedUpdate())
            {
                Raylib.DrawText(World.DebugFixedUpdateString(), 10, 90, 20, Color.Green);
            }

            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }

    public static String result;
    public static String fastStr;
    
    public static void BunnyCheck()
    {
        if (Raylib.IsMouseButtonDown(MouseButton.Left) && ToSpawn == 0 && ToDespawn == 0)
        {
            ToSpawn = Math.Clamp(World.MaxEntities() - World.ActiveEntities(), 0, CHANGE_AMOUNT);
        }
        else if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            ToDespawn = Math.Clamp(World.ActiveEntities(), 0, CHANGE_AMOUNT);
        }

        if (ToSpawn > 0)
        {
            for (int i = TO_CHANGE; i > 0; i--)
            {
                SpawnBunny();
                ToSpawn--;
                if (ToSpawn == 0)
                    break;
            }
        }

        if (ToDespawn > 0)
        {
            for (int i = TO_CHANGE; i > 0; i--)
            {
                World.DeleteEntity(World.GetLastActive());
                ToDespawn--;
                if (ToDespawn == 0)
                    break;
            }
        }
    }

    private const int CHANGE_AMOUNT = 1000;
    private const int TO_CHANGE = 50;

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
            mover.Transform.Rotation = rotationDegrees;
            
        }
        
        Dictionary<Type, Component[]> renderers;
        renderers = World.Query([typeof(ECSRenderComponent)]);
        
        foreach (var r in renderers[typeof(ECSRenderComponent)])
        {
            var render = (ECSRenderComponent)r;
            render.Dest.X = render.Transform.Position.X;
            render.Dest.Y = render.Transform.Position.Y;

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
                 time.TimeMod += 0.1f;
            }
            else
            {
                var time = World.Time;
                time.TimeMod -= 0.1f;
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

        if(rng.Next(10) > 8)
            World.AddComponent(entity, new NullComponent());
        
        
        var color = new Color(rng.Next(255), rng.Next(255), rng.Next(255), 255);
        if (ecs)
        {
            World.AddComponent(entity, new ECSRenderComponent(BunnySprite, color));
        }
        else
        {
            World.AddComponent(entity, new RenderComponent(BunnySprite, color));
        }
        
    }
}