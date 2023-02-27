using Microsoft.EntityFrameworkCore;

namespace Chatiks.Tools.EF;

public static class ModelBuilderExtensions
{
    public static void ConfigureDateTimeTypes(this ModelBuilder builder)
    {
        foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(DateTime)
                                 || p.ClrType == typeof(DateTime?))
                     .Where(p => p.GetColumnType() == null))
        {
            property.SetColumnType("timestamp without time zone");
        }
    }
}