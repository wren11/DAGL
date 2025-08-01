using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzlePlayPane : ControlPane
{
    private readonly List<PuzzleTile> _tiles = [];
    private readonly List<PuzzleTile> _selectedTiles = [];
    private int _gridSize = 8;
    private int _level = 1;
    private int _score = 0;
    private bool _isCompleted = false;
    private DateTime _lastUpdate = DateTime.Now;

    public bool IsCompleted => _isCompleted;
    public int GetScore() => _score;

    public void Initialize(int level)
    {
        _level = level;
        _score = 0;
        _isCompleted = false;
        _selectedTiles.Clear();
        _tiles.Clear();

        _gridSize = Math.Min(8, 4 + level);
        GeneratePuzzle();
    }

    private void GeneratePuzzle()
    {
        var random = new Random();
        var colors = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            
        for (var y = 0; y < _gridSize; y++)
        {
            for (var x = 0; x < _gridSize; x++)
            {
                var tile = new PuzzleTile
                {
                    X = x,
                    Y = y,
                    Color = colors[random.Next(colors.Length)],
                    IsSelected = false
                };
                _tiles.Add(tile);
            }
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var tileSize = 40;
        var startX = 50;
        var startY = 50;

        foreach (var tile in _tiles)
        {
            var rect = new Rectangle(
                startX + tile.X * tileSize,
                startY + tile.Y * tileSize,
                tileSize - 2,
                tileSize - 2
            );

            var color = tile.IsSelected ? Color.Yellow : GetTileColor(tile.Color);
            spriteBatch.DrawRectangle(rect, color);
            spriteBatch.DrawRectangle(rect, Color.Black);
        }
    }

    private Color GetTileColor(int colorIndex)
    {
        return colorIndex switch
        {
            1 => Color.Red,
            2 => Color.Blue,
            3 => Color.Green,
            4 => Color.Yellow,
            5 => Color.Purple,
            6 => Color.Orange,
            7 => Color.Pink,
            8 => Color.Cyan,
            _ => Color.Gray
        };
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is MouseEvent me && me.Type == EventType.LButtonDown)
        {
            var tileSize = 40;
            var startX = 50;
            var startY = 50;

            var gridX = (me.X - startX) / tileSize;
            var gridY = (me.Y - startY) / tileSize;

            if (gridX >= 0 && gridX < _gridSize && gridY >= 0 && gridY < _gridSize)
            {
                var clickedTile = _tiles.Find(t => t.X == gridX && t.Y == gridY);
                if (clickedTile != null)
                {
                    HandleTileClick(clickedTile);
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }

    private void HandleTileClick(PuzzleTile tile)
    {
        if (_selectedTiles.Count == 0)
        {
            tile.IsSelected = true;
            _selectedTiles.Add(tile);
        }
        else if (_selectedTiles.Count == 1)
        {
            var firstTile = _selectedTiles[0];
            if (AreAdjacent(firstTile, tile) && firstTile.Color == tile.Color)
            {
                tile.IsSelected = true;
                _selectedTiles.Add(tile);
                RemoveSelectedTiles();
                _score += 10;
            }
            else
            {
                firstTile.IsSelected = false;
                _selectedTiles.Clear();
                tile.IsSelected = true;
                _selectedTiles.Add(tile);
            }
        }
    }

    private bool AreAdjacent(PuzzleTile tile1, PuzzleTile tile2)
    {
        return (Math.Abs(tile1.X - tile2.X) == 1 && tile1.Y == tile2.Y) ||
                (Math.Abs(tile1.Y - tile2.Y) == 1 && tile1.X == tile2.X);
    }

    private void RemoveSelectedTiles()
    {
        foreach (var tile in _selectedTiles)
        {
            _tiles.Remove(tile);
        }
        _selectedTiles.Clear();

        if (_tiles.Count == 0)
        {
            _isCompleted = true;
        }
    }

    public override void Update(float deltaTime)
    {
        var now = DateTime.Now;
        if ((now - _lastUpdate).TotalMilliseconds > 1000)
        {
            _lastUpdate = now;
        }
    }

    public override void Dispose()
    {
        _tiles.Clear();
        _selectedTiles.Clear();
    }
}