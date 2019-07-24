﻿using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Extensions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.TextGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class ContentManager : IContentManager
	{
		private readonly ITopicBuilder _topicBuilder; 
		private readonly ITextGeneration _textGeneration;
		public ContentManager(ITopicBuilder topicBuilder, ITextGeneration textGeneration)
		{
			_topicBuilder = topicBuilder;
			_textGeneration = textGeneration;
		}
		public async Task<List<TopicsModel>> GetTopics(List<string> topics, int takeSuggested = -1, int limit = -1)
		{
			List<TopicsModel> totalFound = new List<TopicsModel>();
			foreach(var topic in topics) { 
				var res = await _topicBuilder.Build(topic, takeSuggested, limit);
				if(res!=null)
					totalFound.Add(res);
			}

			return totalFound;
		}
		public async Task<IEnumerable<string>> GetHashTags (string topic, string subcategory, string language, int limit, int pickAmount)
		{
			return await _topicBuilder.BuildHashtags(topic,subcategory,language,limit,pickAmount);
		}
		public string GenerateText(string topic,string lang, int type, int limit, int size)
		{
			//return _textGeneration.MarkovTextGenerator(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\{0}.csv",
			//	type,topic,lang,size,limit) ;
			return _textGeneration.MarkovIt(type,topic,lang,size,limit).GetAwaiter().GetResult();
		}
		public string GenerateMediaInfo(Topics topicSelect, string topicSelected, string language, string credit = null)
		{
			List<string> selections = new List<string>();
			List<string> hashtagsToUse = new List<string>();
			//possibly analyse image and give out relevant hashtags
			QuarklessContexts.Models.Profiles.SubTopics selectASubTopic;
			if (!string.IsNullOrEmpty(topicSelected))
			{
				var dist = topicSelect.SubTopics.Select(s=>s.TopicName.Similarity(topicSelected)).Min(x=>x);
				var pos = topicSelect.SubTopics.FindIndex(n => n.TopicName.Similarity(topicSelected) == dist);
				selectASubTopic = topicSelect.SubTopics.ElementAtOrDefault(pos);
			}
			else { 
				selectASubTopic = topicSelect.SubTopics.ElementAtOrDefault(SecureRandom.Next(topicSelect.SubTopics.Count-1));
			}
			if (selectASubTopic != null)
			{
				selections.Add(selectASubTopic.TopicName);
				selections.Add(topicSelect.TopicFriendlyName);
				selections.AddRange(selectASubTopic.RelatedTopics);

				var hashes = GetHashTags(topicSelect.TopicFriendlyName, topicSelected, language, 200, 15).GetAwaiter().GetResult();
				if(hashes!=null && hashes.Count() > 0)
					selections.AddRange(hashes);
			}

			hashtagsToUse.AddRange(selections.Take(SecureRandom.Next(20,24)).Select(s=> $"#{s}"));
			var hashtags = hashtagsToUse.Select(j=>j.Replace(" ","")).JoinEvery(Environment.NewLine, 3);
			var caption_ = GenerateText(topicSelect.TopicFriendlyName.ToLower(), language.ToUpper(), 1, SecureRandom.Next(4), SecureRandom.Next(2,6)).Split(',')[0];
			string creditLine = string.Empty;
			if (credit != null)
				creditLine = $"credit: @{credit}";

			return caption_ + Environment.NewLine + creditLine + Environment.NewLine + hashtags;
		}
		public string GenerateComment(string topic, string language)
		{
			var comment = GenerateText(topic.ToLower(), language.ToUpper(), 0 , SecureRandom.Next(4) ,SecureRandom.Next(2,6)).Split(',')[0];
			return comment;
		}

		public Task<Topics> GetTopic(UserStoreDetails userstoredetails, ProfileModel profile, int takeSuggested = -1)
		{
			_topicBuilder.Init(userstoredetails);
			return _topicBuilder.BuildTopics(profile,takeSuggested);
		}
	}
}
