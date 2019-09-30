﻿using System.ComponentModel;

namespace QuarklessContexts.Enums
{
	public enum ContentType
	{
		Image,
		Collection,
		Video,
		Story,
		Carousel,
		Comment,
		Bio,
		DirectMessage,
		ITV
	}
	public enum ActionType
	{
		[Description("none")]
		None,
		[Description("createpost")]
		CreatePost,
		[Description("createstory")]
		CreateStory,
		[Description("comment")]
		CreateCommentMedia,
		[Description("commentreply")]
		CreateCommentReply,
		[Description("createbio")]
		CreateBiography,
		[Description("followhashtag")]
		FollowHashtag,
		[Description("unfollowhashtag")]
		UnFollowHashtag,
		[Description("followuser")]
		FollowUser,
		[Description("unfollowuser")]
		UnFollowUser,
		[Description("likemedia")]
		LikePost,
		[Description("unlikemedia")]
		UnlikePost,
		[Description("likecomment")]
		LikeComment,
		[Description("unlikecomment")]
		UnlikeComment,
		[Description("actionchecker")]
		MaintainAccount,
		[Description("GetRecentActivityFeed")]
		RecentActivityFeed,
		[Description("refreshlogin")]
		RefreshLogin,
		[Description("changedProfilePicture")]
		ChangeProfilePicture,
		[Description("getinbox")]
		GetInbox,
		[Description("getthread")]
		GetThread,
		[Description("senddirectmessage")]
		SendDirectMessage,
		[Description("sendmessagetext")]
		SendDirectMessageText,
		[Description("sendmessagelink")]
		SendDirectMessageLink,
		[Description("sendmessagephoto")]
		SendDirectMessagePhoto,
		[Description("sendmessagevideo")]
		SendDirectMessageVideo,
		[Description("sendmessageaudio")]
		SendDirectMessageAudio,
		[Description("sendmessageprofile")]
		SendDirectMessageProfile,
		[Description("sharemessagemedia")]
		SendDirectMessageMedia
	}
}
