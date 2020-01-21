﻿using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public class Preview
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("fileSizeInBytes")]
		public int FileSizeInBytes { get; set; }

		[JsonProperty("w")]
		public int W { get; set; }

		[JsonProperty("h")]
		public int H { get; set; }
	}
}