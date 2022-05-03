using System;

namespace NetCoreJobsMicroservice.Models
{
    public interface Modelo_DB
    {
        public Guid token { get; set; }
    }

    public interface Modelo_Base
    {
        public string Name { get; set; }
        public string Document { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
    }
    public class Modelo : Modelo_DB, Modelo_Base
    {
        public Guid token { get; set; }
        public string Name { get; set; }
        public string Document { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }

    }
    public class teste
    {
        public Modelo_DB a { get; set; }
        public Modelo_Base b { get; set; }
    }
}
