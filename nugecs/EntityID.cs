namespace nugecs;

public struct EntityID
{
    public int Index = -1;
    public int Generation = 0;

    public EntityID(int i = 0, int gen = 0)
    {
        Index = i;
        Generation = gen;
    }

    public EntityID()
    {

    }

    public static EntityID Null()
    {
        return new EntityID(-1, -1);
    }
    
    // Alternatively, you could implement a Clone method
    public EntityID Clone()
    {
        return new EntityID(this);
    }
    
    public EntityID(EntityID other)
    {
        // Copy all fields/properties from 'other' to this instance
        Index = other.Index;
        Generation = other.Generation;
    }
    
    public override string ToString()
    {
        return $"Entity ID - Index: {Index}, Gen: {Generation}";
    }

    public static bool operator ==(EntityID e1, EntityID e2)
    {
        return e1.Index == e2.Index && e1.Generation == e2.Generation;
    }

    public static bool operator !=(EntityID e1, EntityID e2)
    {
        return !(e1.Index == e2.Index && e1.Generation == e2.Generation);
    }

}
