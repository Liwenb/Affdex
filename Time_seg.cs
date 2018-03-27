using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMGExpression
{
    public class Time_seg
    {
        public int Seg_id { get; set; }
        public int Test_id { get; set; }
        public string Period { get; set; }//时间段
        public string Period_name { get; set; }//段名称
        public int State { get; set; }//阶段状态
    }
}
