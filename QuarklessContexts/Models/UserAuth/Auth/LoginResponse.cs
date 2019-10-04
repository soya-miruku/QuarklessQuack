﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.UserAuth.Auth
{
	public class LoginResponse
	{
		public string IdToken { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string Username { get; set; }
		public int ExpiresIn { get; set; }
	}
}