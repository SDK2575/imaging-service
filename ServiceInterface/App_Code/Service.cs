//------------------------------------------------
//	$Archive: /Microsoft Domain/MS Development/Njm/Enterprise/Imaging/2.0/ImagingServices/ServiceInterface/App_Code/Service.cs $
//	$Revision: 1 $ 1.0
//	$Date: 1/14/09 2:12p $Author: 
//
//	All information contained herein is the sole property of NJM 
//  Insurance Company
//------------------------------------------------
using System;
using System.Web;
using System.Web.Services;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Web.Caching; 
using System.Xml;
using System.Xml.Serialization;
using System.Text; 
using EnterpriseImaging.ImagingServices.DataAccess;
using EnterpriseImaging.ImagingServices.Entities;



namespace EnterpriseImaging.ImagingServices.ServiceInterface
{
   /// <summary>
   /// Public Services for the Imaging System
   /// </summary>
   [WebService(Namespace = "http://ws.njm.com/Imaging/1.0/ImagingServices")]
   [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
   public class ImageService : System.Web.Services.WebService
   {
      readonly string MstrUserName = System.Configuration.ConfigurationManager.AppSettings["OnBaseUserName"];
      readonly string MstrPassword = System.Configuration.ConfigurationManager.AppSettings["OnBasePassword"];
      readonly string MstrDataSource = System.Configuration.ConfigurationManager.AppSettings["OnBaseDataSource"];
      readonly string MstrDatabaseConnection = System.Configuration.ConfigurationManager.AppSettings["OnBaseDatabaseConnection"];
      private CacheItemRemovedCallback OnRemove = null;

      /// <summary>
      /// Default Constructor
      /// </summary>
      public ImageService()
      {
         //Uncomment the following line if using designed components 
         //InitializeComponent(); 

         OnRemove += new CacheItemRemovedCallback(CacheDumped);

      }


      #region WEB METHODS
      /// <summary>
      /// Get the keywords for the document
      /// </summary>
      /// <param name="PlngDocumentID">ID of the document</param>
      /// <returns>keywords for the document in XML format</returns>
       [WebMethod]
       public Document GetKeywords([XmlElement("DocumentID")]long PlngDocumentID)
       {
           Document LobjKeywordsDocument = new Document();
           LobjKeywordsDocument.ID = PlngDocumentID;


           try
           {
               NameValueCollection LobjKeywords;
               //sample valid ID's are 840, 1336
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjKeywords = LobjOnBase.GetKeywords(PlngDocumentID.ToString());
               }

               System.Collections.ArrayList LalKeywords = new System.Collections.ArrayList();

               foreach (string LobjKey in LobjKeywords.AllKeys)
               {
                   foreach (string LobjValue in LobjKeywords.GetValues(LobjKey))
                   {
                       DocumentKeyword LobjDocumentKeyword = new DocumentKeyword();
                       LobjDocumentKeyword.Key = LobjKey;
                       LobjDocumentKeyword.Value = LobjValue;
                       LalKeywords.Add(LobjDocumentKeyword);
                   }
               }

               LobjKeywordsDocument.Keywords = (DocumentKeyword[])LalKeywords.ToArray(typeof(DocumentKeyword));


           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<long>(new long[] { PlngDocumentID })
                  + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("GetKeywords Error", LstrErrorMessage, "Imaging Services", PlngDocumentID.ToString());
               throw LobjException;
           }
           return LobjKeywordsDocument;
       }

      /// <summary>
      /// Gets all document types in the system
      /// </summary>
      /// <returns>document types  in XML format</returns>
       [WebMethod]
       public DocumentTypes GetDocumentTypes()
       {
           int i = 0;
           DocumentTypes LobjDocumentTypes = new DocumentTypes();
           try
           {
               NameValueCollection LobjOnbaseDocumentTypes;
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnbaseDocumentTypes = LobjOnBase.GetDocumentTypes();
               }
               LobjDocumentTypes.DocumentType = new DocumentType[LobjOnbaseDocumentTypes.Count];

               foreach (string LobjKey in LobjOnbaseDocumentTypes.AllKeys)
               {
                   foreach (string LobjValue in LobjOnbaseDocumentTypes.GetValues(LobjKey))
                   {
                       DocumentType LobjXmlDocumentType = new DocumentType();
                       LobjXmlDocumentType.ID = LobjKey;
                       LobjXmlDocumentType.Value = LobjValue;
                       LobjDocumentTypes.DocumentType[i] = LobjXmlDocumentType;
                       i++;
                   }
               }

           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(LstrErrorMessage);
               Logger.WriteToLog("GetDocumentTypes Error", LstrErrorMessage, "Imaging Services", "");

               throw LobjException;
           }
           return LobjDocumentTypes;
       }

      /// <summary>
      /// Gets  document types for document type group specified
      /// </summary>
      /// <param name="PstrGroupName">document type group name</param>
      /// <returns>document type groups  in XML format</returns>

       [WebMethod]
       public DocumentTypes GetDocumentTypesForGroup(string PstrGroupName)
       {
           int i = 0;
           DocumentTypes LobjDocumentTypes = new DocumentTypes();

           try
           {

               NameValueCollection LobjOnbaseDocumentTypes = this.Context.Cache[PstrGroupName] as NameValueCollection;

               if (LobjOnbaseDocumentTypes == null)
               {

                   using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
                   {
                       LobjOnbaseDocumentTypes = LobjOnBase.GetDocumentTypes(new string[] { PstrGroupName });
                   }
                   this.Context.Cache.Add(PstrGroupName, LobjOnbaseDocumentTypes, null, Cache.NoAbsoluteExpiration, new TimeSpan(12, 0, 0), CacheItemPriority.High, OnRemove);


               }
               LobjDocumentTypes.DocumentType = new DocumentType[LobjOnbaseDocumentTypes.Count];

               foreach (string LobjKey in LobjOnbaseDocumentTypes.AllKeys)
               {
                   foreach (string LobjValue in LobjOnbaseDocumentTypes.GetValues(LobjKey))
                   {
                       DocumentType LobjXmlDocumentType = new DocumentType();
                       LobjXmlDocumentType.ID = LobjKey;
                       LobjXmlDocumentType.Value = LobjValue;
                       LobjDocumentTypes.DocumentType[i] = LobjXmlDocumentType;
                       i++;
                   }
               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<string>(new string[] { PstrGroupName })
                              + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("GetDocumentTypesForGroup Error: " + PstrGroupName, LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return LobjDocumentTypes;
       }

      /// <summary>
      /// Gets  document types for document type group specified
      /// </summary>
      /// <param name="PstrGroupName">document type group name</param>
      /// <returns>document type groups  in XML format</returns>

       [WebMethod]
       public DocumentTypes GetDocumentTypesForGroups(string[] PastrGroupNames)
       {
           int i = 0;
           DocumentTypes LobjDocumentTypes = new DocumentTypes();
           try
           {
               NameValueCollection LobjOnbaseDocumentTypes;
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnbaseDocumentTypes = LobjOnBase.GetDocumentTypes(PastrGroupNames);
               }

               LobjDocumentTypes.DocumentType = new DocumentType[LobjOnbaseDocumentTypes.Count];

               foreach (string LobjKey in LobjOnbaseDocumentTypes.AllKeys)
               {
                   foreach (string LobjValue in LobjOnbaseDocumentTypes.GetValues(LobjKey))
                   {
                       DocumentType LobjXmlDocumentType = new DocumentType();
                       LobjXmlDocumentType.ID = LobjKey;
                       LobjXmlDocumentType.Value = LobjValue;
                       LobjDocumentTypes.DocumentType[i] = LobjXmlDocumentType;
                       i++;
                   }
               }


           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<string>(PastrGroupNames)
                             + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("GetDocumentTypesForGroups Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return LobjDocumentTypes;
       }

      /// <summary>
      /// Gets all document type groups in the system
      /// </summary>
      /// <returns>document type groups  in XML format</returns>
       [WebMethod]
       public DocumentTypes GetDocumentGroups()
       {
           int i = 0;
           DocumentTypes LobjDocumentTypes = new DocumentTypes();
           try
           {
               NameValueCollection LobjOnbaseDocumentTypes;
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnbaseDocumentTypes = LobjOnBase.GetDocumentTypeGroups();
               }
               LobjDocumentTypes.DocumentType = new DocumentType[LobjOnbaseDocumentTypes.Count];

               foreach (string LobjKey in LobjOnbaseDocumentTypes.AllKeys)
               {
                   foreach (string LobjValue in LobjOnbaseDocumentTypes.GetValues(LobjKey))
                   {
                       DocumentType LobjXmlDocumentType = new DocumentType();
                       LobjXmlDocumentType.ID = LobjKey;
                       LobjXmlDocumentType.Value = LobjValue;
                       LobjDocumentTypes.DocumentType[i] = LobjXmlDocumentType;
                       i++;
                   }
               }
           }
           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(LstrErrorMessage);
               Logger.WriteToLog("GetDocumentGroups Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return LobjDocumentTypes;
       }



      /// <summary>
      /// gets document details ( no binaries )
      /// </summary>
      /// <param name="PlngDocumentID"></param>
      /// <returns>document details in XML format</returns>
       [WebMethod]
       public Document GetDocumentData([XmlElement("DocumentID")]long PlngDocumentID)
       {

           Document LobjReturnDocument = null;
           try
           {
               OnBaseDocument LobjOnbaseDocument;
               //sample valid ID's are 840, 1336
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnbaseDocument = LobjOnBase.GetDocumentProperties(PlngDocumentID.ToString());
               }
               if (LobjOnbaseDocument != null)
               {
                   LobjReturnDocument = CastDocument(LobjOnbaseDocument);
               }
           }
           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<long>(new long[] { PlngDocumentID })
                  + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("GetDocumentData Error", LstrErrorMessage, "Imaging Services", PlngDocumentID.ToString());
               throw LobjException;
           }
           return LobjReturnDocument;
       }






      /// <summary>
      /// Update the keywords for a document
      /// </summary>
      /// <param name="PxmlUpdatedDocumentInfo">XML document with the updated keyword data</param>
       [WebMethod]
       public void UpdateKeywords([XmlElement("Document")]Document PobjUpdatedDocumentInfo)
       {
           NameValueCollection LobjUpdatedKeywords = new NameValueCollection();
           foreach (DocumentKeyword LobjKeyword in PobjUpdatedDocumentInfo.Keywords)
           {
               LobjUpdatedKeywords.Add(LobjKeyword.Key, LobjKeyword.Value);
           }
           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnBase.UpdateKeywords(PobjUpdatedDocumentInfo.ID.ToString(), LobjUpdatedKeywords);
               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<Document>(new Document[] { PobjUpdatedDocumentInfo })
                              + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("UpdateKeywords Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
       }


      /// <summary>
      /// Update the keywords and document type for a document
      /// </summary>
      /// <param name="PxmlUpdatedDocumentInfo">XML document with the updated keyword data</param>
       [WebMethod]
       public void UpdateDocument([XmlElement("Document")]Document PobjUpdatedDocumentInfo)
       {
           NameValueCollection LobjUpdatedKeywords = new NameValueCollection();
           foreach (DocumentKeyword LobjKeyword in PobjUpdatedDocumentInfo.Keywords)
           {
               LobjUpdatedKeywords.Add(LobjKeyword.Key, LobjKeyword.Value);
           }
           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnBase.UpdateDocument(PobjUpdatedDocumentInfo.ID.ToString(), PobjUpdatedDocumentInfo.Type, LobjUpdatedKeywords);
               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<Document>(new Document[] { PobjUpdatedDocumentInfo })
                              + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("UpdateDocument Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
       }


      /// <summary>
      /// Insert a document
      /// </summary>
      /// <param name="PbytFile">raw bytes for the file</param>
      /// <param name="PxmlDocumentInfo">Document Information in XML format [keywords and properties]</param>
      /// <returns>The new document ID</returns>
       [WebMethod]
       public long InsertDocument([XmlElement("FileBytes")]byte[] PabytFile, [XmlElement("Document")]Document PobjDocumentInfo)
       {
           string LstrType = PobjDocumentInfo.Type;
           FileFormat LobjFileFormat = (FileFormat)Enum.Parse(typeof(FileFormat), PobjDocumentInfo.FileFormat.ToString(), true);

           long LlngDocumentID;
           NameValueCollection LobjUpdatedKeywords = new NameValueCollection();
           foreach (DocumentKeyword LobjKeyword in PobjDocumentInfo.Keywords)
           {
               LobjUpdatedKeywords.Add(LobjKeyword.Key, LobjKeyword.Value);
           }

           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LlngDocumentID = long.Parse(LobjOnBase.InsertDocument(PabytFile, LstrType, LobjFileFormat, LobjUpdatedKeywords));
               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<Document>(new Document[] { PobjDocumentInfo })
                              + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("InsertDocument Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return LlngDocumentID;
       }


      /// <summary>
      /// deletes specified doc
      /// </summary>
      /// <param name="PlngDocumentID"></param>
       [WebMethod]
       public void DeleteDocument([XmlElement("DocumentID")]long PlngDocumentID)
       {
           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjOnBase.DeleteDocument(PlngDocumentID.ToString());
               }
           }
           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<long>(new long[] { PlngDocumentID })
                                       + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("DeleteDocument Error", LstrErrorMessage, "Imaging Services", PlngDocumentID.ToString());
               throw LobjException;
           }
       }



      /// <summary>
      /// retrieves documents by keywords
      /// </summary>
      /// <param name="PaKeywords"></param>
      /// <returns></returns>
       [WebMethod]
       public Document[] FindDocuments([XmlElement("Keywords")]DocumentKeyword[] PaKeywords)
       {
           System.Collections.ArrayList LalFoundDocuments = new System.Collections.ArrayList();
           NameValueCollection LobjKeywords = new NameValueCollection();

           foreach (DocumentKeyword LobjDocumentKeyword in PaKeywords)
           {
               LobjKeywords.Add(LobjDocumentKeyword.Key, LobjDocumentKeyword.Value);

           }
           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   foreach (OnBaseDocument LobjOnbaseDoc in LobjOnBase.FindDocuments(LobjKeywords))
                   {
                       LalFoundDocuments.Add(CastDocument(LobjOnbaseDoc));
                   }
               }
           }
           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<DocumentKeyword>(PaKeywords)
                                     + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("FindDocuments Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return (Document[])LalFoundDocuments.ToArray(typeof(Document));
       }


      /// <summary>
      /// retrieves documents by keywords and document type
      /// </summary>
      /// <param name="PaKeywords"></param>
      /// <returns></returns>
       [WebMethod]
       public Document[] FindDocumentsWithDocumentType(string[] PastrDocumentTypes, [XmlElement("Keywords")]DocumentKeyword[] PaKeywords)
       {
           System.Collections.ArrayList LalFoundDocuments = new System.Collections.ArrayList();
           NameValueCollection LobjKeywords = new NameValueCollection();

           foreach (DocumentKeyword LobjDocumentKeyword in PaKeywords)
           {
               LobjKeywords.Add(LobjDocumentKeyword.Key, LobjDocumentKeyword.Value);

           }
           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   foreach (OnBaseDocument LobjOnbaseDoc in LobjOnBase.FindDocuments(PastrDocumentTypes, null, LobjKeywords))
                   {
                       LalFoundDocuments.Add(CastDocument(LobjOnbaseDoc));
                   }

               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<DocumentKeyword>(PaKeywords)
                  + System.Environment.NewLine + GetIncomingInformation<string>(PastrDocumentTypes)
                  + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("FindDocumentsWithDocumentType Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }

           return (Document[])LalFoundDocuments.ToArray(typeof(Document));
       }

       [WebMethod]
       public string GetUsers(int intGroupId)
       {
           List<OnBaseUser> LobjUsers = new List<OnBaseUser>();
           try
           {
               using (OnbaseDatabaseDa LobjOnBase = new OnbaseDatabaseDa(MstrDatabaseConnection))
               {
                   LobjUsers = LobjOnBase.GetUsersForGroups(intGroupId);
               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<int>(new int[] { intGroupId })
                           + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("GetUsers Error for GroupId " + intGroupId.ToString(), LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }

           if (LobjUsers.Count == 0) return "";

           string LstrReturn = "";

           foreach (OnBaseUser LobjUser in LobjUsers)
           {
               LstrReturn += string.Format("{0},", LobjUser.Name);
           }

           return LstrReturn;
       }

      /// <summary>
      /// retrieves documents by keywords and document type
      /// </summary>
      /// <param name="PaKeywords"></param>
      /// <returns></returns>
       [WebMethod]
       public Document[] FindDocumentsWithDocumentTypeGroup(string[] PastrDocumentTypeGroups, [XmlElement("Keywords")]DocumentKeyword[] PaKeywords)
       {
           System.Collections.ArrayList LalFoundDocuments = new System.Collections.ArrayList();
           NameValueCollection LobjKeywords = new NameValueCollection();

           foreach (DocumentKeyword LobjDocumentKeyword in PaKeywords)
           {
               LobjKeywords.Add(LobjDocumentKeyword.Key, LobjDocumentKeyword.Value);

           }
           try
           {
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   foreach (OnBaseDocument LobjOnbaseDoc in LobjOnBase.FindDocuments(null, PastrDocumentTypeGroups, LobjKeywords))
                   {
                       LalFoundDocuments.Add(CastDocument(LobjOnbaseDoc));
                   }

               }
           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<DocumentKeyword>(PaKeywords)
                  + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("FindDocumentsWithDocumentTypeGroup Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return (Document[])LalFoundDocuments.ToArray(typeof(Document));
       }





      /// <summary>
      /// returns Doc Type name provided by ID
      /// </summary>
      /// <param name="PobjDocumentType"></param>
      /// <returns></returns>
       [WebMethod]
       public DocumentType GetDocumentTypeNameById([XmlElement("DocumentTypesDocumentType")]DocumentType PobjDocumentType)
       {
           DocumentType LobjDocType = new DocumentType();
           try
           {
               NameValueCollection LobjReturn;
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjReturn = LobjOnBase.GetDocumentTypeNameById(int.Parse(PobjDocumentType.ID));
               }

               foreach (string LobjKey in LobjReturn.AllKeys)
               {
                   foreach (string LobjValue in LobjReturn.GetValues(LobjKey))
                   {
                       LobjDocType.ID = LobjKey;
                       LobjDocType.Value = LobjValue;
                   }
               }

           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<DocumentType>(new DocumentType[] { PobjDocumentType })
                           + System.Environment.NewLine + GetErrorMessage(LobjException));
               Logger.WriteToLog("GetDocumentTypeNameById Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }

           return LobjDocType;

       }



      /// <summary>
      /// returns Doc Type name provided by ID
      /// </summary>
      /// <param name="PobjDocumentType"></param>
      /// <returns></returns>
       [WebMethod]
       public DocumentType GetDocumentTypeIdByName([XmlElement("DocumentTypesDocumentType")]DocumentType PobjDocumentType)
       {
           DocumentType LobjDocType = new DocumentType();
           try
           {
               NameValueCollection LobjReturn;
               using (OnBaseDa LobjOnBase = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
               {
                   LobjReturn = LobjOnBase.GetDocumentTypeIdByName(PobjDocumentType.Value);
               }
               foreach (string LobjKey in LobjReturn.AllKeys)
               {
                   foreach (string LobjValue in LobjReturn.GetValues(LobjKey))
                   {
                       LobjDocType.ID = LobjKey;
                       LobjDocType.Value = LobjValue;
                   }
               }


           }

           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(GetIncomingInformation<DocumentType>(new DocumentType[] { PobjDocumentType })
                           + System.Environment.NewLine + LstrErrorMessage);
               Logger.WriteToLog("GetDocumentTypeIdByName Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }
           return LobjDocType;
       }


      [WebMethod]
      public Users GetUsersForGroups( string PastrUserGroups )
      {
         List<OnBaseUser> LobjUsers = new List<OnBaseUser>();
         try
         {

             LobjUsers = this.Context.Cache[PastrUserGroups] as List<OnBaseUser>;

             if (LobjUsers == null)
             {

                 //int LiGroupId = GetGroupIdFromCache(PastrUserGroups);
                 using (OnbaseDatabaseDa LobjOnBase = new OnbaseDatabaseDa(MstrDatabaseConnection))
                 {
                     LobjUsers = LobjOnBase.GetUsersForGroups(PastrUserGroups);
                 }
                 this.Context.Cache.Add(PastrUserGroups, LobjUsers, null, Cache.NoAbsoluteExpiration, new TimeSpan(12, 0, 0), CacheItemPriority.High, OnRemove);



             }
         }
         catch (Exception LobjException)
         {
             string LstrErrorMessage = GetErrorMessage(LobjException);
             Logger.LogError(GetIncomingInformation<string>(new string[] { PastrUserGroups })
                         + System.Environment.NewLine + LstrErrorMessage);
             Logger.WriteToLog("GetUsersForGroups Error for User Group: " + PastrUserGroups, LstrErrorMessage, "Imaging Services", "");
             throw LobjException;
         }

         int i = 0;
         LobjUsers.Sort(); 
         Users LobjReturn = new Users();
         LobjReturn.User = new User[LobjUsers.Count];

         foreach (OnBaseUser LobjOnBaseUser in LobjUsers)
         {
            User LobjReturnUser = new User();
            LobjReturnUser.ID = LobjOnBaseUser.Name;
            LobjReturnUser.DisplayName = LobjOnBaseUser.DisplayName;
            LobjReturnUser.OnbaseId = LobjOnBaseUser.ID;
            LobjReturnUser.GroupName = LobjOnBaseUser.GroupName;
            LobjReturn.User[i] = LobjReturnUser;

            i++;

         }

         return LobjReturn;
      }

      #endregion
      #region HELPERS

      /// <summary>
        /// casts OnBaseDocument to Document
        /// </summary>
        /// <param name="PobjOnbaseDocument"></param>
        /// <returns></returns>

      private Document CastDocument(OnBaseDocument PobjOnbaseDocument)
      {
         Document LobjReturnDocument = new Document();
         LobjReturnDocument.ID = PobjOnbaseDocument.DocumentId;
         LobjReturnDocument.IDSpecified = true;
         LobjReturnDocument.DocumentTypeID = PobjOnbaseDocument.DocumentTypeID;
         LobjReturnDocument.Type = PobjOnbaseDocument.DocumentType;
         LobjReturnDocument.FileFormat = (FileFormatType)Enum.Parse(typeof(FileFormatType), PobjOnbaseDocument.FileFormat.ToString(), true);
         LobjReturnDocument.FileFormatSpecified = true;

         System.Collections.ArrayList LalKeywords = new System.Collections.ArrayList();

         foreach (string LobjKey in PobjOnbaseDocument.Keywords.AllKeys)
         {
            foreach (string LobjValue in PobjOnbaseDocument.Keywords.GetValues(LobjKey))
            {
               DocumentKeyword LobjDocumentKeyword = new DocumentKeyword();
               LobjDocumentKeyword.Key = LobjKey;
               LobjDocumentKeyword.Value = LobjValue;
               LalKeywords.Add(LobjDocumentKeyword);
            }
         }
         LobjReturnDocument.Keywords = (DocumentKeyword[])LalKeywords.ToArray(typeof(DocumentKeyword));

         return LobjReturnDocument;
      }

      #endregion
      #region GET ERROR DATA
      /// <summary>
      /// collect error informatiom
      /// </summary>
      /// <param name="LobjException"></param>
      /// <returns></returns>
      private string GetErrorMessage(Exception LobjException)
      {
         string LstrErrorMessage = "User: " + this.Context.Request.LogonUserIdentity.Name + Environment.NewLine
             + "Server: " + this.Context.Server.MachineName;
         LstrErrorMessage += Environment.NewLine + LobjException.Message + Environment.NewLine;
         LstrErrorMessage += Environment.NewLine + LobjException.StackTrace;
         return LstrErrorMessage;
      }

 /// <summary>
 /// collect input information
 /// </summary>
 /// <typeparam name="T"></typeparam>
 /// <param name="PstrIncomingValues"></param>
 /// <returns></returns>
      private string GetIncomingInformation<T>( T[] PstrIncomingValues)
      {
         string LstrValue = "Input: ";

         if (PstrIncomingValues == null) return LstrValue += "NULL"; 
         foreach ( T LtValue in PstrIncomingValues )
         {
            if ( LtValue != null )
               LstrValue += LtValue.ToString() ; 

         }
         return LstrValue;

      }

      #endregion

      #region WORK WITH CACHE

      private int GetGroupIdFromCache(string PstrGroupName)
      {
         int LiGroupID = 0;
         //Logger.LogMessage("GetGroupIdFromCache"); 
         List<OnBaseUserGroup> LobjUserGroups = this.Context.Cache["UserGroups"] as List<OnBaseUserGroup>;

         // cache is empty 
         if (LobjUserGroups == null)
         {
            //Logger.LogMessage("No cache content for groups"); 
            LobjUserGroups = GetUserGroups();
            this.Context.Cache.Add("UserGroups", LobjUserGroups, null, Cache.NoAbsoluteExpiration, new TimeSpan ( 12, 0, 0 ), CacheItemPriority.High, OnRemove);


            string LstrUserGroups = System.Configuration.ConfigurationManager.AppSettings["UserGroups"].ToString().Trim();
            string[] LastrUserGroups = (LstrUserGroups == "") ? null : LstrUserGroups.Split(new char[] { ',' });


            if (LastrUserGroups != null)
            {
               foreach (string LstrGroup in LastrUserGroups)
               {
                  try
                  {
                     this.Context.Cache.Add(LstrGroup, GetUsersForGroups(LstrGroup), null, Cache.NoAbsoluteExpiration, new TimeSpan(12, 0, 0), CacheItemPriority.High, OnRemove);
                  }
                  // eat exception if usergroup does not exist
                  catch
                  { }
               }
            }
         
         
         
         
         
         }
        

         foreach (OnBaseUserGroup LobjOnBaseUserGroup in LobjUserGroups)
         {
            if (LobjOnBaseUserGroup.Name == PstrGroupName)
            {
               LiGroupID = LobjOnBaseUserGroup.ID;
               break; 
            }

         }

         if (LiGroupID == 0)
         {
            throw new Exception ( string.Format ( "User Group {0} not found ", PstrGroupName )) ; 
         }
         return LiGroupID; 
      }



      /// <summary>
      /// Cache on remove callback
      /// will be called when cache is invalidated
      /// </summary>
      /// <param name="key"></param>
      /// <param name="value"></param>
      /// <param name="removedReason"></param>

      public void CacheDumped(string key, object value, CacheItemRemovedReason removedReason)
      {
         Logger.LogMessage("Start refreshing Cache"); 

         try
         {

            this.Context.Cache.Add("UserGroups", GetUserGroups(), null, Cache.NoAbsoluteExpiration, new TimeSpan(12, 0, 0), CacheItemPriority.High, OnRemove);

            string LstrUserGroups = System.Configuration.ConfigurationManager.AppSettings["UserGroups"].ToString().Trim();
            string[] LastrUserGroups = (LstrUserGroups == "") ? null : LstrUserGroups.Split(new char[] { ',' });


            if (LastrUserGroups != null)
            {
               foreach (string LstrGroup in LastrUserGroups)
               {
                  try
                  {
                     this.Context.Cache.Add(LstrGroup, GetUsersForGroups(LstrGroup), null, Cache.NoAbsoluteExpiration, new TimeSpan(12, 0, 0), CacheItemPriority.High, OnRemove);
                  }
                     // eat exception if usergroup does not exist
                  catch
                  { }
               }
            }
            // not sure about caching doc types
            string LstrDocTypeGroups = System.Configuration.ConfigurationManager.AppSettings["DocTypesGroups"].ToString();
            string[] LastrDocTypeGroups = (LstrDocTypeGroups == "") ? null : LstrDocTypeGroups.Split(new char[] { ',' });

            if (LastrDocTypeGroups != null)
            {
               foreach (string LstrDocType in LastrDocTypeGroups)
               {
                  this.Context.Cache.Add(LstrDocType, GetDocumentTypesForGroup(LstrDocType), null, Cache.NoAbsoluteExpiration, new TimeSpan(12, 0, 0), CacheItemPriority.High, OnRemove);
               }
            }
            Logger.LogMessage("Cache refreshed");
         }
         catch ( Exception LobjException )
         {
            //Logger.LogError("Error refreshing Cache: " + LobjException.Message );
            Logger.WriteToLog("CacheDumped Error", LobjException.Message, "Imaging Services", "");
         }
      
      }


      /// <summary>
      /// reads user groups from Onbase
      /// </summary>
      /// <returns></returns>
       private List<OnBaseUserGroup> GetUserGroups()
       {
           List<OnBaseUserGroup> LobjUserGroups = new List<OnBaseUserGroup>();
           try
           {
               using (OnbaseDatabaseDa LobjOnBase = new OnbaseDatabaseDa(MstrDatabaseConnection))
               {
                   LobjUserGroups = LobjOnBase.GetUserGroups();
               }
           }
           catch (Exception LobjException)
           {
               string LstrErrorMessage = GetErrorMessage(LobjException);
               Logger.LogError(LstrErrorMessage);
               Logger.WriteToLog("GetUserGroups Error", LstrErrorMessage, "Imaging Services", "");
               throw LobjException;
           }

           return LobjUserGroups;
       }

      #endregion


   }




}

