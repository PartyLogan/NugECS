using System.Runtime.Intrinsics.X86;
using System.Text.Formatting;
using Raylib_cs;

namespace NugEcsTestMark;

using nugecs;

public class Program
{
    public static World World;
    public static Texture2D BunnySprite;
    public static Random rng = new Random();
    public static void Main()
    {
        World = new World(200_000, true);
        World.Init();
        Raylib.InitWindow(1280, 720, "Test App");
        
        BunnySprite = Raylib.LoadTexture("../../../resources/wabbit_alpha.png");
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            
            Update();
            World.Render();
            
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

    public static void Update()
    {
        if (Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            for (int i = 0; i < 100; i++)
            {
                if (World.ActiveEntities() < World.MaxEntities())
                {
                    SpawnBunny();
                }
            }
        }
        else if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            for (int i = 0; i < 100; i++)
            {
                if (World.ActiveEntities() > 0)
                {
                    World.DeleteEntity(World.GetLastActive());
                }
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
        else
        {
            World.AddComponent(entity, new MoverComponent(rng));
        }
        
        var color = new Color(rng.Next(255), rng.Next(255), rng.Next(255), 255);
        World.AddComponent(entity, new RenderComponent(BunnySprite, color));
    }
}