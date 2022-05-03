using System;
using System.Collections.Generic;

namespace Commons.Base
{

    /// <summary>
    /// objeto com as informaçoes basicas
    /// </summary>
    public class Hierarchy
    {
        public List<HierarchyGroup> Groups { get; set; }
    }

    public class HierarchyGroup
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<gallery> gallery { get; set; }
        public List<HierarchyCompany> Companies { get; set; }
    }
    public class gallery
    {
        public string type { get; set; }
        public string url { get; set; }
        public bool? isdefault { get; set; }
    }
    public class HierarchyCompany
    {
        public string Name { get; set; }
        public string TradingName { get; set; }
        public string Code { get; set; }
        public string[] Branches { get; set; }
    }
}