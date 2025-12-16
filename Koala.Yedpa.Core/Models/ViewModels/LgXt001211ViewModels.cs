using Koala.Yedpa.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    public class LgXt001211ListViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; } = Tools.CreateGuidStr();

        // ---------- LOGO'DAN GELEN VERİLER ----------
        /// <summary>
        /// 
        /// </summary>
        public int LogRef { get; set; } // Sadece veri, PK değil, Identity değil
        /// <summary>
        /// 
        /// </summary>
        public int? ParLogRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? GroupCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? GroupName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ClientCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public short? CustomerType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? BegDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndDate { get; set; }
       
    }
}
