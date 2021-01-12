#region

using System.Collections.Generic;


#endregion

namespace Darkages.Types.Templates
{
    public class ServerTemplate : Template
    {
          public IList<Politics> Politics = new List<Politics>();

          public Dictionary<string, int> Variables = new Dictionary<string, int>();

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}