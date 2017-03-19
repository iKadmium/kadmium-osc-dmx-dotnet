using Microsoft.EntityFrameworkCore;
using kadmium_osc_dmx_dotnet_core.Fixtures;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

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

        public async Task RecursiveLoadAsync<T>(T item) where T : class
        {
            foreach(var collection in Entry(item).Collections)
            {
                await collection.LoadAsync();
                foreach(var thing in collection.CurrentValue)
                {
                    await RecursiveLoadAsync(thing);
                }
            }
        }
        
        private async Task FilchGroups()
        {
            var groups = await FileAccess.LoadGroups();
            Groups.AddRange(groups);
        }

        private void DeleteData()
        {
            var tables = new[] { "ColorWheelEntry", "DMXChannel", "Fixture", "MovementAxis", "Universe",
                "VenuePresets", "Venues", "FixtureDefinitions", "Groups" };
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
                var definition = definitionJson.ToObject<FixtureDefinition>();
                FixtureDefinitions.Add(definition);
            }
        }

        private async Task FilchVenues()
        {
            foreach (var venueName in FileAccess.GetVenueNames())
            {
                var venueJson = await FileAccess.LoadVenue(venueName);
                var venue = Venue.Load(venueJson, this);
                Venues.Add(venue);
            }
        }

        private async Task FilchVenuePresets()
        {
            foreach (var venuePresetName in FileAccess.GetVenuePresetNames())
            {
                var venuePresetJson = await FileAccess.LoadVenuePreset(venuePresetName);
                var venuePreset = VenuePreset.Load(venuePresetJson, this);
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

        public async Task UpdateCollection<TObject, TKey>(DbSet<TObject> original, IEnumerable<TObject> modified, Func<TObject, TKey> getKey) 
            where TObject : class 
        {
            foreach(var item in modified)
            {
                var key = getKey(item);
                TObject originalItem = await original.FindAsync(key);
                if(originalItem == null)
                {
                    await original.AddAsync(item);
                }
                else
                {
                    this.Entry(originalItem).CurrentValues.SetValues(item);
                }
            }

            var toDelete = from originalItem in original
                           where !modified.Any(x => getKey(x).Equals(getKey(originalItem)))
                           select originalItem;
            
            foreach(TObject item in toDelete)
            {
                original.Remove(item);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FixtureDefinition>()
                .HasMany(x => x.Channels)
                .WithOne()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            modelBuilder.Entity<FixtureDefinition>()
                .HasMany(x => x.ColorWheel)
                .WithOne()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            modelBuilder.Entity<FixtureDefinition>()
                .HasMany(x => x.Movements)
                .WithOne()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            modelBuilder.Entity<Fixture>()
                .HasOne(x => x.FixtureDefinition)
                .WithMany()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            modelBuilder.Entity<Fixture>()
                .HasOne(x => x.Group)
                .WithMany()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Venue>()
                .HasMany(x => x.Universes)
                .WithOne()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            modelBuilder.Entity<Universe>()
                .HasMany(x => x.Fixtures)
                .WithOne()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            modelBuilder.Entity<VenuePreset>()
                .HasMany(x => x.Fixtures)
                .WithOne()
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);
        }

        public async Task<Venue> LoadVenue(int id)
        {
            var venue = await Venues.FindAsync(id);

            await Entry(venue).Collection(x => x.Universes).LoadAsync();
            foreach(var universe in venue.Universes)
            {
                await Entry(universe).Collection(x => x.Fixtures).LoadAsync();
                foreach(var fixture in universe.Fixtures)
                {
                    await Entry(fixture).Reference(x => x.Group).LoadAsync();
                    await Entry(fixture).Reference(x => x.FixtureDefinition).LoadAsync();
                    foreach(var collection in Entry(fixture.FixtureDefinition).Collections)
                    {
                        await collection.LoadAsync();
                    }
                }
            }

            return venue;
        }

        public async Task<Group> LoadGroup(string groupName)
        {
            var grp = await Groups.SingleAsync(x => x.Name == groupName);
            return grp;
        }

        public async Task<FixtureDefinition> LoadFixtureDefinition(string manufacturer, string model)
        {
            var definition = await FixtureDefinitions.SingleAsync(x => x.Manufacturer == manufacturer && x.Model == model);
            foreach(var collection in Entry(definition).Collections)
            {
                await collection.LoadAsync();
            }
            return definition;
        }

        public async Task<FixtureDefinition> LoadFixtureDefinition(int id)
        {
            var definition = await FixtureDefinitions.FindAsync(id);
            foreach (var collection in Entry(definition).Collections)
            {
                await collection.LoadAsync();
            }
            return definition;
        }

        public async Task<VenuePreset> LoadVenuePreset(int id)
        {
            var preset = await VenuePresets.FindAsync(id);
            foreach(var collection in Entry(preset).Collections)
            {
                await collection.LoadAsync();
            }
            foreach (var fixture in preset.Fixtures)
            {
                await Entry(fixture).Reference(x => x.Group).LoadAsync();
                await Entry(fixture).Reference(x => x.FixtureDefinition).LoadAsync();
                foreach (var collection in Entry(fixture.FixtureDefinition).Collections)
                {
                    await collection.LoadAsync();
                }
            }
            return preset;
        }
    }

    public class DatabaseLogger : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger();
        }

        public void Dispose()
        { }

        private class MyLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }

}