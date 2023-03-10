namespace Chatiks.Tools.Domain;

public abstract class UniqueDomainModelBase
{
    protected UniqueDomainModelBase(long? id)
    {
        Id = id;
    }

    protected UniqueDomainModelBase()
    {
        
    }

    public long? Id { get; }
    
    public bool IsNew()
    {
        return Id == null;
    }
}

public abstract class UniqueDeletableDomainModelBase: UniqueDomainModelBase
{
    protected UniqueDeletableDomainModelBase(long? id) : base(id)
    {
    }
    
    protected UniqueDeletableDomainModelBase()
    {
    }

    public bool IsDeleted { get; private set; }
    
    public void Delete()
    {
        if (IsNew())
        {
            throw new Exception("Can't delete new entity");
        }
        
        IsDeleted = true;
    }
    
    public void ThrowOperationExceptionIfDeleted()
    {
        if (IsDeleted)
        {
            throw new Exception("Can't operate with deleted entity");
        }
    }
}