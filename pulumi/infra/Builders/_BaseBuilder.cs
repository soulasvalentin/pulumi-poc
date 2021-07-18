using System;
using System.Collections.Generic;
using System.Text;

namespace AwsPulumiPoc.Builders
{
    public abstract class BaseBuilder
    {
        public BaseBuilder(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

            Name = name;
        }

        public string Name { get; }
    }
}
