using System;
using System.Text;
using System.Numerics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using System.Drawing;
using System.Collections.Generic;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.UI;

/// <summary>
/// Dialog pane for displaying skill and spell information.
/// Shows detailed information about skills and spells including descriptions, requirements, and effects.
/// </summary>
public class SkillSpellInfoDialogPane : DialogPane
{
    private const int MAX_DESCRIPTION_LINES = 32;
    private const int MAX_SUB_DESCRIPTIONS = 5;
    private const int DESCRIPTION_BUFFER_SIZE = 0x8000;
        
    private bool isSpell;
    private int skillId;
    private int spellId;
    private string skillName;
    private string skillDescription;
    private string[] subDescriptions;
    private int[] subDescriptionFlags;
    private int[] subDescriptionPositions;
    private int[] subDescriptionColors;
    private int namePositionX;
    private int namePositionY;
    private int descriptionPositionX;
    private int descriptionPositionY;
    private int subDescriptionCount;
    private string iconFileName;
    private bool isVisible;
    private bool isActive;
        
    /// <summary>
    /// Initializes a new instance of the SkillSpellInfoDialogPane.
    /// </summary>
    /// <param name="isSpell">True if displaying spell info, false for skill info</param>
    /// <param name="id">The skill or spell ID</param>
    public SkillSpellInfoDialogPane(bool isSpell, int id)
    {
        InitializeVTable();
        InitializeDefaultProperties();
            
        this.isSpell = isSpell;
        if (isSpell)
        {
            spellId = id;
            skillId = 0;
        }
        else
        {
            skillId = id;
            spellId = 0;
        }
            
        LoadSkillSpellData();
    }
        
    /// <summary>
    /// Initializes the virtual function table pointers.
    /// </summary>
    private void InitializeVTable()
    {
        // Set vtable pointers (equivalent to C++ vftable assignment)
        // In C#, this is handled by the inheritance hierarchy
    }
        
    /// <summary>
    /// Initializes default properties for the dialog pane.
    /// </summary>
    private void InitializeDefaultProperties()
    {
        isVisible = false;
        isActive = false;
        skillName = string.Empty;
        skillDescription = string.Empty;
        subDescriptions = new string[MAX_SUB_DESCRIPTIONS];
        subDescriptionFlags = new int[MAX_SUB_DESCRIPTIONS];
        subDescriptionPositions = new int[MAX_SUB_DESCRIPTIONS * 4]; // x, y, width, height
        subDescriptionColors = new int[MAX_SUB_DESCRIPTIONS * 4]; // r, g, b, a
        subDescriptionCount = 0;
            
        // Initialize positions
        namePositionX = 0;
        namePositionY = 0;
        descriptionPositionX = 0;
        descriptionPositionY = 0;
            
        // Set default icon file
        iconFileName = isSpell ? "spell001.epf" : "skill001.epf";
            
        // Set default size
        SetSize(400, 300);
    }
        
    /// <summary>
    /// Loads skill or spell data from the game database.
    /// </summary>
    private void LoadSkillSpellData()
    {
        try
        {
            if (isSpell)
            {
                LoadSpellData(spellId);
            }
            else
            {
                LoadSkillData(skillId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load skill/spell data: {ex.Message}");
            LoadDefaultData();
        }
    }
        
    /// <summary>
    /// Loads spell data from the database.
    /// </summary>
    /// <param name="spellId">The spell ID to load</param>
    private void LoadSpellData(int spellId)
    {
        // Load spell information from database
        var spellData = SpellDatabase.GetSpell(spellId);
        if (spellData != null)
        {
            skillName = spellData.Name;
            skillDescription = spellData.Description;
            subDescriptionCount = Math.Min(spellData.SubDescriptions.Length, MAX_SUB_DESCRIPTIONS);
                
            for (var i = 0; i < subDescriptionCount; i++)
            {
                subDescriptions[i] = spellData.SubDescriptions[i];
                subDescriptionFlags[i] = spellData.SubDescriptionFlags[i];
                    
                // Set positions and colors
                subDescriptionPositions[i * 4] = spellData.Positions[i].X;
                subDescriptionPositions[i * 4 + 1] = spellData.Positions[i].Y;
                subDescriptionPositions[i * 4 + 2] = spellData.Positions[i].Width;
                subDescriptionPositions[i * 4 + 3] = spellData.Positions[i].Height;
                    
                subDescriptionColors[i * 4] = spellData.Colors[i].R;
                subDescriptionColors[i * 4 + 1] = spellData.Colors[i].G;
                subDescriptionColors[i * 4 + 2] = spellData.Colors[i].B;
                subDescriptionColors[i * 4 + 3] = spellData.Colors[i].A;
            }
        }
    }
        
    /// <summary>
    /// Loads skill data from the database.
    /// </summary>
    /// <param name="skillId">The skill ID to load</param>
    private void LoadSkillData(int skillId)
    {
        // Load skill information from database
        var skillData = SkillDatabase.GetSkill(skillId);
        if (skillData != null)
        {
            skillName = skillData.Name;
            skillDescription = skillData.Description;
            subDescriptionCount = Math.Min(skillData.SubDescriptions.Length, MAX_SUB_DESCRIPTIONS);
                
            for (var i = 0; i < subDescriptionCount; i++)
            {
                subDescriptions[i] = skillData.SubDescriptions[i];
                subDescriptionFlags[i] = skillData.SubDescriptionFlags[i];
                    
                // Set positions and colors
                subDescriptionPositions[i * 4] = skillData.Positions[i].X;
                subDescriptionPositions[i * 4 + 1] = skillData.Positions[i].Y;
                subDescriptionPositions[i * 4 + 2] = skillData.Positions[i].Width;
                subDescriptionPositions[i * 4 + 3] = skillData.Positions[i].Height;
                    
                subDescriptionColors[i * 4] = skillData.Colors[i].R;
                subDescriptionColors[i * 4 + 1] = skillData.Colors[i].G;
                subDescriptionColors[i * 4 + 2] = skillData.Colors[i].B;
                subDescriptionColors[i * 4 + 3] = skillData.Colors[i].A;
            }
        }
    }
        
    /// <summary>
    /// Loads default data when database loading fails.
    /// </summary>
    private void LoadDefaultData()
    {
        skillName = isSpell ? "Unknown Spell" : "Unknown Skill";
        skillDescription = "No description available.";
        subDescriptionCount = 0;
    }
        
    /// <summary>
    /// Handles input events for the dialog pane.
    /// </summary>
    /// <param name="inputEvent">The input event to process</param>
    /// <returns>True if the event was handled, false otherwise</returns>
    public override bool HandleInput(Event inputEvent)
    {
        if (!isVisible || !isActive)
            return false;
                
        // Handle key events
        if (inputEvent is KeyEvent keyEvent)
        {
            return HandleKeyEvent(keyEvent);
        }
            
        // Handle mouse events
        if (inputEvent is MouseEvent mouseEvent)
        {
            return HandleMouseEvent(mouseEvent);
        }
            
        return base.HandleInput(inputEvent);
    }
        
    /// <summary>
    /// Handles key events for the dialog pane.
    /// </summary>
    /// <param name="keyEvent">The key event to process</param>
    /// <returns>True if the event was handled, false otherwise</returns>
    private bool HandleKeyEvent(KeyEvent keyEvent)
    {
        if (keyEvent.Type != EventType.KeyDown)
            return false;
                
        switch ((int)keyEvent.Key)
        {
        case 27: // Escape key
            CloseDialog();
            return true;
                    
        case 13: // Enter key
            CloseDialog();
            return true;
                    
        case 32: // Space key
            CloseDialog();
            return true;
        }
            
        return false;
    }
        
    /// <summary>
    /// Handles mouse events for the dialog pane.
    /// </summary>
    /// <param name="mouseEvent">The mouse event to process</param>
    /// <returns>True if the event was handled, false otherwise</returns>
    private bool HandleMouseEvent(MouseEvent mouseEvent)
    {
        if (mouseEvent.Type == EventType.MouseDown && mouseEvent.Button == Core.Events.MouseButton.Left)
        {
            // Check if click is outside the dialog area
            if (!IsPointInBounds(mouseEvent.X, mouseEvent.Y))
            {
                CloseDialog();
                return true;
            }
        }
            
        return false;
    }
        
    /// <summary>
    /// Closes the dialog and hides it.
    /// </summary>
    private void CloseDialog()
    {
        isVisible = false;
        isActive = false;
            
        // Notify parent that dialog is closed
        OnDialogClosed();
    }
        
    /// <summary>
    /// Shows the dialog with the current skill/spell information.
    /// </summary>
    public void ShowDialog()
    {
        isVisible = true;
        isActive = true;
            
        // Update layout
        UpdateLayout();
            
        // Notify parent that dialog is shown
        OnDialogShown();
    }
        
    /// <summary>
    /// Updates the dialog layout and positioning.
    /// </summary>
    private void UpdateLayout()
    {
        // Calculate positions based on dialog size
        namePositionX = X + 20;
        namePositionY = Y + 20;
        descriptionPositionX = X + 20;
        descriptionPositionY = Y + 60;
            
        // Update sub-description positions
        for (var i = 0; i < subDescriptionCount; i++)
        {
            subDescriptionPositions[i * 4] = descriptionPositionX;
            subDescriptionPositions[i * 4 + 1] = descriptionPositionY + (i + 1) * 25;
            subDescriptionPositions[i * 4 + 2] = Width - 40;
            subDescriptionPositions[i * 4 + 3] = 20;
        }
    }
        
    /// <summary>
    /// Renders the dialog pane and its contents.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    public override void Render(SpriteBatch spriteBatch)
    {
        if (!isVisible)
            return;
                
        // Render dialog background
        RenderDialogBackground(spriteBatch);
            
        // Render skill/spell icon
        RenderIcon(spriteBatch);
            
        // Render skill/spell name
        RenderName(spriteBatch);
            
        // Render main description
        RenderDescription(spriteBatch);
            
        // Render sub-descriptions
        RenderSubDescriptions(spriteBatch);
    }
        
    /// <summary>
    /// Renders the dialog background.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    private void RenderDialogBackground(SpriteBatch spriteBatch)
    {
        // Create background rectangle
        var backgroundRect = new System.Drawing.Rectangle(X, Y, Width, Height);
            
        // Fill with semi-transparent background
        spriteBatch.FillRectangle(backgroundRect, System.Drawing.Color.FromArgb(128, 0, 0, 0));
            
        // Draw border
        var borderRect = new System.Drawing.Rectangle(X, Y, Width, Height);
        spriteBatch.DrawRectangle(borderRect, System.Drawing.Color.White);
    }
        
    /// <summary>
    /// Renders the skill/spell icon.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    private void RenderIcon(SpriteBatch spriteBatch)
    {
        // Load and render the appropriate icon
        var iconPath = GetIconPath();
        if (!string.IsNullOrEmpty(iconPath))
        {
            var iconRect = new System.Drawing.Rectangle(X + 10, Y + 10, 32, 32);
            //spriteBatch.DrawImage(iconPath, iconRect); // This needs to be implemented
        }
    }
        
    /// <summary>
    /// Gets the path to the icon file.
    /// </summary>
    /// <returns>Path to the icon file</returns>
    private string GetIconPath()
    {
        var iconId = isSpell ? spellId : skillId;
        return $"{iconFileName}#{iconId}";
    }
        
    /// <summary>
    /// Renders the skill/spell name.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    private void RenderName(SpriteBatch spriteBatch)
    {
        if (!string.IsNullOrEmpty(skillName))
        {
            var font = FontManager.GetFont("default") as SimpleFont;
            if (font != null)
            {
                spriteBatch.DrawString(font, skillName, new Vector2(namePositionX, namePositionY), System.Drawing.Color.White);
            }
        }
    }
        
    /// <summary>
    /// Renders the main description.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    private void RenderDescription(SpriteBatch spriteBatch)
    {
        if (!string.IsNullOrEmpty(skillDescription))
        {
            var font = FontManager.GetFont("default") as SimpleFont;
            if (font == null) return;

            // Split description into lines for proper rendering
            string[] lines = SplitDescriptionIntoLines(skillDescription);
                
            for (var i = 0; i < lines.Length && i < MAX_DESCRIPTION_LINES; i++)
            {
                var yPos = descriptionPositionY + i * 20;
                spriteBatch.DrawString(font, lines[i], new Vector2(descriptionPositionX, yPos), System.Drawing.Color.White);
            }
        }
    }
        
    /// <summary>
    /// Renders sub-descriptions.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for rendering</param>
    private void RenderSubDescriptions(SpriteBatch spriteBatch)
    {
        var font = FontManager.GetFont("default") as SimpleFont;
        if (font == null) return;

        for (var i = 0; i < subDescriptionCount; i++)
        {
            if (!string.IsNullOrEmpty(subDescriptions[i]))
            {
                var x = subDescriptionPositions[i * 4];
                var y = subDescriptionPositions[i * 4 + 1];
                var colorInt = GetSubDescriptionColor(i);
                var color = System.Drawing.Color.FromArgb(colorInt);

                spriteBatch.DrawString(font, subDescriptions[i], new Vector2(x, y), color);
            }
        }
    }
        
    /// <summary>
    /// Splits description text into lines for rendering.
    /// </summary>
    /// <param name="description">The description text to split</param>
    /// <returns>Array of text lines</returns>
    private string[] SplitDescriptionIntoLines(string description)
    {
        // Simple line splitting - in practice, this would be more sophisticated
        return description.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    }
        
    /// <summary>
    /// Gets the color for a sub-description based on its flags.
    /// </summary>
    /// <param name="index">Index of the sub-description</param>
    /// <returns>Color value for the text</returns>
    private int GetSubDescriptionColor(int index)
    {
        if (index >= subDescriptionCount)
            return unchecked((int)0xFFFFFFFF);
                
        var r = subDescriptionColors[index * 4];
        var g = subDescriptionColors[index * 4 + 1];
        var b = subDescriptionColors[index * 4 + 2];
        var a = subDescriptionColors[index * 4 + 3];
            
        return (a << 24) | (r << 16) | (g << 8) | b;
    }
        
    /// <summary>
    /// Event raised when the dialog is closed.
    /// </summary>
    protected virtual void OnDialogClosed()
    {
        // Notify parent components
        var dialogEvent = new CustomEvent(EventType.DialogClosed, new DialogEventData
        {
            DialogType = DialogType.SkillSpellInfo,
            IsSpell = isSpell,
            Id = isSpell ? spellId : skillId
        });
        EventManager.Instance.DispatchEvent(dialogEvent);
    }
        
    /// <summary>
    /// Event raised when the dialog is shown.
    /// </summary>
    protected virtual void OnDialogShown()
    {
        // Notify parent components
        var dialogEvent = new CustomEvent(EventType.DialogShown, new DialogEventData
        {
            DialogType = DialogType.SkillSpellInfo,
            IsSpell = isSpell,
            Id = isSpell ? spellId : skillId
        });
        EventManager.Instance.DispatchEvent(dialogEvent);
    }
        
    /// <summary>
    /// Gets whether the dialog is currently visible.
    /// </summary>
    /// <returns>True if visible, false otherwise</returns>
    public bool IsVisible => isVisible;
        
    /// <summary>
    /// Gets whether the dialog is currently active.
    /// </summary>
    /// <returns>True if active, false otherwise</returns>
    public bool IsActive => isActive;
        
    /// <summary>
    /// Gets the skill/spell name.
    /// </summary>
    /// <returns>The skill/spell name</returns>
    public string GetName() => skillName;
        
    /// <summary>
    /// Gets the skill/spell description.
    /// </summary>
    /// <returns>The skill/spell description</returns>
    public string GetDescription() => skillDescription;
}