using System;
using System.Collections.Generic;

namespace Arcsecond
{
    public partial class Parser
    {
        public static Func<Parser, Parser> Between(Parser left, Parser right) => delegate (Parser content)
        {
            return SequenceOf(new Parser[] { left, content, right })
                .Map((results) => ((List<object>)results)[1]);
        };
    }
}
