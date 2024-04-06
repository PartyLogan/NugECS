using System.Runtime.Intrinsics.X86;
using System.Text.Formatting;
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
    
    public static void Main()
    {
        World = new World(200_000);
        World.Init();
        World.RegisterComponent<NullComponent>();
        World.RegisterComponent<MoverComponent>();
        World.RegisterComponent<FixedMoverComponent>();
        World.RegisterComponent<RenderComponent>();
        Raylib.InitWindow(1280, 720, "Test App");
        
        BunnySprite = Raylib.LoadTexture("../../../resources/wabbit_alpha.png");
        ToSpawn = 100;
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            
            //Update();
            //World.Render();
            EcsUpdate();
            
            Raylib.DrawFPS(10, 10);
            var entCount = World.ActiveEntities();
            String result = StringBuffer.Format("Entities: {0}", entCount);
            Raylib.DrawText(result, 10, 30, 20, Color.Green);
            if (World.IsFixedUpdate())
            {
                //Raylib.DrawText(World.DebugUpdateString(), 10, 60, 20, Color.Green);
            }
            
            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }

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
    private const int TO_CHANGE = 10;
    
    public static void EcsUpdate()
    {
        BunnyCheck();
        
        var delta = Raylib.GetFrameTime();
        var movers = World.QueryHighAlloc([typeof(MoverComponent)], [typeof(NullComponent)]);
        foreach (var m in movers[typeof(MoverComponent)])
        { 
            var mover = m as MoverComponent;
            mover.Update(delta);
        }
        
        var renderers = World.Query([typeof(MoverComponent), typeof(RenderComponent)]);
        foreach (var r in renderers[typeof(RenderComponent)])
        {
            var render = r as RenderComponent;
            render.Render();
        }
    }
    
    public static void Update()
    {
        BunnyCheck();
        
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
        else
        {
            World.AddComponent(entity, new MoverComponent(rng));
        }

        if(rng.Next(10) > 8)
            World.AddComponent(entity, new NullComponent());
        
        
        var color = new Color(rng.Next(255), rng.Next(255), rng.Next(255), 255);
        World.AddComponent(entity, new RenderComponent(BunnySprite, color));
    }
}