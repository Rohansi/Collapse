using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace Collapse
{
    public static class Program
    {
        public const int TileSize = 8;

        public static RenderWindow Window;
        public static TileMap Map;

        static void Main(string[] args)
        {
            Window = new RenderWindow(new VideoMode(1280, 720), "Collapse");
            Window.SetFramerateLimit(60);
            Window.Closed += (sender, eventArgs) => Window.Close();

            Map = new TileMap(200, 90);
            for (var y = 85; y < 100; y++)
            {
                for (var x = 0; x < 200; x++)
                {
                    Map[x, y] = Tile.Floor;
                }
            }

            Window.MouseMoved += (sender, eventArgs) =>
            {
                var mousePosF = Window.MapPixelToCoords(new Vector2i(eventArgs.X, eventArgs.Y));
                var mousePos = new Vector2i((int)mousePosF.X / TileSize, (int)mousePosF.Y / TileSize);

                if (Mouse.IsButtonPressed(Mouse.Button.Left))
                {
                    Map[mousePos.X, mousePos.Y] = Tile.Block;
                }

                if (Mouse.IsButtonPressed(Mouse.Button.Right))
                {
                    Map[mousePos.X, mousePos.Y] = Tile.None;
                }

                if (Mouse.IsButtonPressed(Mouse.Button.Middle))
                {
                    Map[mousePos.X, mousePos.Y] = Tile.Floor;
                }
            };

            while (Window.IsOpen())
            {
                Window.DispatchEvents();

                Window.Clear(new Color(100, 149, 237));
                Window.Draw(Map);
                Window.Display();
            }
        }
    }
}
