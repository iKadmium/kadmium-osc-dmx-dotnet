﻿using kadmium_osc_dmx_dotnet_core.Solvers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace kadmium_osc_dmx_dotnet_core.Fixtures
{
    public class Fixture
    {
        public int StartChannel { get; set; }
        public int EndChannel { get { return StartChannel + Definition.Channels.Max(x => x.RelativeAddress); } }

        public Definition Definition { get; }
        public Dictionary<string, Solvers.Attribute> Settables { get; }
        public Dictionary<string, Solvers.Attribute> FrameSettables { get; }
        public Dictionary<string, MovementAxis> MovementAxis { get; }
        public List<FixtureSolver> Solvers { get; }
        public JObject Options;
        public Group Group { get; }
        
        public Fixture(Definition definition, int startChannel, Group group, JObject options)
        {
            Definition = definition;
            Solvers = new List<FixtureSolver>();
            Settables = new Dictionary<string, Solvers.Attribute>();
            FrameSettables = new Dictionary<string, Solvers.Attribute>();
            foreach (Solvers.Attribute attribute in Definition.Channels)
            {
                Settables.Add(attribute.Name, attribute);
                FrameSettables.Add(attribute.Name, attribute);
            }
            MovementAxis = new Dictionary<string, MovementAxis>();
            foreach(var axis in Definition.Axis)
            {
                MovementAxis.Add(axis.Name, axis);
            }
            Solvers.AddRange(FixtureSolver.GetDefaultSolvers(this, options));
            Options = options;
            StartChannel = startChannel;
            Group = group;
            Group.Fixtures.Add(this);
        }
        
        internal void Update()
        {
            foreach(KeyValuePair<string, Solvers.Attribute> kvp in Settables)
            {
                FrameSettables[kvp.Key].Value = kvp.Value.Value;
            }

            foreach(FixtureSolver solver in Solvers.Where(solver => solver != null))
            {
                solver.Solve(FrameSettables);
            }
        }

        public JObject Serialize()
        {
            Group group = MasterController.Instance.Groups.Values.Single(x => x.Fixtures.Contains(this));

            JObject obj = new JObject(
                new JProperty("channel", StartChannel),
                new JProperty("type", Definition.Name),
                new JProperty("group", group.Name),
                new JProperty("options", 
                    new JArray(Options)
                )
            );

            return obj;
        }

        public void Dispose()
        {
            Group.Fixtures.Remove(this);
        }

        public static Fixture Load(JObject obj)
        {
            int startChannel = obj["channel"].Value<int>();
            string type = obj["type"].Value<string>();
            Definition definition = Definition.Load(type);
            Group group = MasterController.Instance.Groups[obj["group"].Value<string>()];
            JObject options = obj["options"].Value<JObject>();
            Fixture fixture = new Fixture(definition, startChannel, group, options);
            return fixture;
        }

        public void Render(byte[] dmx)
        {
            var channels = from channel in Definition.Channels
                           group channel by channel.RelativeAddress into chanGroup
                           select chanGroup;

            foreach (var channelGroup in channels)
            {
                DMXChannel channel = channelGroup.OrderByDescending(chan => chan.Value).ThenByDescending(chan => chan.Min).First();
                dmx[channel.RelativeAddress + StartChannel - 2] = channel.ByteValue;
            }
        }

        public override string ToString()
        {
            return Definition.Name + " [" + StartChannel + " - " + EndChannel + "]";
        }
    }
}
