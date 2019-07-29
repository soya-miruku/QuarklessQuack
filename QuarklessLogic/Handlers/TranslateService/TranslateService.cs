﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.ContentSearch.SeleniumClient;
using QuarklessLogic.Handlers.RestSharpClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace QuarklessLogic.Handlers.TranslateService
{
	#region Containers
	public class Detection
	{
		public string language { get; set; }
		public bool isReliable { get; set; }
		public float confidence { get; set; }
	}
	public class BatchResultData
	{
		public List<List<Detection>> detections { get; set; }
	}
	public class BatchResult
	{
		public BatchResultData data { get; set; }
	}
	public struct YandexLanguageResults
	{
		[JsonProperty("lang")]
		public string Lang { get; set; }
		[JsonProperty("code")]
		public int Code { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
	}
	public class TranslateOptions
	{
		public string YandexAPIKey { get; set; }
		public string DetectLangAPIKey { get; set; }
	}
	#endregion


	public class TranslateService : ITranslateService
	{
		private readonly ISeleniumClient _seleniumClient;
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly string _yandexAPIKey;
		private readonly string _detectAPIKey;
		public TranslateService(IOptions<TranslateOptions> tOptions, ISeleniumClient seleniumClient, 
			IRestSharpClientManager restSharpClient)
		{
			_restSharpClient = restSharpClient;
			_seleniumClient = seleniumClient;
			_yandexAPIKey = tOptions.Value.YandexAPIKey;
			_detectAPIKey = tOptions.Value.DetectLangAPIKey;

			_seleniumClient.AddArguments(
				"--headless",
				"--no-sandbox",
				"--disable-web-security",
				"--allow-running-insecure-content",
				"--enable-features=NetworkService",
				"--log-level=3",
				"--silent",
				"--disable-extensions",
				"test-type",
				"--ignore-certificate-errors"
			);
		}
		static int pos = 0;
		public void AddProxy(ProxyModel proxy)
		{
			if (proxy != null)
			{
				_restSharpClient.AddProxy(proxy);
				string proxyLine = string.Empty;
				if (string.IsNullOrEmpty(proxy.Username))
				{
					proxyLine = $"{proxy.Address}:{proxy.Port}";
				}
				else
				{
					proxyLine = $"{proxy.Username}:{proxy.Password}@{proxy.Address}:{proxy.Port}";
				}
				_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
			}
		}
		public IEnumerable<string> DetectLanguageYandex(params string[] texts)
		{
			var trykey = _yandexAPIKey.Split('|');
			if (pos >= trykey.Length)
			{
				var res = DetectLanguageRest(texts);
				foreach(var det in res)
				{
					if (det != null)
					{
						yield return det;
					}
				}
			}
			else 
			{ 
				string url = "https://translate.yandex.net/api/v1.5/tr.json/detect?key="+trykey[pos]+"&text=";
				foreach(var text in texts)
				{
					var postRes = _restSharpClient.PostRequest(url + HttpUtility.UrlEncode(text), null, null);
					if (postRes != null && !string.IsNullOrEmpty(postRes.Content))
					{
						YandexLanguageResults yanRes = JsonConvert.DeserializeObject<YandexLanguageResults>(postRes.Content);
						if (yanRes.Code != 404 && yanRes.Code!=401)
						{
							yield return yanRes.Lang;
						}
						else
						{
							pos++;
							DetectLanguageYandex(texts);
						}
					}
				}
			}
		}
		public IEnumerable<string> DetectLanguageRest(params string[] texts)
		{
			string url = "https://ws.detectlanguage.com/";
			var res = _restSharpClient.PostRequest(url,"/0.2/detect",JsonConvert.SerializeObject(new { q = texts }),username: _detectAPIKey ,password:"");
			if (res.IsSuccessful)
			{
				var batchResult = JsonConvert.DeserializeObject<BatchResult>(res.Content);
				return batchResult.data.detections.Select(s => s.FirstOrDefault().language);
			}
			else
			{
				return DetectLanguageViaGoogle(texts: texts);
			}
		}

		public IEnumerable<string> DetectLanguageViaGoogle(bool selectMostOccuring = false, string splitPattern = "-", params string[] @texts)
		{
			var url = "https://translate.google.com/#view=home&op=translate&sl=auto&tl=en&text={0}";
			string pattern = "goog-inline-block jfk-button jfk-button-standard jfk-button-collapse-right jfk-button-checked";
			var items = texts.Where(_ => !string.IsNullOrEmpty(_)).ToArray().CleanText().ToArray();
			var res = _seleniumClient.DetectLanguageViaGoogle(url, pattern, true, items);
			if(res==null) return null;
			if (!selectMostOccuring) 
				return splitPattern!=null ? res.Select(rf=>rf.Split(splitPattern)[0]) : res;
			else
			{
				return res.GroupBy(x=>x).OrderByDescending(g=>g.Count()).Select(g=>g.Key);
			}
		}
	}
}
