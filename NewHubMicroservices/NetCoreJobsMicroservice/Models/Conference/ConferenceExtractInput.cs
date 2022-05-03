using System;
using System.ComponentModel.DataAnnotations;

namespace NetCoreJobsMicroservice.Models
{
	public class ConferenceExtractInput<T>
	{
		[Required]
		public Guid hubguid { get; set; }

		[Required]
		public string aggregator { get; set; }

		[Required]
		public Guid providerguid { get; set; }

		[Required]
		public T operation { get; set; }
	}

	public class Operation
	{
		[Required]
		public double value { get; set; }

		[Required]
		public Guid invoicevalidationguid { get; set; }
	}

	public class OperationType : Operation
	{
		[Required]
		public string type { get; set; }
	}

	public class ConferenceExtractType
	{
		public const string Debit = "DEBITO";
		public const string Credit = "CREDITO";
	}
}