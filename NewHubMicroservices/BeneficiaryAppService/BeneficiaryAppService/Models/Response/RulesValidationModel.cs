using Commons;
using System;
using System.Collections.Generic;


namespace BeneficiaryAppService.Models.Response
{
    public class RulesValidationModel : MethodFeedback
    {
        public Guid personguid { get; set; }
        public List<RulesValidationError> errors { get; set; }
    }

    public class RulesValidationError
    {
        public string Field { get; set; }
        public string Type { get; set; }
        public List<string> List { get; set; }
    }
}
