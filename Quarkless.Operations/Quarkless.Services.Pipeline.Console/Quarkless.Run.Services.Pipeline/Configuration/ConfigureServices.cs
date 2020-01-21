﻿using Microsoft.Extensions.DependencyInjection;
using Quarkless.Logic.Services.Pipeline;
using Quarkless.Models.Services.Pipeline.Interfaces;
using Quarkless.Models.Shared.Extensions;

namespace Quarkless.Run.Services.Pipeline.Configuration
{
	public static class ConfigureServices
	{
		public static void IncludeServices(this IServiceCollection serviceCollection)
		{
			var environments = new Config().Environments;
			serviceCollection.AddSingleton<IAccountCreatedTransfer, AccountCreatedTransfer>();
		}
	}
}
