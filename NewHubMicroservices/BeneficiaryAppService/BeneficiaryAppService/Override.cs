using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace BeneficiaryAppService
{
    [AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
    public class NotEmptyAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "The {0} field must not be empty";
        public NotEmptyAttribute() : base(DefaultErrorMessage) { }

        public override bool IsValid(object value)
        {
            //NotEmpty doesn't necessarily mean required
            if (value is null)
            {
                return false;
            }

            switch (value)
            {
                case Guid guid:
                    return guid != Guid.Empty;
                default:
                    return true;
            }
        }
    }

    [AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class ListCannotBeEmptyAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "'{0}' must have at least one element.";
        public ListCannotBeEmptyAttribute() : base(DefaultErrorMessage) { }

        public override bool IsValid(object value)
        {
            IList list = value as IList;
            return (list != null && list.Count > 0);
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(this.ErrorMessageString, name);
        }
    }
}
