﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using kadmium_osc_dmx_dotnet_core;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace kadmium_osc_dmx_dotnet_webui.Controllers
{
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        // GET: api/values
        [HttpGet]
        public JObject Get()
        {
            return FileAccess.LoadSettings();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]JObject value)
        {
            FileAccess.SaveSettings(value);
        }
    }


}
