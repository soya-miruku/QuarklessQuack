﻿using QuarklessContexts.Models.AgentModels;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Logic.InstagramAccountLogic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.ServicesLogic.AgentLogic
{
	public class AgentLogic : IAgentLogic
	{
		/// <summary>
		/// The idea was to make this a basic class that triggers the agent service, 
		/// agent on -> process in the background runs
		/// agent off -> process in the background stops
		/// </summary>
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		public AgentLogic(IInstagramAccountLogic instagramAccountLogic)
		{
			_instagramAccountLogic = instagramAccountLogic;
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAccounts()
		{
			return await _instagramAccountLogic.GetActiveAgentInstagramAccounts();
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetAllAccounts(int type = 0)
		{
			return await _instagramAccountLogic.GetInstagramAccounts(type);
		}
		public async Task<AgentResponse> Start(string accountId, string instagramAccountId)
		{
			var _instaAccount = await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);

			await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
				new InstagramAccountModel
				{
					AgentState = (int)AgentState.Running
				});

			return new AgentResponse
			{
				HttpStatus = HttpStatusCode.OK,
				Message = $"Started for {accountId}, of {_instaAccount.Username}"
			};
		}
		public async Task<AgentResponse> Stop(string accountId, string instagramAccountId)
		{
			try
			{
				await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
					new InstagramAccountModel
					{
						AgentState = (int)AgentState.Stopped	
					});
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Stopped"
				};
			}
			catch (Exception ee)
			{
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}

	}
}