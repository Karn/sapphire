using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Core.Service.Requests {
	public class RequestQueue {

		private static RequestQueue _RequestQueue { get; set; }

		private Dictionary<CancellationTokenSource, HttpRequestMessage> QueuedRequets { get; set; }

		public RequestQueue() {
			QueuedRequets = new Dictionary<CancellationTokenSource, HttpRequestMessage>();
		}

		public RequestQueue GetInstance() {
			if (_RequestQueue == null)
				_RequestQueue = new RequestQueue();
			return _RequestQueue;
		}

		public void Enqueue(HttpRequestMessage request, CancellationTokenSource cancellationToken = null) {
			if (cancellationToken == null)
				cancellationToken = new CancellationTokenSource();
			QueuedRequets.Add(cancellationToken, request);
		}

		public void Dequeue(CancellationTokenSource token) {
			if (QueuedRequets.ContainsKey(token)) {
				QueuedRequets.Remove(token);
				token.Cancel();
			}
		}
	}
}
