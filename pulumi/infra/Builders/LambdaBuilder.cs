using Pulumi;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Sqs;
using System;

namespace AwsPulumiPoc.Builders
{
    public class LambdaBuilder : BaseBuilder, IBuilder<Function>
    {
        const string DefaultRuntime = "dotnetcore3.1";
        const string BinBaseDirectory = "../../src";

        /// <param name="name">To be used as name of the lambda resource and relative path in: "../src/{Name}/bin/Release/netcoreapp3.1/publish" where the code should be located</param>
        public LambdaBuilder(string name, Output<string> roleArn) : base(name)
        {
            RoleArn = roleArn;
        }

        public Queue? QueueTrigger { get; set; }
        public Output<string> RoleArn { get; set; }

        /// <summary>
        /// Makes the function to be triggered by SQS messages
        /// </summary>
        /// <param name="queueTrigger"></param>
        /// <returns></returns>
        public LambdaBuilder WithSqsTrigger(Queue queueTrigger)
        {
            QueueTrigger = queueTrigger;
            return this;
        }

        public Function Build()
        {
            var function = new Function(Name, new FunctionArgs
            {
                Runtime = DefaultRuntime,
                Handler = $"{Name}::DotnetLambda.Function::FunctionHandler",
                Code = new FileArchive($"{BinBaseDirectory}/{Name}/bin/Release/netcoreapp3.1/publish"),
                Role = RoleArn
            });

            if (QueueTrigger != null)
            {
                new EventSourceMapping($"{Name}-queueTrigger", new EventSourceMappingArgs
                {
                    EventSourceArn = QueueTrigger.Arn,
                    FunctionName = function.Arn
                });
            }

            return function;
        }
    }
}
