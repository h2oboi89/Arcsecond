using System;
using System.Collections.Generic;

namespace Arcsecond
{
    public class Between : Parser
    {
        public static Func<Parser, Parser> Create(Parser left, Parser right)
        {
            return delegate (Parser content)
            {
                return new SequenceOf(new Parser[] { left, content, right })
                    .Map((results) => ((List<object>)results)[1]);
            };
        }
    }
}
