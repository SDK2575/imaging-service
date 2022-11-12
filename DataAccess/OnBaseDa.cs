using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized; 


// Onbase Core
//using Hyland.Public;
//using Hyland.Public.Services;
using DMCOREX;


using EnterpriseImaging.ImagingServices.Entities; 

namespace EnterpriseImaging.ImagingServices.DataAccess
{
   public class OnBaseDa :IDisposable 
   {
      // query limit. can not call Execute method more than 200 in the context of 1 session
      const int DOCQUERY_LIMIT = 199 ; 

      OBXDataSourceManager MobjDsnManager = new OBXDataSourceManager();

      IOBXDataSource MobjDataSource = null;

      IOBXSession MobjSession = null ;

      string MstrUserName;

      string MstrPassword; 

      int MiDocumentQueryCounter = 0;

      #region CONSTRUCTOR -----------------------------------------------------------------------------------
      /// <summary>
      /// constructor
      /// </summary>
      /// <param name="PstrDataSourceName"></param>
      /// <param name="PstrUserName"></param>
      /// <param name="PstrPassword"></param>
      public OnBaseDa( string PstrDataSourceName, string PstrUserName, string PstrPassword )
      {
         try
         {
            MobjDataSource = MobjDsnManager.GetDataSourceByName(PstrDataSourceName);
            MstrUserName = PstrUserName;
            MstrPassword = PstrPassword;

            MobjDataSource.Open();
         }
         catch
         {
            throw new CoreAPIException(PresentError(MobjDataSource));
         }

         GetNewSession(); 

      }

      #endregion




      #region MAIN PROCCESSING ----------------------------------------------------------------------




      /// <summary>
      /// updates document type and keywords,reindex document
      /// </summary>
      /// <param name="PlngDocumentID">document handle</param>
      /// <param name="PstrDocType">document type name</param>
      /// <param name="PalNewKeywordValues">array of keywords</param>
      /// <remarks>You must call get keyword first ot obtain all of the current keyword value and pass the complete set in</remarks>

      //public void UpdateDocument(string PstrDocumentID, string PstrDocType, NameValueCollection PobjNewKeywordValues)
      //{

      //   IOBXPresentationServices LobjPresentationServices = null;
      //   IOBXStorageProvider LobjStorageProvider = null;
      //   IOBXArchivalToken LobjArchivalToken = null;
      //   IOBXDocumentTypeCollection LobjColDocumentType = null;


      //   try
      //   {

      //      // Create the Presentation Services module

      //      LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices");

      //      //Create the StorageProvider

      //      LobjStorageProvider = LobjPresentationServices.CreateStorageProvider();

      //      // Next, create the ArchivalToken object

      //      LobjArchivalToken = LobjStorageProvider.CreateArchivalToken();


      //      //Now that we have the Archival Token created, we can supply the necessary data to it.


      //      //Store the action value for the Token

      //      LobjArchivalToken.Action = "ReindexDocument";

      //      //Enter the Document ID  (document handle number)

      //      LobjArchivalToken.ID =  int.Parse( PstrDocumentID ) ;

      //      //Enter the document type ID

      //      LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

      //      LobjArchivalToken.TypeID = SetDocType(PstrDocType, LobjColDocumentType);


      //      // need for extra valildation here
      //      //ValidateKeywords(LobjColDocumentType, PstrDocType, PalNewKeywordValues); 

      //      //' Add the keyword to the Archival Token via AddKeywordByName()
      //      //' Note: You must supply all the keywords and values otherwise the keyword
      //      //'       value will be null/empty when the Token is processed.


      //      foreach (string LobjKey in PobjNewKeywordValues.AllKeys)
      //      {
      //         foreach (string LobjValue in PobjNewKeywordValues.GetValues(LobjKey))
      //         {
      //            LobjArchivalToken.AddKeywordByName(LobjKey, LobjValue);
      //         }
      //      }


      //      //' Finally, we can process the Archival Token.

      //      LobjStorageProvider.ProcessArchivalToken(LobjArchivalToken);

      //   }
      //   catch (System.Runtime.InteropServices.COMException e)
      //   {
      //      throw new CoreAPIException(PresentError(MobjSession), e);
      //   }

      //   finally
      //   {

      //      ReleaseCOMobject(LobjArchivalToken);
      //      ReleaseCOMobject(LobjStorageProvider);
      //      ReleaseCOMobject(LobjPresentationServices);
      //      ReleaseCOMobject(LobjColDocumentType);
      //   }


      //}

      public void UpdateDocument(string PstrDocumentID, string PstrDocType, NameValueCollection PobjNewKeywordValues )
      {
         UpdateDocument(PstrDocumentID, PstrDocType, PobjNewKeywordValues, DateTime.MinValue );
      }

      public void UpdateDocument(string PstrDocumentID, string PstrDocType, NameValueCollection PobjNewKeywordValues, DateTime PdtDocumentDate)
      {

         IOBXDocumentArchiver LobjDocumentArchiver = null;
         IOBXFileManager LobjFileManager = null;
         IOBXDocumentQuery LobjDocumentQuery = null;
         IOBXDocument LobjDocument = null; 
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXKeywordCollection LobjKeywordCollection = null;
         IOBXKeyword LobjKeyword = null;
         OBXDateTime LobjDocDate = null; 
         try
         {


            LobjDocumentArchiver = (IOBXDocumentArchiver)MobjSession.CreateObject("OBXDocumentArchiver");
            LobjFileManager = (IOBXFileManager)MobjSession.CreateObject("OBXFileManager");

            LobjDocumentQuery = (IOBXDocumentQuery)MobjSession.CreateObject("OBXDocumentQuery");

            LobjDocument = LobjDocumentQuery.GetDocumentByID(int.Parse(PstrDocumentID));

            LockDocument(LobjDocument);

            LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

            LobjDocument.SetDocumentTypeByID( SetDocType(PstrDocType , LobjColDocumentType )  );


            if ( PdtDocumentDate != DateTime.MinValue )  // update doc date only when needed - issue 669
            {
               LobjDocDate = new OBXDateTime();

               LobjDocDate.Initialize(PdtDocumentDate.Month, PdtDocumentDate.Day, PdtDocumentDate.Year, PdtDocumentDate.Hour, PdtDocumentDate.Minute, PdtDocumentDate.Second);

               LobjDocument.DocumentDate = LobjDocDate;
            }
       

            LobjKeywordCollection = LobjDocument.Keywords;
            // keywords obtained at this point


            //loop through arraylist and update the relative keyword value.  This assumes that
            //the keyword collection is in the same order as the arraylist.

            int i;
            for (i = 0; i < LobjKeywordCollection.Count(); i++)
            {
               LobjKeyword = LobjKeywordCollection.Item(i);
               LobjKeyword.Value = "";
            }


            //ValidateKeywords(LobjColDocumentType, LobjDocument.DocumentType.Name, PobjNewKeywordValues);


            foreach (string LobjKey in PobjNewKeywordValues.AllKeys)
            {
               foreach (string LobjValue in PobjNewKeywordValues.GetValues(LobjKey))
               {
                  // add only those keywords that pertain to this doc type, ignore otherwise
                  try
                  {
                     LobjDocument.AddKeywordByName(LobjKey, LobjValue);
                  }
                  catch 
                  {
                     continue ;
                  }


               }
            }


            LobjDocument.AutoName(); 

            LobjDocumentArchiver.StoreDocument(LobjDocument, OBX_ARCHIVER_STOREDOCUMENT.OBX_ARCHIVER_STOREDOCUMENT_UPDATE, 1);  // attn: 1 - means skip workflow!!!! 

            // per Hyland suggestion we do not need to commit document - prod issue w DMI
            //LobjFileManager.CommitDocument(LobjDocument, 0);

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            if (e.Message == string.Format("Failed to obtain document with ID #({0})", PstrDocumentID))
            {
               throw new ImagingServicesException( string.Format("Document with ID {0} does not exist", PstrDocumentID)); ;
            }
            throw new CoreAPIException(PresentError(MobjSession), e);
         }

         finally
         {
            UnLockDocument (  LobjDocument );
            ReleaseCOMobject(LobjKeyword);
            ReleaseCOMobject(LobjKeywordCollection); 
            ReleaseCOMobject(LobjColDocumentType); 
            ReleaseCOMobject(LobjDocument);
            ReleaseCOMobject(LobjDocumentQuery);
            ReleaseCOMobject(LobjFileManager);
            ReleaseCOMobject(LobjDocumentArchiver);
            ReleaseCOMobject(LobjDocDate);
         }


      }




      /// <summary>
      /// updates keywords, does not reindex document
      /// </summary>
      /// <param name="PstrDocumentID"></param>
      /// <param name="PobjNewKeywordValues"></param>
      public void UpdateKeywords(string PstrDocumentID, NameValueCollection PobjNewKeywordValues)
      {
         IOBXDocument LobjDocument = null;
         IOBXDocumentQuery LobjDocumentQuery = null;
         IOBXKeywordCollection LobjKeywordCollection = null;
         IOBXKeyword LobjKeyword = null;
         IOBXDocumentTypeCollection LobjColDocumentType = null;

         try
         {


            LobjDocumentQuery = MobjSession.CreateQuery();

            LobjDocument = LobjDocumentQuery.GetDocumentByID(int.Parse(PstrDocumentID));


            LockDocument( LobjDocument ); 

            LobjKeywordCollection = LobjDocument.Keywords;
            // keywords obtained at this point


            //loop through arraylist and update the relative keyword value.  This assumes that
            //the keyword collection is in the same order as the arraylist.

            int i;
            for (i = 0; i < LobjKeywordCollection.Count(); i++)
            {
               LobjKeyword = LobjKeywordCollection.Item(i);
               LobjKeyword.Value = "";
            }

            LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

            //ValidateKeywords(LobjColDocumentType, LobjDocument.DocumentType.Name, PobjNewKeywordValues);


            foreach (string LobjKey in PobjNewKeywordValues.AllKeys)
            {
               foreach (string LobjValue in PobjNewKeywordValues.GetValues(LobjKey))
               {
                  try
                  {
                     LobjDocument.AddKeywordByName(LobjKey, LobjValue);
                  }
                  catch
                  {
                     // ignore invalid keyword 
                     continue; 
                  }
               }
            }

            LobjDocument.AutoName();  
            LobjDocument.StoreKeywords();


         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            if (e.Message == string.Format("Failed to obtain document with ID #({0})", PstrDocumentID))
            {
               throw new ImagingServicesException(string.Format("Document with ID {0} does not exist", PstrDocumentID)); ;
            }
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         finally
         {
            UnLockDocument(LobjDocument); 
            ReleaseCOMobject(LobjKeyword);
            ReleaseCOMobject(LobjKeywordCollection);
            ReleaseCOMobject(LobjColDocumentType);
            ReleaseCOMobject(LobjDocument);
            ReleaseCOMobject(LobjDocumentQuery);
         }


      }



      /// <summary>
      /// retrieves document properties for passed doc handle
      /// no binary data is returned
      /// </summary>
      /// <param name="PstrDocumentID"></param>
      /// <returns></returns>
      public OnBaseDocument GetDocumentProperties(string PstrDocumentID)
      {

         IOBXDocumentQuery LobjDocumentQuery = null;
         IOBXPresentationServices LobjPresentationServices = null;
         IOBXDocumentDataProvider LobjDocumentDataProvider = null;
         IOBXPropertyBag LobjPropertyBag = null;
         IOBXDocument LobjDocument = null;
         IOBXKeyword LobjKeyword = null;


         try
         {
            LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices");

            LobjDocumentQuery = (IOBXDocumentQuery)MobjSession.CreateObject("OBXDocumentQuery");

            LobjDocument = LobjDocumentQuery.GetDocumentByID(int.Parse(PstrDocumentID));

            if (LobjDocument == null)
            {
               return null;
            }

            return CastDocument( LobjDocument );


         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            if (e.Message == string.Format("Failed to obtain document with ID #({0})", PstrDocumentID)) 
            {
               return null; 
            }


            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         finally
         {

            ReleaseCOMobject(LobjPresentationServices);
            ReleaseCOMobject(LobjDocumentDataProvider);
            ReleaseCOMobject(LobjPropertyBag);
            ReleaseCOMobject(LobjDocument);
            ReleaseCOMobject(LobjDocumentQuery);
            ReleaseCOMobject(LobjKeyword);
         }

      }


      /// <summary>
      /// retrieves document keywords only
      /// </summary>
      /// <param name="PstrDocumentID"></param>
      /// <returns></returns>
      public NameValueCollection GetKeywords(string PstrDocumentID)
      {
         OnBaseDocument LobjOnBaseDocument = GetDocumentProperties(PstrDocumentID) ;

         return (LobjOnBaseDocument == null) ? new NameValueCollection() : LobjOnBaseDocument.Keywords; 
         
      }



      /// <summary>
      /// inserts document  - overload - skip workflow by default 
      /// </summary>
      /// <param name="PabytFile"></param>
      /// <param name="PstrType"></param>
      /// <param name="PstrFileFormat"></param>
      /// <param name="PobjKeywordValues"></param>
      /// <returns></returns>
      public string InsertDocument(byte[] PabytFile, string PstrType, FileFormat PstrFileFormat, NameValueCollection PobjKeywordValues )
      {

         // skip workflow by default 
         return InsertDocument(PabytFile, PstrType, PstrFileFormat, PobjKeywordValues, System.DateTime.Today , true); 
      }


      /// <summary>
      /// inserts document  - overload - skip workflow by default 
      /// </summary>
      /// <param name="PabytFile"></param>
      /// <param name="PstrType"></param>
      /// <param name="PstrFileFormat"></param>
      /// <param name="PobjKeywordValues"></param>
      /// <returns></returns>
      public string InsertDocument(byte[] PabytFile, string PstrType, FileFormat PstrFileFormat, NameValueCollection PobjKeywordValues, System.DateTime PdtDocumentDate)
      {

         // skip workflow by default 
         return InsertDocument(PabytFile, PstrType, PstrFileFormat, PobjKeywordValues, PdtDocumentDate, true );
      }


      /// <summary>
      /// inserts document
      /// </summary>
      /// <param name="PabytFile"></param>
      /// <param name="PstrType"></param>
      /// <param name="PstrFileFormat"></param>
      /// <param name="PobjKeywordValues"></param>
      /// <returns></returns>
      public string InsertDocument(byte[] PabytFile, string PstrType, FileFormat PstrFileFormat, NameValueCollection PobjKeywordValues, System.DateTime PdtDocumentDate,  bool PbSkipWorkflow )
      {

         IOBXPresentationServices LobjPresentationServices = null;
         IOBXStorageProvider LobjStorageProvider = null;
         IOBXDocumentDataProvider LobjDocumentDataProvider = null;
         IOBXArchivalToken LobjArchivalToken = null;
         IOBXPropertyBag LobjPropertyBag = null;
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXPageData LobjPageData = null;


         try

         {
            LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices");

            ////Create a document storage provider
            LobjStorageProvider = LobjPresentationServices.CreateStorageProvider();

            LobjDocumentDataProvider = LobjPresentationServices.CreateDocumentDataProvider();


            //// Create an Archival Token off of the StorageProvider
            LobjArchivalToken = LobjStorageProvider.CreateArchivalToken();

            LobjArchivalToken.Action = "NewDocument";


            // set doc type
            LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

            LobjArchivalToken.TypeID = SetDocType(PstrType, LobjColDocumentType);


            LobjArchivalToken.Date = PdtDocumentDate.ToString(); // System.DateTime.Today.ToString();


            if (PbSkipWorkflow)
            {
               // need to add extra validation for keyword types

               // this is a hack - we do not want to fire workflow in some cases
               // for example when document is inserted thru Claim Center interface
               // workflow engine will analyze UseWorkflow keyword
               // if it is not equal to "" than workflow will not be fired.


               PobjKeywordValues.Add("UseWorkflow", "1");
            }


            NameValueCollection LobjValidKeywords =   GetValidKeywords(LobjColDocumentType, PstrType, PobjKeywordValues);
            foreach (string LobjKey in LobjValidKeywords.AllKeys)
            {
               foreach (string LobjValue in LobjValidKeywords.GetValues(LobjKey))
               {
                    LobjArchivalToken.AddKeywordByName(LobjKey, LobjValue);
               }
            }


            LobjPageData = LobjStorageProvider.CreatePageDataOnBytes(PabytFile);


            LobjArchivalToken.FileTypeID = (int)PstrFileFormat;

            LobjArchivalToken.AddPageFromPageData(LobjPageData);


            LobjPropertyBag = LobjStorageProvider.ProcessArchivalTokenEx(LobjArchivalToken);


            return (LobjPropertyBag.GetProperty("documentID") != null) ? LobjPropertyBag.GetProperty("documentID").ToString() : "0";
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }

         finally
         {
            ReleaseCOMobject(LobjPageData);
            ReleaseCOMobject(LobjColDocumentType);
            ReleaseCOMobject(LobjPropertyBag);
            ReleaseCOMobject(LobjArchivalToken);

            ReleaseCOMobject(LobjDocumentDataProvider);
            ReleaseCOMobject(LobjStorageProvider);
            ReleaseCOMobject(LobjPresentationServices);

         }


      }


      /// <summary>
      /// inserts document
      /// </summary>
      /// <param name="PabytFile"></param>
      /// <param name="PstrType"></param>
      /// <param name="PstrFileFormat"></param>
      /// <param name="PobjKeywordValues"></param>
      /// <returns></returns>
      public string InsertDocument(byte[] PabytFile, string PstrType, FileFormat PstrFileFormat, NameValueCollection PobjKeywordValues,  bool PbSkipWorkflow)
      {
         // set doc date to today's date by default
         return InsertDocument(PabytFile, PstrType, PstrFileFormat, PobjKeywordValues, System.DateTime.Today, PbSkipWorkflow ); 


      }



      /// <summary>
      /// reserved, might be used in future
      /// </summary>
      /// <param name="PabytFile"></param>
      /// <param name="PstrType"></param>
      /// <param name="PstrFileFormat"></param>
      /// <param name="PobjKeywordValues"></param>
      /// <returns></returns>

      public string InsertDocument(byte[] PabytFile, string PstrType, FileFormat PstrFileFormat, NameValueCollection PobjKeywordValues, System.DateTime PdtDocumentDate,  bool PbSkipWorkflow, string PstrFileExtention )
      {

      // Create a IOBXDocumentArchiver
      IOBXDocumentArchiver LobjDocumentArchiver  = null ; 
      IOBXPresentationServices LobjPresentationServices = null ;
      IOBXDocument  LobjDocument = null ; 
      IOBXStorageProvider LobjStorageProvider = null;
      IOBXPageData LobjPageData = null ;
      IOBXDocumentTypeCollection LobjColDocumentType = null;
      IOBXFileManager LobjFileManager = null;
      IOBXFile LobjFile = null;
      //IOBXDiskgroupCollection colDiskGroups = null;
      IOBXDocumentPage LobjDocumentPage = null;
      IOBXDocumentType objDocumentType = null ; 
      OBXDateTime LobjDocDate = null ; 

      int LiDocId; 
      try
      {
         LobjDocumentArchiver = (IOBXDocumentArchiver)MobjSession.CreateObject("OBXDocumentArchiver");



         //Create a Document from the DocumentArchiver

         LobjDocument = LobjDocumentArchiver.CreateDocument();


         LobjDocument.SetDocumentTypeByName(PstrType);

         LobjDocDate = new OBXDateTime();
 
         LobjDocDate.Initialize ( PdtDocumentDate.Month , PdtDocumentDate.Day , PdtDocumentDate.Year, PdtDocumentDate.Hour, PdtDocumentDate.Minute, PdtDocumentDate.Second ) ;

         LobjDocument.DocumentDate = LobjDocDate ;

         LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

         objDocumentType = LobjColDocumentType.FindByName( PstrType );

         int LiDiskGroupId = objDocumentType.DiskGroupID;  //((IOBXDiskgroup)colDiskGroups.Item(0)).ID;

         NameValueCollection LobjValidKeywords = GetValidKeywords(LobjColDocumentType, PstrType, PobjKeywordValues);

         foreach (string LobjKey in LobjValidKeywords.AllKeys)
         {
            foreach (string LobjValue in LobjValidKeywords.GetValues(LobjKey))
            {
               LobjDocument.AddKeywordByName(LobjKey, LobjValue);
            }
         }

         // Autoname the Document. The changes here are only done in memory until the document
         // is stored to the system using the document archiver object
         LobjDocument.AutoName();

         LobjPageData = (IOBXPageData)MobjSession.CreateObject("OBXPageData");


         LobjPageData.LoadFromBytes(PabytFile);

         //colDiskGroups = MobjSession.Diskgroups;



         LobjFileManager = (IOBXFileManager)MobjSession.CreateObject("OBXFileManager");


         LobjFile = LobjFileManager.CopyPageDataToDiskgroupByID(LiDiskGroupId, LobjPageData, PstrFileExtention );


         LobjDocumentPage = (IOBXDocumentPage)LobjFile.CreatePage(0, 0, 0, 0, 4);


         // Set the filetype for the database page. Notice this is done on the
         // IOBXDocumentPage and not the IOBXDocument
         LobjDocumentPage.FileTypeID = (int)PstrFileFormat; ;

         // Add the page To the Document being created
         LobjDocument.AddPage(LobjDocumentPage, 0, 0);

         int LiSkipWorkflow = (PbSkipWorkflow) ? 1 : 0;
         LobjDocumentArchiver.StoreDocument(LobjDocument, 0, LiSkipWorkflow ); // 1 = means skip workflow
         
         LiDocId = LobjDocument.ID ; 

      }

      catch (System.Runtime.InteropServices.COMException e)
      {
         throw new CoreAPIException(PresentError(MobjSession), e);
      }

         finally
         {
            //LobjTimer.Start();
            ReleaseCOMobject(LobjDocDate); 
            ReleaseCOMobject(LobjDocumentArchiver);
            ReleaseCOMobject(LobjPresentationServices);
            ReleaseCOMobject(LobjDocument);
            ReleaseCOMobject(LobjStorageProvider);

            ReleaseCOMobject(LobjPageData);
            ReleaseCOMobject(LobjColDocumentType);
            ReleaseCOMobject(LobjFileManager);
            ReleaseCOMobject(LobjFile);
            ReleaseCOMobject(objDocumentType);
            ReleaseCOMobject(LobjDocumentPage);
         }

         return LiDocId.ToString(); 
      }

      /// <summary>
      /// retrieves list of all doc types
      /// </summary>
      /// <returns></returns>
      public NameValueCollection GetDocumentTypes()
      {
         NameValueCollection LobjDocTypes = new NameValueCollection();
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXDocumentType LobjDocumentType = null;

         try
         {
            LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

            for (int i = 0; i < LobjColDocumentType.Count(); i++)
            {
               LobjDocumentType = LobjColDocumentType.Item(i);
               LobjDocTypes.Add(LobjDocumentType.ID.ToString(), LobjDocumentType.Name);
            }

            return LobjDocTypes;
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         finally
         {
            ReleaseCOMobject(LobjDocumentType);
            ReleaseCOMobject(LobjColDocumentType);

         }


      }


      /// <summary>
      /// retrieves doc type by doc type id
      /// </summary>
      /// <returns></returns>
      public NameValueCollection GetDocumentTypeNameById( int PiDocumentTypeId )
      {
         NameValueCollection LobjDocTypes = new NameValueCollection();
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXDocumentType LobjDocumentType = null;

         try
         {
            LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         try
         {
            LobjDocumentType = LobjColDocumentType.FindByID(PiDocumentTypeId);

            LobjDocTypes.Add(LobjDocumentType.ID.ToString(), LobjDocumentType.Name);

            return LobjDocTypes; 

         }
         catch ( System.Runtime.InteropServices.COMException e )
         {
            throw new ImagingServicesException (string.Format ( "Document Type with ID {0} does not exist ", PiDocumentTypeId ));
         }
         finally
         {
            ReleaseCOMobject(LobjDocumentType);
            ReleaseCOMobject(LobjColDocumentType);

         }


      }


      /// <summary>
      /// retrieves doc type by doc type id
      /// </summary>
      /// <returns></returns>
      public NameValueCollection GetDocumentTypeIdByName(string PstrDocumentTypeName)
      {
         NameValueCollection LobjDocTypes = new NameValueCollection();
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXDocumentType LobjDocumentType = null;

         try
         {
            LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         try
         {
            LobjDocumentType = LobjColDocumentType.FindByName(PstrDocumentTypeName);

            LobjDocTypes.Add(LobjDocumentType.ID.ToString(), LobjDocumentType.Name);

            return LobjDocTypes;

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new ImagingServicesException(string.Format("Document Type with Name `{0}` does not exist ", PstrDocumentTypeName));
         }
         finally
         {
            ReleaseCOMobject(LobjDocumentType);
            ReleaseCOMobject(LobjColDocumentType);

         }


      }


      /// <summary>
      /// retrieves list of all doc types for passed Doc Type Groups
      /// </summary>
      /// <param name="PastrGroupNames"></param>
      /// <returns></returns>
      public NameValueCollection GetDocumentTypes(string[] PastrGroupNames)
      {
         NameValueCollection LobjDocTypes = new NameValueCollection();
         IOBXDocumentTypeGroupCollection LobjColDocumentTypeGroup = null;
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXDocumentType LobjDocumentType = null;


         try
         {


            LobjColDocumentTypeGroup = (IOBXDocumentTypeGroupCollection)MobjSession.GetCollection("OBXDocumentTypeGroupCollection");


            foreach (string LstrGroupName in PastrGroupNames)
            {
               try
               {
                  LobjColDocumentType = LobjColDocumentTypeGroup.FindByName(LstrGroupName).DocumentTypes;
               }
               catch (System.Runtime.InteropServices.COMException e)
               {
                  throw new ImagingServicesException(string.Format("Invalid Document Type Group '{0}' supplied.", LstrGroupName));
               }
               for (int i = 0; i < LobjColDocumentType.Count(); i++)
               {
                  LobjDocumentType = LobjColDocumentType.Item(i);
                  LobjDocTypes.Add(LobjDocumentType.ID.ToString(), LobjDocumentType.Name);
               }
            }

            return LobjDocTypes;
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         finally
         {
            ReleaseCOMobject(LobjDocumentType);
            ReleaseCOMobject(LobjColDocumentType);
            ReleaseCOMobject(LobjColDocumentTypeGroup);

         }


      }


      /// <summary>
      /// 
      /// deletes document
      /// </summary>
      /// <param name="PstrDocumentID"></param>
      public void DeleteDocument1(string PstrDocumentID)
      {
         IOBXDocumentQuery LobjDocumentQuery = null;

         IOBXDocument LobjDocument = null;

         OnBaseDocument LobjOnbaseDocument = new OnBaseDocument();
   
         try
         {


            LobjDocumentQuery = (IOBXDocumentQuery)MobjSession.CreateObject("OBXDocumentQuery");

            LobjDocument = LobjDocumentQuery.GetDocumentByID(int.Parse(PstrDocumentID));

            if (LobjDocument == null)
            {
               return;
            }

            LobjDocument.DeleteDocument();

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            if (e.Message == string.Format("Failed to obtain document with ID #({0})", PstrDocumentID))
            {
               return ;
            }
            
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         finally
         {

            ReleaseCOMobject(LobjDocument);
            ReleaseCOMobject(LobjDocumentQuery);


         }

      }

      /// <summary>
      /// retrieves document groups
      /// </summary>
      /// <returns></returns>
      public NameValueCollection GetDocumentTypeGroups()
      {
         NameValueCollection LobjDocTypes = new NameValueCollection();
         IOBXDocumentTypeGroupCollection LobjColDocumentTypeGroup = null;
         IOBXDocumentTypeGroup LobjDocumentType = null;

         try
         {

            LobjColDocumentTypeGroup = (IOBXDocumentTypeGroupCollection)MobjSession.GetCollection("OBXDocumentTypeGroupCollection");

            for (int i = 0; i < LobjColDocumentTypeGroup.Count(); i++)
            {
               LobjDocumentType = LobjColDocumentTypeGroup.Item(i);
               LobjDocTypes.Add(LobjDocumentType.ID.ToString(), LobjDocumentType.Name);
            }

            return LobjDocTypes;
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
         finally
         {
            ReleaseCOMobject(LobjDocumentType);
            ReleaseCOMobject(LobjColDocumentTypeGroup);

         }


      }


      /// <summary>
      /// finds documents by keywords and document type 
      /// </summary>
      /// <param name="PstrDocumentType"></param>
      /// <param name="PobjKeywords"></param>
      /// <returns>list of Doc handles</returns>
      public System.Collections.ArrayList FindDocumentIds(NameValueCollection PobjKeywords)
      {


         System.Collections.ArrayList LalFoundDocuments = FindDocuments(null, null, PobjKeywords);
         System.Collections.ArrayList LalFoundDocumentIds = new System.Collections.ArrayList();

         foreach (OnBaseDocument LobjFoundDocument in LalFoundDocuments)
         {
            LalFoundDocumentIds.Add(LobjFoundDocument.DocumentId); 
         }
         return LalFoundDocumentIds; 

      }



            /// <summary>
      /// finds documents by keywords and document type 
      /// </summary>
      /// <param name="PstrDocumentType"></param>
      /// <param name="PobjKeywords"></param>
      /// <returns></returns>
      public System.Collections.ArrayList FindDocuments(string[] PastrDocumentTypes, string[] PastrDocumentTypeGroups,  NameValueCollection PobjKeywords)
      {
         IOBXDocumentQuery LobjDocumentQuery = null;

         IOBXDocumentResults LobjDocumentResults = null;
         IOBXDocument LobjDocument = null;
         IOBXDocumentTypeCollection LobjColDocumentType = null;
         IOBXDocumentTypeGroupCollection LobjColDocumentTypeGroup = null;

         System.Collections.ArrayList LalFoundDocuments = new System.Collections.ArrayList();

         RecycleSessionIfNeeded();


         try
         {

            LobjDocumentQuery = (IOBXDocumentQuery)MobjSession.CreateObject("OBXDocumentQuery");
            MiDocumentQueryCounter++;

            foreach (string LobjKey in PobjKeywords.AllKeys)
            {
               foreach (string LobjValue in PobjKeywords.GetValues(LobjKey))
               {
                  try
                  {
                     LobjDocumentQuery.AddKeywordByName(LobjKey, LobjValue);
                  }
                  catch
                  {
                  }
               }
            }

            if (PastrDocumentTypes != null)
            {
               LobjColDocumentType = (IOBXDocumentTypeCollection)MobjSession.GetCollection("OBXDocumentTypeCollection");

               ValidateDocumentTypes(PastrDocumentTypes, LobjColDocumentType);

               foreach (string LstrDocType in PastrDocumentTypes)
               {
                  LobjDocumentQuery.AddDocumentTypeByName(LstrDocType);
               }

            }


            if (PastrDocumentTypeGroups != null)
            {
               LobjColDocumentTypeGroup = (IOBXDocumentTypeGroupCollection)MobjSession.GetCollection("OBXDocumentTypeGroupCollection");

               ValidateDocumentTypeGroups(PastrDocumentTypeGroups, LobjColDocumentTypeGroup);

               foreach (string LstrDocTypeGrp in PastrDocumentTypeGroups)
               {
                  LobjDocumentQuery.AddDocumentTypeGroupByName(LstrDocTypeGrp);
               }

            }



            LobjDocumentResults = LobjDocumentQuery.Execute();

            while ((LobjDocument = LobjDocumentResults.GetNextDocument()) != null)
            {

               LalFoundDocuments.Add(CastDocument(LobjDocument));

            }


            return LalFoundDocuments;
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession));
         }
         finally
         {
            ReleaseCOMobject(LobjDocument);
            ReleaseCOMobject(LobjDocumentResults);
            ReleaseCOMobject(LobjDocumentQuery);
            ReleaseCOMobject(LobjColDocumentType);
            ReleaseCOMobject(LobjColDocumentTypeGroup);

         }
      }

      /// <summary>
      /// finds documents by keywords
      /// </summary>
      /// <param name="PobjKeywords"></param>
      /// <returns></returns>
      public System.Collections.ArrayList FindDocuments( NameValueCollection PobjKeywords )
      {
         return FindDocuments(null, null, PobjKeywords);

      }


      /// <summary>
      /// finds documents by keywords and doc type 
      /// </summary>
      /// <param name="PobjKeywords"></param>
      /// <returns></returns>
      public System.Collections.ArrayList FindDocuments(string[] PastrDocumentTypes, NameValueCollection PobjKeywords)
      {
         return FindDocuments(PastrDocumentTypes, null, PobjKeywords);

      }



      /// <summary>
      /// finds documents by keywords and doc type group
      /// </summary>
      /// <param name="PobjKeywords"></param>
      /// <returns></returns>
      public System.Collections.ArrayList FindDocumentsInGroup(string[] PastrDocumentTypeGroups, NameValueCollection PobjKeywords)
      {
         return FindDocuments(null, PastrDocumentTypeGroups, PobjKeywords);

      }


      /// <summary>
      /// find users for groups specified
      /// </summary>
      /// <param name="PastrGroups"></param>
      /// <returns></returns>

      public List<OnBaseUser> GetUsersForGroups(string[] PastrGroups)
      {
         IOBXPresentationServices LobjPresentationServices = null;
         IOBXElementProvider LobjElementProvider = null;
         IOBXElementCollection LobjUsersCollection = null;
         IOBXElementCollection LobjGroupsCollection = null;
         IOBXElement LobjGroup = null;
         try
         {
           LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices")  ;

           LobjElementProvider = LobjPresentationServices.CreateElementProvider();

           List<OnBaseUser> LobjAllUsers = new List<OnBaseUser>();

            // get all groups
           //SimpleTimer tm = new SimpleTimer();
           //tm.Start(); 
           LobjGroupsCollection = LobjElementProvider.GetElements("UserGroup", 0, "None");

           //tm.Stop();
           //System.Diagnostics.Debug.WriteLine("USER GROUPS:" + tm.GetSeconds()); 
           
            foreach (string LstrGroup in PastrGroups)
           {

                 try
                 {

                    // find group info
                    LobjGroup = LobjGroupsCollection.FindByName(LstrGroup);
                 }
                 catch (System.Runtime.InteropServices.COMException e)
                 {
                    continue; // eat exception if group is not found
                 }

                 string LstrId = LobjGroup.Properties.GetProperty("ID").ToString();

                 //tm.Start(); 
                 LobjUsersCollection = LobjElementProvider.GetElements("User", int.Parse( LstrId ) , "UserGroup");
                 //tm.Stop();
                 //System.Diagnostics.Debug.WriteLine("USERS:" + tm.GetSeconds()); 

                 List<OnBaseUser> LobjUsers = ReadUsersFromCollection(LobjUsersCollection, LstrGroup );


                 foreach (OnBaseUser LobjFoundUser in LobjUsers)
                 {
                    if (!LobjAllUsers.Contains(LobjFoundUser))

                       LobjAllUsers.Add(LobjFoundUser); 

                 }

                 

           }
           LobjAllUsers.Sort(); 
           return LobjAllUsers; 

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession));
         }

         finally
         {
            ReleaseCOMobject(LobjGroup);            
            ReleaseCOMobject(LobjGroupsCollection);
            ReleaseCOMobject(LobjUsersCollection);
            ReleaseCOMobject(LobjElementProvider);
            ReleaseCOMobject(LobjPresentationServices);
         }

      }

      /// <summary>
      /// this method returns list of users for group ( if existing group id passed )
      /// or all users if 0 is passed
      /// </summary>
      /// <param name="PiGroupId"></param>
      /// <returns></returns>

      public List<OnBaseUser> GetUsers(int PiGroupId)
      {
         IOBXPresentationServices LobjPresentationServices = null;
         IOBXElementProvider LobjElementProvider = null;
         IOBXElementCollection LobjElementCollection = null;

         try
         {
            LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices");

            LobjElementProvider = LobjPresentationServices.CreateElementProvider();

            //SimpleTimer tm = new SimpleTimer();
            //tm.Start(); 
            LobjElementCollection = LobjElementProvider.GetElements("User", PiGroupId, (PiGroupId == 0) ? "None" : "UserGroup");
            //tm.Stop();
            //System.Diagnostics.Debug.WriteLine(tm.GetSeconds()); 
            List<OnBaseUser> LobjUsers = ReadUsersFromCollection(LobjElementCollection, "" );

            return LobjUsers;

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession));
         }

         finally
         {

            ReleaseCOMobject(LobjElementCollection);
            ReleaseCOMobject(LobjElementProvider);
            ReleaseCOMobject(LobjPresentationServices);
         }

      }



      public List<OnBaseUserGroup> GetUserGroups()
      {
         IOBXPresentationServices LobjPresentationServices = null;
         IOBXElementProvider LobjElementProvider = null;
         IOBXElementCollection LobjElementCollection = null;

         try
         {
            LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices");

            LobjElementProvider = LobjPresentationServices.CreateElementProvider();

            LobjElementCollection = LobjElementProvider.GetElements("UserGroup", 0, "None");

            List<OnBaseUserGroup> LobjUserGroups = ReadGroupsFromCollection(LobjElementCollection);

            return LobjUserGroups;

         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession));
         }

         finally
         {
            ReleaseCOMobject(LobjElementCollection);
            ReleaseCOMobject(LobjElementProvider);
            ReleaseCOMobject(LobjPresentationServices);
         }

      }



      #endregion




      #region IDISPOSABLE -----------------------------------------------------------------------------------

      /// <summary>
      /// destructor - required by IDisposable
      /// release datasource after done using Onbase functionality
      /// </summary>
      public void Dispose()
      {

         if (MobjSession.SessionInfo.Connected == 1)
         {

            MobjSession.Disconnect();

         }

         ReleaseCOMobject(MobjSession);
         ReleaseCOMobject(MobjDataSource);
         ReleaseCOMobject(MobjDsnManager);
      }         

      #endregion




      #region PRIVATE HELPERS -----------------------------------------------------------------------





      /// <summary>
      /// casts IOBXDocument to OnBaseDocument
      /// </summary>
      /// <param name="PobjDocument"></param>
      /// <returns></returns>
      private  OnBaseDocument CastDocument(IOBXDocument PobjDocument )
      {

         OnBaseDocument LobjOnbaseDocument = new OnBaseDocument();
         IOBXKeyword LobjKeyword = null; 


         LobjOnbaseDocument.DocumentId = (long)PobjDocument.ID;
         LobjOnbaseDocument.Keywords = new NameValueCollection();

         for (int i = 0; i < PobjDocument.Keywords.Count(); i++)
         {
            LobjKeyword = PobjDocument.Keywords.Item(i);

            LobjOnbaseDocument.Keywords.Add(LobjKeyword.TypeName, LobjKeyword.Value);
         }

         LobjOnbaseDocument.CreatedUserID = PobjDocument.User.ID;
         LobjOnbaseDocument.DocumentDate = PobjDocument.DocumentDate.DateTime.ToString();
         LobjOnbaseDocument.DocumentType = PobjDocument.DocumentType.Name;
         LobjOnbaseDocument.DocumentTypeID = PobjDocument.DocumentType.ID;
         LobjOnbaseDocument.Name = PobjDocument.Name;

         LobjOnbaseDocument.FileFormat = (FileFormat)PobjDocument.Pages.Item(0).FileTypeID;

         ReleaseCOMobject(LobjKeyword); 
         return LobjOnbaseDocument;
      }


      /// <summary>
      /// reads error info from COM object
      /// </summary>
      /// <param name="PobjErroneuos">COM object under investigation</param>
      /// <returns></returns>
      private string PresentError(object PobjErroneuos)
      {

         OBXErrorRetriever LobjErrorRetriever = new OBXErrorRetriever();

         IOBXErrorInfo LobjColErrorInfo = LobjErrorRetriever.GetErrors(PobjErroneuos);

         string LstrError = "OnBase Exception occured:";

         IOBXErrorItem LobjErrItem;
         for (int i = 0; i < LobjColErrorInfo.Count(); i++)
         {
            LobjErrItem = LobjColErrorInfo.Item(i);
            LstrError += Environment.NewLine + LobjErrItem.Message;
         }

         return LstrError;
      }

      /// <summary>
      /// releases COM objects explicitely
      /// </summary>
      /// <param name="API_COMobject">COM object for release</param>
      private void ReleaseCOMobject(object PobjCOMobject)
      {
         // The entire purpose of this call is to call Marshal and release the resources.
         // We have to check for null or the application will blow up when making ReleaseComObject(null) .. 
         // we incorporate this.
         if (PobjCOMobject != null)
         {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(PobjCOMobject);
         }
      }


      /// <summary>
      /// retrives session and connects it 
      /// </summary>
      /// <returns></returns>
      private void GetNewSession()
      {
         try
         {
            MobjSession = MobjDataSource.NewSession( MstrUserName, MstrPassword);
            
            MobjSession.Connect() ;
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new CoreAPIException(PresentError(MobjSession), e);
         }
      }


      /// <summary>
      /// closes and reopens session when doc query conter reaches limit
      /// </summary>

      private void RecycleSessionIfNeeded()
      {
         if (MiDocumentQueryCounter == DOCQUERY_LIMIT )
         {
            MobjSession.Disconnect();
            ReleaseCOMobject(MobjSession);
            MobjSession = null;
            MiDocumentQueryCounter = 0;
         }

         GetNewSession(); 

      }
      /// <summary>
      /// finds DOC typeID for doc type specified
      /// </summary>
      /// <param name="PstrType">Doc type under search</param>
      /// <param name="PobjColDocumentType">Collection of Document Types</param>
      /// <returns></returns>
      private static int SetDocType(string PstrType, IOBXDocumentTypeCollection PobjColDocumentType)
      {
         try
         {
            return PobjColDocumentType.FindByName(PstrType).ID;
         }
         catch (System.Runtime.InteropServices.COMException e)
         {
            throw new ImagingServicesException(string.Format("Invalid Document Type '{0}' supplied.", PstrType));
         }

      }



      /// <summary>
      /// Validates passed document types
      /// </summary>
      /// <param name="PstrTypes"></param>
      /// <param name="PobjColDocumentType"></param>
      
      private static void ValidateDocumentTypes ( string[] PstrTypes, IOBXDocumentTypeCollection PobjColDocumentType)
      {
         foreach (string LstrPstrType in PstrTypes)
         {
            // try to find doc type. If not found then exception will be thrown
            try
            {
               PobjColDocumentType.FindByName(LstrPstrType);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
               //ImagingServicesException exception is re-thrown instead of COMException in order to provide 
               // extended info to user
               throw new ImagingServicesException(string.Format("Invalid Document Type '{0}' supplied.", LstrPstrType));
            }

         }

      }


      /// <summary>
      /// Validates passed document type groups
      /// </summary>
      /// <param name="PstrTypes"></param>
      /// <param name="PobjColDocumentType"></param>

      private static void ValidateDocumentTypeGroups(string[] PstrTypeGroups, IOBXDocumentTypeGroupCollection PobjColDocumentTypeGroup)
      {
         foreach (string LstrPstrTypeGrp in PstrTypeGroups)
         {
            // try to find doc type group. If not found then exception will be thrown
            try
            {
               PobjColDocumentTypeGroup.FindByName(LstrPstrTypeGrp);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
               //ImagingServicesException exception is re-thrown instead of COMException in order to provide 
               // extended info to user
               throw new ImagingServicesException(string.Format("Invalid Document Type Group'{0}' supplied.", LstrPstrTypeGrp));
            }

         }

      }

      ///// <summary>
      ///// finds DOC type group ID for doc type group specified
      ///// </summary>
      ///// <param name="PstrTypeGroup">Doc type group under search</param>
      ///// <param name="PobjColDocumentTypeGroup">Collection of Document Types</param>
      ///// <returns></returns>
      //private static int SetDocTypeGroup(string PstrTypeGroup, IOBXDocumentTypeGroupCollection PobjColDocumentTypeGroup)
      //{
      //   try
      //   {
      //      return PobjColDocumentTypeGroup.FindByName(PstrTypeGroup).ID;
      //   }
      //   catch (System.Runtime.InteropServices.COMException e)
      //   {
      //      throw new ImagingServicesException(string.Format("Invalid Document Type Group'{0}' supplied.", PstrTypeGroup));
      //   }

      //}




      /// <summary>
      /// validates keywords
      /// </summary>
      /// <param name="PobjDocumentTypeCollection">Collection of Document Types</param>
      /// <param name="PstrDocType">Doc type </param>
      /// <param name="PalKeywords">Array of keywords under validation</param>
      private void ValidateKeywords(IOBXDocumentTypeCollection PobjDocumentTypeCollection, string PstrDocType, NameValueCollection PobjKeywords)
      {
         IOBXKeywordTypeCollection LobjColKeywordType = PobjDocumentTypeCollection.FindByName(PstrDocType).KeywordTypes;
         IOBXKeywordType LobjKeywordType = null;
         bool LbValid = true;
         string LstrError = "";


         foreach (string LobjKey in PobjKeywords.AllKeys)
         {
            try
            {
               LobjKeywordType = LobjColKeywordType.FindByName(LobjKey);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
               LbValid &= false;
               LstrError += string.Format("{2}Keyword '{0}' does not belong to Document Type '{1}'", LobjKey, PstrDocType, Environment.NewLine);
            }
         }

         ReleaseCOMobject(LobjKeywordType);
         ReleaseCOMobject(LobjColKeywordType);
         if (!LbValid)
         {
            throw new ImagingServicesException(string.Format("Invalid keyword(s) supplied: {0}", LstrError));
         }

      }

      /// <summary>
      /// find keywords that are valid for doc type passed
      /// </summary>
      /// <param name="PobjDocumentTypeCollection"></param>
      /// <param name="PstrDocType"></param>
      /// <param name="PobjKeywords"></param>
      /// <returns></returns>

      private NameValueCollection GetValidKeywords(IOBXDocumentTypeCollection PobjDocumentTypeCollection, string PstrDocType, NameValueCollection PobjKeywords)
      {
         NameValueCollection LobjValidKeywords = new NameValueCollection(); 
         IOBXKeywordTypeCollection LobjColKeywordType = PobjDocumentTypeCollection.FindByName(PstrDocType).KeywordTypes;
         IOBXKeywordType LobjKeywordType = null;

         foreach (string LobjKey in PobjKeywords.AllKeys)
         {
            try
            {
               LobjKeywordType = LobjColKeywordType.FindByName(LobjKey);
               foreach (string LobjValue in PobjKeywords.GetValues(LobjKey))
               {
                  LobjValidKeywords.Add(LobjKey, LobjValue );
               }
            }
            catch 
            {
            }
         }

         ReleaseCOMobject(LobjKeywordType);
         ReleaseCOMobject(LobjColKeywordType);


         return LobjValidKeywords; 
      }


      /// <summary>
      /// locks document for edit
      /// </summary>
      /// <param name="PobjDocument"></param>
      private void LockDocument(IOBXDocument PobjDocument)
      {
         if (PobjDocument == null) return;
         //LobjLogger.Log("Inside lock"); 
         IOBXPropertyBag LobjPropertyBag = PobjDocument.LockObjectEx(32); 
         // excerpt from Onbase Core API help:
         //This value (32) will attempt to lock a document for keyword/page modification. 
         //This lock would be used for modifying the order of pages, 
         //adding/removing pages, changing keywords, or reindexing a document

         //result Boolean The result of the call to lock the document, either a true or false value will be returned 
         //status Long The status after the attempt to lock the document. Valid values are: 
         //0 The document has been successfully locked. 
         //-2147220733 Lock attempt failed. The document was locked by another user. 
         //-2147220728 Lock attempt failed. The document was locked by records management. 
         //-2147220726 Lock attempt failed. The document was already locked by this user. This can occur if two or more users log in with the same user account. 

         //userID Long In the case of a failure to attain a lock this will represent the user ID of the user who already had a lock on this document. In the case of a successful lock this will be the current user's ID 


         if (! (bool.Parse(LobjPropertyBag.GetProperty("result").ToString())) ) // lock failed 
         {
            string LstrError = "" ;
            if (LobjPropertyBag.GetProperty("status").ToString() == "-2147220733" || LobjPropertyBag.GetProperty("status").ToString() == "-2147220726")
            {
               LstrError = "Document is locked by another User";
            }
            else
            {
               LstrError = "Document is locked by records management.";

            }
            ReleaseCOMobject (LobjPropertyBag) ;
            throw new ImagingServicesException(LstrError); 

         }
         ReleaseCOMobject (LobjPropertyBag) ; 

      }


      /// <summary>
      /// unlocks document
      /// </summary>
      /// <param name="PobjDocument"></param>
      private void UnLockDocument(IOBXDocument PobjDocument)
      {
         if (PobjDocument == null) return; 
         IOBXPropertyBag LobjPropertyBag = PobjDocument.UnlockObjectEx(32);
         // excerpt from Onbase Core API help:
         //  This method will allow a caller to attempt to unlock a document that they have already locked. 

         //Parameters:
         // dwLocks  The lock type that you would like to remove. Current supported values are: 
         //Value Description 
         //32 This value will attempt to remove a lock placed on a document for keyword/page modification. 

         //Return values:
         // ppProps  Pointer to an output variable that is to receive the property bag. The list of valid return properties is discussed below. 
         //Property Data Type Description 
         //result Boolean The result of the call to unlock the document, either a true or false value will be returned 
         //status Long The status after the attempt to unlock the document. Valid values are: 
         //0 The document has been successfully unlocked. Any other value indicates a failure to unlock the document  





         if (!(bool.Parse(LobjPropertyBag.GetProperty("result").ToString()))) // lock failed 
         {
               ReleaseCOMobject (LobjPropertyBag) ; 
               throw new ImagingServicesException("Failed to unlock document.");

            }
            ReleaseCOMobject (LobjPropertyBag) ; 
       }




      private List<OnBaseUserGroup> ReadGroupsFromCollection(IOBXElementCollection LobjElementCollection)
      {
         IOBXElement LobjElement = null;

         IOBXPropertyBag LobjPropertyBag = null;

         List<OnBaseUserGroup> LobjReturn = new List<OnBaseUserGroup>();

         for (int i = 0; i < LobjElementCollection.Count(); i++)
         {
            LobjElement = LobjElementCollection.Item(i);

            LobjPropertyBag = LobjElement.Properties;

            string LstrId = LobjPropertyBag.GetProperty("ID").ToString() ;

            OnBaseUserGroup LobjEntity = new OnBaseUserGroup(int.Parse(LstrId),
               (LobjPropertyBag.GetProperty("Name") == null) ? "" : LobjPropertyBag.GetProperty("Name").ToString()); 

           LobjReturn.Add( LobjEntity );
         }

         ReleaseCOMobject(LobjPropertyBag);

         ReleaseCOMobject(LobjElement);

         return LobjReturn;
      }



      private List<OnBaseUser> ReadUsersFromCollection(IOBXElementCollection LobjElementCollection, string PstrGroupName )
      {
         IOBXElement LobjElement = null;

         IOBXPropertyBag LobjPropertyBag = null;

         List<OnBaseUser> LobjReturn = new List<OnBaseUser>();

         for (int i = 0; i < LobjElementCollection.Count(); i++)
         {
            LobjElement = LobjElementCollection.Item(i);

            LobjPropertyBag = LobjElement.Properties;

            string LstrId = LobjPropertyBag.GetProperty("ID").ToString();

            OnBaseUser LobjEntity = new OnBaseUser(int.Parse(LstrId),
               (LobjPropertyBag.GetProperty("Name") == null) ? "" : LobjPropertyBag.GetProperty("Name").ToString(), 
               ( LobjPropertyBag.GetProperty("DisplayName") == null ) ? "" : LobjPropertyBag.GetProperty("DisplayName").ToString(),
                PstrGroupName ); 

            LobjReturn.Add(LobjEntity);
         }

         ReleaseCOMobject(LobjPropertyBag);

         ReleaseCOMobject(LobjElement);

         return LobjReturn;
      }

      #endregion



       /// <summary>
       /// 
       /// deletes document
       /// </summary>
       /// <param name="PstrDocumentID"></param>
       public void DeleteDocument(string PstrDocumentID)
       {

          IOBXPresentationServices LobjPresentationServices = null;
          OBXPropertyBag LobjPropertyBag = null;
          IOBXDocumentActionHandler LobjDocumentActionHandler = null;

          try
          {
              LobjPresentationServices = (IOBXPresentationServices)MobjSession.CreateServicesModule("OnBase.PresentationServices");
              LobjDocumentActionHandler = (IOBXDocumentActionHandler)LobjPresentationServices.CreateHandler("OBXDocumentActionHandler");
              LobjPropertyBag = new OBXPropertyBag();
              LobjPropertyBag.SetProperty("documentID", long.Parse(PstrDocumentID));
              LobjDocumentActionHandler.DoVerb("DeleteDocument", LobjPropertyBag);


          }
          catch (System.Runtime.InteropServices.COMException e)
          {
             if (e.Message == string.Format("Failed to obtain document with ID #({0})", PstrDocumentID))
             {
                return;
             }

             throw new CoreAPIException(PresentError(MobjSession), e);
          }
          finally
          {

             ReleaseCOMobject(LobjDocumentActionHandler);
             ReleaseCOMobject(LobjPropertyBag);
             ReleaseCOMobject(LobjPresentationServices);

          }

       }

   }

    
}
