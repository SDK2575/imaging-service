using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// Summary description for UnitTest1
   /// </summary>
   [TestClass]
   public class ImageServiceTest
   {
       readonly string MstrImageServiceUrl = ConfigurationManager.AppSettings["ImageServiceUrl"];
       readonly string MstrDocumentType = ConfigurationManager.AppSettings["DocumentType"];
       int MiKeywordCount = 0;

      public ImageServiceTest()
      {
         //
         // TODO: Add constructor logic here
         //
      }

      #region Additional test attributes
      //
      // You can use the following additional attributes as you write your tests:
      //
      // Use ClassInitialize to run code before running the first test in the class
      // [ClassInitialize()]
      // public static void MyClassInitialize(TestContext testContext) { }
      //
      // Use ClassCleanup to run code after all tests in a class have run
      // [ClassCleanup()]
      // public static void MyClassCleanup() { }
      //
      // Use TestInitialize to run code before running each test 
      // [TestInitialize()]
      // public void MyTestInitialize() { }
      //
      // Use TestCleanup to run code after each test has run
      // [TestCleanup()]
      // public void MyTestCleanup() { }
      //
      #endregion
       [TestMethod]
       public void InsertDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           long LlId = 0;
           long LlTestId = 0;
           LlId = InsertDoc(LalKeywords, "");
           //delete the document 
           DeleteDoc(LlId);

           Assert.AreNotEqual(LlTestId, LlId);
       }

       private long InsertDoc(ArrayList PalKeywords, string PstrDocType)
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           System.IO.FileStream LobjStream = new System.IO.FileStream(@"C:\sampledoc.tif", System.IO.FileMode.Open, System.IO.FileAccess.Read);
           System.IO.BinaryReader LobjBinaryReader = new System.IO.BinaryReader(LobjStream);
           byte[] LabytData = LobjBinaryReader.ReadBytes((int)LobjStream.Length);
           LobjBinaryReader.Close();
           LobjStream.Close();

           Document LobjDocument = new Document();
           if (PstrDocType.Length.Equals(0))
           {
               LobjDocument.Type = MstrDocumentType;
           }
           else
           {
               LobjDocument.Type = PstrDocType;
           }
           
           LobjDocument.FileFormat = FileFormatType.ImageFileFormat;
           LobjDocument.FileFormatSpecified = true;
           MiKeywordCount = PalKeywords.Count;
           LobjDocument.Keywords = (Keyword[])PalKeywords.ToArray(typeof(Keyword));

           long LlId = LobjWSImageService.InsertDocument(LabytData, LobjDocument);
           return LlId;
       }

       private ArrayList AddKeywordToArrayList(ArrayList PalKeywords, string PstrKey, string PstrValue)
       {
           Keyword LobjKeyword = new Keyword();
           LobjKeyword.Key = PstrKey;
           LobjKeyword.Value = PstrValue;
           PalKeywords.Add(LobjKeyword);
           return PalKeywords;
       }
       private void DeleteDoc(long PlDocId)
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           LobjWSImageService.DeleteDocument(PlDocId);
       }

       [TestMethod]
       public void InsertWrongKeywordsDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Invalid Keyword", "SU");

           long LlId = 0;
           long LlTestId = 0;
           LlId = InsertDoc(LalKeywords, "");
           //delete the document 
           DeleteDoc(LlId);

           Assert.AreNotEqual(LlTestId, LlId);
       }

       [TestMethod]
       [ExpectedException(typeof(System.Web.Services.Protocols.SoapException))]
       public void InsertWrongDocTypeDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "Invalid Document Type");
       }

       [TestMethod]
       public void UpdateKeywordsTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           //change a keyword in the arraylist
           string LstrExpectedResult = "";
           foreach (Keyword LobjKeyword in LalKeywords)
           {
               if (LobjKeyword.Key.Equals("Assigned ToID"))
               {
                   LobjKeyword.Value = "TestID";
                   LstrExpectedResult = LobjKeyword.Value;
                   break;
               }
           }

           //instantiate document and set the doc id to the one created above
           Document LobjDocument = new Document();
           LobjDocument.ID = LlId;
           LobjDocument.IDSpecified = true;
           //add the updated keyword arraylist to the document
           LobjDocument.Keywords = (Keyword[])LalKeywords.ToArray(typeof(Keyword));

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           //update the document keywords
           LobjWSImageService.UpdateKeywords(LobjDocument);

           //retrieve the updated document
           Document LobjReturnDocument = LobjWSImageService.GetKeywords(LlId);
           //delete the document 
           DeleteDoc(LlId);
           //get the keyword that should have been updated from the retrieved document
           string LstrActualResult = "";
           foreach (Keyword LobjKeyword in LobjReturnDocument.Keywords)
           {
               if (LobjKeyword.Key.Equals("Assigned ToID"))
               {
                   LstrActualResult = LobjKeyword.Value;
                   break;
               }
           }
           Assert.AreEqual(LstrExpectedResult.ToUpper(), LstrActualResult);
       }

       [TestMethod]
       public void UpdateInvalidKeywordsTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");
           Keyword LobjKw1 = (Keyword)LalKeywords[0];
           string LstrExpectedResult = LobjKw1.Value.ToUpper();
           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           //add invalid keyword to arraylist
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Invalid Keyword", "123456");

           //instantiate document and set the doc id to the one created above
           Document LobjDocument = new Document();
           LobjDocument.ID = LlId;
           LobjDocument.IDSpecified = true;

           //add the updated keyword arraylist with 2 keywords to the document
           LobjDocument.Keywords = (Keyword[])LalKeywords.ToArray(typeof(Keyword));

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           //update the document keywords
           LobjWSImageService.UpdateKeywords(LobjDocument);

           //retrieve the updated document
           Document LobjReturnDocument = LobjWSImageService.GetKeywords(LlId);
           //delete the document 
           DeleteDoc(LlId);

           //there should only be 1 keyword
           Assert.AreEqual(1, LobjReturnDocument.Keywords.Length);

           //get the value of the only keyword
           string LstrActualResult = "";
           Keyword LobjKw = (Keyword)LobjReturnDocument.Keywords.GetValue(0);
           LstrActualResult = LobjKw.Value;

           //the value of the keyword should not have changed
           Assert.AreEqual(LstrExpectedResult.ToUpper(), LstrActualResult);
       }

       [TestMethod]
       [ExpectedException(typeof(System.Web.Services.Protocols.SoapException))]
       public void UpdateInvalidDocTypeTest()
       {
           //The inserted document must be manually deleted.

           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           Document LobjDocument = new Document();
           LobjDocument.ID = LlId;
           LobjDocument.IDSpecified = true;
           LobjDocument.Type = "Invalid Doc Type";

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           LobjWSImageService.UpdateDocument(LobjDocument);
       }

       [TestMethod]
       public void UpdateDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           //change a keyword in the arraylist
           string LstrExpectedResult = "";
           foreach (Keyword LobjKeyword in LalKeywords)
           {
               if (LobjKeyword.Key.Equals("Assigned ToID"))
               {
                   LobjKeyword.Value = "TestID";
                   break;
               }
           }

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           //retrieve the inserted document
           Document LobjInsertedDocument = LobjWSImageService.GetDocumentData(LlId);

           Document LobjDocument = new Document();
           LobjDocument.ID = LlId;
           LobjDocument.IDSpecified = true;
           LobjDocument.Type = "GC  Legal";

           LobjDocument.Keywords = (Keyword[])LalKeywords.ToArray(typeof(Keyword));
           LobjWSImageService.UpdateDocument(LobjDocument);

           //retrieve the updated document
           Document LobjReturnDocument = LobjWSImageService.GetDocumentData(LlId);

           //delete the document 
           DeleteDoc(LlId);

       }

       
       [TestMethod]
       public void UpdateDocumentTypeTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           //retrieve the inserted document
           Document LobjInsertedDocument = LobjWSImageService.GetDocumentData(LlId);

           Document LobjDocument = new Document();
           LobjDocument.ID = LlId;
           LobjDocument.IDSpecified = true;
           LobjDocument.Type = "GC  Medical";

           LobjDocument.Keywords = (Keyword[])LalKeywords.ToArray(typeof(Keyword));
           LobjWSImageService.UpdateDocument(LobjDocument);

           //retrieve the updated document
           Document LobjReturnDocument = LobjWSImageService.GetDocumentData(LlId);

           //delete the document 
           DeleteDoc(LlId);

           //the document types should be different
           Assert.AreNotEqual(LobjInsertedDocument.Type, LobjReturnDocument.Type);
           //the number of keywords should be different because "GC  Medical" does not have the
           //"Assigned ToID" keyword
           Assert.AreNotEqual(LobjInsertedDocument.Keywords.Length, LobjReturnDocument.Keywords.Length);
       }

       [TestMethod]
       public void DeleteDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");
           long LlZero=0;
           Assert.AreNotEqual(LlZero, LlId);

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           LobjWSImageService.DeleteDocument(LlId);
           //there is not currently a way to automatically determine if a document has actually
           //been deleted.  when retrieving by id, a deleted document will have a status of 16
           //but it will still retain all of its values.  the data access layer does not currently
           //have a mechanism for checking the status of a document.
       }

       [TestMethod]
       [ExpectedException(typeof(System.Web.Services.Protocols.SoapException))]
       public void DeleteNonExistentDocumentTest()
       {

           long LlId = -1;
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           LobjWSImageService.DeleteDocument(LlId);
       }

       [TestMethod]
       public void GetDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           Document LobjReturnDocument = LobjWSImageService.GetDocumentData(LlId);

           //delete document now in case one of the asserts fails
           LobjWSImageService.DeleteDocument(LlId);

           Assert.AreEqual(LlId, LobjReturnDocument.ID, "ID is  not correct");

           Assert.AreEqual(FileFormatType.ImageFileFormat, LobjReturnDocument.FileFormat, "FileFormat is  not correct");
           Assert.AreEqual(MstrDocumentType, LobjReturnDocument.Type, "Type  is  not correct");

           //1 is added to the keyword count because the insert routine is adding an additional keyword
           Assert.AreEqual(MiKeywordCount + 1, LobjReturnDocument.Keywords.Length, "Keyword count is not correct");

       }

       [TestMethod]
       public void GetKeywordsTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //insert document
           long LlId = 0;
           LlId = InsertDoc(LalKeywords, "");

           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           Document LobjReturnDocument = LobjWSImageService.GetKeywords(LlId);

           //delete document now in case one of the asserts fails
           LobjWSImageService.DeleteDocument(LlId);

           //1 is added to the keyword count because the insert routine is adding an additional keyword
           Assert.AreEqual(LalKeywords.Count + 1, LobjReturnDocument.Keywords.Length, "Keyword count is not correct");

       }

      [TestMethod]
      public void GetAllDocumentTypesTest()
      {
         int LiExpectedCount = 1032; //
         ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

         DocumentType[] LaDocumentTypes = LobjWSImageService.GetDocumentTypes();
         Assert.AreEqual(LiExpectedCount, LaDocumentTypes.Length, "Data types count is incorrect") ; 
      }


      [TestMethod]
      public void GetAllDocumentGroupsTest()
      {
         int LiExpectedCount = 51; //
         ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

         DocumentType[] LaDocumentTypes = LobjWSImageService.GetDocumentGroups();
         Assert.AreEqual(LiExpectedCount, LaDocumentTypes.Length, "Data groups count is incorrect");
      }

      [TestMethod]
      public void GetAllDocumentTypesForGroupTest()
      {
         int LiExpectedCount = 3; //
         ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

         DocumentType[] LaDocumentTypes = LobjWSImageService.GetDocumentTypesForGroup("General Claims");
         //Assert.AreEqual(LiExpectedCount, LaDocumentTypes.Length, "Data groups count is incorrect");
      }



      [TestMethod]
      [ExpectedException(typeof(System.Web.Services.Protocols.SoapException))]

      public void GetAllDocumentTypesForInvalidGroupTest()
      {
         int LiExpectedCount = 4; //
         ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

         DocumentType[] LaDocumentTypes = LobjWSImageService.GetDocumentTypesForGroup("Invalid Group");
         //Assert.AreEqual(LiExpectedCount, LaDocumentTypes.Length, "Data groups count is incorrect");
      }


       [TestMethod]
       public void GetDocumentsTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST31");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //Set how many documents to insert
           int LiDocumentCount = 3;
           //Create array to hold doc ids to be deleted after test
           List<long> LalDocIds = new List<long>();
           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           //insert LiDocumentCount documents
           long LlId = 0;
           for (int i = 0; i < LiDocumentCount; i++)
           {
               LlId = InsertDoc(LalKeywords, "");
               LalDocIds.Add(LlId);
           }

           Document[] LaDocuments = LobjWSImageService.FindDocuments((Keyword[])LalKeywords.ToArray(typeof(Keyword)));

           foreach (long LlDeleteId in LalDocIds)
           {
               LobjWSImageService.DeleteDocument(LlDeleteId);
           }
           Assert.AreEqual(LiDocumentCount, LaDocuments.Length, "Documents count is incorrect");
       }

       [TestMethod]
       public void GetDocumentsWithDocTypeTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST32");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //Set how many documents to insert
           int LiDocumentCount = 3;
           //Create array to hold doc ids to be deleted after test
           List<long> LalDocIds = new List<long>();
           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           //insert LiDocumentCount documents
           long LlId = 0;
           for (int i = 0; i < LiDocumentCount; i++)
           {
               LlId = InsertDoc(LalKeywords, "");
               LalDocIds.Add(LlId);
           }

           Document[] LaDocuments = LobjWSImageService.FindDocumentsWithDocumentType(new string[] { MstrDocumentType }, (Keyword[])LalKeywords.ToArray(typeof(Keyword)));

           foreach (long LlDeleteId in LalDocIds)
           {
               LobjWSImageService.DeleteDocument(LlDeleteId);
           }

           Assert.AreEqual(LiDocumentCount, LaDocuments.Length, "Documents count is incorrect");

       }
       public void GetDocumentsWithDocTypeGroupTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST33");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           //Set how many documents to insert
           int LiDocumentCount = 3;
           //Create array to hold doc ids to be deleted after test
           List<long> LalDocIds = new List<long>();
           //instantiate the web service
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           //insert LiDocumentCount documents
           long LlId = 0;
           for (int i = 0; i < LiDocumentCount; i++)
           {
               LlId = InsertDoc(LalKeywords, "");
               LalDocIds.Add(LlId);
           }

           //string[] LaGroup = new string[] { "Policyholder Documents" };
           //Document[] LaDocuments = LobjWSImageService.FindDocumentsWithDocumentTypeGroup(LaGroup, (Keyword[])LalKeywords.ToArray(typeof(Keyword)));

           Document[] LaDocuments = LobjWSImageService.FindDocumentsWithDocumentTypeGroup("Policyholder Documents", (Keyword[])LalKeywords.ToArray(typeof(Keyword)));

           foreach (long LlDeleteId in LalDocIds)
           {
               LobjWSImageService.DeleteDocument(LlDeleteId);
           }
           Assert.AreEqual(LiDocumentCount, LaDocuments.Length, "Documents count is incorrect");
       }

      [TestMethod]
      public void GetAllDocumentTypesForGroupsTest()
      {
         int LiExpectedCount = 10; //
         ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

         DocumentType[] LaDocumentTypes = LobjWSImageService.GetDocumentTypesForGroups(new string[] { "General Claims", "General Claims Reports" });
         Assert.AreEqual(LiExpectedCount, LaDocumentTypes.Length, "Data groups count is incorrect");
      }

       [TestMethod]
       [ExpectedException(typeof(System.Web.Services.Protocols.SoapException))]
       public void UpdateKeywordsForNonExistentDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           Document LobjDocument = new Document();
           LobjDocument.ID = -1;
           LobjDocument.IDSpecified = true;

           LobjDocument.Keywords = (Keyword[])LalKeywords.ToArray(typeof(Keyword));
           LobjWSImageService.UpdateKeywords(LobjDocument);
       }


       [TestMethod]
       [ExpectedException(typeof(System.Web.Services.Protocols.SoapException))]
       public void UpdateDocumentTypeForNonExistentDocumentTest()
       {
           //set initial keyword values
           ArrayList LalKeywords = new ArrayList();
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Claim Number", "123456TEST30");
           LalKeywords = AddKeywordToArrayList(LalKeywords, "Assigned ToID", "SU");

           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           Document LobjDocument = new Document();

           LobjDocument.ID = -1;
           LobjDocument.IDSpecified = true;
           LobjDocument.Type = "GC  Legal";

           LobjDocument.Keywords = (Keyword[])LalKeywords.ToArray(typeof(Keyword));
           LobjWSImageService.UpdateDocument(LobjDocument);
       }

       [TestMethod]
       public void GetNonExistentDocumentTest()
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);

           Document LobjReturnDocument = LobjWSImageService.GetDocumentData(-1);

           Assert.IsNull(LobjReturnDocument, "Document is  not NULL");
       }

       [TestMethod]
       public void GetUsersTest()
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           int i = 1;
           string LstrUsers = LobjWSImageService.GetUsers(i);
           Assert.AreNotEqual("", LstrUsers);
       }

       [TestMethod]
       public void GetDocumentTypeIdByNameTest()
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           DocumentType LobjType = new DocumentType();
           LobjType.Value = "GC  Legal";
           DocumentType LobjRetType = LobjWSImageService.GetDocumentTypeIdByName(LobjType);
           //assumes okay as long as something is returned. gets soap exception if not.
           Assert.IsNotNull(LobjRetType);
       }


       [TestMethod]
       public void GetDocumentTypeNameByIdTest()
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           DocumentType LobjType = new DocumentType();
           LobjType.ID = "1000";
           DocumentType LobjRetType = LobjWSImageService.GetDocumentTypeNameById(LobjType);
           Assert.IsNotNull(LobjRetType);
       }

       [TestMethod]
       public void GetUsersForGroupsTest()
       {
           ImageService LobjWSImageService = new ImageService(MstrImageServiceUrl);
           User[] LobjRetType = LobjWSImageService.GetUsersForGroups("MANAGER");
           Assert.AreNotEqual(0, LobjRetType.Length);
       }
   }

