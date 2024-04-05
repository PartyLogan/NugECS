namespace nugecs;

public abstract class Component
{
    protected World _world;
    protected EntityID _owner;
    public EntityID Owner
    {
        get => _owner;
    }

    public void SetWorld(World world)
    {
        _world = world;
    }
    
    public void SetOwner(EntityID id)
    {
        _owner = id;
    }
    public virtual void Init() { }
    // TODO: Add enabling and disabling
    public virtual void Enable() { }
    public virtual void Disable() { }
}



public interface IUpdater
{
    public void Update(float delta);
}

public interface IRenderer
{
    public void Render();
}

public class ComponentMapper
{
    private Type _type;
    private Component[] _components;
    private List<EntityID> _active = new List<EntityID>();
    
    public ComponentMapper(Type type, int maxEntities)
    {
        _type = type;
        _components = new Component[maxEntities];
        _active.Capacity = maxEntities;
    }

    public Component GetComponent(EntityID id)
    {
        return _components[id.Index];
    }
    
    public Type GetType()
    {
        return _type;
    }

    public void Init()
    {
        foreach (var e in _active)
        {
            _components[e.Index].Init();
        }
    }

    public void Update(float delta)
    {
        foreach (var e in _active)
        {
            IUpdater updater = _components[e.Index] as IUpdater;
            updater.Update(delta);
        }
    }

    public void Render()
    {
        foreach (var e in _active)
        {
            IRenderer renderer = _components[e.Index] as IRenderer;
            renderer.Render();
        }
    }

    public void AddComponent(EntityID entity, Component component)
    {
        if (_active.Contains(entity))
        {
            Console.WriteLine($"Entity {entity} already has component of type: {_type}");
            return;
        }
        _active.Add(entity);
        _components[entity.Index] = component;
    }

    public void RemoveComponent(EntityID entity)
    {
        if (_active.Contains(entity))
        {
            _active.Remove(entity);
            _components[entity.Index] = null;
        }
    }

}