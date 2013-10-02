using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace Collapse
{
    public enum Tile
    {
        None = -1, Floor, Block
    }

    public class TileMap : Drawable
    {
        private static readonly Color FloorColor = new Color(21, 175, 41);
        private static readonly Color BlockCOlor = new Color(91, 91, 91);

        private class TileEntity : Drawable
        {
            private TileMap _map;
            private RectangleShape _shape;
            private Vector2f _position;
            private Vector2f _speed;

            public bool Dead { get; private set; }

            public TileEntity(TileMap map, Vector2f position)
            {
                _map = map;

                _shape = new RectangleShape(new Vector2f(Program.TileSize, Program.TileSize));
                _shape.FillColor = BlockCOlor;
                _shape.Position = position;

                _position = position;
                _speed = new Vector2f(0, 0);
            }

            public void Draw(RenderTarget target, RenderStates states)
            {
                if (Dead)
                    return;

                var x = (int)_position.X / Program.TileSize;
                var y = (int)_position.Y / Program.TileSize;

                if (_map[x, y] != Tile.None || _position.Y >= 1000)
                {
                    if (_map[x, y - 1] == Tile.None)
                        _map[x, y - 1] = Tile.Block;

                    Dead = true;
                    return;
                }

                _speed += new Vector2f(0, 1);
                _position += _speed;
                _shape.Position = _position;
                target.Draw(_shape);
            }
        }

        private int _width;
        private int _height;
        private Tile[,] _tiles;
        private List<TileEntity> _fallingTiles;
          
        public TileMap(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new Tile[width, height];
            _fallingTiles = new List<TileEntity>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    _tiles[x, y] = Tile.None;
                }
            }
        }

        public Tile this[int x, int y]
        {
            get
            {
                if (x < 0 || y < 0 || x >= _width || y >= _height)
                    return Tile.None;

                return _tiles[x, y];
            }

            set
            {
                if (x < 0 || y < 0 || x >= _width || y >= _height)
                    return;

                _tiles[x, y] = value;

                if (value == Tile.Floor)
                    return;

                var pos = new Vector2i(x, y);

                if (value != Tile.None)
                {
                    if (this[pos.X, pos.Y].IsCollapsable() && !IsStable(pos, new Vector2i(-1, -1)))
                        Collapse(pos);
                }
                else
                {
                    var offsetPos = pos;
                    offsetPos.Y -= 1;
                    if (this[offsetPos.X, offsetPos.Y].IsCollapsable() && !IsStable(offsetPos, new Vector2i(-1, -1)))
                        Collapse(offsetPos);

                    offsetPos = pos;
                    offsetPos.X -= 1;
                    if (this[offsetPos.X, offsetPos.Y].IsCollapsable() && !IsStable(offsetPos, new Vector2i(-1, -1)))
                        Collapse(offsetPos);

                    offsetPos = pos;
                    offsetPos.X += 1;
                    if (this[offsetPos.X, offsetPos.Y].IsCollapsable() && !IsStable(offsetPos, new Vector2i(-1, -1)))
                        Collapse(offsetPos);
                }
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _fallingTiles.RemoveAll(t => t.Dead);
            foreach (var tile in _fallingTiles)
            {
                target.Draw(tile);
            }

            var shape = new RectangleShape(new Vector2f(Program.TileSize, Program.TileSize));

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    Color color;

                    switch (_tiles[x, y])
                    {
                        case Tile.Floor:
                            color = FloorColor;
                            break;
                        case Tile.Block:
                            color = BlockCOlor;
                            break;
                        default:
                            continue;
                    }

                    shape.FillColor = color;
                    shape.Position = new Vector2f(x * Program.TileSize, y * Program.TileSize);
                    target.Draw(shape);
                }
            }
        }

        private bool IsStable(Vector2i pos, Vector2i ignorePos, int horizontal = 0, int vertical = 0)
        {
            Vector2i? collapse;
            var result = IsStable(pos, ignorePos, horizontal, vertical, out collapse);
            if (collapse.HasValue)
                Collapse(collapse.Value);
            return result;
        }

        private bool IsStable(Vector2i pos, Vector2i ignorePos, int horizontal, int vertical, out Vector2i? collapse)
        {
            const int maxHorizontal = 10;
            const int maxVertical = 50;

            collapse = null;

            if (pos.X < 0 || pos.Y < 0 || pos.X >= _width || pos.Y >= _height)
                return false;

            if (_tiles[pos.X, pos.Y] == Tile.None)
                return false;

            if (_tiles[pos.X, pos.Y] == Tile.Floor)
                return true;

            if (horizontal >= maxHorizontal)
                return false;

            var effectiveMaxVertical = (float)Math.Ceiling(maxVertical * (2 - horizontal / (float)maxHorizontal));
            if (vertical >= effectiveMaxVertical)
            {
            Console.WriteLine("hi");
                collapse = pos;
                return true;
            }

            var newPos = pos;
            newPos.Y += 1;
            if (!newPos.EqualTo(ignorePos) && IsStable(newPos, pos, horizontal, vertical + 1))
                return true;

            Vector2i? leftCollapse = null;
            Vector2i? rightCollapse = null;

            newPos = pos;
            newPos.X -= 1;
            if (!newPos.EqualTo(ignorePos) && IsStable(newPos, pos, horizontal + 1, vertical, out leftCollapse))
                return true;

            newPos = pos;
            newPos.X += 1;
            if (!newPos.EqualTo(ignorePos) && IsStable(newPos, pos, horizontal + 1, vertical, out rightCollapse))
                return true;

            if (leftCollapse.HasValue)
                Collapse(leftCollapse.Value);

            if (rightCollapse.HasValue)
                Collapse(rightCollapse.Value);

            return false;
        }

        private void Collapse(Vector2i pos)
        {
            this[pos.X, pos.Y] = Tile.None;
            _fallingTiles.Add(new TileEntity(this, new Vector2f(pos.X * Program.TileSize, pos.Y * Program.TileSize)));
        }
    }

    public static class TileUtil
    {
        public static bool EqualTo(this Vector2i vec1, Vector2i vec2)
        {
            return vec1.X == vec2.X && vec1.Y == vec2.Y;
        }

        public static bool IsCollapsable(this Tile tile)
        {
            return tile == Tile.Block;
        }
    }
}
