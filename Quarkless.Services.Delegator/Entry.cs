﻿using System;
using System.IO;
using System.Linq;
using Docker.DotNet;
using System.Diagnostics;
using Docker.DotNet.Models;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using Quarkless.Common.Clients;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using QuarklessLogic.ServicesLogic.AgentLogic;
using Microsoft.Extensions.DependencyInjection;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Services.Delegator
{
	class Entry
	{
		#region Declerations
		#region Constants
		private const string DOCKER_URL = "npipe://./pipe/docker_engine";
		private const string CLIENT_SECTION = "Client";
		private const string SERVER_IP = "localhost";

		private const string HEARTBEAT_IMAGE_NAME = "quarkless/quarkless.services.heartbeat:latest";
		private const string HEARTBEAT_CONTAINER_NAME = "/quarkless.heartbeat.";

		private const string AUTOMATOR_IMAGE_NAME = "quarkless/quarkless.services.automator:latest";
		private const string AUTOMATOR_CONTAINER_NAME = "/quarkless.automator.";

		private const string NETWORK_MODE = "localnet";
		#endregion
		private static IAgentLogic _agentLogic; 
		private static DockerClient Client => new DockerClientConfiguration(new Uri(DOCKER_URL)).CreateClient();
		#endregion

		#region Build Services
		private static IConfiguration MakeConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile("appconfigs.json").Build();
		}
		private static IServiceCollection InitialiseClientServices()
		{
			var cIn = new ClientRequester(SERVER_IP, false);
			if (!cIn.TryConnect().GetAwaiter().GetResult())
				throw new Exception("Invalid Client");

			var caller = MakeConfigurationBuilder().GetSection(CLIENT_SECTION).Get<AvailableClient>();

			var validate = cIn.Send(new InitCommandArgs
			{
				Client = caller,
				CommandName = "Validate Client"
			});
			if (!(bool)validate)
				throw new Exception("Could not validated");

			var services = (IServiceCollection)cIn.Send(new BuildCommandArgs()
			{
				CommandName = "Build Services",
				ServiceTypes = new[]
				{
					ServiceTypes.AddConfigurators,
					ServiceTypes.AddContexts,
					ServiceTypes.AddHandlers,
					ServiceTypes.AddLogics,
					ServiceTypes.AddRepositories
				}
			});
			return services;
		}
		#endregion

		#region Docker Api Stuff
		static void RunCommand(string command)
		{
			try
			{
				var process = new Process()
				{
					StartInfo = new ProcessStartInfo
					{
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						RedirectStandardInput = true,
						FileName = "cmd.exe",
						Arguments = "/C "+command
					}
				};
				process.ErrorDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.OutputDataReceived += (o, e) =>
				{
					if (e.Data.Contains("Successfully tagged"))
					{
						Task.Delay(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
						process.Close();
					}
					Console.WriteLine(e.Data);
				};
				process.Exited += (o, e) =>
				{

				};
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				//var oud = process.StandardError.ReadToEnd();
				//var dm = process.StandardOutput.ReadToEnd();
				process.WaitForExit((int)TimeSpan.FromMinutes(3.5).TotalMilliseconds);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
		static async Task<IList<ContainerListResponse>> GetContainersByName(string name)
		{
			return await Client.Containers.ListContainersAsync(new ContainersListParameters
			{
				All = true,
				Filters = new Dictionary<string, IDictionary<string, bool>>
				{
					{ "name",
						new Dictionary<string, bool>
						{
							{name, true}
						}
					}
				}
			});
		}

		static async Task CreateAndRunHeartbeatContainers(List<ShortInstagramAccountModel> customers, ExtractOperationType type)
		{
			var workers = new NextList<ShortInstagramAccountModel>(await _agentLogic.GetAllAccounts(1));
			if (!workers.Any()) return;

			foreach (var customer in customers)
			{
				var worker = workers.MoveNextRepeater();
				await Client.Containers.CreateContainerAsync(new CreateContainerParameters
				{
					Image = HEARTBEAT_IMAGE_NAME,
					Name = $"quarkless.heartbeat.{type.ToString()}.{customer.AccountId}.{customer.Id}.{worker.Id}",
					HostConfig = new HostConfig
					{
						NetworkMode = NETWORK_MODE,
						RestartPolicy = new RestartPolicy()
						{
							Name = RestartPolicyKind.Always
						}
					},
					Env = new List<string>
					{
						$"USER_ID={customer.AccountId}",
						$"USER_INSTAGRAM_ACCOUNT={customer.Id}",
						$"WORKER_USER_ID={worker.AccountId}",
						$"WORKER_INSTAGRAM_ACCOUNT={worker.Id}",
						$"OPERATION_TYPE={((int)type).ToString()}"
					},
					AttachStderr = true,
					AttachStdout = true
				});
				Console.WriteLine("Successfully created automator app for customer {0}", customer.Username);
			}

			//start the containers
			foreach (var container in await GetContainersByName(HEARTBEAT_CONTAINER_NAME))
			{
				await Client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
				Console.WriteLine("Successfully started heartbeat app with ID {0}", container.ID);
			}
		}
		static async Task CreateAndAutomatorContainers(List<ShortInstagramAccountModel> customers)
		{
			foreach (var customer in customers)
			{
				await Client.Containers.CreateContainerAsync(new CreateContainerParameters
				{
					Image = AUTOMATOR_IMAGE_NAME,
					Name = $"quarkless.automator.{customer.AccountId}.{customer.Id}",
					HostConfig = new HostConfig
					{
						NetworkMode = NETWORK_MODE,
						PortBindings = new Dictionary<string, IList<PortBinding>>
						{
							{ "51242/tcp", 
								new List<PortBinding>
								{
									new PortBinding
									{
										HostPort = "51242"
									}
								}
							}
						},
						RestartPolicy = new RestartPolicy()
						{
							Name = RestartPolicyKind.Always
						}
					},
					
					NetworkingConfig = new NetworkingConfig()
					{
						EndpointsConfig = new Dictionary<string, EndpointSettings>
						{
							{ "localnet", 
								new EndpointSettings()
								{
									Aliases = new List<string>{ "quarkless.local.automator" }
								}
							}
						}
					},
					Env = new List<string>
					{
						$"USER_ID={customer.AccountId}",
						$"USER_INSTAGRAM_ACCOUNT={customer.Id}",
					},
					AttachStderr = true,
					AttachStdout = true
				});
				Console.WriteLine("Successfully created automator app for customer {0}", customer.Username);
			}

			//start the containers
			foreach (var container in await GetContainersByName(AUTOMATOR_CONTAINER_NAME))
			{
				await Client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
				Console.WriteLine("Successfully started automator app with ID {0}", container.ID);
			}
		}
		#endregion

		/// <summary>
		/// This application requires the quarkless.security application to run on your local machine
		/// for that please go to the quarkless.security project and run the .exe in the bin folder
		/// (create a shortcut for ease)
		/// Create images for heartbeat and automator services
		/// Delete and Recreate the containers for each user
		/// </summary>
		/// <returns></returns>
		private static async Task CarryOutOperation()
		{
			var services = InitialiseClientServices().BuildServiceProvider();
			_agentLogic = services.GetService<IAgentLogic>();


			var images = await Client.Images.ListImagesAsync(new ImagesListParameters());

			if (!images.Any(image => image.RepoTags.Contains(HEARTBEAT_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", HEARTBEAT_IMAGE_NAME);
				try
				{
					var projPath = Directory.GetParent(Environment.CurrentDirectory.Split("bin")[0]).FullName;
					var solutionPath = Directory.GetParent(projPath);
					var heartBeatPath = solutionPath + @"\Quarkless.Services.Heartbeat";
					RunCommand($"cd {solutionPath} & docker build -t {HEARTBEAT_IMAGE_NAME} -f {heartBeatPath + @"\Dockerfile"} .");
				}
				catch (Exception ee)
				{
					Console.WriteLine(ee);
				}
			}

			if (!images.Any(image => image.RepoTags.Contains(AUTOMATOR_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", AUTOMATOR_IMAGE_NAME);
				try
				{
					var projPath = Directory.GetParent(Environment.CurrentDirectory.Split("bin")[0]).FullName;
					var solutionPath = Directory.GetParent(projPath);
					var automatorPath = solutionPath + @"\Quarkless.Services";
					RunCommand($"cd {solutionPath} & docker build -t {AUTOMATOR_IMAGE_NAME} -f {automatorPath + @"\Dockerfile"} .");
				}
				catch (Exception ee)
				{
					Console.WriteLine(ee.Message);
				}
			}

			Console.WriteLine("Clearing out current containers...");
			Console.WriteLine("Beginning with heartbeat app...");
			//get all heartbeat containers then remove them
			foreach (var container in await GetContainersByName(HEARTBEAT_CONTAINER_NAME))
			{
				Console.WriteLine("Removed {0}", container.ID);
				await Client.Containers.RemoveContainerAsync(container.ID,
					new ContainerRemoveParameters
					{
						Force = true
					});
			}
			Console.WriteLine("Finished with heartbeat, now starting automator");
			//get all automation service containers then remove them
			foreach (var container in await GetContainersByName(AUTOMATOR_CONTAINER_NAME))
			{
				Console.WriteLine("Removed {0}", container.ID);
				await Client.Containers.RemoveContainerAsync(container.ID,
					new ContainerRemoveParameters()
					{
						Force = true
					});
			}

			Console.WriteLine("Recreating new instances of containers");
			// recreate the heartbeat containers for users
			var customers = (await _agentLogic.GetAllAccounts(0))?.ToList();

			if (customers == null || !customers.Any()) return;

			var opsTypes = Enum.GetValues(typeof(ExtractOperationType)).Cast<ExtractOperationType>();
			foreach (var opsType in opsTypes)
			{
				await CreateAndRunHeartbeatContainers(customers, opsType);
			}
			Console.WriteLine("Finished Heartbeat containers, now starting automator");

			await Task.Delay(TimeSpan.FromMinutes(2.5)); // wait around 2.5 minutes before starting automation (populate data first)

			await CreateAndAutomatorContainers(customers);
			Console.WriteLine("Finished creating containers for automator");
		}

		static async Task Main(string[] args)
		{
			await CarryOutOperation();
		}
	}
}