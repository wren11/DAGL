using System.Drawing;
using DarkAges.Library.IO;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.GameLogic;
using System;

namespace DarkAges.Library.UI;

public class CreateUserDialogPane : DialogPane
{
    private TextEditControlPane _nameInput;
    private TextEditControlPane _passwordInput;
    private TextEditControlPane _passwordConfirmInput;
    private TextEditControlPane _ncidInput;
    private TextEditControlPane _ncpwInput;
    private TextEditControlPane _emailInput;
        
    private RadioGroupControlPane _genderGroup;
    private TextButtonExControlPane _angleLeft, _angleRight;
    private TextButtonExControlPane _hairLeft, _hairRight;
    private TextButtonExControlPane _colorLeft, _colorRight;
        
    private UserShapeControlPane _characterPreview;
    private HumanImageRenderer _characterRenderer;
        
    private TextButtonExControlPane _createButton;
    private TextButtonExControlPane _cancelButton;
        
    private short _currentGender = 0;
    private short _currentAngle = 1;
    private short _currentHair = 1;
    private short _currentColor = 1;

    public event EventHandler<User> UserCreated;

    public CreateUserDialogPane()
    {
        LoadLayout();
        SetupControls();
        UpdateCharacterPreview();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_ncreate.txt");
                
            var nameRect = layout.GetRect("NameInput");
            var passwordRect = layout.GetRect("PasswordInput");
            var passwordConfirmRect = layout.GetRect("PasswordConfirmInput");
            var ncidRect = layout.GetRect("NCIDInput");
            var ncpwRect = layout.GetRect("NCPWInput");
            var emailRect = layout.GetRect("EmailInput");
                
            _nameInput = new TextEditControlPane("", nameRect);
            _passwordInput = new TextEditControlPane("", passwordRect);
            _passwordInput.IsPasswordField = true;
            _passwordConfirmInput = new TextEditControlPane("", passwordConfirmRect);
            _passwordConfirmInput.IsPasswordField = true;
            _ncidInput = new TextEditControlPane("", ncidRect);
            _ncpwInput = new TextEditControlPane("", ncpwRect);
            _ncpwInput.IsPasswordField = true;
            _emailInput = new TextEditControlPane("", emailRect);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
        }
    }

    private void SetupControls()
    {
        var maleButton = new TextButtonExControlPane("Male");
        var femaleButton = new TextButtonExControlPane("Female");
        _genderGroup = new RadioGroupControlPane(maleButton, femaleButton);
            
        _angleLeft = new TextButtonExControlPane("<");
        _angleRight = new TextButtonExControlPane(">");
        _hairLeft = new TextButtonExControlPane("<");
        _hairRight = new TextButtonExControlPane(">");
        _colorLeft = new TextButtonExControlPane("<");
        _colorRight = new TextButtonExControlPane(">");
            
        _createButton = new TextButtonExControlPane("Create");
        _cancelButton = new TextButtonExControlPane("Cancel");
            
        _characterRenderer = new HumanImageRenderer();
        _characterPreview = new UserShapeControlPane(null);
            
        _angleLeft.Click += (s, e) => { _currentAngle = (short)((_currentAngle - 1 + 8) % 8); UpdateCharacterPreview(); };
        _angleRight.Click += (s, e) => { _currentAngle = (short)((_currentAngle + 1) % 8); UpdateCharacterPreview(); };
        _hairLeft.Click += (s, e) => { _currentHair = (short)((_currentHair - 1 + 10) % 10); UpdateCharacterPreview(); };
        _hairRight.Click += (s, e) => { _currentHair = (short)((_currentHair + 1) % 10); UpdateCharacterPreview(); };
        _colorLeft.Click += (s, e) => { _currentColor = (short)((_currentColor - 1 + 10) % 10); UpdateCharacterPreview(); };
        _colorRight.Click += (s, e) => { _currentColor = (short)((_currentColor + 1) % 10); UpdateCharacterPreview(); };
            
        _createButton.Click += OnCreateClicked;
        _cancelButton.Click += OnCancelClicked;
            
        AddChild(_nameInput);
        AddChild(_passwordInput);
        AddChild(_passwordConfirmInput);
        AddChild(_ncidInput);
        AddChild(_ncpwInput);
        AddChild(_emailInput);
        AddChild(_genderGroup);
        AddChild(_angleLeft);
        AddChild(_angleRight);
        AddChild(_hairLeft);
        AddChild(_hairRight);
        AddChild(_colorLeft);
        AddChild(_colorRight);
        AddChild(_characterPreview);
        AddChild(_createButton);
        AddChild(_cancelButton);
    }

    private void UpdateCharacterPreview()
    {
        _characterRenderer.SetCharacter(_currentGender, _currentAngle, _currentHair, _currentColor);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        spriteBatch.DrawRectangle(Bounds, System.Drawing.Color.LightGray);
        spriteBatch.DrawRectangle(Bounds, System.Drawing.Color.Black);
            
        base.Render(spriteBatch);
    }

    private void OnCreateClicked(object sender, EventArgs e)
    {
        if (ValidateInput())
        {
            var user = new User
            {
                Name = _nameInput.Text,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(_passwordInput.Text),
                NCID = _ncidInput.Text,
                NCPassword = _ncpwInput.Text,
                Email = _emailInput.Text,
                Gender = _currentGender,
                HairStyle = _currentHair,
                HairColor = _currentColor
            };
                
            UserCreated?.Invoke(this, user);
            Close(1);
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(0);
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_nameInput.Text) || _nameInput.Text.Length < 3)
            return false;
                
        if (string.IsNullOrWhiteSpace(_passwordInput.Text) || _passwordInput.Text.Length < 6)
            return false;
                
        if (_passwordInput.Text != _passwordConfirmInput.Text)
            return false;
                
        if (string.IsNullOrWhiteSpace(_ncidInput.Text))
            return false;
                
        if (string.IsNullOrWhiteSpace(_ncpwInput.Text))
            return false;
                
        return true;
    }
}