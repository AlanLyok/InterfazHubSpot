using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BatchSpertaAPI.Core;
using BatchSpertaAPI.Interfaces;
using Mastersoft.Framework.Standard;

namespace BatchSpertaAPI
{
    public class MSContextProvider : IMSContextProvider
    {
        public MSContext GetMSContext()
        {
            return Util.GetMSContext();
        }
    }
}


