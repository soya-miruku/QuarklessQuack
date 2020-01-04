﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients.Configs;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Common.Clients
{
	public class ClientRequester
	{
		private readonly Socket _clientSocket;
		private readonly string _host;
		private readonly bool _inDocker;
		private const int BYTE_LIMIT = 8192;
		public ClientRequester(string host, bool inDocker = true)
		{
			_host = host;
			_inDocker = inDocker;
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public async Task<bool> TryConnect()
		{
			var attempts = 0;
			while (!_clientSocket.Connected && attempts < 15)
			{
				try
				{
					_clientSocket.Connect(_host, _inDocker ? 65115 : 65116);
					return true;
				}
				catch (SocketException se)
				{
					Console.WriteLine($"Connection failed: {se.Message}");
					attempts++;
				}
				await Task.Delay(TimeSpan.FromSeconds(2));
			}
			return false;
		}
		public void TryDisconnect()
		{
			try
			{
				if (!_clientSocket.Connected) return;
				_clientSocket.Disconnect(false);
				_clientSocket.Shutdown(SocketShutdown.Both);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				_clientSocket.Close();
				_clientSocket.Dispose();
			}
		}
		private IServiceCollection BuildServices(in EnvironmentsAccess access, params ServiceTypes[] serviceTypes)
			=> Config.BuildServices(access, serviceTypes);

		public IServiceCollection Build(AvailableClient client, params ServiceTypes[] servicesToBuild)
		{
			try
			{
				var argData = new ArgData
				{
					Client = client,
					Services = servicesToBuild
				};
				var request = Encoding.ASCII.GetBytes(argData.Serialize());

				_clientSocket.Send(request);

				var bytesReceived = new byte[BYTE_LIMIT];
				var requestReceivedLen = _clientSocket.Receive(bytesReceived);
				var data = new byte[requestReceivedLen];
				Array.Copy(bytesReceived, data, requestReceivedLen);

				var response = Encoding.ASCII.GetString(data);

				return response != null
					? BuildServices(response.Deserialize<EnvironmentsAccess>(), servicesToBuild)
					: null;
			}
			catch(Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public EndPoints GetPublicEndPoints(GetPublicEndpointCommandArgs args)
		{
			try
			{
				var request = Encoding.ASCII.GetBytes(args.Serialize());

				_clientSocket.Send(request);

				var bytesReceived = new byte[BYTE_LIMIT];
				var requestReceivedLen = _clientSocket.Receive(bytesReceived);
				var data = new byte[requestReceivedLen];
				Array.Copy(bytesReceived, data, requestReceivedLen);

				var response = Encoding.ASCII.GetString(data);

				return response.Deserialize<EndPoints>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}

		/*
		public object Send(object dataToSend)
		{
			try
			{
				var request = dataToSend.Serialize();
				var buffer = Encoding.ASCII.GetBytes(request);
				_clientSocket.Send(buffer);
				var bytesReceived = new byte[BYTE_LIMIT];
				var requestReceivedLen = _clientSocket.Receive(bytesReceived);
				var data = new byte[requestReceivedLen];
				Array.Copy(bytesReceived, data, requestReceivedLen);
				var response = Encoding.ASCII.GetString(data);

				switch (dataToSend)
				{
					case InitCommandArgs arg:
						return response == "Validated;";
					case BuildCommandArgs arg:
						var access = response.Deserialize<EnvironmentsAccess>();
						return access != null ? BuildServices(access, arg.ServiceTypes) : null;
					case GetPublicEndpointCommandArgs arg:
						return response.Deserialize<EndPoints>();
					default:
						throw new Exception("Invalid Command");
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}*/
	}
}
