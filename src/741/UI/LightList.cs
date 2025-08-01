using System.Collections.Generic;

namespace DarkAges.Library.UI;

public class LightList
{
    private readonly List<LightSource> _lights = [];

    public void AddLight(LightSource light)
    {
        _lights.Add(light);
    }

    public void Clear()
    {
        _lights.Clear();
    }

    public void UpdateLights()
    {
        // The original code in sub_4AF0C0 seems to iterate through lights
        // and apply their effects to a lighting map or buffer.
        // This would require a more complex rendering pipeline to handle lighting.
    }
}