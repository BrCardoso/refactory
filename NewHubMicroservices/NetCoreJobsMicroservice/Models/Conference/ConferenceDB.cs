using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetCoreJobsMicroservice.Models
{
	public class ConferenceDB
	{
		public class Error
		{
			public string hubvalue { get; set; }

			public string providervalue { get; set; }

			[Required]
			public string type { get; set; }

			public string review { get; set; }
		}

		public class ErrorRequired : Error
		{
			[Required]
			public new string review { get; set; }
		}

		public class OwnerShip
		{
			public string name { get; set; }

			[Required]
			public string cpf { get; set; }

			public string cardnumber { get; set; }
		}

		public class InvoiceDetail<TError>
		{
			[JsonIgnore]
			public Guid? PersonGuid { get; set; }

			[Required]
			public string name { get; set; }

			[Required]
			public string cpf { get; set; }

			[Required]
			public string productcode { get; set; }

			public string productdescription { get; set; }

			[Required]
			public string contract { get; set; }

			[Required]
			public double value { get; set; }

			public string cardnumber { get; set; }

			[Required]
			public DateTime birthdate { get; set; }

			[Required]
			[RegularExpression(@"^\d{1,2}/\d{4}$", ErrorMessage = "'refdate' nao está no formato correto: mm/aaaa")]
			public string refdate { get; set; }

			[Required]
			public string typeuser { get; set; }

			[Required]
			public OwnerShip ownership { get; set; }

			public List<TError> errorList { get; set; } = new List<TError>();

			//Guarda valores passados no documento (isso nao é retornando no json, apenas para servir como auxiliar)
			[JsonIgnore]
			public ProviderValue Provider { get; set; }

			public class ProviderValue
			{
				[JsonIgnore]
				public string cpf { get; set; }

				[JsonIgnore]
				public string name { get; set; }

				[JsonIgnore]
				public DateTime birthdate { get; set; }

				[JsonIgnore]
				public double value { get; set; }

				[JsonIgnore]
				public string productcode { get; set; }

				[JsonIgnore]
				public string contract { get; set; }

				[JsonIgnore]
				public string cardnumber { get; set; }
			}
		}

		public class Conference<TError>
		{
			public Guid guid { get; set; }

			[Required]
			public Guid hubguid { get; set; }

			[Required]
			public string aggregator { get; set; }

			public string docType { get; set; } = "InvoiceValidation";

			[Required]
			public Guid providerguid { get; set; }

			public DateTime? incdate { get; set; }

			public string status { get; set; }

			public string dispatch { get; set; }

			public string receipt { get; set; }

			[Required]
			public string receiptnumber { get; set; }

			[Required]
			[RegularExpression(@"^\d{1,2}/\d{4}$", ErrorMessage = "'refdate' nao está no formato correto: mm/aaaa")]
			public string refdate { get; set; }

			[Required]
			public string file { get; set; }

			[Required]
			public double invvalue { get; set; }

			public double copartvalue { get; set; }

			public double ratevalue { get; set; }

			[Required]
			public List<string> contracts { get; set; }

			[Required]
			public List<InvoiceDetail<TError>> invoiceDetails { get; set; }
		}
	}
}