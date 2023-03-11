namespace Chatiks.Tools.Domain;

public static class UniqueDomainModelExtensions
{
    public static ICollection<TModel> NotDeleted<TModel>(this ICollection<TModel> models)
        where TModel: UniqueDeletableDomainModelBase
    {
        return models.Where(m => !m.IsDeleted).ToList();
    }
    
    public static ICollection<TModel> Deleted<TModel>(this ICollection<TModel> models)
        where TModel: UniqueDeletableDomainModelBase
    {
        return models.Where(m => m.IsDeleted).ToList();
    }
}