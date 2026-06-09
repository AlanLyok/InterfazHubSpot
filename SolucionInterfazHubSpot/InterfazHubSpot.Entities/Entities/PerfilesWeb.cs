
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Mastersoft.Framework.Interfaces;

namespace InterfazHubSpot.Entities
{
    public partial class PerfilesWeb : Entity
    {
        public int Id { get; set; }
        public Nullable<int> EmpresaId { get; set; }
        public string Descripcion { get; set; }

        public PerfilesWeb()
        {
        }
    }
}





