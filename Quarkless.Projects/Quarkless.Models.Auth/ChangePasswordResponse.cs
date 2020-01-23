﻿using System.Collections.Generic;
using Amazon.CognitoIdentityProvider;

namespace Quarkless.Models.Auth
{
	public class ChangePasswordResponse
	{
		public string Username { get; set; }
		public ChallengeNameType ChallengeNameType { get; set; }
		public string Session { get; set; }
		public Dictionary<string, string> ChallengeParams { get; set; }
	}
}