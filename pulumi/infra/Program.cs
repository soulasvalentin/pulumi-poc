using System.Threading.Tasks;
using Pulumi;

namespace AwsPulumiPoc
{
    class Program
    {
        static Task<int> Main() => Deployment.RunAsync<MainStack>();
    }
}