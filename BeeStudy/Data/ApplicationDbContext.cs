using BeeStudy.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeeStudy.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Registration>().HasKey(re => new
            {
                re.CourseId,
                re.LearnerId
            });

            modelBuilder.Entity<Registration>().HasOne(c => c.Course).WithMany(re => re.Registrations).HasForeignKey(c => c.CourseId);

            modelBuilder.Entity<Registration>().HasOne(u => u.Learner).WithMany(re => re.Registrations).HasForeignKey(u => u.LearnerId);

            base.OnModelCreating(modelBuilder);

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Learner> Learners { get; set; }
        public DbSet<Registration> Registrations { get; set; }
    }


}

