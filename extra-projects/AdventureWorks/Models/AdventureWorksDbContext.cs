using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Models;

public partial class AdventureWorksDbContext : DbContext
{
	public AdventureWorksDbContext()
	{
	}

	public AdventureWorksDbContext(DbContextOptions<AdventureWorksDbContext> options)
		: base(options)
	{
	}


	protected override void OnModelCreating(ModelBuilder modelBuilder) => OnModelCreatingPartial(modelBuilder);

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
