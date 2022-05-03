using System;
using System.Collections.Generic;

namespace Commons.Base
{
    public class Log
    {
        public string Status { get; set; }
        public List<LogList> LogList { get; set; }
    }
    public class LogList : MethodFeedback
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
    }
}