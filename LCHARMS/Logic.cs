using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS
{
    [DataContract]
    public enum COMPARE_TYPE { NEQ, EQ, LT, GT, LT_EQ, GT_EQ };
    [DataContract]
    public enum LOGIC_OP { AND, OR };
    class Logic
    {
    }
}
