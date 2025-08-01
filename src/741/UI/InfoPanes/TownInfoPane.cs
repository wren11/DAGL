using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class TownInfoPane : InfoPane
{
    private TownInfo _currentTown;

    public TownInfoPane()
    {
        _title = "Town Information";
    }

    public void SetTown(TownInfo town)
    {
        _currentTown = town;
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        if (_currentTown == null) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, $"Name: {_currentTown.Name}", 70, 100, Color.Black);
        //spriteBatch.DrawString(font, $"Population: {_currentTown.Population}", 70, 120, Color.Black);
        //spriteBatch.DrawString(font, $"Services: {_currentTown.Services}", 70, 140, Color.Black);
        //spriteBatch.DrawString(font, $"Description: {_currentTown.Description}", 70, 160, Color.Black);
    }
}