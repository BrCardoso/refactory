using Commons.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Models
{

    public class MandatoryRules
    {
        [JsonProperty("guid")]
        public Guid Guid { get; set; }

        [JsonProperty("providerguid")]
        public Guid Providerguid { get; set; }

        [JsonProperty("hubguid")]
        public Guid Hubguid { get; set; }

        [JsonProperty("segcode")]
        public string Segcode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("answer")]
        public MRAnswer Answer { get; set; }

        [JsonProperty("kinship")]
        public List<string> Kinship { get; set; }

        [JsonProperty("movimenttype")]
        [NotEmpty]
        public List<MovimentTypeEnum> MovimentType { get; set; }

        [JsonProperty("ruletype")]
        [NotEmpty]
        public RuleTypeEnum RuleType { get; set; }

        [JsonProperty("location")]
        [NotEmpty]
        public MRLocation Location { get; set; }

        [JsonProperty("condition")]
        public List<MRCondition> Condition { get; set; }

        public class MRAnswer
        {
            [JsonProperty("type")]
            [NotEmpty]
            public string Type { get; set; }

            [JsonProperty("optionstype")]
            public string OptionsType { get; set; }

            [JsonProperty("default")]
            public List<string> Default { get; set; }
        }
        public class MRLocation
        {
            [JsonProperty("atribut")]
            [NotEmpty]
            public string Atribut { get; set; }

            [JsonProperty("type")]
            [NotEmpty]
            public LocationTypeEnum Type { get; set; }
        }

        public class MRCondition
        {
            [JsonProperty("atribut")]
            [NotEmpty]
            public string Atribut { get; set; }

            [JsonProperty("location")]
            [NotEmpty]
            public LocationTypeEnum Location { get; set; }

            [JsonProperty("type")]
            [NotEmpty]
            public OperatorEnum Type { get; set; }

            [JsonProperty("value")]
            [NotEmpty]
            public string Value { get; set; }

            [JsonProperty("valueType")]
            [NotEmpty]
            public ValueTypeEnum ValueType { get; set; }
        }
    }
}
