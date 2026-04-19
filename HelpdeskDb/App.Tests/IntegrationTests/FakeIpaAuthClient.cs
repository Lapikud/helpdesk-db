using FreeIPA.DotNet.Models;
using FreeIPA.DotNet.Models.Login;
using FreeIPA.DotNet.Models.RPC;
using WebApp.ApiControllers.Identity;

namespace App.Tests.IntegrationTests;

public class FakeIpaAuthClient : IIpaAuthClient
{
    public string[] Groups { get; set; } = { "admins", "members", "pixels", "helpdesk_db_admins" };

    public Task<IpaResultModel<IpaLoginResponseModel>> LoginWithPassword(IpaLoginRequestModel request)
    {
        var result = new IpaResultModel<IpaLoginResponseModel>
        {
            Success = true,
            Data = new IpaLoginResponseModel(),
            Message = "OK"
        };
        return Task.FromResult(result);
    }

    public Task<IpaResultModel<IpaRpcResponseModel>> SendRpcRequest(IpaRpcRequestModel request)
    {
        var memberOfJson = string.Join(",", Groups.Select(g => $"\"{g}\""));
        var payload = $"{{\"result\":{{\"memberof_group\":[{memberOfJson}]}}}}";

        var rpc = new IpaRpcResponseModel
        {
            Result = System.Text.Json.JsonDocument.Parse(payload).RootElement
        };

        var result = new IpaResultModel<IpaRpcResponseModel>
        {
            Success = true,
            Data = rpc,
            Message = "OK"
        };
        return Task.FromResult(result);
    }
}
