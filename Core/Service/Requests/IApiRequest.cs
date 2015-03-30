using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Core.Service.Requests {
	public interface IApiRequest {

		string RequestURI { get; set; }

		string Verb { get; set; }

		/// <summary>
		/// Token to handle task cancellation
		/// </summary>
		CancellationTokenSource CancellationToken { get; set; }

		Task<HttpResponseMessage> CreateRequest(CancellationTokenSource cancellationToken);

		void Started();

		void Completed();

		void Failed();

	}
}
