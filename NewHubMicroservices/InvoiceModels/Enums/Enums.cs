using System;

namespace NetCoreJobsMicroservice.Enum
{
    public class ProviderGuid
    {
        private ProviderGuid(string value) { Value = value; }

        public string Value { get; set; }

        public static ProviderGuid GNDI { get { return new ProviderGuid("5748db6d-a8b4-4633-8931-82b3887902cb"); } }
        public static ProviderGuid AMIL { get { return new ProviderGuid("79083a04-b580-4baa-b238-22be2cdd0453"); } }
        public static ProviderGuid error { get { return new ProviderGuid(""); } }

        public static explicit operator ProviderGuid(string v)
        {
            return new ProviderGuid(v);
        }
    }
}
