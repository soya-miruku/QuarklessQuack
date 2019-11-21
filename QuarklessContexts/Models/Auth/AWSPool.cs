﻿using System;

namespace QuarklessContexts
{
	[Serializable]
	public class AWSPool
	{
		public string AppClientID { get; set; }
		public string AppClientSecret { get; set;}
		public string AuthUrl { get; set; }
		public string PoolID { get; set; }
		public string Region { get; set; }
	}
}