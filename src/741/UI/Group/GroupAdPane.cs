using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class GroupAdPane : ControlPane
{
    private readonly List<GroupAd> _ads = [];
    private TextButtonExControlPane _refreshButton;

    public GroupAdPane()
    {
        _refreshButton = new TextButtonExControlPane("Refresh");
        _refreshButton.Position = new Point(400, 50);
        _refreshButton.Click += (s, e) => RefreshAds();
        AddChild(_refreshButton);
    }

    public void AddAd(GroupAd ad)
    {
        _ads.Add(ad);
    }

    private void RefreshAds()
    {
        _ads.Clear();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Group Advertisements", 400, 20, Color.Black);

        for (var i = 0; i < _ads.Count; i++)
        {
            var ad = _ads[i];
            //spriteBatch.DrawString(font, $"{ad.GroupName} - {ad.Message}", 400, 80 + i * 20, Color.Black);
        }
            
        base.Render(spriteBatch);
    }
}