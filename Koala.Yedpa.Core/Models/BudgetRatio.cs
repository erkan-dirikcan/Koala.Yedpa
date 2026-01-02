using System;
using System.Collections.Generic;
using System.Text;
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    public class BudgetRatio:CommonProperties
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public decimal Ratio { get; set; }
        public decimal TotalBugget { get; set; }
        public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
        public BuggetTypeEnum BuggetType { get; set; }
    }
}
