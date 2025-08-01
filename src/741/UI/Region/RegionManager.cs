using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Region;

public class RegionManager
{
    private readonly List<Region> _regions = [];
    private readonly Dictionary<int, Region> _regionMap = new Dictionary<int, Region>();
    private Region _currentRegion;
    private Region _previousRegion;

    public event EventHandler<RegionEventArgs> RegionChanged;
    public event EventHandler<RegionEventArgs> RegionAdded;
    public event EventHandler<RegionEventArgs> RegionRemoved;

    public Region CurrentRegion => _currentRegion;
    public Region PreviousRegion => _previousRegion;

    public void AddRegion(Region region)
    {
        if (region == null)
            return;

        _regions.Add(region);
        _regionMap[region.Id] = region;

        region.RegionActivated += (s, e) => OnRegionActivated(e);
        region.RegionDeactivated += (s, e) => OnRegionDeactivated(e);

        RegionAdded?.Invoke(this, new RegionEventArgs(region));
    }

    public void RemoveRegion(Region region)
    {
        if (region == null)
            return;

        _regions.Remove(region);
        _regionMap.Remove(region.Id);

        if (_currentRegion == region)
        {
            SwitchToRegion((Region)null);
        }

        RegionRemoved?.Invoke(this, new RegionEventArgs(region));
    }

    public Region GetRegion(int id)
    {
        return _regionMap.TryGetValue(id, out var region) ? region : null;
    }

    public Region GetRegion(string name)
    {
        return _regions.Find(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void SwitchToRegion(Region region)
    {
        if (_currentRegion == region)
            return;

        _previousRegion = _currentRegion;

        if (_currentRegion != null)
        {
            _currentRegion.Exit();
            _currentRegion.Deactivate();
        }

        _currentRegion = region;

        if (_currentRegion != null)
        {
            _currentRegion.Enter();
            _currentRegion.Activate();
        }

        RegionChanged?.Invoke(this, new RegionEventArgs(_currentRegion));
    }

    public void SwitchToRegion(int regionId)
    {
        var region = GetRegion(regionId);
        SwitchToRegion(region);
    }

    public void SwitchToRegion(string regionName)
    {
        var region = GetRegion(regionName);
        SwitchToRegion(region);
    }

    public void Update()
    {
        _currentRegion?.Update();
    }

    public void Render()
    {
        _currentRegion?.Render();
    }

    public bool HandleEvent(Event e)
    {
        return _currentRegion?.HandleEvent(e) ?? false;
    }

    private void OnRegionActivated(RegionEventArgs e)
    {
    }

    private void OnRegionDeactivated(RegionEventArgs e)
    {
    }

    public void Clear()
    {
        foreach (var region in _regions)
        {
            region.Dispose();
        }
        _regions.Clear();
        _regionMap.Clear();
        _currentRegion = null;
        _previousRegion = null;
    }

    public List<Region> GetAllRegions()
    {
        return [.._regions];
    }

    public List<Region> GetRegionsByType(string type)
    {
        return _regions.FindAll(r => string.Equals(r.GetType().Name, type, StringComparison.OrdinalIgnoreCase));
    }
}