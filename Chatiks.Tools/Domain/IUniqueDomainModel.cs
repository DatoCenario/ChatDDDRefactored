namespace Chatiks.Tools.Domain;

public interface IUniqueDomainModel<TIdentifier>
{
    TIdentifier? Id { get; }
    
    public bool IsNew()
    {
        return Id == null;
    }
}