﻿namespace ImpactParry;

/// <summary> Class containing the cheat used for testing ImpactParry. </summary>
public class ImpactCheat : ICheat
{
    public string LongName => "Impact Parry Test";
    public string Identifier => "doomahreal.impactparrycheat";
    public string ButtonEnabledOverride => null;
    public string ButtonDisabledOverride => null;
    public string Icon => null;
    public bool DefaultState => false;
    public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;
    public bool IsActive { get; private set; }

    public void Enable(CheatsManager manager)
    {
        if (IsActive) return;
        IsActive = true;
        var mgr = ImpactManager.Instance;
        if (mgr != null) mgr.forceEffect = true;
        mgr?.EnableEffect();
    }

    public void Disable()
    {
        if (!IsActive) return;
        IsActive = false;
        var mgr = ImpactManager.Instance;
        if (mgr != null) mgr.forceEffect = false;
        mgr?.DisableEffect();
    }
}