namespace Chatiks.Tools.Domain;

public abstract class UniqueDomainModelBase
{
    protected UniqueDomainModelBase(long? id)
    {
        Id = id;
    }

    public long? Id { get; }
    
    public bool IsNew()
    {
        return Id == null;
    }
}