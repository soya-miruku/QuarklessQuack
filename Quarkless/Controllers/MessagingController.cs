﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.MessagingModels;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.MessagingLogic;
using QuarklessLogic.Logic.ResponseLogic;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
    public class MessagingController : ControllerBase
    {
	    private readonly IUserContext _userContext;
	    private readonly IResponseResolver _responseResolver;
	    private readonly IMessagingLogic _messagingLogic;
	    public MessagingController(IUserContext userContext, IResponseResolver responseResolver, IMessagingLogic messagingLogic)
	    {
		    _userContext = userContext;
		    _messagingLogic = messagingLogic;
		    _responseResolver = responseResolver;
	    }

	    [HttpGet]
	    [Route("api/messaging/inbox/{limit}")]
	    public async Task<IActionResult> GetInbox(int limit)
	    {
		    if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
		    var results = await _responseResolver
			    .WithResolverAsync(await _messagingLogic.GetDirectInboxAsync(limit), ActionType.GetInbox, limit.ToString());
		    if (results == null) return BadRequest("Invalid Request");
		    if (results.Succeeded)
		    {
			    return Ok(results.Value);
		    }

		    return BadRequest(results.Info);
	    }

	    [HttpPost]
	    [Route("api/messaging/text")]
		public async Task<IActionResult> SendDirectText([FromBody] SendDirectTextModel sendDirectText)
		{
		    if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
		    if (sendDirectText == null) return BadRequest("Invalid Params");

		    var recipients = string.Empty;
		    var threads = string.Empty;
			if(sendDirectText.Recipients!=null && sendDirectText.Recipients.Any()) 
				recipients = string.Join(",", sendDirectText.Recipients);
			if (sendDirectText.Threads != null && sendDirectText.Threads.Any())
				threads = string.Join(",", sendDirectText.Threads);

			var results = await _responseResolver
				.WithResolverAsync(
					await _messagingLogic.SendDirectTextAsync(recipients, threads, sendDirectText.TextMessage), 
					ActionType.SendDirectMessageText, sendDirectText.ToJsonString());
			if (results == null) return BadRequest("Invalid Request");
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			return BadRequest(results.Info);
		}

		[HttpPost]
		[Route("api/messaging/link")]
		public async Task<IActionResult> SendDirectLink([FromBody] SendDirectLinkModel sendDirectLink)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");			
			if (sendDirectLink == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithResolverAsync(
					await _messagingLogic.SendDirectLinkAsync(sendDirectLink.TextMessage, sendDirectLink.Link,
						sendDirectLink.Threads.ToArray(), sendDirectLink.Recipients.ToArray()), 
					ActionType.SendDirectMessageLink,
					sendDirectLink.ToJsonString());
			if (results == null) return BadRequest("Invalid Request");
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			return BadRequest(results.Info);
		}

		[HttpPost]
		[Route("api/messaging/photo")]
		public async Task<IActionResult> SendDirectPhoto([FromBody] SendDirectPhotoModel sendDirectPhoto)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (sendDirectPhoto == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithResolverAsync(
					await _messagingLogic.SendDirectPhotoToRecipientsAsync(sendDirectPhoto.Image, sendDirectPhoto.Recipients.ToArray()), 
					ActionType.SendDirectMessagePhoto,
					sendDirectPhoto.ToJsonString());
			if (results == null) return BadRequest("Invalid Request");
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			return BadRequest(results.Info);
		}

		[HttpPost]
		[Route("api/messaging/video")]
		public async Task<IActionResult> SendDirectVideo([FromBody] SendDirectVideoModel sendDirectVideo)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (sendDirectVideo == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithResolverAsync(
					await _messagingLogic.SendDirectVideoToRecipientsAsync(sendDirectVideo.Video, sendDirectVideo.Recipients.ToArray()), 
					ActionType.SendDirectMessageVideo,
					sendDirectVideo.ToJsonString());
			if (results == null) return BadRequest("Invalid Request");
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			return BadRequest(results.Info);
		}

		[HttpPost]
		[Route("api/messaging/profile")]
		public async Task<IActionResult> SendProfile([FromBody] SendDirectProfileModel sendDirectProfile)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (sendDirectProfile == null) return BadRequest("Invalid Params");
			var recipients = string.Empty;
			if (sendDirectProfile.Recipients != null && sendDirectProfile.Recipients.Any())
				recipients = string.Join(",", sendDirectProfile.Recipients);
			var results = await _responseResolver
				.WithResolverAsync(
					await _messagingLogic.SendDirectProfileToRecipientsAsync(sendDirectProfile.userId, recipients), 
					ActionType.SendDirectMessageProfile,
					sendDirectProfile.ToJsonString());
			if (results == null) return BadRequest("Invalid Request");
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}

			return BadRequest(results.Info);
		}

    }
}