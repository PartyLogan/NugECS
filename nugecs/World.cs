using System.Reflection;

namespace nugecs;

public class World
{
    private bool _running = false;
    private int _maxEntities;
    private int _nextId = 0;
    private List<EntityID> _freeIDs = new List<EntityID>();
    private List<EntityID> _activeIDs = new List<EntityID>();
    private EntityID[] _entityIDs;

    private Dictionary<Type, ComponentMapper> _componentMappers = new Dictionary<Type, ComponentMapper>();
    private List<ComponentMapper> _updaters = new List<ComponentMapper>();
    private List<ComponentMapper> _renderers = new List<ComponentMapper>();

    public World(int maxEntities = 10_000)
    {
        _maxEntities = maxEntities;

        _entityIDs = new EntityID[maxEntities];
        _freeIDs.Capacity = maxEntities;
        _activeIDs.Capacity = maxEntities;
        Console.WriteLine($"World created with max entities: {_maxEntities}");
    }

    public void Init()
    {
        if (_running) return;
        foreach (var cm in _componentMappers.Values)
        {
            cm.Init();
        }

        _running = true;
    }

    public void Update()
    {
        foreach (var cm in _updaters)
        {
            cm.Update();
        }
    }

    public void Render()
    {
        foreach (var cm in _renderers)
        {
            cm.Render();
        }
    }

    public int MaxEntities()
    {
        return _maxEntities;
    }

    public int ActiveEntities()
    {
        return _activeIDs.Count;
    }

    public void RegisterComponent<T>() where T : Component
    {
        Console.WriteLine($"Registering component of type: {typeof(T)}");
        var mapper = new ComponentMapper(typeof(T), _maxEntities);
        _componentMappers[typeof(T)] = mapper;

        bool updates = typeof(IUpdater).IsAssignableFrom(typeof(T));
        bool renders = typeof(IRenderer).IsAssignableFrom(typeof(T));
        if (updates)
        {
            _updaters.Add(mapper);
        }

        if (renders)
        {
            _renderers.Add(mapper);
        }
    }

    public ComponentMapper GetComponentMapper<T>() where T : Component
    {
        if (_componentMappers.TryGetValue(typeof(T), out var mapper))
        {
            return (ComponentMapper)mapper;
        }
        else
        {
            return null;
        }
    }

    public ComponentMapper GetComponentMapper(Type type)
    {
        foreach (var key in _componentMappers.Keys)
        {
            if (key == type)
            {
                ComponentMapper value = _componentMappers[key] as ComponentMapper;
                return value;
            }
        }
        return null;
    }

    public Dictionary<Type, ComponentMapper> GetComponentMappers()
    {
        return _componentMappers;
    }

    public T GetComponent<T>(EntityID entity) where T : Component
    {
        var mapper = GetComponentMapper<T>();
        var comp = mapper.GetComponent(entity);
        return comp as T;
    }
    
    public void AddComponent<T>(EntityID entity, T component) where T : Component
    {
        //Console.WriteLine($"Adding component {component} to {entity}");
        if (!_componentMappers.ContainsKey(typeof(T)))
        {
            RegisterComponent<T>();
        }
        var mapper = GetComponentMapper<T>();
        mapper.AddComponent(entity, component);
        component.SetOwner(entity);
        component.SetWorld(this);

        if (_running)
        {
            component.Init();
        }
    }

    public void RemoveComponent<T>(EntityID entity) where T : Component
    {
        //Console.WriteLine($"Removing component of type {typeof(T)} from {entity}");
        var mapper = GetComponentMapper<T>();
        mapper.RemoveComponent(entity);
    }

    public void RemoveEntityFromMappers(EntityID entity)
    {
        //Console.WriteLine($"Deleting all components from {entity}");
        foreach (var kvp in _componentMappers)
        {
            var mapper = kvp.Value;
            mapper.RemoveComponent(entity);
        }
    }

    public EntityID CreateEntity()
    {
        if (_activeIDs.Count == _activeIDs.Capacity)
        {
            // We full fool
            Console.WriteLine("Max entitites met!");
            return new EntityID();
        }
        if (_freeIDs.Count > 0)
        {
            var recycledID = _freeIDs.Last<EntityID>();
            _freeIDs.Remove(recycledID);
            recycledID.Generation += 1;
            _activeIDs.Add(recycledID);
            _entityIDs[recycledID.Index] = recycledID;
            //Console.WriteLine($"Recycled ID: {recycledID}");
            return recycledID;
        }
        var newID = new EntityID(_nextId++, 0);
        _activeIDs.Add(newID);
        _entityIDs[newID.Index] = newID;
        //Console.WriteLine($"New ID: {newID}");
        return newID;
    }

    public bool DeleteEntity(EntityID id)
    {
        if (_activeIDs.Contains(id))
        {
            //Console.WriteLine($"Deleted: {id}");
            _activeIDs.Remove(id);
            _freeIDs.Add(id);
            RemoveEntityFromMappers(id);
            return true;
        }
        Console.WriteLine($"Tried to delete non active Entity: {id} ");
        return false; // Not in the actives so it fails
    }

    public EntityID GetLastActive()
    {
        return _activeIDs.Last();
    }

    public bool IsLive(EntityID id)
    {
        if (_activeIDs.Contains(id))
        {
            return _entityIDs[id.Index] == id;
        }
        Console.WriteLine($"Attempted to check non active entity: {id}");
        return false;
    }



    /// <summary>
    ///  This is only for testing stuff
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public EntityID GetEntityFromIndex(int index)
    {
        foreach (var id in _activeIDs)
        {
            if (id.Index == index)
            {
                return id;
            }
        }
        Console.WriteLine($"Entity not active at index: {index}");
        return new EntityID();
    }
}
