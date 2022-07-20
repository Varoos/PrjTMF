using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrjTMF.Models
{
    public class CFFilter
    {
        public string Year { get; set; }
        public string Type { get; set; }
        public string CompanyId { get; set; }
    }
    public class CFList
    {
        public string Head { get; set; }
        public string Year { get; set; }
        public string FirstQtr { get; set; }
        public string SecondQtr { get; set; }
        public string ThirdQtr { get; set; }
        public string FourthQtr { get; set; }
        public string Jan { get; set; }
        public string Feb { get; set; }
        public string Mar { get; set; }
        public string Apr { get; set; }
        public string May { get; set; }
        public string Jun { get; set; }
        public string Jul { get; set; }
        public string Aug { get; set; }
        public string Sep { get; set; }
        public string Oct { get; set; }
        public string Nov { get; set; }
        public string Dec { get; set; }
        public int b { get; set; }
        public int cf { get; set; }
    }
    public class CFCls
    {
        public CFFilter _filter { get; set; }
        public List<CFList> _list { get; set; }
    }
}