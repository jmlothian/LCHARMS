LCHARMS
=======
Library Catalog Hierarchy Access Retrieval and Manipulation System (LCHARMS)

At its core, LCHARMS is a document management system.  The purpose is to provide access to documents and data 
in a universal fashion (JSON) that is completely free of any visualization or user interface.  This provides the basis
for an adaptive interface system to be constructed where users can control how they interact with their data.

The system itself is constructed to facilitate distributed document access across multiple user identities
for a single user - no matter which user a person logs in as, they can access all of their files wherever they
are stored.  The location of any particular document is unimportant to the user, as the backend services
can be used to retrieve a document from an external source using a proxy of the user's identity.

To facilitate this distributed storage system, every document is globally uniquely identifiable and can be 
referenced by specific version numbers. All documents are referenced by headers that contain identifiction 
information, version information, a reference to where a document was copied from, document type, and other 
misc. metadata.  

The document itself is stored or (if binary) referenced within a JSON data structure.  Stored 
documents can further be seperated into multiple parts to facilitate distributed storage and access that doesn't
require opening the entire file.

Documents can be added to arbitrary collections via a specific tag and collections can be also be defined by 
logical groupings of tags.  For example, in an image gallery there could be a collection defined as 
(Photos AND BlackAndWhite OR (CLOCKS AND (NOT WATCHES))) giving you all images that were photos that are
either black and white or clocks (but not watches). These will automatically update as tags are added and 
removed from documents.

LCHARMS also supports the idea of addingdocuments to multiple hierarchies.  This allows using them as part of 
larger data structures.  Every object inside of LCHARMS is itself a document (including collections and 
hierarchies), so all operations and references work universally.
