﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Quarkless.Logic.Security.Extensions;
using Quarkless.Logic.Security.ServEntry;
using Quarkless.Models.ClientSender;
using Quarkless.Models.ClientSender.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Security.Interfaces;

namespace Quarkless.Logic.Security
{
	public sealed class ServerNetwork : IServerNetwork, IDisposable
	{
		private readonly Socket _serverSocket;
		private readonly ClientSockets _clients;
		private readonly byte[] _buffer;
		private const int BYTE_LIMIT = 8192;
		public ServerNetwork()
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_clients = new ClientSockets();
			_buffer = new byte[BYTE_LIMIT];
			Setup();
		}
		private void Setup()
		{
			_serverSocket.Bind(new IPEndPoint(IPAddress.Any, 65115));
			_serverSocket.Listen(5);
		}
		public void StartAccepting()
		{
			_serverSocket.BeginAccept(AcceptCallback, null);
			Console.WriteLine("Ready...");
		}
		private void AcceptCallback(IAsyncResult asyncResult)
		{
			try
			{
				var socket = _serverSocket.EndAccept(asyncResult);
				_clients.AddClient(new ClientSocket(socket));
				Console.WriteLine($"Client Connected: {socket.LocalEndPoint}");
				socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
				_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
			}
			catch (SocketException socketException)
			{
				Console.WriteLine(socketException.Message);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
		private void ReceiveCallback(IAsyncResult asyncResult)
		{
			try
			{
				if (_clients.Count() > 10)
					_clients.Clean();
				
				var socket = (Socket)asyncResult.AsyncState;
				var client = _clients.GetClient(socket);

				var received = socket.EndReceive(asyncResult);
				
				var dataBuffer = new byte[received];
				Array.Copy(_buffer, dataBuffer, received);
				var data = Encoding.ASCII.GetString(dataBuffer);

				var tryParse = data.TryConvertObjectOfInterfaceType(typeof(ICommandArgs));

				switch (tryParse)
				{
					case ArgData arg:
						if (!client.IsValidated)
							socket.SendResponse(client.ValidateClient(arg.Client)
								? client.GetEnvData(arg.UseLocal)
								: string.Empty);
						else
							socket.SendResponse(client.GetEnvData(arg.UseLocal));
						break;
					case GetPublicEndpointCommandArgs arg:
						socket.SendResponse(client.GetPublicEndpoints(arg.UseLocal));
						break;
				}

				socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
			}
			catch (SocketException sException)
			{
				Console.WriteLine(sException.Message);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}

		public void Dispose()
		{
			_serverSocket.Shutdown(SocketShutdown.Both);
			_serverSocket.Close();
			_serverSocket.Dispose();
		}
	}
}