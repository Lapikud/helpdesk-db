using FreeIPA.DotNet.Models;
using FreeIPA.DotNet.Models.Login;
using FreeIPA.DotNet.Models.RPC;

namespace WebApp.ApiControllers.Identity;

public interface IIpaAuthClient
{
    Task<IpaResultModel<IpaLoginResponseModel>> LoginWithPassword(IpaLoginRequestModel request);
    Task<IpaResultModel<IpaRpcResponseModel>> SendRpcRequest(IpaRpcRequestModel request);
}
