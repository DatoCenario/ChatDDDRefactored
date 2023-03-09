using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatMessageImageDomainModel: UniqueDeletableDomainModelBase
{
   public ChatMessageImageDomainModel(long? id, string base64Text) : base(id)
   {
      Base64Text = base64Text;
   }

   public string Base64Text { get; set; }
}