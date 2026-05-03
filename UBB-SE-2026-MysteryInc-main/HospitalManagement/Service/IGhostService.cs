using System;

namespace HospitalManagement.Service;

internal interface IGhostService
{
    public event EventHandler? ExorcismTriggered;

    public bool IsExorcismTriggered();

    public void SawAGhost();
}
