using System.Data.Entity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() : base("connectionstring")
    {
    }

    public DbSet<User> Users { get; set; }
}
