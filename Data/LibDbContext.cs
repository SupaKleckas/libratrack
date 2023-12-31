﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LibraTrack.Data.Entities;
using LibraTrack.Auth.Model;

namespace LibraTrack.Data
{
    public class LibDbContext : IdentityDbContext<User>
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
