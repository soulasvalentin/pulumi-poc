using Pulumi;
using Pulumi.Aws.ApiGatewayV2;
using Pulumi.Aws.Lambda;
using System.Collections.Generic;
using System;
using Pulumi.Aws.Sqs;

namespace AwsPulumiPoc.Builders
{
    public class ApiGatewayBuilder : BaseBuilder, IBuilder<(Api api, Stage stage)>
    {
        /// <param name="name">To be used as name of the api gateway resource</param>
        /// <param name="protocolType">The API protocol. Valid values: `HTTP`, `WEBSOCKET`.</param>
        public ApiGatewayBuilder(string name, string protocolType) : base(name)
        {
            if (string.IsNullOrEmpty(protocolType))
                throw new ArgumentException($"'{nameof(protocolType)}' cannot be null or empty.", nameof(protocolType));

            ProtocolType = protocolType;
            ApiEndpointsParams = new List<ApiEndpointParams>();
        }

        public string ProtocolType { get; }
        public List<ApiEndpointParams> ApiEndpointsParams { get; }

        public ApiGatewayBuilder AddEndpoint(ApiEndpointParams endpointParams)
        {
            endpointParams.Validate();
            ApiEndpointsParams.Add(endpointParams);
            return this;
        }

        public (Api api, Stage stage) Build()
        {
            // api gateway
            var api = new Api(Name, new ApiArgs
            {
                ProtocolType = ProtocolType
            });

            // endpoints
            var routes = new InputList<Resource>();

            foreach (var apiEndpointParams in ApiEndpointsParams)
            {
                Permission permission = null;
                Integration integration = null;

                // TODO: improve type handling
                if (apiEndpointParams.Type == ApiEndpointType.LAMBDA)
                {
                    permission = new Permission($"{apiEndpointParams.UniqueName}-lambdaPermission", new PermissionArgs
                    {
                        Action = "lambda:InvokeFunction",
                        Principal = "apigateway.amazonaws.com",
                        Function = (apiEndpointParams.Resource as Function).Name,
                        SourceArn = api.ExecutionArn.Apply(x => $"{x}/*/*"),
                    }, new CustomResourceOptions
                    {
                        DependsOn = { api, apiEndpointParams.Resource }
                    });
                    integration = CreateLambdaIntegration(api, apiEndpointParams);
                }
                else if (apiEndpointParams.Type == ApiEndpointType.SQS)
                    integration = CreateSqsIntegration(api, apiEndpointParams);
                else
                    throw new Exception("Unknown api endpoint type");

                var route = new Route($"{apiEndpointParams.UniqueName}-route", new RouteArgs
                {
                    ApiId = api.Id,
                    RouteKey = $"{apiEndpointParams.HttpMethod} /{apiEndpointParams.Route}",
                    Target = integration.Id.Apply(x => $"integrations/{x}"),
                });
                routes.Add(route);
            }
            
            // stage
            var stage = new Stage(Name, new StageArgs
            {
                ApiId = api.Id,
                AutoDeploy = true
            }, new CustomResourceOptions
            {
                DependsOn = routes
            });

            return (api, stage);
        }
        
        private Integration CreateLambdaIntegration(Api api, ApiEndpointParams apiEndpointParams)
        {
            return new Integration($"{apiEndpointParams.UniqueName}-lambdaIntegration", new IntegrationArgs
            {
                ApiId = api.Id,
                IntegrationType = "AWS_PROXY",
                IntegrationUri = (apiEndpointParams.Resource as Function).Arn,
                IntegrationMethod = apiEndpointParams.HttpMethod,
                PayloadFormatVersion = "2.0",
                PassthroughBehavior = "WHEN_NO_MATCH",
            });
        }

        private Integration CreateSqsIntegration(Api api, ApiEndpointParams apiEndpointParams)
        {
            var integrationRole = new IAMBuilder($"{apiEndpointParams.UniqueName}-integration")
                .AddServicePrincipal("lambda.amazonaws.com") // TODO: analyze
                .AddPolicyAction("logs:*") // TODO: reduce scope
                .AddPolicyAction("sqs:*") // TODO: reduce scope
                .AddResource("*") // TODO: reduce scope
                .Build();

            return new Integration($"{apiEndpointParams.UniqueName}-lambdaIntegration", new IntegrationArgs
            {
                ApiId = api.Id,
                IntegrationType = "AWS_PROXY",
                IntegrationSubtype = "SQS-SendMessage",
                CredentialsArn = integrationRole.Arn,
                RequestParameters =
                {
                    { "QueueUrl", (apiEndpointParams.Resource as Queue).Url },
                    { "MessageBody", "$request.body" },
                }
            });
        }
    }

    public class ApiEndpointParams
    {
        public string UniqueName { get; set; }
        public ApiEndpointType Type { get; set; }
        public Resource Resource { get; set; }
        /// <summary>
        /// Without starting "/" char
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// Must be upercase
        /// </summary>
        public string HttpMethod { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(UniqueName))
                throw new ArgumentException($"'{nameof(UniqueName)}' cannot be null or empty.", nameof(UniqueName));

            if (Resource is null)
                throw new ArgumentNullException(nameof(Resource));

            if (string.IsNullOrEmpty(Route))
                throw new ArgumentException($"'{nameof(Route)}' cannot be null or empty.", nameof(Route));

            if (string.IsNullOrEmpty(HttpMethod))
                throw new ArgumentException($"'{nameof(HttpMethod)}' cannot be null or empty.", nameof(HttpMethod));
        }
    }

    public enum ApiEndpointType
    {
        LAMBDA,
        SQS
    }
}
