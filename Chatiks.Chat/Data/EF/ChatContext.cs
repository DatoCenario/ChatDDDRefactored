using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Tools.EF;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Data.EF;

public class ChatContext : DbContext
{
    public ChatContext()
    {
        
    }
    
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {
    }

    public DbSet<Domain.Chat.Chat> Chats { get; set; }
    public DbSet<Domain.Chat.ChatUser> ChatUsers { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatRole> ChatRoles { get; set; }
    public DbSet<ChatMessageImageLink> ImageLinks { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // For migrator because ef tools v7 not support --provider option
        base.OnConfiguring(optionsBuilder.UseNpgsql());
    }

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

        builder.Entity<ChatMessageImageLink>()
            .HasIndex(c => c.Id);
        
        builder.Entity<ChatUser>()
            .HasIndex(c => c.Id);
        
        base.OnModelCreating(builder);
    }
}