
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Models
{
    public class ResultExportModel
    {
        public List<MSErrorMessage> Errores { get; set; }
        public string DownloadKey { get; set; }

        public ResultExportModel()
        {
            this.Errores = new List<MSErrorMessage>();
            this.DownloadKey = "";
        }
    }

    public class ResultMultiExportModel
    {
        public List<MSErrorMessage> Errores { get; set; }
        public List<string> ListaUrls { get; set; }

        public ResultMultiExportModel()
        {
            this.Errores = new List<MSErrorMessage>();
            this.ListaUrls = new List<string>();
        }
    }
}

