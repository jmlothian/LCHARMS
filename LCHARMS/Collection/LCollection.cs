using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using LCHARMS.Document;

namespace LCHARMS.Collection
{
    [DataContract]
    public class LTag
    {
        [DataMember]
        public string Tag = "";
    }

    [DataContract]
    public class TagToDocument
    {
        [DataMember]
        LTag Tag = new LTag();
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public bool SystemGenerated = false;
    }
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
        LTag Tag = new LTag();


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
        public bool CheckNeg(bool R=true)
        {
            if(R)
            {
                return RightNegation == true ? !RightMatched : RightMatched;
            } else
            {
                return LeftNegation == true ? !LeftMatched : LeftMatched;
            }
        }
    }

    //file parts of a collection are the headers of the files in the collection
    //collections can contain collections!
    [DataContract]
    public class LCollection
    {
        [DataMember]
        LDocumentHeader CollectionHeader = new LDocumentHeader();
        [DataMember]
        public LCollectionTag CollectionTags = new LCollectionTag();
        [DataMember]
        public List<string> DocumentLRIs = new List<string>();
        //Anony collections are those that are not defined by tags
        // generally used for temporary collections, or collections of otherwise unrelated files (which should instead be in a heirarchy)
        [DataMember]
        public bool AnonymousCollection = false;
        //System generated Collection, we should probably strongly id these somehow, use for everything from "marked for delete" to colors, date ranges, or "currently indexing", etc.
        [DataMember]
        public bool SystemCollection = false;
    }

    //basically a search index / reverse lookup, consider doing this another way
    [DataContract]
    public class LCollectionMemberShip
    {
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public List<string> CollectionLRIs = new List<string>();
    }

    //workspace contains collections and other info
}
