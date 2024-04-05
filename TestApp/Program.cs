using System.Runtime.Intrinsics.X86;
using Raylib_cs;

namespace NugEcsTestMark;

using nugecs;

public class Program
{
    public static World World;
    public static Texture2D BunnySprite;
    public static void Main()
    {
        World = new World(1_000_000);
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
            Raylib.DrawText($"Entities: {entCount}", 10, 30, 20, Color.Green);
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
        World.Update();
    }

    public static void SpawnBunny()
    {
        var entity = World.CreateEntity();
        var mousePos = Raylib.GetMousePosition();
        World.AddComponent(entity, new MoverComponent(mousePos.X, mousePos.Y));

        var rng = new Random();
        var color = new Color(rng.Next(255), rng.Next(255), rng.Next(255), 255);
        World.AddComponent(entity, new RenderComponent(BunnySprite, color));
    }
}