using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class MonsterListPane : InfoPane
{
    private readonly List<MonsterInfo> _monsters = [];
    private readonly List<TextButtonExControlPane> _monsterButtons = [];

    public MonsterListPane()
    {
        _title = "Monster List";
        InitializeMonsters();
    }

    private void InitializeMonsters()
    {
        var monsterNames = new[] { "Goblin", "Orc", "Troll", "Dragon", "Skeleton", "Zombie" };
            
        for (var i = 0; i < monsterNames.Length; i++)
        {
            var monster = new MonsterInfo
            {
                Name = monsterNames[i],
                Level = i + 1,
                HP = 100 + i * 50,
                MaxHP = 100 + i * 50,
                Attack = 20 + i * 10,
                Defense = 10 + i * 5,
                Location = $"Area {i + 1}"
            };
            _monsters.Add(monster);

            var button = new TextButtonExControlPane($"{monster.Name} (Lv.{monster.Level})");
            button.Position = new Point(70, 100 + i * 30);
            _monsterButtons.Add(button);
            AddChild(button);
        }
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Available Monsters:", 70, 80, Color.Black);
    }
}