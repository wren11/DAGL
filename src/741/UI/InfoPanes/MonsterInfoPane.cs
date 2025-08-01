using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class MonsterInfoPane : InfoPane
{
    private MonsterInfo _currentMonster;

    public MonsterInfoPane()
    {
        _title = "Monster Information";
    }

    public void SetMonster(MonsterInfo monster)
    {
        _currentMonster = monster;
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        if (_currentMonster == null) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, $"Name: {_currentMonster.Name}", 70, 100, Color.Black);
        //spriteBatch.DrawString(font, $"Level: {_currentMonster.Level}", 70, 120, Color.Black);
        //spriteBatch.DrawString(font, $"HP: {_currentMonster.HP}/{_currentMonster.MaxHP}", 70, 140, Color.Black);
        //spriteBatch.DrawString(font, $"Attack: {_currentMonster.Attack}", 70, 160, Color.Black);
        //spriteBatch.DrawString(font, $"Defense: {_currentMonster.Defense}", 70, 180, Color.Black);
        //spriteBatch.DrawString(font, $"Location: {_currentMonster.Location}", 70, 200, Color.Black);
    }
}