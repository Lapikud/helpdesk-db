using FreeIPA.DotNet;
using FreeIPA.DotNet.Models;
using FreeIPA.DotNet.Models.Login;
using FreeIPA.DotNet.Models.RPC;

namespace WebApp.ApiControllers.Identity;

public class IpaAuthClient : IIpaAuthClient
{
    private readonly IpaClient _client = new("https://ipa.lapikud.ee");

    public Task<IpaResultModel<IpaLoginResponseModel>> LoginWithPassword(IpaLoginRequestModel request)
        => _client.LoginWithPassword(request);

    public Task<IpaResultModel<IpaRpcResponseModel>> SendRpcRequest(IpaRpcRequestModel request)
        => _client.SendRpcRequest(request);
}
