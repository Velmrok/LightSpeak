namespace LightSpeak.AppHost.src;

public sealed class AppParameters
{
    public IResourceBuilder<ParameterResource> KcAdminUser { get; init; }
    public IResourceBuilder<ParameterResource> KcAdminPassword { get; init; }

    public IResourceBuilder<ParameterResource> KcGatewaySecret { get; init; }

    public IResourceBuilder<ParameterResource> KcAdminSecret { get; init; }

    public IResourceBuilder<ParameterResource> AppBaseUrl { get; init; }

    public IResourceBuilder<ParameterResource> ClientAudience { get; init; }

    public ReferenceExpression GatewayUrl { get; set; }
    public ReferenceExpression ClientAuthority { get; set; }
   
}