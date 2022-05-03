using Commons.Enums;

using System.Net;
using System.Text.Json.Serialization;

namespace LoginAppService.Model
{
	public class Response<TObject>
	{
		public HttpStatusCode StatusCode { get; set; }
		public MessageCode MessageCode { get; set; }

		[JsonPropertyName("response")]
		public TObject Object { get; set; }
	}
}