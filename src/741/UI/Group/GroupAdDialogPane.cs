using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class GroupAdDialogPane : ControlPane
{
    private TextEditControlPane _nameInput;
    private TextEditControlPane _descriptionInput;
    private TextButtonExControlPane _createButton;
    private TextButtonExControlPane _cancelButton;

    public event EventHandler<GroupInfo> GroupJoined;

    public GroupAdDialogPane()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        _nameInput = new TextEditControlPane("", new Rectangle(100, 100, 200, 25));
        _descriptionInput = new TextEditControlPane("", new Rectangle(100, 140, 200, 60));
        _createButton = new TextButtonExControlPane("Create Group");
        _cancelButton = new TextButtonExControlPane("Cancel");

        _createButton.Position = new Point(100, 220);
        _cancelButton.Position = new Point(200, 220);

        _createButton.Click += (s, e) => CreateGroup();
        _cancelButton.Click += (s, e) => Hide();

        AddChild(_nameInput);
        AddChild(_descriptionInput);
        AddChild(_createButton);
        AddChild(_cancelButton);
    }

    private void CreateGroup()
    {
        var group = new GroupInfo
        {
            Name = _nameInput.Text,
            Description = _descriptionInput.Text,
            MemberCount = 1,
            MaxMembers = 20,
            LeaderName = "Player",
            IsPublic = true,
            CreatedDate = DateTime.Now
        };

        GroupJoined?.Invoke(this, group);
        Hide();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(50, 50, 300, 200);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, "Create Group", 150, 70, Color.Black);
        //spriteBatch.DrawString(font, "Name:", 50, 105, Color.Black);
        //spriteBatch.DrawString(font, "Description:", 50, 145, Color.Black);
            
        base.Render(spriteBatch);
    }
}