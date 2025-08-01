using DarkAges.Library.GameLogic;
using DarkAges.Library.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace DarkAges.Library.UI;

public class UserShapeControlPane : ControlPane
{
    private readonly User _user;
    private readonly ImagePane _imagePane;
        
    public byte Direction { get; set; }
    public short AnimationFrame { get; set; }
    public short EmotionFrame { get; set; }

    public UserShapeControlPane(User user)
    {
        _user = user;
        _imagePane = new ImagePane();
        AddChild(_imagePane);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;
            
        if(_user.IsDirty)
        {
            var composedImage = HumanImageRenderer.Compose(_user, Direction, AnimationFrame, EmotionFrame);
            if (composedImage != null)
            {
                var basePalette = PaletteManager.GetPalette("default"); 
                var hairTable = ColoringTableManager.GetTable($"hair_{_user.HairColor}");
                var skinTable = ColoringTableManager.GetTable($"skin_{_user.SkinColor}");
                    
                var finalPalette = new Palette(basePalette);
                finalPalette.ApplyColoringTable(hairTable);
                finalPalette.ApplyColoringTable(skinTable);

                _imagePane.SetImage(composedImage, finalPalette);
            }
            _user.IsDirty = false;
        }

        base.Render(spriteBatch);
    }
}