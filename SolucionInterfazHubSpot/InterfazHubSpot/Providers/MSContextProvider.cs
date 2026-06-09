using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InterfazHubSpot.Core;
using InterfazHubSpot.Interfaces;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot
{
    public class MSContextProvider : IMSContextProvider
    {
        public MSContext GetMSContext()
        {
            return Util.GetMSContext();
        }
    }
}


