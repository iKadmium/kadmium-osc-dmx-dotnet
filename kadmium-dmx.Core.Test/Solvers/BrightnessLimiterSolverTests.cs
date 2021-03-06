﻿using kadmium_dmx_core.Solvers;
using kadmium_dmx_data.Types.Fixtures;
using Newtonsoft.Json.Linq;
using Xunit;

namespace kadmium_dmx.Core.Test.Solvers
{
    public class BrightnessLimiterSolverTests
    {
        [Fact]
        public void TestDefaultSolverAddition()
        {
            var options = GetLimitedOptions(0.5f);
            var fixture = HSBtoRGBSolverTests.GetRGBFixture(options);
            Assert.Contains(fixture.Solvers, (x => x is BrightnessLimiterSolver));
        }

        [Theory]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        public void TestBrightnessLimit(float limit)
        {
            var options = GetLimitedOptions(limit);
            var fixture = HSBtoRGBSolverTests.GetRGBFixture(options);
            fixture.Settables["Brightness"].Value = 1f;
            fixture.Update();
            Assert.Equal(limit, fixture.FrameSettables["Red"].Value);
            Assert.Equal(limit, fixture.FrameSettables["Green"].Value);
            Assert.Equal(limit, fixture.FrameSettables["Blue"].Value);
        }

        static FixtureOptions GetLimitedOptions(float limit)
        {
            FixtureOptions options = new FixtureOptions
            {
                MaxBrightness = limit
            };
            return options;
        }
    }
}
