using System;
using SoftFX.Net.Core;

namespace TTQuoteHistoryClient
{
    public class QuoteHistoryClient : IDisposable
    {
        private int _timeout = 15000;
        private readonly string _address;
        private readonly ClientSession _session;

        #region Constructors

        public QuoteHistoryClient(string address) : this(address, 5020)
        {
        }

        public QuoteHistoryClient(string address, int port) : this(address, port, new ClientSessionOptions(port))
        {
        }

        public QuoteHistoryClient(string address, int port, ClientSessionOptions options)
        {
            _address = address;
            _session = new ClientSession("QuoteHistoryClient", SoftFX.Net.QuoteHistoryCacheProtocol.Info.QuoteHistoryCacheProtocol, options);
            _session.OnConnect += OnConnect;
            _session.OnConnectError += OnConnectError;
            _session.OnDisconnect += OnDisconnect;
        }

        private void OnConnect(ClientSession clientSession)
        {
            IsConnected = true;
        }

        private void OnConnectError(ClientSession clientSession)
        {
            IsConnected = false;
        }

        private void OnDisconnect(ClientSession clientSession, string text)
        {
            IsConnected = false;
        }

        #endregion

        #region Connection

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            _session.Connect(_address);
            if (!_session.WaitConnect(_timeout))
                throw new TimeoutException("Connect timeout");
        }

        public void Disconnect()
        {
            _session.Disconnect("Disconnect client");
            if (!_session.WaitDisconnect(_timeout))
                throw new TimeoutException("Connect timeout");
        }

        #endregion

        #region IDisposable

        ~QuoteHistoryClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Disconnect();
        }

        #endregion
    }
}
