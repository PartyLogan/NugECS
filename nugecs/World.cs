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
    private Dictionary<string, EntityID> _taggedEntities = new Dictionary<string, EntityID>();
    private List<EntityID> _toDelete = new List<EntityID>();
    private Transform[] _transforms;
    private Dictionary<Type, ComponentMapper> _componentMappers = new Dictionary<Type, ComponentMapper>();
    private List<ComponentMapper> _updaters = new List<ComponentMapper>();
    private List<ComponentMapper> _renderers = new List<ComponentMapper>();
    private Dictionary<Type, object> _resources = new Dictionary<Type, object>();
    private Dictionary<string, object> _taggedResources = new Dictionary<string, object>();

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
        
        RegisterResource<TimeResource>(_time, "Time");
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

    public void Maintain()
    {
        if (_toDelete.Any())
        {
            foreach (var e in _toDelete)
            {
                DeleteEntityNow(e);
            }

            _toDelete.Clear();
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
    public void RegisterResource<T>(T resource, string tag = "")
    {
        _resources.TryAdd(typeof(T), resource);
        if (tag.Length > 0)
        {
            _taggedResources.TryAdd(tag, resource);
        }
    }

    public void TagResource<T>(string tag)
    {
        if (_resources.ContainsKey(typeof(T)))
        {
            _taggedResources.TryAdd(tag, _resources[typeof(T)]);
        }
    }

    public void UnregisterResource<T>()
    {
        var remove = false;
        object res = null;
        if (_resources.ContainsKey(typeof(T)))
        {
            res = _resources[typeof(T)];
            _resources.Remove(typeof(T));
            remove = true;

        }
        
        if (remove)
        {
            if (_taggedResources.ContainsValue(res))
            {
                var key = "";
                foreach (var r in _taggedResources)
                {
                    if (r.Value == res)
                    {
                        key = r.Key;
                        break;
                    }
                }

                if (key.Length > 0)
                {
                    _taggedResources.Remove(key);
                }
            }
        }
    }

    public T GetResource<T>()
    {
        object res;
        _resources.TryGetValue(typeof(T), out res);
        return (T)res;
    }

    public T GetTaggedResource<T>(string tag)
    {
        object res;
        var success = _taggedResources.TryGetValue(tag, out res);
        return (T)res;
    }
    
    public object GetTaggedResource(string tag)
    {
        object res;
        var success = _taggedResources.TryGetValue(tag, out res);
        if (!success)
        {
            return null;
        }
        return res;
    }

    public void UntagResource(string tag)
    {
        if(_taggedResources.ContainsKey(tag))
            _taggedResources.Remove(tag);
    }

    public void UntagEntity(string tag)
    {
        if(_taggedEntities.ContainsKey(tag))
            _taggedEntities.Remove(tag);
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
            return EntityID.Null().Clone();
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
            return recycledID.Clone();
        }
        var newID = new EntityID(_nextId++, 0);
        _activeIDs.Add(newID);
        _entityIDs[newID.Index] = newID;
        _transforms[newID.Index] = new Transform();
        //Console.WriteLine($"New ID: {newID}");
        return newID.Clone();
    }
    
    public EntityID CreateEntity(Transform transform)
    {
        var id = CreateEntity();
        if (id.Index != -1)
        {
            _transforms[id.Index] = transform;
        }
        return id.Clone();
    }
    
    public EntityID CreateEntity(float x, float y, float z, float scale = 1f, float rot = 0f)
    {
        var transform = new Transform(new Vector3(x, y, z), new Vector3(scale, scale, scale), rot);
        var id = CreateEntity();
        if (id.Index != -1)
        {
            _transforms[id.Index] = transform;
        }
        return id.Clone();
    }
    
    public EntityID CreateEntity(Vector3 pos, Vector3 scale, float rot = 0f)
    {
        var transform = new Transform(pos, scale, rot);
        var id = CreateEntity();
        if (id.Index != -1)
        {
            _transforms[id.Index] = transform;
        }
        return id.Clone();
    }

    public void TagEntity(EntityID entity, string tag)
    {
        _taggedEntities.TryAdd(tag, entity);
    }

    public EntityID GetTaggedEntity(string tag)
    {
        EntityID ent;
        var success = _taggedEntities.TryGetValue(tag, out ent);
        if (!success)
            return EntityID.Null();
        return ent.Clone();
    }

    public bool DeleteEntityNow(EntityID id)
    {
        if (_activeIDs.Contains(id))
        {
            //Console.WriteLine($"Deleted: {id}");
            _activeIDs.Remove(id);
            _freeIDs.Add(id);
            _transforms[id.Index] = new Transform();
            RemoveEntityFromMappers(id);
            if (_taggedEntities.ContainsValue(id))
            {
                var key = "";
                foreach (var k in _taggedEntities)
                {
                    if (k.Value == id)
                    {
                        key = k.Key;
                    }
                }

                _taggedEntities.Remove(key);
            }
            return true;
        }
        Console.WriteLine($"Tried to delete non active Entity: {id} ");
        return false; // Not in the actives so it fails
    }
    
    public bool DeleteEntity(EntityID id)
    {
        if (_activeIDs.Contains(id))
        {
            _toDelete.Add(id);
            return true;
        }
        Console.WriteLine($"Tried to delete non active Entity: {id} ");
        return false; // Not in the actives so it fails
    }

    public EntityID GetLastActive()
    {
        return _activeIDs.Last().Clone();
    }
    
    // TODO: I should probably use when things call into the World as a first check and a quicker fail. For instance getting components from EntityID
    public bool IsLive(EntityID id)
    {
        if (id.IsNull())
            return false;
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
                return id.Clone();
            }
        }
        Console.WriteLine($"Entity not active at index: {index}");
        return EntityID.Null().Clone();
    }
}

