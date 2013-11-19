﻿using System;
using System.Net.Sockets;

namespace Graphite
{
    public class GraphiteTcpClient : IGraphiteClient, IDisposable
    {
        public string Hostname { get; private set; }
        public int Port { get; private set; }
        public string KeyPrefix { get; private set; }

        private readonly TcpClient _tcpClient;

        public GraphiteTcpClient(string hostname, int port = 2003, string keyPrefix = null)
        {
            Hostname = hostname;
            Port = port;
            KeyPrefix = keyPrefix;

			try
			{
				_tcpClient = new TcpClient(Hostname, Port);
			}
			catch
			{
				// Supress all exceptions for now.
			}
        }

        public void Send(string path, int value, DateTime timeStamp)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(KeyPrefix))
                {
                    path = KeyPrefix+ "." + path;
                }
                
                var message = new PlaintextMessage(path, value, timeStamp).ToByteArray();

                _tcpClient.GetStream().Write(message, 0, message.Length);
            }
            catch
            {
                // Supress all exceptions for now.
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_tcpClient != null)
            {
                _tcpClient.Close();
            }
        }

        #endregion
    }
}