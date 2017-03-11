using Microsoft.EntityFrameworkCore;
using kadmium_osc_dmx_dotnet_core.Fixtures;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kadmium_osc_dmx_dotnet_core
{
    public class DatabaseContext : DbContext
    {
        public DbSet<FixtureDefinition> FixtureDefinitions { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<VenuePreset> VenuePresets { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./DMX.db");
        }

        public async Task FilchData()
        {
            DeleteData();
            await FilchGroups();
            var count = SaveChanges();
            System.Console.WriteLine("{0} changes written", count);
            await FilchFixtureDefinitions();
            count = SaveChanges();
            System.Console.WriteLine("{0} changes written", count);
            await FilchVenues();
            count = SaveChanges();
            System.Console.WriteLine("{0} changes written", count);
            await FilchVenuePresets();
            count = SaveChanges();
            System.Console.WriteLine("{0} changes written", count);
            //var definition = await FixtureDefinition.Load("Generic", "Chinese Moving Wash");
            //FixtureDefinitions.Add(definition);


        }

        private async Task FilchGroups()
        {
            var groups = await FileAccess.LoadGroups();
            Groups.AddRange(groups);
        }

        private void DeleteData()
        {
            var tables = new[] { "ColorWheelEntry", "DMXChannel", "Fixture", "MovementAxis", "Universe",
                "VenuePresetFixtureEntry", "VenuePresets", "Venues", "FixtureDefinitions", "Groups" };
            foreach(string table in tables)
            {
                Database.ExecuteSqlCommand("delete from " + table);
            }
        }

        private async Task FilchFixtureDefinitions()
        {
            foreach (var fixtureKey in FileAccess.GetAllFixtures())
            {
                var definitionJson = await FileAccess.LoadFixtureDefinition(fixtureKey.Item1, fixtureKey.Item2);
                var definition = FixtureDefinition.Load(definitionJson);
                FixtureDefinitions.Add(definition);
            }
        }

        private async Task FilchVenues()
        {
            foreach (var venueName in FileAccess.GetVenueNames())
            {
                var venueJson = await FileAccess.LoadVenue(venueName);
                var venue = Venue.Load(venueJson);
                Venues.Add(venue);
            }
        }

        private async Task FilchVenuePresets()
        {
            foreach (var venuePresetName in FileAccess.GetVenuePresetNames())
            {
                var venuePresetJson = await FileAccess.LoadVenuePreset(venuePresetName);
                var venuePreset = VenuePreset.Load(venuePresetJson);
                VenuePresets.Add(venuePreset);
            }
        }

        public void UpdateCollection<T>(List<T> original, List<T> modified)
        {
            var common = original.Intersect(modified).ToList();
            var newItems = modified.Except(original).ToList();
            var removingItems = original.Except(common).ToList();
            foreach (T item in removingItems)
            {
                original.Remove(item);
            }
            original.AddRange(newItems);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FixtureDefinition>()
                .HasMany(x => x.Channels)
                .WithOne();

            modelBuilder.Entity<FixtureDefinition>()
                .HasMany(x => x.ColorWheel)
                .WithOne();

            modelBuilder.Entity<FixtureDefinition>()
                .HasMany(x => x.Movements)
                .WithOne();

            modelBuilder.Entity<Venue>()
                .HasMany(x => x.Universes)
                .WithOne();

            modelBuilder.Entity<Universe>()
                .HasMany(x => x.Fixtures)
                .WithOne();

            modelBuilder.Entity<VenuePreset>()
                .HasMany(x => x.FixtureEntries)
                .WithOne();
        }
    }
}