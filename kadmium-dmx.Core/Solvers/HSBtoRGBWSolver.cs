﻿using System.Collections.Generic;
using System.Linq;
using kadmium_dmx_core.Fixtures;
using kadmium_dmx_core.Color;
using kadmium_dmx_data.Types;
using kadmium_dmx_data.Types.Fixtures;

namespace kadmium_dmx_core.Solvers
{
    public class HSBtoRGBWSolver : FixtureSolver
    {
        public HSBtoRGBWSolver(Fixture fixture) : base(fixture, "Hue", "Saturation", "Brightness")
        {
            fixture.Settables["Red"].Controlled = true;
            fixture.Settables["Green"].Controlled = true;
            fixture.Settables["Blue"].Controlled = true;
            fixture.Settables["White"].Controlled = true;
        }

        public override void Solve(Dictionary<string, FixtureAttribute> Attributes)
        {
            float saturation = Attributes["Saturation"].Value;
            HSB hsb = new HSB(Attributes["Hue"].Value * 360.0, Attributes["Saturation"].Value, Attributes["Brightness"].Value);
            RGB rgb = ColorSpaceHelper.HSBtoRGB(hsb);

            int minimum = new [] {rgb.Red, rgb.Green, rgb.Blue}.Min();

            Attributes["Red"].Value = (rgb.Red - minimum) / 255f;
            Attributes["Green"].Value = (rgb.Green - minimum) / 255f;
            Attributes["Blue"].Value = (rgb.Blue - minimum) / 255f;
            Attributes["White"].Value = (1 - saturation) * Attributes["Brightness"].Value;
        }

        public static bool SuitableFor(IFixtureDefinition definition)
        {
            return definition.Channels.Any(x => x.Name == "Red") &&
                definition.Channels.Any(x => x.Name == "Green") &&
                definition.Channels.Any(x => x.Name == "Blue") &&
                definition.Channels.Any(x => x.Name == "White");
        }

    }


}
