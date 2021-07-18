using AwsPulumiPoc.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Test
{
    public class IAMBuilderToolsTest
    {
        [Theory]
        [InlineData("1", "\"1\"")]
        [InlineData("1,2", "[\"1\",\"2\"]")]
        public void BuildDynamicValue(string itemsStr, string expected)
        {
            var r = IAMBuilderTools.BuildDynamicItemsJsonValue(itemsStr.Split(',').ToList());
            Assert.Equal(expected, r);
        }

        [Theory]
        [InlineData("s.1", "\"s.1\"")]
        [InlineData("s.1,s.2", "[\"s.1\",\"s.2\"]")]
        public void BuildServicesJson(string servicesStr, string expected)
        {
            var r = IAMBuilderTools.BuildServicesJson(servicesStr.Split(',').ToList());
            Assert.Equal($@"{{
                        ""Version"": ""2012-10-17"",
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [{{
                            ""Action"": ""sts: AssumeRole"",
                            ""Principal"": {{
                                ""Service"": {expected}
                                }},
                                ""Effect"": ""Allow"",
                                ""Sid"": """"
                            }}
                        ]
                    }}", r);
            JsonConvert.DeserializeObject(r); // validate valid json
        }

        [Theory]
        [InlineData("a:1", "\"a:1\"")]
        [InlineData("a:1,a:2", "[\"a:1\",\"a:2\"]")]
        public void BuildPolicyActionsJson(string actionsStr, string expected)
        {
            var r = IAMBuilderTools.BuildPolicyJson(actionsStr.Split(',').ToList(), new List<string> { "*" });
            Assert.Equal($@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [{{
                            ""Effect"": ""Allow"",
                            ""Action"": {expected},
                            ""Resource"": ""*""
                        }}]
                    }}", r);
            JsonConvert.DeserializeObject(r); // validate valid json
        }
    }
}
