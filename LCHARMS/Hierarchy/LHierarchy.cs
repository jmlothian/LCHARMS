using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using LCHARMS.Document;

namespace LCHARMS.Hierarchy
{
    [DataContract]
    public class LHierarchyNode
    {
        [DataMember]
        public string HierarchyLRI = "";
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public string ParentDocumentLRI = "";
        [DataMember]
        public List<string> ChildLRIs = new List<string>();
        //association types to be specified by the system using the heirarchy
        // think of these as "flags" for the parent/child relationship
        // the backend will not manage this data, nbut will save and retrieve it
        // we may want functions to retrieve a list of files by association type
        [DataMember]
        List<string> AssociationTypes = new List<string>();
        //json encoded data and metadata about this association
        //lcharms backend will not manage this data, but will save and retrieve it
        [DataMember]
        public string JsonData = "";


        //not data members, these are provided so that we can navigate these nodes in code when we need to
        LHierarchyNode PSibling = null;
        LHierarchyNode NSibling = null;
        LHierarchyNode ParentNode = null;
        List<LHierarchyNode> ChildNodes = new List<LHierarchyNode>();

        //we should consider adding jquery-like selectors
        public LHierarchyNode PreviousSibling { get { return PSibling; } set { PSibling = value; } }
        public LHierarchyNode NextSibling { get { return NSibling; } set { NSibling = value; } }
        public LHierarchyNode Parent { get { return ParentNode; } set { ParentNode = value; } }
        public List<LHierarchyNode> Children { get { return ChildNodes; } set { ChildNodes = value; } }
        public LHierarchy AssignedHeirarchy = null;

        public LHierarchyNode FirstChild 
        { 
            get 
            { 
                if(ChildNodes.Count > 0)
                    return ChildNodes[0];
                return null;
            } 
            private set{}
        }
        public LHierarchyNode LastChild
        {
            get
            {
                if (ChildNodes.Count > 0)
                    return ChildNodes[ChildNodes.Count - 1];
                return null;
            }
            private set{}
        }
        public void RemoveChild(LHierarchyNode node)
        {
            //make sure its in the same hierarchy
            if (node.AssignedHeirarchy == this.AssignedHeirarchy 
                && node.Parent == this) //and that this is actually the right parent
            {
                //detach from heirarchy
                node.AssignedHeirarchy.RemoveNodeFromHierarchy(node);
                Children.Remove(node);
                //rewire siblings
                if (node.PreviousSibling != null)
                {
                    node.PreviousSibling.NextSibling = node.NextSibling;
                }
                if (node.NextSibling != null)
                {
                    node.NextSibling.PreviousSibling = node.PreviousSibling;
                }
                //rewire all the children
                // removed from tree, assign to null
                node.ReHierarchy(null);
            }
        }
        /*
         * //leave this idea for now.  The concept is to remove the parent, and promote the children in its place
         * // so...  1 2 * 4 5, where * has children A,B,C,D will become 1 2 A B C D 4 5 after * is removed
        public void RemoveAndMergeChild(LHierarchyNode node)
        {
            //make sure its in the same hierarchy
            if (node.AssignedHeirarchy == this.AssignedHeirarchy
                && node.Parent == this) //and that this is actually the right parent
            {
                //detach from heirarchy
                node.AssignedHeirarchy.NodesInHierarchy.Remove(node);

                if (node.Children.Count > 0)
                {
                    int index = Children.IndexOf(node);
                    foreach (LHierarchyNode childNode in Children)
                    {
                        //wire first, before we break the index
                        //
                        if (index != 0)
                        {
                            childNode.PreviousSibling = Children[index - 1];
                        }
                        if (index < Children.Count)
                        {
                            childNode.NextSibling = Children[index + 1];
                        }

                        Children.Insert(index, childNode);
                        childNode.Parent = this;
                        childNode.ParentDocumentLRI = this.DocumentLRI;
                    }
                }
                Children.Remove(node);

                //rewire siblings
                if (node.PreviousSibling != null)
                {
                    node.PreviousSibling.NextSibling = node.NextSibling;
                }
                if (node.NextSibling != null)
                {
                    node.NextSibling.PreviousSibling = node.PreviousSibling;
                }
            }
        } */

        public void ReHierarchy(LHierarchy lhier)
        {
            AssignedHeirarchy.RemoveNodeFromHierarchy(this);
            AssignedHeirarchy = lhier;
            if (lhier != null)
            {
                HierarchyLRI = AssignedHeirarchy.HierarchyHeader.DocumentLRI;
                AssignedHeirarchy.AddNodeToHierarchy(this);
            }
            foreach (LHierarchyNode childNode in Children)
            {
                childNode.ReHierarchy(lhier);
            }
        }
        public void AppendChild(LHierarchyNode node)
        {

            if (LastChild != null)
            {
                //prevent funny business
                if (node.AssignedHeirarchy != this.AssignedHeirarchy)
                {
                    //we want to make sure we remove it from its existing location
                    if (node.Parent != null)
                    {
                        node.Parent.RemoveChild(node);
                    }
                    else
                    {
                        node.AssignedHeirarchy.RootNode = null;
                    }
                }

                node.NextSibling = null;
                node.PreviousSibling = null; 
                node.Parent = this;
                node.HierarchyLRI = this.HierarchyLRI;
                node.ParentDocumentLRI = this.DocumentLRI;

                LastChild.NextSibling = node;
                node.PreviousSibling = LastChild;
            }
            //rewire all the children
            node.ReHierarchy(this.AssignedHeirarchy);
            ChildNodes.Add(node);
        }
        public void InsertAtChild(LHierarchyNode node, int index)
        {
            //prevent funny business
            node.NextSibling = null;
            node.PreviousSibling = null;
            //handle head/tail cases just to make this easier
            if (index == 0)
            {
                PrependChild(node);
            }
            else if (index == Children.Count - 1)
            {
                AppendChild(node);
            }
            else
            {
                if(index < ChildNodes.Count)
                {
                    //setup parent
                    node.Parent = this;
                    node.ParentDocumentLRI = this.DocumentLRI;
                    node.HierarchyLRI = this.HierarchyLRI;
                    //we are not at the head or tail, thus we break two sibling links
                    LHierarchyNode prevSibling = ChildNodes[index - 1];
                    LHierarchyNode nextSibling = ChildNodes[index];
                    //insert
                    ChildNodes.Insert(index, node);
                    //wire
                    prevSibling.NextSibling = node;
                    nextSibling.PreviousSibling = node;
                    node.PreviousSibling = prevSibling;
                    node.NextSibling = nextSibling;
                }
                //rewire all the children
                node.ReHierarchy(this.AssignedHeirarchy);
            }
        }
        public void PrependChild(LHierarchyNode node)
        {
            if (FirstChild != null)
            {
                //prevent funny business
                node.NextSibling = null;
                node.PreviousSibling = null;
                node.HierarchyLRI = this.HierarchyLRI;
                //setup parent
                node.Parent = this;
                node.ParentDocumentLRI = this.DocumentLRI; 
                FirstChild.PreviousSibling = node;
                node.NextSibling = FirstChild;
            }
            //rewire all the children
            node.ReHierarchy(this.AssignedHeirarchy);
            ChildNodes.Insert(0, node);
        }

    }

    [DataContract]
    public class LHierarchy
    {
        [DataMember]
        public string _id = "";
        [DataMember]
        public string _rev = null;
        //not required, but it might be useful to have a name for searching.
        // remember, these are documents, so they can be tagged as well.
        [DataMember]
        public string HierarchyName = "";
        //hierarchy parts are the node infos for each file in the heirarchy
        [DataMember]
        public LDocumentHeader HierarchyHeader = new LDocumentHeader();

        //need to make sure that saving does not include rootnode, this can be regenerated from the list
        [DataMember]
        public LHierarchyNode RootNode = new LHierarchyNode();
        //only association types registered to the heirarchy will be available for further functions
        [DataMember]
        public List<string> RegisteredAssociationTypes = new List<string>();
        //json encoded data and metadata about this heirarchy
        //lcharms backend will not manage this data, but will save and retrieve it
        [DataMember]
        public string JsonData = "";
        //we need to save -this- list, so that we can actually rebuild the tree
        [DataMember]
        public List<LHierarchyNode> NodesInHierarchy = new List<LHierarchyNode>();
        
        public Dictionary<string, LHierarchyNode> NodesByLRI = new Dictionary<string, LHierarchyNode>();

        internal void AddNodeToHierarchy(LHierarchyNode node)
        {
            NodesInHierarchy.Remove(node);
            NodesByLRI.Remove(node.DocumentLRI);
        }
        internal void RemoveNodeFromHierarchy(LHierarchyNode node)
        {
            NodesInHierarchy.Add(node);
            NodesByLRI[node.DocumentLRI] = node;
        }
        public void SetRoot(LHierarchyNode newRoot)
        {
            //can only be done when no root node exists
            if (newRoot == null)
            {
                RootNode.ReHierarchy(null);
                RootNode = null;
            }
            else
            {
                if (RootNode == null)
                {
                    RootNode = newRoot;
                    RootNode.ReHierarchy(this);
                }
            }
        }
        public void Initialize()
        {
            //build Nodes by LRI
            for (int i = 0; i < NodesInHierarchy.Count; i++)
            {
                NodesByLRI[NodesInHierarchy[i].DocumentLRI] = NodesInHierarchy[i];
                if (NodesInHierarchy[i].ParentDocumentLRI == "")
                {
                    RootNode = NodesInHierarchy[i];
                }
            }
            SetupTree(null);
        }
        public void SetupTree(LHierarchyNode CurrentNode = null)
        {
            //recursively build pointer list from heirarchy nodes
            if (CurrentNode == null)
            {
                //root node
                //wire root lri, just in case
                RootNode.HierarchyLRI = HierarchyHeader.DocumentLRI;
                SetupTree(RootNode);
            }
            else
            {
                //assign it to this heirarchy
                //AddNodeToHierarchy(CurrentNode); //we dont call this, because at this point these are loaded
                CurrentNode.AssignedHeirarchy = this;
                for (int i = 0; i < CurrentNode.ChildLRIs.Count; i++)
                {
                    //find the child.
                    LHierarchyNode childNode = NodesByLRI[CurrentNode.ChildLRIs[i]];
                    //recurse here, depth-first, attach leaves before parents
                    SetupTree(childNode);
                    CurrentNode.AppendChild(childNode);
                }
            }
        }
    }
}
