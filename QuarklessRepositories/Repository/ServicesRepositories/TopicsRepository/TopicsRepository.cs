﻿using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
namespace QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository
{
	public class TopicsRepository : ITopicsRepository
	{
		private readonly IRepositoryContext _context;
		public TopicsRepository(IRepositoryContext repositoryContext)
		{
			_context = repositoryContext;
		}

		public async Task<bool> AddOrUpdateTopic(TopicsModel topics)
		{
			try
			{
				var res = await _context.Topics.ReplaceOneAsync<TopicsModel>(_ => _.TopicName == topics.TopicName,
					topics,
					new UpdateOptions
					{
						IsUpsert = true
					});
				if (res.IsAcknowledged)
				{
					return true;
				}
				return false;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public async Task<TopicsModel> GetTopicByName(string topicName)
		{
			try
			{
				return (await _context.Topics.FindAsync<TopicsModel>(_ => _.TopicName == topicName)).FirstOrDefault();
			}
			catch (Exception e)
			{
				return null;
			}
		}
		public async Task<IEnumerable<TopicsModel>> GetTopics()
		{
			try
			{
				return (await _context.Topics.FindAsync<TopicsModel>(_=>true)).ToList();
			}
			catch(Exception e)
			{
				return null;
			}
		}
	}
}
