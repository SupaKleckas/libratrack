using Microsoft.EntityFrameworkCore;
using LibraTrack.Data.Entities;

namespace LibraTrack.Data
{
	public class LibDbContext : DbContext
	{
		private readonly IConfiguration _configuration;
		public DbSet<Library> Libraries { get; set; }
		public DbSet<Section> Sections { get; set; }
		public DbSet<Book> Books { get; set; }

		public LibDbContext(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostGreSQL"));
		}
	}
}
