namespace nugecs;

public class TimeResource
{
    //public float Delta = 0f;
    private float _fixedDelta = 1.0f / 60.0f;
    private float _timeMod = 1.0f;
    private float _delta;

    public float TimeMod
    {
        get => _timeMod;
        set => _timeMod = Math.Clamp(value, 0.01f, 5.0f);
    }
    public float FixedDelta
    {
        get => _fixedDelta * _timeMod;
        set => _fixedDelta = value;
    }
    public float Delta
    {
        get => _delta * TimeMod;
        set => _delta = value;
    }

    public override string ToString()
    {
        return $"T Mod: {_timeMod:0.###}, Fixed: {_fixedDelta*100:0.###}ms, Delta: {_delta * 100:0.###}ms";
    }
}