using System;
using System.Collections.Generic;
using System.Text;

namespace AwsPulumiPoc.Builders
{
    public interface IBuilder<T>
    {
        T Build();
    }
}
