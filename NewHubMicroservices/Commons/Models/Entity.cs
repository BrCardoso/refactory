using System;
using System.Collections.Generic;

namespace Commons.Base
{
    public class Entity
    {
        public string Type { get; set; }
        public List<ListItem> List { get; set; }

        //public Entity()
        //{
        //    this.List = new List<ListItem> { new ListItem()};
        //}
    }

    public class ListItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ListItem2
    {
        public string Type { get; set; }
    }
}
