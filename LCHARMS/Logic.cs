using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS
{
    [DataContract]
    public enum COMPARE_TYPE { 
		[EnumMember]
		NEQ,
		[EnumMember]
		EQ,
		[EnumMember]
		LT,
		[EnumMember]
		GT,
		[EnumMember]
		LT_EQ,
		[EnumMember]
		GT_EQ 
	};
    [DataContract]
    public enum LOGIC_OP {
		[EnumMember]
		AND,
		[EnumMember]
		OR
	};
    class Logic
    {
    }
}
