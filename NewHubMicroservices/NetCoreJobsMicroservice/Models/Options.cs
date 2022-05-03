using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public class Options
    {
        public List<OptionsItem> list { get; set; }

        public class OptionsItem
        {
            public string type { get; set; }
            public List<string> items { get; set; }
        }
    }
}