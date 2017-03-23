﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace kadmium_osc_dmx_dotnet_core
{
    public class Venue : IDisposable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        [NotMapped]
        public static Status Status { get; set; }

        public List<Universe> Universes { get; set; }
        
        public Venue(string name, IEnumerable<Universe> universes)
        {
            Name = name;
            Universes = new List<Universe>();
            foreach (Universe universe in universes)
            {
                Universes.Add(universe);
            }
        }

        public Venue() : this("", Enumerable.Empty<Universe>())
        {
        }
        
        internal void Update()
        {
            foreach (Universe universe in Universes)
            {
                universe.Update();
            }
        }

        public VenueSkeleton GetSkeleton()
        {
            return new VenueSkeleton
            {
                Id = Id,
                Name = Name
            };
        }

        public async Task Render()
        {
            foreach (Universe universe in Universes)
            {
                await universe.Render();
            }
        }

        public void Dispose()
        {
            foreach (Universe universe in Universes)
            {
                universe.Dispose();
            }
        }

        public async Task Initialize(DatabaseContext context)
        {
            foreach(var universe in Universes)
            {
                await universe.Initialize(context);
            }
        }

        public void Activate()
        {
            Universes.ForEach(x => x.Activate());
            Status.Update(StatusCode.Success, Name + " running", this);
        }

        public void Deactivate()
        {
            Universes.ForEach(x => x.Deactivate());
        }
    }

    public class VenueSkeleton
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
