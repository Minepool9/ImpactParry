public class sdiybtwiltedemoji : ICheat
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
        var mgr = notabigfanofthegovernment.Instance;
        if (mgr != null) mgr.EnableEffect();
    }

    public void Disable()
    {
        if (!IsActive) return;
        IsActive = false;
        var mgr = notabigfanofthegovernment.Instance;
        if (mgr != null) mgr.DisableEffect();
    }
}