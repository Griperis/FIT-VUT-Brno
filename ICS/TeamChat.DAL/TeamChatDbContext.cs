using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TeamChat.DAL.Entities;

namespace TeamChat.DAL
{
    public class TeamChatDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public TeamChatDbContext()
        { }

        public TeamChatDbContext(DbContextOptions<TeamChatDbContext> contextOptions) : base(contextOptions)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<TeamUser> TeamUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Inharitance settings
            modelBuilder.Entity<Comment>().HasBaseType<Activity>();
            modelBuilder.Entity<Post>().HasBaseType<Activity>();
            
            // Join table settings
            modelBuilder.Entity<TeamUser>()
                .HasKey(x => new {x.UserId, x.TeamId});
            modelBuilder.Entity<TeamUser>()
                .HasOne(t => t.Team)
                .WithMany(u => u.Members);
            modelBuilder.Entity<TeamUser>()
                .HasOne(tu => tu.User)
                .WithMany(t => t.Teams);
           
            // Relations settings
            modelBuilder.Entity<Team>()
                .HasMany(p => p.Posts);
            modelBuilder.Entity<User>()
                .HasMany(a => a.Activities);
            modelBuilder.Entity<Post>()
                .HasMany(c => c.Comments)
                .WithOne(p => p.BelongsTo);


        }
    }
}
