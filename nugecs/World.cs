using System.Numerics;
using System.Reflection;

namespace nugecs;

public class World
{
    private bool _fixedUpdate = false;
    private double _frameTimeCount = 0.0; // Current delta happened for this update
    private int _fixedFps; // The fps update target
    private double _fixedUpdateCheck; // the time that once gone past a update is called
    private int _framesWithoutUpdate = 0;
    private int _updatesProcessedLast = 0;
    
    private bool _running = false;
    private int _maxEntities;
    private int _nextId = 0;
    private List<EntityID> _freeIDs = new List<EntityID>();
    private List<EntityID> _activeIDs = new List<EntityID>();
    private EntityID[] _entityIDs;
    private Transform[] _transforms;
    private Dictionary<Type, ComponentMapper> _componentMappers = new Dictionary<Type, ComponentMapper>();
    private List<ComponentMapper> _updaters = new List<ComponentMapper>();
    private List<ComponentMapper> _renderers = new List<ComponentMapper>();

    public World(int maxEntities = 10_000, bool fixedUpdate = false, int fixedFps = 60)
    {
        _maxEntities = maxEntities;
        _fixedUpdate = fixedUpdate;
        _fixedFps = fixedFps;
        
        _entityIDs = new EntityID[maxEntities];
        _transforms = new Transform[maxEntities];
        _freeIDs.Capacity = maxEntities;
        _activeIDs.Capacity = maxEntities;
        Console.WriteLine($"World created with max entities: {_maxEntities}");
    }
    
    public void Init()
    {
        if (_running) return;

        _fixedUpdateCheck = 1.0 / _fixedFps; // Set the amount of delta time need to pass for an update
        
        foreach (var cm in _componentMappers.Values)
        {
            cm.Init();
        }

        _running = true;
    }

    public void Update(double delta)
    {
        if (_fixedUpdate)
        {
            // Add the delta to the current frame count
            _frameTimeCount += delta;
            if (_frameTimeCount < _fixedUpdateCheck)
            {
                _framesWithoutUpdate += 1;
            }
            else
            {
                _framesWithoutUpdate = 0;
                _updatesProcessedLast = 0;
            }
                
            while (_frameTimeCount >= _fixedUpdateCheck) // If it is = to the time need or over
            {
                _updatesProcessedLast++;
                _frameTimeCount -= _fixedUpdateCheck; // Minus the one update call time
                foreach (var cm in _updaters)
                {
                    cm.Update((float)_fixedUpdateCheck);
                }
            }
        }
        else
        {
            foreach (var cm in _updaters)
            {
                cm.Update((float)delta);
            }
        }

    }

    public bool IsFixedUpdate()
    {
        return _fixedUpdate;
    }

    public int GetFramesWithoutUpdate()
    {
        return _framesWithoutUpdate;
    }

    public string DebugUpdateString()
    {
        return $"Update FPS: {_fixedFps} - Check Time: {_fixedUpdateCheck} - Updates called last: {_updatesProcessedLast}";
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
            Console.WriteLine("Max entities met!");
            return new EntityID();
        }
        if (_freeIDs.Count > 0)
        {
            var recycledID = _freeIDs.Last<EntityID>();
            _freeIDs.Remove(recycledID);
            recycledID.Generation += 1;
            _activeIDs.Add(recycledID);
            _entityIDs[recycledID.Index] = recycledID;
            _transforms[recycledID.Index] = new Transform();
            //Console.WriteLine($"Recycled ID: {recycledID}");
            return recycledID;
        }
        var newID = new EntityID(_nextId++, 0);
        _activeIDs.Add(newID);
        _entityIDs[newID.Index] = newID;
        _transforms[newID.Index] = new Transform();
        //Console.WriteLine($"New ID: {newID}");
        return newID;
    }
    
    public EntityID CreateEntity(Transform transform)
    {
        var id = CreateEntity();
        if (id.Index != -1)
        {
            _transforms[id.Index] = transform;
        }
        return id;
    }
    
    public EntityID CreateEntity(float x, float y, float scale = 1f, float rot = 0f)
    {
        var transform = new Transform(new Vector2(x, y), new Vector2(scale, scale), rot);
        var id = CreateEntity();
        if (id.Index != -1)
        {
            _transforms[id.Index] = transform;
        }
        return id;
    }

    public bool DeleteEntity(EntityID id)
    {
        if (_activeIDs.Contains(id))
        {
            //Console.WriteLine($"Deleted: {id}");
            _activeIDs.Remove(id);
            _freeIDs.Add(id);
            _transforms[id.Index] = new Transform();
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
    
    // TODO: I should probably use when things call into the World as a first check and a quicker fail. For instance getting components from EntityID
    public bool IsLive(EntityID id)
    {
        if (_activeIDs.Contains(id))
        {
            return _entityIDs[id.Index] == id;
        }
        Console.WriteLine($"Attempted to check non active entity: {id}");
        return false;
    }

    public Transform GetTransform(EntityID id)
    {
        return _transforms[id.Index];
    }

    public Transform[] GetTransforms()
    {
        return _transforms;
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
