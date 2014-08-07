using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Collection
{
    //composite mapping of a tag to a collection, so we can AND/OR etc
    [DataContract]
    public class LCollectionTag
    {
        [DataMember]
        public LOGIC_OP Operator = LOGIC_OP.AND;
        [DataMember]
        public bool Negation = false;
        [DataMember]
        public bool LeftNegation = false;
        [DataMember]
        public bool RightNegation = false;
        [DataMember]
        public LCollectionTag LeftCondition = null;
        [DataMember]
        public LCollectionTag RightCondition = null;
        [DataMember]
        private bool LeftMatched = false;
        [DataMember]
        private bool RightMatched = false;
        [DataMember]
        public bool IsComposite = false;
        [DataMember]
        public LTag Tag = new LTag();

		[DataMember]
        private bool Matched = false;

        //todo: come back to this, setting up for single-unit negation
        public bool Matches(LTag e)
        {
            if (IsComposite)
            {
                if (LeftCondition.Matches(e))
                    LeftMatched = true;
                if (RightCondition.Matches(e))
                    RightMatched = true;

                if (Operator == LOGIC_OP.AND && (CheckNeg() && CheckNeg(false)))
                {
                    Matched = true;
                }
                else if (CheckNeg() || CheckNeg(false)) //must be OR op
                {
                    Matched = true;
                }
            }
            else
            {
                if (Tag.Tag == e.Tag && Negation == false)
                {
                    Matched = true;
                }
                else if (Tag.Tag != e.Tag && Negation == true)
                {
                    Matched = true;
                }
            }
            return Matched;
        }
        public bool CheckNeg(bool R = true)
        {
            if (R)
            {
                return RightNegation == true ? !RightMatched : RightMatched;
            }
            else
            {
                return LeftNegation == true ? !LeftMatched : LeftMatched;
            }
        }
    }
}
