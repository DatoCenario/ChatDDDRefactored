using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatMessageImageDomainModel: UniqueDeletableDomainModelBase
{
   public ChatMessageImageDomainModel(long? id = null, string base64Text = null) : base(id)
   {
      Base64Text = base64Text;
   }

   public long? ImageExternalId { get; private set; }
   public string Base64Text { get; set; }
}