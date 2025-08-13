using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ai.Tutor.Infrastructure.Data;

public class AiTutorDbContextFactory : IDesignTimeDbContextFactory<AiTutorDbContext>
{
    public AiTutorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AiTutorDbContext>();
        var conn = "Host=localhost;Port=5432;Database=ai-tutor-db;Username=postgres;Password=postgres;Include Error Detail=true";
        optionsBuilder.UseNpgsql(conn);
        return new AiTutorDbContext(optionsBuilder.Options);
    }
}
