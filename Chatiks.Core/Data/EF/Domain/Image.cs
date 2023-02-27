using Chatiks.Tools.EF;

namespace Chatiks.Core.Data.EF.Domain;

public class Image: IEntity
{
    public long Id { get; set; }
    
    public string Base64Text { get; set; }
}