﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Quarkless.Security.ServerListener.ServEntry;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Security.ServerListener
{
	internal static class SocketExtensions
	{
		internal static bool SendResponse(this Socket @socket, string resp)
		{
			try
			{
				var encoded = Encoding.ASCII.GetBytes(resp);
				@socket.BeginSend(encoded, 0, encoded.Length, SocketFlags.None,
					ar => { ((Socket)ar.AsyncState).EndSend(ar); },
					@socket);
				return true;
			}
			catch (SocketException socketException)
			{
				Console.WriteLine(socketException.Message);
				return false;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
		}
	}

	internal sealed class ServerNetwork : IServerNetwork, IDisposable
	{
		private readonly Socket _serverSocket;
		private readonly ClientSockets _clients;
		private readonly byte[] _buffer;
		private static IPAddress IpAddress => Dns.GetHostEntry(Dns.GetHostName()).AddressList.Last();

		public ServerNetwork()
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_clients = new ClientSockets();
			_buffer = new byte[4096];
			Setup();
		}
		private void Setup()
		{
			//_serverSocket.Bind(new IPEndPoint(IpAddress, 8022));
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
					case InitCommandArgs arg:
						if (!client.IsValidated)
						{
							var validated = client.ValidateClient(arg.Client);
							socket.SendResponse(validated ? "Validated;" : "Invalid Request");
						}
						break;
					case BuildCommandArgs arg:
						socket.SendResponse(client.GetEnvData());
						break;
					case GetPublicEndpointCommandArgs arg:
						socket.SendResponse(client.GetPublicEndpoints());
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
