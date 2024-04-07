namespace nugecs;

public class TimeResource
{
    //public float Delta = 0f;
    private float _fixedDelta = 1.0f / 60.0f;
    private float _timeScale = 1.0f;
    private float _delta;

    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Clamp(value, 0.1f, 5.0f);
    }
    public float FixedDelta
    {
        get => _fixedDelta * _timeScale;
        set => _fixedDelta = value;
    }
    public float Delta
    {
        get => _delta * TimeScale;
        set => _delta = value;
    }

    public override string ToString()
    {
        return $"Time - Scale: {_timeScale:0.##}, Fixed: {_fixedDelta*1000:0.##}ms, Delta: {_delta * 1000:0.##}ms";
    }
}