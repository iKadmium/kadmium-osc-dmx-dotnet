using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using kadmium_dmx_core.Transmitters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;
using kadmium_dmx_data;
using kadmium_dmx_data.Storage;
using kadmium_dmx_data.Types.Settings;
using kadmium_dmx_data.Types;
using kadmium_dmx_core;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace kadmium_dmx_webapi.Controllers
{
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        private ISettingsStore Store { get; set; }
        private IMasterController MasterController { get; set; }

        public SettingsController(ISettingsStore store, IMasterController masterController)
        {
            Store = store;
            MasterController = masterController;
        }

        // GET: api/values
        [HttpGet]
        [SwaggerOperation("getSettings")]
        public async Task<ISettings> Get()
        {
            return await Store.GetSettings();
        }

        // POST api/values
        [HttpPost]
        [SwaggerOperation("postSettings")]
        public async Task Post([FromBody]Settings value)
        {
            await Store.SaveSettings(value);
            MasterController.SetSettings(value);
        }
    }
}
