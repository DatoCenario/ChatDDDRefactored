using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Tools.EF;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Data.EF;

public class ChatContext : DbContext
{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {
    }

    public DbSet<Domain.Chat.Chat> Chats { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatRole> ChatRoles { get; set; }
    public DbSet<ChatMessageImageLink> ImageLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDateTimeTypes();

        builder.Entity<Domain.Chat.Chat>()
            .HasMany(u => u.ChatUsers)
            .WithOne(e => e.Chat)
            .HasForeignKey(e => e.ChatId);

        builder.Entity<Domain.Chat.Chat>()
            .HasMany(u => u.Messages)
            .WithOne(e => e.Chat)
            .HasForeignKey(e => e.ChatId);

        builder.Entity<ChatMessage>()
            .HasMany(u => u.MessageImageLinks)
            .WithOne(e => e.ChatMessage)
            .HasForeignKey(e => e.ChatMessageId);

        builder.Entity<ChatMessage>();

        base.OnModelCreating(builder);
    }
}