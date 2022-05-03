
using System;
using System.Collections.Generic;

namespace Commons.Models
{
	public class TransferenceEvent
	{
		public Guid personGuid { get; set; }
		public string personName { get; set; }
		public Guid providerGuid { get; set; }
		public string providerName { get; set; }
		public string currentProductCode { get; set; }
		public string currentProduct { get; set; }
		public DateTime? BlockDate { get; set; }
		public string newProductCode { get; set; }
		public string newProduct { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? syncDate { get; set; }
		public bool sync { get; set; }
		public bool synced { get; set; }
	}

	public class Copy2ndRequestEvent
	{
		public Guid personGuid { get; set; }
		public string personName { get; set; }
		public Guid providerGuid { get; set; }
		public string providerName { get; set; }
		public string providerProductCode { get; set; }
		public string Product { get; set; }
		public string cardNumber { get; set; }
		public DateTime? dateRequest { get; set; }
		public string reissueReason { get; set; }
		public DateTime? syncDate { get; set; }
	}

	public class BlockEvent
	{
		public Guid personGuid { get; set; }
		public string personName { get; set; }
		public Guid providerGuid { get; set; }
		public string providerName { get; set; }
		public string providerProductCode { get; set; }
		public string Product { get; set; }
		public DateTime? BlockDate { get; set; }
		public string BlockReason { get; set; }
		public DateTime? syncDate { get; set; }
	}

	public class Event
	{
		public string docType { get; set; } = "Events";
		public Guid guid { get; set; }
		public Guid hubguid { get; set; }
		public string aggregator { get; set; }
		public List<TransferenceEvent> transferences { get; set; }
		public List<Copy2ndRequestEvent> copy2ndRequest { get; set; }
		public List<BlockEvent> blocks { get; set; }
	}
}