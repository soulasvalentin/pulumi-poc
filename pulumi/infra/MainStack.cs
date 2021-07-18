using AwsPulumiPoc.Modules;
using Pulumi;
using Pulumi.Aws.S3;

namespace AwsPulumiPoc
{
    public class MainStack : Stack
    {
        public MainStack()
        {
            var usersModule = new UsersModule();
            usersModule.Create();

            UsersHttpApiEndpoint = Output.All(usersModule.HttpApi.ApiEndpoint, usersModule.HttpApiStage.Name)
                .Apply(x => $"{x[0]}/{x[1]}");
        }

        [Output]
        public Output<string> UsersHttpApiEndpoint { get; set; }
    }
}
