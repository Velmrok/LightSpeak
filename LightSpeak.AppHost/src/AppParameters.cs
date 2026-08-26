namespace LightSpeak.AppHost.src;

public sealed class AppParameters
{
    public required IResourceBuilder<ParameterResource> KcAdminUser { get; init; }
    public required IResourceBuilder<ParameterResource> KcAdminPassword { get; init; }

    public required IResourceBuilder<ParameterResource> KcGatewaySecret { get; init; }

    public required  IResourceBuilder<ParameterResource> KcAdminSecret { get; init; }

    public required IResourceBuilder<ParameterResource> AppBaseUrl { get; init; }

    public required IResourceBuilder<ParameterResource> ClientAudience { get; init; }
    public required IResourceBuilder<ParameterResource> RabbitUser { get; init; }
    public required IResourceBuilder<ParameterResource> RabbitPassword { get; init; }
    public required IResourceBuilder<ParameterResource> PostgresUser { get; init; }
    public required IResourceBuilder<ParameterResource> PostgresPassword { get; init; }
    public ReferenceExpression GatewayUrl { get; set; }
    public ReferenceExpression ClientAuthority { get; set; }
    
}