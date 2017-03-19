﻿using kadmium_osc_dmx_dotnet_core.Fixtures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace kadmium_osc_dmx_dotnet_core
{
    public class Universe : IDisposable
    {
        public static int DMX_UNIVERSE_SIZE = 512;

        public int Id { get; set; }
        public string Name { get; set; }
        [JsonProperty(PropertyName ="universeID")]
        public int UniverseNumber { get; set; }
        public List<Fixture> Fixtures { get; }
        [NotMapped]
        [JsonIgnore]
        public byte[] DMX { get; }
        public event EventHandler<UpdateEventArgs> Updated;
        public event EventHandler<DMXEventArgs> Rendered;

        public Universe()
        {
            Name = "";
            UniverseNumber = 1;
            Fixtures = new List<Fixture>();
            DMX = new byte[DMX_UNIVERSE_SIZE];
        }

        public Universe(string name, int universeID, List<Fixture> fixtures) : this()
        {
            Name = name;
            UniverseNumber = universeID;
            Fixtures.AddRange(fixtures);
        }

        public JObject Serialize()
        {
            JObject obj = new JObject(
                new JProperty("name", Name),
                new JProperty("universeID", UniverseNumber)
            );
            return obj;
        }

        public void Dispose()
        {
            foreach (Fixture fixture in Fixtures)
            {
                fixture.Dispose();
            }
        }

        public static Universe Load(JObject universeElement, DatabaseContext context)
        {
            string name = universeElement["name"].Value<string>();
            int universeID = universeElement["universeID"].Value<int>();
            IEnumerable<Fixture> fixturesQuery = from fixture in universeElement["fixtures"].Values<JObject>()
                                                       select Fixture.Load(fixture, context);
            
            List<Fixture> fixtures = fixturesQuery.ToList();

            Universe universe = new Universe(name, universeID, fixtures);
            return universe;
        }

        public void Update()
        {
            foreach (Fixture fixture in Fixtures)
            {
                fixture.Update();
                fixture.Render(DMX);
            }
            Updated?.Invoke(this, new UpdateEventArgs(UniverseNumber, Fixtures));
        }

        public void Render()
        {
            MasterController.Instance.Transmitter.Transmit(DMX, UniverseNumber);
            Rendered?.Invoke(this, new DMXEventArgs(DMX));
        }

        public void Activate(DatabaseContext context)
        {
            Fixtures.ForEach(x => x.Initialize());
            Fixtures.ForEach(x => x.Activate(context));
        }

        public void Deactivate()
        {
            Fixtures.ForEach(x => x.Deactivate());
        }
    }

    public class DMXEventArgs : EventArgs
    {
        public byte[] DMX { get; }
        public DMXEventArgs(byte[] dmx)
        {
            DMX = dmx;
        }
    }

    public class UpdateEventArgs : EventArgs
    {
        public int UniverseID { get; set; }
        public IEnumerable<Fixture> Fixtures { get; set; }

        public UpdateEventArgs(int universeID, IEnumerable<Fixture> fixtures)
        {
            this.UniverseID = universeID;
            this.Fixtures = fixtures;
        }
    }
}
