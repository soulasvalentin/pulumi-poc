using AwsPulumiPoc.Builders;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.ApiGatewayV2;
using Pulumi.Aws.Sqs;

namespace AwsPulumiPoc.Modules
{
    public class UsersModule
    {
        public Function GetUsersLambda { get; private set; }
        public Function CreateUserLambda { get; private set; }
        public Api HttpApi { get; private set; }
        public Stage HttpApiStage { get; private set; }
        public Queue UsersCreationsQueue { get; private set; }

        public void Create()
        {
            // SQS
            // TODO: create SQS builder
            UsersCreationsQueue = new Queue("users-creations", new QueueArgs
            {
            });

            // Lambdas
            var lambdaRole = new IAMBuilder("userlambda")
                    .AddServicePrincipal("lambda.amazonaws.com")
                    .AddPolicyAction("logs:*") // TODO: reduce scope
                    .AddPolicyAction("sqs:*") // TODO: reduce scope
                    .AddResource("*") // TODO: reduce scope
                    .Build();

            GetUsersLambda = new LambdaBuilder(nameof(GetUsersLambda), lambdaRole.Arn)
                .Build();
            CreateUserLambda = new LambdaBuilder(nameof(CreateUserLambda), lambdaRole.Arn)
                .WithSqsTrigger(UsersCreationsQueue)
                .Build();

            // API Gateway
            (HttpApi, HttpApiStage) = new ApiGatewayBuilder("usersapi", "HTTP")
                .AddEndpoint(new ApiEndpointParams
                {
                    UniqueName = nameof(GetUsersLambda),
                    Type = ApiEndpointType.LAMBDA,
                    Resource = GetUsersLambda,
                    HttpMethod = "GET",
                    Route = "users"
                })
                .AddEndpoint(new ApiEndpointParams
                {
                    UniqueName = nameof(CreateUserLambda),
                    Type = ApiEndpointType.SQS,
                    Resource = UsersCreationsQueue,
                    HttpMethod = "POST",
                    Route = "users"
                })
                .Build();
        }
    }
}
