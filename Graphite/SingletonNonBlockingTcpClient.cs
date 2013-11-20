using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphite
{
	// This is an extremely quiet, polite class. It's meant to not complain, ever. 
	// Which means when it fails, we're not entirely sure why it does. 
	// If we'd like to make it more verbose, we have the option of modifying GrapiteTcpClient or any of hte 
	// derived classes to log failures. 
	public class SingletonNonBlockingTcpClient
	{
		private static object syncRoot = new Object();
		private static volatile NonBlockingTcpClient _logger = null;

		private SingletonNonBlockingTcpClient() { /* CreateLogger should be used instead of a constructor */ }

		public static NonBlockingTcpClient GetLogger
		{
			get
			{
				//Can't throw exceptions, this class needs to be completely silent.
				//returning null isn't too much better, but hopefully the client will realize what's up.
				//if (_logger == null)
					//throw new InvalidOperationException("Client not created - use CreateLogger(...)");

				return _logger;
			}
		}

		public static NonBlockingTcpClient CreateLogger(string address, int port, string prefix)
		{
			if (_logger == null)
			{
				lock (syncRoot)
				{
					if (_logger == null)
					{
						_logger = new NonBlockingTcpClient(address, port, prefix);
					}
				}
			}

			return _logger;
		}
	}
}
