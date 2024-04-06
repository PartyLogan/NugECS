using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;


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
    private Dictionary<Type, object> _resources = new Dictionary<Type, object>();

    private TimeResource _time = new TimeResource();
    public TimeResource Time { get => _time; private set => _time = value; }

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

    public void SetFixedUpdate(bool value)
    {
        _fixedUpdate = value;
    }
    
    public void Init()
    {
        if (_running) return;
        
        RegisterResource<TimeResource>(_time);
        _fixedUpdateCheck = 1.0 / _fixedFps; // Set the amount of delta time need to pass for an update
        _time.FixedDelta = (float)_fixedUpdateCheck;
        
        foreach (var cm in _componentMappers.Values)
        {
            cm.Init();
        }

        _running = true;
    }

    public void Update(float delta)
    {
        _time.Delta = delta;
        _time.FixedDelta = (float)_fixedUpdateCheck;
        
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
                    cm.Update(_time.FixedDelta);
                }
            }
        }
        else
        {
            foreach (var cm in _updaters)
            {
                cm.Update(_time.Delta);
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
    
    public string DebugFixedUpdateString()
    {
        return $"Update FPS: {_fixedFps} - Updates called last: {_updatesProcessedLast}";
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
    
    /// <summary>
    /// Can only have one resource of a type registered.
    /// </summary>
    /// <param name="resource"></param>
    /// <typeparam name="T"></typeparam>
    public void RegisterResource<T>(T resource)
    {
        _resources.TryAdd(typeof(T), resource);
    }

    public T GetResource<T>()
    {
        object res;
        _resources.TryGetValue(typeof(T), out res);
        return (T)res;
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
                ComponentMapper value = _componentMappers[key];
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
        return (T)comp;
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
    ///  Slightly faster but has WAY more allocations and thus GC spikes
    /// </summary>
    /// <param name="has"></param>
    /// <returns></returns>
    public Dictionary<Type, Component[]> Query(Type[] has, Type[] doesNotHave)
    {
        var result = new Dictionary<Type, Component[]>();

        // Pre-fetch all EntityIDs for each 'has' type.
        var hasIdSets = has.Select(t => new HashSet<EntityID>(_componentMappers[t].GetIDs())).ToList();

        // Pre-fetch all EntityIDs to exclude from 'doesNotHave' types.
        var excludeIds = new HashSet<EntityID>(doesNotHave.SelectMany(d => _componentMappers[d].GetIDs()));

        // Start with all IDs from the first 'has' set, then intersect with the rest to find common IDs.
        // Exclude the 'doesNotHave' IDs from this resulting set.
        var remainingIds = hasIdSets
            .Aggregate((current, next) => { current.IntersectWith(next); return current; })
            .Where(id => !excludeIds.Contains(id))
            .ToList();

        // Now, iterate over the 'has' types once to fill the dictionary.
        foreach (var type in has)
        {
            var mapper = _componentMappers[type];
            var components = remainingIds.Select(id => mapper.GetComponent(id)).ToList();
            result[type] = components.ToArray();
        }

        return result;
    }
    
    public Dictionary<Type, Component[]> QueryLessAlloc(Type[] has, Type[] doesNotHave)
    {
        var result = new Dictionary<Type, Component[]>();

        if (has == null || has.Length == 0)
        {
            return result;
        }

        // Initialize commonIds with the IDs from the first 'has' type, if available.
        HashSet<EntityID> commonIds = new HashSet<EntityID>(_componentMappers[has[0]].GetIDs());

        // Intersect commonIds with the IDs from the remaining 'has' types.
        for (int i = 1; i < has.Length; i++)
        {
            var ids = new HashSet<EntityID>(_componentMappers[has[i]].GetIDs());
            commonIds.IntersectWith(ids);
        }

        // If there are 'doesNotHave' types, exclude their IDs from commonIds.
        if (doesNotHave != null)
        {
            foreach (var type in doesNotHave)
            {
                var excludeIds = _componentMappers[type].GetIDs();
                commonIds.RemoveWhere(id => excludeIds.Contains(id));
            }
        }

        // Now that we have the filtered commonIds, populate the result dictionary.
        foreach (var type in has)
        {
            var mapper = _componentMappers[type];
            var componentList = new List<Component>(commonIds.Count); // Allocate with known capacity to minimize resizing.
        
            foreach (var id in commonIds)
            {
                componentList.Add(mapper.GetComponent(id));
            }

            result.Add(type, componentList.ToArray());
        }

        return result;
    }

    
    public Dictionary<Type, Component[]> QueryLessAlloc(Type[] has)
    {
        var result = new Dictionary<Type, Component[]>();

        if (has == null || has.Length == 0)
        {
            return result;
        }

        HashSet<EntityID> commonIds = null;

        // Directly work with the first set of IDs and intersect in-place.
        foreach (var type in has)
        {
            var ids = _componentMappers[type].GetIDs();
            if (commonIds == null)
            {
                commonIds = new HashSet<EntityID>(ids);
            }
            else
            {
                commonIds.IntersectWith(ids);
            }
        }

        if (commonIds == null || commonIds.Count == 0)
        {
            return result; // No common entities found.
        }

        // Convert the hash set to a list once after all intersections are done.
        var commonIdList = commonIds.ToList();

        // Avoid LINQ for component retrieval to minimize allocations.
        foreach (var type in has)
        {
            var mapper = _componentMappers[type];
            var components = new List<Component>(commonIdList.Count);
            foreach (var id in commonIdList)
            {
                components.Add(mapper.GetComponent(id));
            }
            result[type] = components.ToArray();
        }

        return result;
    }

    /// <summary>
    ///  Slightly faster but has WAY more allocations and thus GC spikes
    /// </summary>
    /// <param name="has"></param>
    /// <returns></returns>
    public Dictionary<Type, Component[]> Query(Type[] has)
    {
        var result = new Dictionary<Type, Component[]>();

        // Ensure there are types to process.
        if (has == null || has.Length == 0)
        {
            return result;
        }

        // Pre-fetch all EntityIDs for each 'has' type.
        var hasIdSets = has.Select(t => new HashSet<EntityID>(_componentMappers[t].GetIDs())).ToList();

        // Use the first set as the starting point for finding common IDs, if available.
        var remainingIds = new HashSet<EntityID>(hasIdSets.FirstOrDefault() ?? new HashSet<EntityID>());

        // Intersect with the rest to find common IDs.
        foreach (var idSet in hasIdSets.Skip(1))
        {
            remainingIds.IntersectWith(idSet);
        }

        // Convert back to a list after finding common IDs.
        var commonIds = remainingIds.ToList();

        // Now, iterate over the 'has' types once to fill the dictionary.
        foreach (var type in has)
        {
            var mapper = _componentMappers[type];
            var components = commonIds.Select(id => mapper.GetComponent(id)).ToList();
            result.Add(type, components.ToArray());
        }

        return result;
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

