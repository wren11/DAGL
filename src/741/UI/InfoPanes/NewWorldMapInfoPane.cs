using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class NewWorldMapInfoPane : InfoPane
{
    private WorldMapInfo _currentMap;

    public NewWorldMapInfoPane()
    {
        _title = "World Map Information";
    }

    public void SetMap(WorldMapInfo map)
    {
        _currentMap = map;
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        if (_currentMap == null) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, $"Region: {_currentMap.Region}", 70, 100, Color.Black);
        //spriteBatch.DrawString(font, $"Size: {_currentMap.Width}x{_currentMap.Height}", 70, 120, Color.Black);
        //spriteBatch.DrawString(font, $"Towns: {_currentMap.TownCount}", 70, 140, Color.Black);
        //spriteBatch.DrawString(font, $"Dungeons: {_currentMap.DungeonCount}", 70, 160, Color.Black);
    }
}