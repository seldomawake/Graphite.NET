using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/* 
 * From http://stackoverflow.com/questions/6262626/until-when-does-networkstream-write-block
 * Speficially:
 * Q: 
 * Until when does NetworkStream.Write block?
 * A: 
 * Until data is written to the send buffer on the sender side.
 * So if buffer is full, it will block.
 * The send buffer can be full if it didn't transmit data yet, because of network issues or because receive buffer is full on the receiver side.
 * There is an experiment you can conduct: make a sender and receiver, set sender's socket send buffer to something small and receiver's receive buffer to something small to.
 * Start sending, accept connection on the receiver side, but don't receive. The socket will be blocked when the sent bytes number is about SenderSendBuffer + ReceiverReceiveBuffer.
 * 
 * This class implements a buffer so that blocking behavior doens't bother the consumer.
 */
namespace Graphite
{
	public class NonBlockingTcpClient : GraphiteTcpClient
	{
		private BlockingCollection<Tuple<string, int>> _toSend;

		public NonBlockingTcpClient(string hostname, int port = 2003, string keyPrefix = null)
			: base(hostname, port, keyPrefix)
		{
			_toSend = new BlockingCollection<Tuple<string, int>>();
			Task t = Task.Run(() => SendToServer());
		}
		 
		public void AddToSendQueue(Tuple<string, int> toAdd, int msToTryBeforeTimeout = 0)
		{
			if (msToTryBeforeTimeout > 0)
				_toSend.TryAdd(toAdd, msToTryBeforeTimeout);
			else
				_toSend.Add(toAdd);
		}

		private void SendToServer()
		{
			foreach (var item in _toSend.GetConsumingEnumerable())
			{
				((IGraphiteClient)this).Send(item.Item1, item.Item2);
				//Console.WriteLine("Sent metric {0} with value {1}; {2} items still in send queue.", item.Item1, item.Item2, _toSend.Count());
			}
		}
	}
}
