﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.ProxyRepository;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.Handlers.EventHandlers;

namespace QuarklessLogic.Logic.ProxyLogic
{
	public enum ConnectionType
	{
		Any,

		[Description("Residential")]
		Residential,

		[Description("Mobile")]
		Mobile,

		[Description("Datacenter")]
		Datacenter
	}
	public struct IPResponse
	{
		[JsonProperty("ip")]
		public string IP;
	}
	public class ProxyItem
	{
		[JsonProperty("proxy")]
		public string Proxy { get; set; }

		[JsonProperty("ip")]
		public string IP { get; set; }

		[JsonProperty("port")]
		public string Port { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("lastChecked")]
		public int LastChecked { get; set; }

		[JsonProperty("get")]
		public bool Get { get; set; }

		[JsonProperty("post")]
		public bool Post { get; set; }

		[JsonProperty("cookies")]
		public bool Cookies { get; set; }

		[JsonProperty("referer")]
		public bool Referer { get; set; }

		[JsonProperty("userAgent")]
		public bool UserAgent { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("currentThreads")]
		public int CurrentThreads { get; set; }

		[JsonProperty("threadsAllowed")]
		public int ThreadsAllowed { get; set; }
	}
	public class ProxyResponse
	{
		public Datum[] data { get; set; }
		public int count { get; set; }
	}
	public class Datum
	{
		public string ipPort { get; set; }
		public string ip { get; set; }
		public string port { get; set; }
		public string country { get; set; }
		public string last_checked { get; set; }
		public string proxy_level { get; set; }
		public string type { get; set; }
		public string speed { get; set; }
		public Support support { get; set; }
	}
	public class Support
	{
		public int https { get; set; }
		public int get { get; set; }
		public int post { get; set; }
		public int cookies { get; set; }
		public int referer { get; set; }
		public int user_agent { get; set; }
		public int google { get; set; }
	}

	public class ProxyLogic : IProxyLogic, IEventSubscriber<ProfileModel>
	{
		private readonly IProxyRepository _proxyRepository;
		private readonly IReportHandler _reportHandler;
		public ProxyLogic(IProxyRepository proxyRepository, IReportHandler reportHandler)
		{
			_proxyRepository = proxyRepository;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("/Logic/Proxy");
		}

		public bool AddProxies(List<ProxyModel> proxies)
		{
			try
			{
				if (proxies.Any(s => string.IsNullOrEmpty(s.Address))) return false;
				_proxyRepository.AddProxies(proxies);
				return true;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return false;
			}
		}

		public bool AddProxy(ProxyModel proxy)
		{
			try
			{
				_proxyRepository.AddProxy(proxy);
				return true;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return false;
			}
		}
		public async Task<bool> AssignProxy(AssignedTo assignedTo)
		{
			try
			{
				var results = await _proxyRepository.AssignProxy(assignedTo);
				return results;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport($"Failed to assign proxy to user: {assignedTo.Account_Id}, error: {ee}");
				return false;
			}
		}
		public async Task<IEnumerable<ProxyModel>> GetAllAssignedProxies()
		{
			var results = await _proxyRepository.GetAllAssignedProxies();
			return results;
		}

		public static HttpClientHandler RegisterProxy(ProxyModel proxyDetails)
		{
			var proxy = new WebProxy(proxyDetails.Address, proxyDetails.Port)
			{
				//Address = new Uri($"http://{proxyDetails.Address}:{proxyDetails.Port}"), //i.e: http://1.2.3.4.5:8080
				BypassProxyOnLocal = false,
				UseDefaultCredentials = false
			};

			if (proxyDetails.Username != null && proxyDetails.Password != null)
			{
				proxy.Credentials = new NetworkCredential(userName: proxyDetails.Username, password: proxyDetails.Password);
			}
			var httpClientHandler = new HttpClientHandler()
			{
				Proxy = proxy,
			};

			if (proxyDetails.NeedServerAuth)
			{
				httpClientHandler.PreAuthenticate = true;
				httpClientHandler.UseDefaultCredentials = false;
				httpClientHandler.Credentials = new NetworkCredential(
					userName: proxyDetails.Username,
					password: proxyDetails.Password);
			}
			return httpClientHandler;
		}
		public async Task<bool> TestProxy(ProxyItem proxy)
		{
			try
			{
				var req = (HttpWebRequest)HttpWebRequest.Create("http://ip-api.com/json");
				req.Timeout = 4000;
				req.Proxy = new WebProxy($"http://{proxy.Proxy}/");

				var resp = await req.GetResponseAsync();
				var json = new StreamReader(resp.GetResponseStream()).ReadToEnd();

				var myip = (string)JObject.Parse(json)["query"];

				return myip == proxy.IP;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}
		public async Task<bool> TestProxy(ProxyModel proxy)
		{
			try
			{
				var req = (HttpWebRequest)WebRequest.Create("http://ip-api.com/json");
				req.Timeout = 4000;
				req.Proxy = new WebProxy($"{proxy.Address}:{proxy.Port}",false);

				var resp = await req.GetResponseAsync();
				var json = new StreamReader(resp.GetResponseStream()).ReadToEnd();

				var myip = (string)JObject.Parse(json)["query"];

				return myip == proxy.Address;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}
		public async Task<bool> TestProxy(Datum proxy)
		{
			try
			{
				var req = (HttpWebRequest)WebRequest.Create("http://ip-api.com/json");
				req.Timeout = 4000;
				req.Proxy = new WebProxy($"http://{proxy.ipPort}/");

				var resp = await req.GetResponseAsync();
				var json = new StreamReader(resp.GetResponseStream() ?? throw new InvalidOperationException()).ReadToEnd();

				var myIp = (string)JObject.Parse(json)["query"];

				return myIp == proxy.ip;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}
		public async Task<ProxyModel> GetProxyAssignedTo(string accountId, string instagramAccountId)
		{
			var results = await _proxyRepository.GetAssignedProxyOf(accountId, instagramAccountId);
			return results ?? null;
		}

		public async Task<ProxyModel> ProxyListGrab()
		{
			try
			{
				const string baseUrl = "https://portal.proxyguys.com";

				var headers = new WebHeaderCollection();
				var request = (HttpWebRequest) WebRequest.Create(baseUrl);
				using(var response = (HttpWebResponse) await request.GetResponseAsync())
					using(var stream = response.GetResponseStream())
					using (var reader = new StreamReader(stream))
					{
						var jsonResponse = reader.ReadToEnd();
						headers = response.Headers;
					}

				var loginUrl = $"{baseUrl}/login";
				var requestLogin = (HttpWebRequest) WebRequest.Create(loginUrl);

				const string loginPost = "username=pgtrial3&password=proxyguys#1";
				var data = Encoding.ASCII.GetBytes(loginPost);

				requestLogin.Headers = headers;
				requestLogin.Method = "POST";
				requestLogin.ContentType = "application/x-www-form-urlencoded";
				requestLogin.ContentLength = data.Length;

				using (var stream = requestLogin.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				var responseLogin = (HttpWebResponse)requestLogin.GetResponse();

				var responseString = new StreamReader(responseLogin.GetResponseStream()).ReadToEnd();




//				ProxyListObject proxyItem;
//				var request = (HttpWebRequest) WebRequest.Create(baseUrl);
//				using (var response = (HttpWebResponse) await request.GetResponseAsync())
//					using(var stream = response.GetResponseStream())
//					using (var reader = new StreamReader(stream))
//					{
//						var jsonResponse = reader.ReadToEnd();
//						proxyItem = JsonConvert.DeserializeObject<ProxyListObject>(jsonResponse);
//					}
//
//				if (string.IsNullOrEmpty(proxyItem?.ip))
//					return await ProxyListGrab();
//				var proxy = new ProxyModel
//				{
//					Address = proxyItem.ip,
//					Port = proxyItem.port,
//					Region = proxyItem.country
//				};
//				if (await TestProxy(proxy))
//				{
//					return proxy;
//				}

				return await ProxyListGrab();
			}
			catch (Exception ee)
			{
				return await ProxyListGrab();
			}
		}
		public async Task<ProxyModel> ProxyGrabber()
		{
			try
			{
				const string baseUrl = "http://pubproxy.com/api/proxy?type=socks5?level=elite";
				ProxyResponse proxyItem;
				var request = (HttpWebRequest)WebRequest.Create(baseUrl);
				using (var response = (HttpWebResponse)await request.GetResponseAsync())
				using (var stream = response.GetResponseStream())
				using (var reader = new StreamReader(stream))
				{
					var jsonResponse = reader.ReadToEnd();
					proxyItem = JsonConvert.DeserializeObject<ProxyResponse>(jsonResponse);
				}

				if (string.IsNullOrEmpty(proxyItem?.data.SingleOrDefault()?.ipPort))
					return await ProxyGrabber();
				if (await TestProxy(proxyItem.data.SingleOrDefault()))
				{
					return new ProxyModel
					{
						Address = proxyItem.data.SingleOrDefault()?.ip,
						Port = int.Parse(proxyItem.data.SingleOrDefault()?.port ?? throw new InvalidOperationException()),
						Region = proxyItem.data.SingleOrDefault()?.country,
						Type = proxyItem.data.SingleOrDefault()?.type
					};
				}

				return await ProxyGrabber();
			}
			catch (Exception ex)
			{
				_reportHandler.MakeReport(ex);
				return await ProxyGrabber();
			}
		}
		public async Task<ProxyModel> RetrieveRandomProxy(bool? get = null, bool? post = null, bool? cookies = null, bool? referer = null,
			bool? userAgent = null, int port = -1, string city = null, string state = null, string country = null,
			ConnectionType connectionType = ConnectionType.Any)
		{
			try
			{
				#region URL BUILD
				var baseUrl = $@"http://falcon.proxyrotator.com:51337/?apiKey=XR4E5JzkxMZcovaYQW2VUBw3PDj876eK";
				if (get != null)
					baseUrl += $"&get={get}";
				if (post != null)
					baseUrl += $"&post={post}";
				if (cookies != null)
					baseUrl += $"&cookies={cookies}";
				if (referer != null)
					baseUrl += $"&referer={referer}";
				if (userAgent != null)
					baseUrl += $"&userAgent={userAgent}";
				if (port != -1)
					baseUrl += $"&port={port}";
				if (!string.IsNullOrEmpty(city))
					baseUrl += $"&city={city}";
				if (!string.IsNullOrEmpty(state))
					baseUrl += $"&state={state}";
				if (!string.IsNullOrEmpty(country))
					baseUrl += $"&country={country}";
				if (connectionType != ConnectionType.Any)
					baseUrl += $"&connectionType={connectionType.GetDescription()}";
				#endregion
				ProxyItem proxyItem;
				var request = (HttpWebRequest)WebRequest.Create(baseUrl);

				using (var response = (HttpWebResponse)await request.GetResponseAsync())
				using (var stream = response.GetResponseStream())
				using (var reader = new StreamReader(stream))
				{
					var jsonResponse = reader.ReadToEnd();
					proxyItem = JsonConvert.DeserializeObject<ProxyItem>(jsonResponse);
				}

				if (string.IsNullOrEmpty(proxyItem?.Proxy) || string.IsNullOrEmpty(proxyItem?.IP))
					return await RetrieveRandomProxy(get, post, cookies, referer, userAgent, port, city, state, country,
						connectionType);
				if (await TestProxy(proxyItem))
				{
					return new ProxyModel
					{
						Address = proxyItem.IP,
						Port = int.Parse(proxyItem.Port),
						Region = proxyItem.Country,
						Type = proxyItem.Type
					};
				}

				return await RetrieveRandomProxy(get, post, cookies, referer, userAgent, port, city, state, country, connectionType);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<bool> RemoveUserFromProxy(AssignedTo assignedTo)
		{
			try
			{
				if ((await GetProxyAssignedTo(assignedTo.Account_Id, assignedTo.InstaId)) != null)
				{
					var results = await _proxyRepository.RemoveUserFromProxy(assignedTo);
					return results;
				}
				return false;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport($"Failed to assign proxy to user: {assignedTo.Account_Id}, error: {ee}");
				return false;
			}
		}

		public async Task Handle(ProfileModel @event)
		{
			await AssignProxy(new AssignedTo
			{
				Account_Id = @event.Account_Id, 
				InstaId = @event.InstagramAccountId
			});
		}
	}
}
