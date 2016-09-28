﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kadmium_osc_dmx_dotnet_webui.ViewHelpers
{
    public class InputSelect : VisibleInputElement
    {
        public Dictionary<string, string> Options { get; set; }
        
        public override string ViewName
        {
            get
            {
                return "InputSelect";
            }
        }
        
        public InputSelect(string id, string label, Dictionary<string, string> options) : base(id, label)
        {
            Options = options;
        }
    }
}
