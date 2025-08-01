using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class NewPatchPane : InfoPane
{
    private readonly List<PatchNote> _patchNotes = [];

    public NewPatchPane()
    {
        _title = "Patch Notes";
        InitializePatchNotes();
    }

    private void InitializePatchNotes()
    {
        _patchNotes.Add(new PatchNote { Version = "1.0.1", Date = "2024-01-15", Description = "Bug fixes and performance improvements" });
        _patchNotes.Add(new PatchNote { Version = "1.0.2", Date = "2024-01-20", Description = "New quest system added" });
        _patchNotes.Add(new PatchNote { Version = "1.0.3", Date = "2024-01-25", Description = "Balance changes and new items" });
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Recent Updates:", 70, 100, Color.Black);

        for (var i = 0; i < _patchNotes.Count; i++)
        {
            var note = _patchNotes[i];
            //spriteBatch.DrawString(font, $"v{note.Version} ({note.Date})", 70, 120 + i * 40, Color.Black);
            //spriteBatch.DrawString(font, note.Description, 90, 140 + i * 40, Color.Black);
        }
    }
}