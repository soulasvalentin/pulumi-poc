using AwsPulumiPoc.Builders;
using FluentAssertions;
using Pulumi.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Aws = Pulumi.Aws;

namespace Test
{
    public class IAMBuilderTestStack : Pulumi.Stack
    {
        public IAMBuilderTestStack()
        {
            new IAMBuilder("simple")
                .AddServicePrincipal("lambda.amazonaws.com")
                .AddPolicyAction("sqs:*")
                .AddResource("*")
                .Build();
        }
    }

    public class IAMBuilderTest
    {
        [Fact]
        public async Task BuildRole()
        {
            var resources = await Testing.RunAsync<IAMBuilderTestStack>();

            var role = resources.OfType<Aws.Iam.Role>().FirstOrDefault();
            role.Should().NotBeNull("IAM Role not found");

            var roleAssumePolicy = await OutputUtilities.GetValueAsync(role.AssumeRolePolicy);
            roleAssumePolicy.Should().NotBeNull("AssumeRolePolicy is not defined");
            roleAssumePolicy.Should().BeEquivalentTo($@"{{
                        ""Version"": ""2012-10-17"",
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [{{
                            ""Action"": ""sts: AssumeRole"",
                            ""Principal"": {{
                                ""Service"": ""lambda.amazonaws.com""
                                }},
                                ""Effect"": ""Allow"",
                                ""Sid"": """"
                            }}
                        ]
                    }}");

            var rolePolicy = resources.OfType<Aws.Iam.RolePolicy>().FirstOrDefault();
            rolePolicy.Should().NotBeNull("IAM Role policy not found");

            var rolePolicyJson = await OutputUtilities.GetValueAsync(rolePolicy.Policy);
            rolePolicyJson.Should().NotBeNull("Policy is not defined");
            rolePolicyJson.Should().BeEquivalentTo($@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [{{
                            ""Effect"": ""Allow"",
                            ""Action"": ""sqs:*"",
                            ""Resource"": ""*""
                        }}]
                    }}");
        }
    }
}
