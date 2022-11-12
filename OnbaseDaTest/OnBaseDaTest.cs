using System;
using System.Text;
using System.Collections;
using System.Collections.Generic ;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using EnterpriseImaging.ImagingServices.Entities;
using EnterpriseImaging.ImagingServices.DataAccess;

namespace OnBaseDaTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class OnBaseDaTest
    {
        readonly string MstrUserName = ConfigurationManager.AppSettings["OnBaseUserName"];
        readonly string MstrPassword = ConfigurationManager.AppSettings["OnBasePassword"];
        readonly string MstrDataSource = ConfigurationManager.AppSettings["OnBaseDataSource"];
        readonly string MstrDatabaseConnection = ConfigurationManager.AppSettings["OnBaseDatabaseConnection"];

        public OnBaseDaTest()
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
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "123456TEST30");
            LobjKeywords.Add("Assigned ToID", "SU");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);
            Assert.AreNotEqual("", LstrDocId);

            //DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void FindDocumentsTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "123456TEST30");
            ArrayList LalDocIds = new ArrayList();
            LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));
            LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));
            LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));
            LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));

            ArrayList LobjReturnDocuments = new ArrayList();
            using (OnBaseDa LobjOnBaseDaSearch = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnDocuments = LobjOnBaseDaSearch.FindDocuments(LobjKeywords);
            }
            foreach (string LstrDocId in LalDocIds)
            {
                DeleteDocument(LstrDocId);
            }
            Assert.AreEqual(4, LobjReturnDocuments.Count);
        }

        [TestMethod]
        public void FindDocumentsWithDoctypeTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "123456TEST30");
            ArrayList LalDocIds = new ArrayList();
            //LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));
            //LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));
            LalDocIds.Add(InsertDocument("GC  Legal", LobjKeywords));
            LalDocIds.Add(InsertDocument("GC  Legal", LobjKeywords));

            ArrayList LobjReturnDocuments = new ArrayList();
            using (OnBaseDa LobjOnBaseDaSearch = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnDocuments = LobjOnBaseDaSearch.FindDocuments(new string[] { "GC  Legal" }, LobjKeywords);
            }
            foreach (string LstrDocId in LalDocIds)
            {
                DeleteDocument(LstrDocId);
            }
            Assert.AreEqual(2, LobjReturnDocuments.Count);
        }
        public void FindDocumentsWithDoctypeGroupTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "123456TEST30");
            ArrayList LalDocIds = new ArrayList();
            LalDocIds.Add(InsertDocument("GC  Medical", LobjKeywords));
            LalDocIds.Add(InsertDocument("GC  Legal", LobjKeywords));

            ArrayList LobjReturnDocuments = new ArrayList();
            using (OnBaseDa LobjOnBaseDaSearch = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {

                LobjReturnDocuments = LobjOnBaseDaSearch.FindDocumentsInGroup(new string[] { "General Claims - ClaimCenter Claims" }, LobjKeywords);
            }
            foreach (string LstrDocId in LalDocIds)
            {
                DeleteDocument(LstrDocId);
            }
            Assert.AreEqual(2, LobjReturnDocuments.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void FindDocumentsWithInvalidDoctypeTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "123456TEST30");
            ArrayList LobjReturnDocuments = new ArrayList();
            using (OnBaseDa LobjOnBaseDaSearch = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnDocuments = LobjOnBaseDaSearch.FindDocuments(new string[] { "Invalid Doctype" }, LobjKeywords);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void FindDocumentsWithInvalidDoctypeGroupTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();
            LobjKeywords.Add("Legacy Object Id", "20082222PPPP");
            ArrayList LobjReturnDocuments = new ArrayList();
            using (OnBaseDa LobjOnBaseDaSearch = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {

                LobjReturnDocuments = LobjOnBaseDaSearch.FindDocumentsInGroup(new string[] { "Invalid DoctypeGroup" }, LobjKeywords);
            }
        }

        [TestMethod]
        //Commented out the [ExpectedException] because the insert logic contains code that ignores
        //invalid keywords, so the document should be inserted with the valid keywords.
        //[ExpectedException(typeof(ImagingServicesException))]
        public void InsertWrongKeywordsDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Legacy Object Id", "20052222XNXX");
            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Claim Number", "2006-123458");
            LobjKeywords.Add("Invalid Keyword", "2006-123458");

            string LstrDocId = InsertDocument("GC  Legal", LobjKeywords);
            Assert.AreNotEqual("", LstrDocId);

            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void InsertWrongDocTypeDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");

            string LstrDocId = InsertDocument("Invalid Doc Type", LobjKeywords);
            //The insert should fail, but just in case...
            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void UpdateKeywordsTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            string LstrOldValue = "123456TEST30";
            LobjKeywords.Add("Claim Number", LstrOldValue);
            LobjKeywords.Add("Assigned ToID", "SU");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            //Change the Claim Number by removing and adding with different value
            LobjKeywords.Remove("Claim Number");
            LobjKeywords.Add("Claim Number", "654321TEST99");
            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateKeywords(LstrDocId, LobjKeywords);
            }
            //Get keywords from changed document
            NameValueCollection LobjReturnKeywords;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnKeywords = LobjOnBaseDaGet.GetKeywords(LstrDocId);
            }
            Assert.AreNotEqual(LstrOldValue, LobjReturnKeywords.Get("Claim Number"));

            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void UpdateInvalidKeywordsTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Claim Number", "2006-123458");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            //Change the Claim Number by removing and adding with different value
            LobjKeywords.Remove("Claim Number");
            LobjKeywords.Add("Claim Number", "654321TEST99");
            LobjKeywords.Add("Invalid Keyword", "2006-123456");
            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateKeywords(LstrDocId, LobjKeywords);
            }
            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void UpdateInvalidDocTypeTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Legacy Object Id", "20052222XNXX");
            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Claim Number", "2006-123458");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            LobjKeywords.Remove("Claim Number");

            LobjKeywords.Add("Claim Number", "2006-888888");
            LobjKeywords.Add("Claim Number", "2006-999998");
            LobjKeywords.Add("Claim Suffix", "SS");

            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateDocument(LstrDocId, "Invalid Doc Type", LobjKeywords);
            }
            //This should throw an exception before getting here, but just in case...
            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void UpdateDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Assigned To", "Lisa Abrams");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            LobjKeywords.Remove("Claim Number");
            LobjKeywords.Remove("Assigned To");

            LobjKeywords.Add("Claim Number", "2006-888888");
            LobjKeywords.Add("Assigned To", "Anthony Alach");

            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateDocument(LstrDocId, "GC  Legal", LobjKeywords);
            }

            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void UpdateDocumentTest2()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Assigned To", "Lisa Abrams");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            LobjKeywords.Remove("Claim Number");
            LobjKeywords.Remove("Assigned To");

            LobjKeywords.Add("Claim Number", "2006-888888");
            LobjKeywords.Add("Assigned To", "Anthony Alach");

            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateDocument(LstrDocId, "GC  Legal", LobjKeywords);
            }

            DeleteDocument(LstrDocId);
        }

        
        /// <summary>
        /// Tests UpdateDocument with datetime overload
        /// </summary>
        [TestMethod]
        public void UpdateDocumentWithDateTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Assigned To", "Lisa Abrams");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            LobjKeywords.Remove("Claim Number");
            LobjKeywords.Remove("Assigned To");

            LobjKeywords.Add("Claim Number", "2006-888888");
            LobjKeywords.Add("Assigned To", "Anthony Alach");

            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateDocument(LstrDocId, "GC  Legal", LobjKeywords, DateTime.Now.AddDays(-1));
            }

            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void UpdateNonExistentDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123458");

            string LstrDocId = "-1";

            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateDocument(LstrDocId, "GC  Legal", LobjKeywords);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void UpdateKeywordsNonExistentDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Legacy Object Id", "20052222XNXX");
            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Claim Number", "2006-123458");

            string LstrDocId = "-1";
            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateKeywords(LstrDocId, LobjKeywords);
            }
        }

        [TestMethod]
        public void DeleteDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Assigned To", "Lisa Abrams");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        [ExpectedException(typeof(CoreAPIException))]
        public void DeleteNonExistentDocumentTest()
        {
            string LstrDocId = "-1";
            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void GetDocumentTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Assigned To", "Lisa Abrams");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            OnBaseDocument LobjReturnDocument;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnDocument = LobjOnBaseDaGet.GetDocumentProperties(LstrDocId);
            }

            Assert.AreNotEqual(LstrDocId, LobjReturnDocument.DocumentId, "ID is  not correct");

            Assert.AreEqual(FileFormat.ImageFileFormat, LobjReturnDocument.FileFormat, "FileFormat is  not correct");
            Assert.AreEqual("GC  Legal", LobjReturnDocument.DocumentType, "Type  is  not correct");

            Assert.AreEqual(LobjKeywords.Count, LobjReturnDocument.Keywords.Count, "Keyword count is not correct");
            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void GetNonExistentDocumentTest()
        {
            string LstrDocId = "-1";
            OnBaseDocument LobjReturnDocument;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnDocument = LobjOnBaseDaGet.GetDocumentProperties(LstrDocId);
            }
            Assert.IsNull(LobjReturnDocument, "Document is  not NULL");
        }

        [TestMethod]
        public void GetKeywordsTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Claim Number", "123456TEST30");
            LobjKeywords.Add("Assigned ToID", "SU");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            NameValueCollection LobjReturnKeywords;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjReturnKeywords = LobjOnBaseDaGet.GetKeywords(LstrDocId);
            }

            Assert.AreEqual(LobjKeywords.Count, LobjReturnKeywords.Count, "Keyword count is not correct");

            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void GetAllDocumentTypesTest()
        {
            NameValueCollection LobjDocTypes;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocTypes = LobjOnBaseDaGet.GetDocumentTypes();
            }
            //Assume that if LobjDocTypes is not null that all doc types are returned.
            Assert.IsNotNull(LobjDocTypes, "No Document Types Returned");
        }

        [TestMethod]
        public void GetAllDocumentGroupsTest()
        {
            NameValueCollection LobjDocTypeGroups;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocTypeGroups = LobjOnBaseDaGet.GetDocumentTypeGroups();
            }
            //Assume that if LobjDocTypeGroups is not null that all doc types are returned.
            Assert.IsNotNull(LobjDocTypeGroups, "No Document Type Groups Returned");
        }

        [TestMethod]
        public void GetAllDocumentTypesForGroupTest()
        {
            NameValueCollection LobjDocTypes;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocTypes = LobjOnBaseDaGet.GetDocumentTypes(new string[] { "General Claims" });
            }
            //Assume that if LobjDocTypes is not null that all doc types are returned.
            Assert.IsNotNull(LobjDocTypes, "No Document Types Returned");
        }

        [TestMethod]
        public void GetAllDocumentTypesForGroupTest2()
        {
            NameValueCollection LobjDocTypes;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocTypes = LobjOnBaseDaGet.GetDocumentTypes(new string[] { "Actuarial & Statistical", "General Claims", "General Claims Reports" });
            }
            //Assume that if LobjDocTypes is not null that all doc types are returned.
            Assert.IsNotNull(LobjDocTypes, "No Document Types Returned");
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void GetAllDocumentTypesForInvalidGroupTest()
        {
            NameValueCollection LobjDocTypes;
            using (OnBaseDa LobjOnBaseDaGet = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocTypes = LobjOnBaseDaGet.GetDocumentTypes(new string[] { "Invalid Group" });
            }
        }

        [TestMethod]
        public void UpdateDocumentInvalidKeywordsTest()
        {
            NameValueCollection LobjKeywords = new NameValueCollection();

            LobjKeywords.Add("Legacy Object Id", "20052222XNXX");
            LobjKeywords.Add("Claim Number", "2006-123456");
            LobjKeywords.Add("Claim Number", "2006-123458");

            string LstrDocId = "";
            LstrDocId = InsertDocument("GC  Legal", LobjKeywords);

            LobjKeywords.Add("Invalid Keyword", "2006-123456");

            using (OnBaseDa LobjOnBaseDaUpdate = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaUpdate.UpdateDocument(LstrDocId, "GC  Legal", LobjKeywords);
            }
            DeleteDocument(LstrDocId);
        }

        [TestMethod]
        public void GetUsersTest()
        {
            List<OnBaseUser> LobjUsers = new List<OnBaseUser>();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjUsers = LobjOnBaseDa.GetUsers(1);
            }
            Assert.AreNotEqual("", LobjUsers);
        }

        [TestMethod]
        public void GetUsersFromDBTest()
        {
            List<OnBaseUser> LobjUsers = new List<OnBaseUser>();
            using (OnbaseDatabaseDa LobjOnBase = new OnbaseDatabaseDa(MstrDatabaseConnection))
            {
                LobjUsers = LobjOnBase.GetUsersForGroups(1);
            }
        }

        [TestMethod]
        public void GetUserGroupsTest()
        {
            List<OnBaseUserGroup> LobjUsers = new List<OnBaseUserGroup>();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjUsers = LobjOnBaseDa.GetUserGroups();
            }
        }

        [TestMethod]
        public void GetUserGroupsFromDBTest()
        {
            List<OnBaseUserGroup> LobjUsers = new List<OnBaseUserGroup>();
            using (OnbaseDatabaseDa LobjOnBase = new OnbaseDatabaseDa(MstrDatabaseConnection))
            {
                LobjUsers = LobjOnBase.GetUserGroups();
            }
        }

        [TestMethod]
        public void GetDocumentTypeNameByIDTest()
        {
            NameValueCollection LobjDocument = new NameValueCollection();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocument = LobjOnBaseDa.GetDocumentTypeNameById(1000);
                //GC  DRP Review - Estimates
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void GetDocumentTypeNameByIDExceptionTest()
        {
            NameValueCollection LobjDocument = new NameValueCollection();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocument = LobjOnBaseDa.GetDocumentTypeNameById(0);
            }
        }

        [TestMethod]
        public void GetDocumentTypeIDByNameTest()
        {
            NameValueCollection LobjDocument = new NameValueCollection();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocument = LobjOnBaseDa.GetDocumentTypeIdByName("GC  Appraisal");
                //1189
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ImagingServicesException))]
        public void GetDocumentTypeIDByNameExceptionTest()
        {
            NameValueCollection LobjDocument = new NameValueCollection();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocument = LobjOnBaseDa.GetDocumentTypeIdByName("Invalid");
                //GC  DRP Review - Estimates
            }
        }

        [TestMethod]
        public void GetUsersForGroupsTest()
        {
            List<OnBaseUser> LobjDocument = new List<OnBaseUser>();
            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjDocument = LobjOnBaseDa.GetUsersForGroups(new string[] { "MANAGER" });
            }
            Assert.AreNotEqual(0, LobjDocument.Count);
        }

        [TestMethod]
        public void GetUsersForGroupsFromDBTest()
        {
            List<OnBaseUser> LobjDocument = new List<OnBaseUser>();
            using (OnbaseDatabaseDa LobjOnBase = new OnbaseDatabaseDa(MstrDatabaseConnection))
            {
                LobjDocument = LobjOnBase.GetUsersForGroups("MANAGER");
            }
            Assert.AreNotEqual(0, LobjDocument.Count);
        }

        private string InsertDocument(string PstrDocType, NameValueCollection PobjKeywords)
        {
            System.IO.FileStream LobjStream = new System.IO.FileStream(@"C:\sampledoc.tif", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader LobjBinaryReader = new System.IO.BinaryReader(LobjStream);
            byte[] LabytData = LobjBinaryReader.ReadBytes((int)LobjStream.Length);
            LobjBinaryReader.Close();
            LobjStream.Close();

            using (OnBaseDa LobjOnBaseDa = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                string LstrDocId = LobjOnBaseDa.InsertDocument(LabytData, PstrDocType, FileFormat.ImageFileFormat, PobjKeywords, System.DateTime.Now, true);
                return LstrDocId;
            }
        }
        private void DeleteDocument(string PstrDocId)
        {
            using (OnBaseDa LobjOnBaseDaDelete = new OnBaseDa(MstrDataSource, MstrUserName, MstrPassword))
            {
                LobjOnBaseDaDelete.DeleteDocument(PstrDocId);
            }
        }
    }
}
