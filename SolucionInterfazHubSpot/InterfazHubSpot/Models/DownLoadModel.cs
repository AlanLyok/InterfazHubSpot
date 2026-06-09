
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Models
{
    public class DownLoadModel
    {
        public List<MSErrorMessage> Errores { get; set; }
        public string DownloadKey { get; set; }

        public DownLoadModel()
        {
            this.Errores = new List<MSErrorMessage>();
            this.DownloadKey = "";
        }
    }


    public class UploadResult
    {
        public int id { get; set; }
        public string contentType { get; set; }
        public string fileName { get; set; }
        public string key { get; set; }
    }


    public class LogoResult
    {
        public List<MSErrorMessage> Errores { get; set; }
        public byte[] ImagenLogo { get; set; }
        public string ContentTypeLogo { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public LogoResult()
        {
            this.Errores = new List<MSErrorMessage>();
            this.ImagenLogo = null;
            this.ContentTypeLogo = "";
            this.width = 0;
            this.height = 0;
        }
    }

}


