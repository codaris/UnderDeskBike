// <copyright file="Context.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace UnderDeskBike.Models
{
    /// <summary>
    /// The database context for the bike data.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class Context : DbContext
    {
        /// <summary>
        /// Gets or sets the workouts.
        /// </summary>
        public DbSet<Workout> Workouts { get; set; }

        /// <summary>
        /// Gets or sets the workout entries.
        /// </summary>
        public DbSet<WorkoutEntry> WorkoutEntries { get; set; }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        public override int SaveChanges()
        {
            foreach (var workout in ChangeTracker.Entries<Workout>().Select(e => e.Entity)) workout.AveragesFromEntries();
            return base.SaveChanges();
        }

        /// <summary>
        /// This method configures the database (and other options) to be used for this context.
        /// </summary>
        /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databaseFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UnderDeskBike.db");
            optionsBuilder.UseSqlite(@"Data Source=" + databaseFullPath);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
