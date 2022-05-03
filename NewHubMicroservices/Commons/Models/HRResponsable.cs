using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Base
{
	public class HRResponsableModel
	{
		public class Responsable
		{
			public Guid guid { get; set; }

			//public Guid? hubguid { get; set; }
			//public string aggregator { get; set; }
			//public string docType { get; set; }
			public string name { get; set; }

			public string cpf { get; set; }
			public string birthdate { get; set; }
			public List<string> sessions { get; set; }
			public Phoneinfo phoneinfos { get; set; }
			public Emailinfo emailinfos { get; set; }
		}

		public class opResponsable
		{
			public Guid? guid { get; set; }

			[NotEmpty]
			public Guid? hubguid { get; set; }

			[NotEmpty]
			public string aggregator { get; set; }
			public string docType { get; set; }

			[NotEmpty]
			public string name { get; set; }

			[NotEmpty]
			public string cpf { get; set; }

			[NotEmpty]
			public string birthdate { get; set; }

			[NotEmpty]
			public List<string> sessions { get; set; }
			public Phoneinfo phoneinfos { get; set; }
			public Emailinfo emailinfos { get; set; }
		}

	}
}
