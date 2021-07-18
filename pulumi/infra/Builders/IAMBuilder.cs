using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Sqs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AwsPulumiPoc.Builders
{
    public class IAMBuilder : BaseBuilder, IBuilder<Role>
    {
        public IAMBuilder(string name) : base(name)
        {
            ServicePrincipals = new List<string>();
            Actions = new List<string>();
            Resources = new List<string>();
        }

        public List<string> ServicePrincipals { get; set; }
        public List<string> Actions { get; set; }
        public List<string> Resources { get; set; }

        /// <summary>
        /// Allows to be invoked by certain service.
        /// </summary>
        /// <param name="servicePrincipal">Service name. Example: "lambda.amazonaws.com"</param>
        /// <returns></returns>
        public IAMBuilder AddServicePrincipal(string servicePrincipal)
        {
            ServicePrincipals.Add(servicePrincipal);
            return this;
        }

        /// <summary>
        /// Adds support for specified policy action.
        /// </summary>
        /// <param name="policy">Policy action name. "*" are allowed. Example: "sqs:CreateMessage"</param>
        /// <returns></returns>
        public IAMBuilder AddPolicyAction(string policy)
        {
            Actions.Add(policy);
            return this;
        }

        /// <summary>
        /// Allow access to certain resource/s.
        /// </summary>
        /// <param name="resource">Resource ARN. "*" are allowed</param>
        /// <returns></returns>
        public IAMBuilder AddResource(string resource)
        {
            Resources.Add(resource);
            return this;
        }

        private void Validate()
        {
            if (ServicePrincipals.HasEmptyItems()) err("service principal");
            if (Actions.HasEmptyItems()) err("policy action");
            if (Resources.HasEmptyItems()) err("resource");

            void err(string s) => throw new Exception($"You must specify at least one {s}");
        }

        public Role Build()
        {
            Validate();

            var role = new Role($"{Name}-role", new RoleArgs
            {
                AssumeRolePolicy = IAMBuilderTools.BuildServicesJson(ServicePrincipals)
            });

            new RolePolicy($"{Name}-policy", new RolePolicyArgs
            {
                Role = role.Id,
                Policy = IAMBuilderTools.BuildPolicyJson(Actions, Resources)
            });

            return role;
        }
    }

    public class IAMBuilderTools
    {
        public static string BuildServicesJson(List<string> servicePrincipals)
        {
            return $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [{{
                            ""Action"": ""sts: AssumeRole"",
                            ""Principal"": {{
                                ""Service"": {BuildDynamicItemsJsonValue(servicePrincipals)}
                                }},
                                ""Effect"": ""Allow"",
                                ""Sid"": """"
                            }}
                        ]
                    }}";
        }

        public static string BuildPolicyJson(List<string> actions, List<string> resources)
        {
            return $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [{{
                            ""Effect"": ""Allow"",
                            ""Action"": {BuildDynamicItemsJsonValue(actions)},
                            ""Resource"": {BuildDynamicItemsJsonValue(resources)}
                        }}]
                    }}";
        }

        /// <summary>
        /// Builds a JSON value. Returns array (["1", "2", "3"]) if multiple items are provided, else, a single item is returned ("1"). Null if empty list
        /// </summary>
        public static string? BuildDynamicItemsJsonValue(List<string> items)
        {
            if (items.Count == 1)
                return $"\"{items[0]}\"";
            else if (items.Count > 0)
                return $"[{string.Join(',', items.Select(x => $"\"{x}\""))}]";
            return null;
        }
    }
}
