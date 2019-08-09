﻿using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.UserAuth.Auth;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.AuthLogic.Auth
{
	public interface IAuthHandler
	{
		Task<bool> SignIn(AccountUser user, bool isPersistent = false);
		Task<bool> CreateAccount(AccountUser accountUser, string password);
		Task<AccountUser> GetUserByUsername(string username);
		Task<bool> UpdateUser(AccountUser accountUser);
		Task<ResultCarrier<AdminInitiateAuthResponse>> Login(LoginRequest loginRequest);
		Task<ResultCarrier<RespondToAuthChallengeResponse>> SetNewPassword(NewPasswordRequest Newrequest);
		Task<ResultCarrier<SignUpResponse>> Register(RegisterAccountModel registerAccount);
		Task<ResultCarrier<GetUserResponse>> GetUser(string accessToken);
		Task<ResultCarrier<ConfirmSignUpResponse>> ConfirmSignUp(EmailConfirmationModel emailConfirmationModel);
		Task<ResultCarrier<AdminAddUserToGroupResponse>> AddUserToGroup(string groupName, string username);
		ResultCarrier<CognitoUser> GetUserById(string userId);
		Task<ResultCarrier<InitiateAuthResponse>> RefreshLogin(string refreshToken, string userName);
	}
}