using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Macro;

public class MacroDialog : ControlPane
{
    private readonly List<MacroDefinition> _macros = [];
    private readonly List<TextButtonExControlPane> _macroButtons = [];
    private TextButtonExControlPane _createButton;
    private TextButtonExControlPane _editButton;
    private TextButtonExControlPane _deleteButton;
    private TextButtonExControlPane _closeButton;
    private MacroDefinition? _selectedMacro;

    public event EventHandler<MacroDefinition> MacroCreated;
    public event EventHandler<MacroDefinition> MacroEdited;
    public event EventHandler<MacroDefinition> MacroDeleted;

    public MacroDialog()
    {
        InitializeControls();
        LoadMacros();
    }

    private void InitializeControls()
    {
        _createButton = new TextButtonExControlPane("Create Macro");
        _editButton = new TextButtonExControlPane("Edit Macro");
        _deleteButton = new TextButtonExControlPane("Delete Macro");
        _closeButton = new TextButtonExControlPane("Close");

        _createButton.Position = new Point(50, 350);
        _editButton.Position = new Point(150, 350);
        _deleteButton.Position = new Point(250, 350);
        _closeButton.Position = new Point(350, 350);

        _createButton.Click += (s, e) => CreateMacro();
        _editButton.Click += (s, e) => EditMacro();
        _deleteButton.Click += (s, e) => DeleteMacro();
        _closeButton.Click += (s, e) => Hide();

        AddChild(_createButton);
        AddChild(_editButton);
        AddChild(_deleteButton);
        AddChild(_closeButton);
    }

    private void LoadMacros()
    {
        _macros.Add(new MacroDefinition { Id = 1, Name = "Heal Macro", Description = "Uses healing spell" });
        _macros.Add(new MacroDefinition { Id = 2, Name = "Attack Macro", Description = "Basic attack sequence" });
        _macros.Add(new MacroDefinition { Id = 3, Name = "Buff Macro", Description = "Applies buffs" });

        UpdateMacroButtons();
    }

    private void UpdateMacroButtons()
    {
        foreach (var button in _macroButtons)
        {
            RemoveChild(button);
        }
        _macroButtons.Clear();

        for (var i = 0; i < _macros.Count; i++)
        {
            var macro = _macros[i];
            var button = new TextButtonExControlPane($"{macro.Name} - {macro.Description}");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectMacro(macro);
            _macroButtons.Add(button);
            AddChild(button);
        }
    }

    private void SelectMacro(MacroDefinition macro)
    {
        _selectedMacro = macro;
    }

    private void CreateMacro()
    {
        var macro = new MacroDefinition
        {
            Id = _macros.Count + 1,
            Name = "New Macro",
            Description = "Macro description"
        };

        _macros.Add(macro);
        UpdateMacroButtons();
        MacroCreated?.Invoke(this, macro);
    }

    private void EditMacro()
    {
        if (_selectedMacro != null)
        {
            MacroEdited?.Invoke(this, _selectedMacro);
        }
    }

    private void DeleteMacro()
    {
        if (_selectedMacro != null)
        {
            _macros.Remove(_selectedMacro);
            UpdateMacroButtons();
            MacroDeleted?.Invoke(this, _selectedMacro);
            _selectedMacro = null;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(30, 30, 450, 400);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, "Macro Manager", 200, 50, Color.Black);
            
        base.Render(spriteBatch);
    }
}