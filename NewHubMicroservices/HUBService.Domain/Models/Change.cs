using System;

namespace HUBService.Domain.Models
{
    public class Change
    {
        public DateTime Date { get; set; }

        public string Attribut { get; set; }

        public object Oldvalue { get; set; }

        public object Newvalue { get; set; }

        public bool Sync { get; set; }
        public DateTime? SyncDate { get; set; }
    }
}