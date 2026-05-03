using System;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal sealed class GhostService : IGhostService
{
    // private static readonly GhostService ServiceInstance = new();
    // TODO: Is this needed? Already registered as singleton in DI container

    private readonly List<DateTime> _sightings = [];

    public event EventHandler? ExorcismTriggered;

    public void SawAGhost()
    {
        CleanOldSightings();
        _sightings.Add(DateTime.Now);
        if (IsExorcismTriggered())
        {
            ExorcismTriggered?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsExorcismTriggered()
    {
        CleanOldSightings();
        return _sightings.Count > 3;
    }

    private void CleanOldSightings()
    {
        _ = _sightings.RemoveAll(s => s < DateTime.Now.AddHours(-24));
    }
}
