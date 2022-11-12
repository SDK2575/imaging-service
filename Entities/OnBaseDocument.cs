using System;
using System.Collections.Specialized; 
using System.Collections.Generic;
using System.Text;

namespace EnterpriseImaging.ImagingServices.Entities
{
   public class OnBaseDocument
   {
      public OnBaseDocument()
      {
      }

      long MlCreatedUserID;
      public long CreatedUserID
      {
         get { return MlCreatedUserID; }
         set { MlCreatedUserID = value; }
      }

      string MstrName;
      public string Name
      {
         get { return MstrName; }
         set { MstrName = value; }
      }

      string MstrDocumentDate;
      public string DocumentDate
      {
         get { return MstrDocumentDate; }
         set { MstrDocumentDate = value; }
      }


      long MlDocumentTypeID;
      public long DocumentTypeID
      {
         get { return MlDocumentTypeID; }
         set { MlDocumentTypeID = value; }
      }


      long MlDocumentId;
      public long DocumentId
      {
         get { return MlDocumentId; }
         set { MlDocumentId = value; }
      }


      FileFormat MiFileFormat;
      public FileFormat FileFormat
      {
         get { return MiFileFormat; }
         set { MiFileFormat = value; }
      }


      string MstrDocumentType;
      public string DocumentType
      {
         get { return MstrDocumentType; }
         set { MstrDocumentType = value; }
      }

      NameValueCollection MobjKeywords;
      public NameValueCollection Keywords
      {
         get { return MobjKeywords; }
         set { MobjKeywords = value; }
      }

      public override string ToString()
      {
         System.Text.StringBuilder LobjStringBuilder = new System.Text.StringBuilder("Onbase Document: " + Environment.NewLine);
         LobjStringBuilder.Append("DocumentDate: " + this.DocumentDate + Environment.NewLine);
         LobjStringBuilder.Append("DocumentId: " + this.DocumentId.ToString() + Environment.NewLine);
         LobjStringBuilder.Append("DocumentType: " + this.DocumentType + Environment.NewLine);
         LobjStringBuilder.Append("DocumentTypeID: " + this.DocumentTypeID + Environment.NewLine);
         LobjStringBuilder.Append("FileFormat: " + this.FileFormat.ToString() + Environment.NewLine);
         LobjStringBuilder.Append("Keywords: " + Environment.NewLine);
         foreach ( string LobjKey in this.Keywords.AllKeys)
         {
            LobjStringBuilder.Append(LobjKey + ":" + Environment.NewLine);
            foreach (string LobjValue in this.Keywords.GetValues(LobjKey))
            {
               LobjStringBuilder.Append("            " + LobjValue + ":" + Environment.NewLine);
            }
         }

         return LobjStringBuilder.ToString();
      }
   }

   // could be user or user group

   /// <summary>
   /// Onbase User/Group information
   /// </summary>
   public class OnBaseEntity 
   {
      int MiEntityId ;
      string MstrEntityName;


      public int ID
      {
         get {return MiEntityId; }
         set {MiEntityId = value; }
      }

      public string Name
      {
         get {return MstrEntityName; }
         set {MstrEntityName = value ;}
      }


      public OnBaseEntity(int PiEntityId, string PstrEntityName )
      {
         MiEntityId = PiEntityId ;
         MstrEntityName = PstrEntityName ;
         //MstrEntityDisplayName = PstrEntityDisplayName ;


      }

      public override bool Equals(object PobjToCompare )
      {

         OnBaseEntity LobjToCompare = PobjToCompare as OnBaseEntity;
         if (LobjToCompare == null)
         {
            throw new ArgumentException("Argument is of Invalid Type"); 
         }
         return this.ID.Equals( LobjToCompare.ID );
      }

      public override int GetHashCode()
      {
         return this.ID.GetHashCode();
      }

      public override string ToString()
      {
         System.Text.StringBuilder LobjStringBuilder = new System.Text.StringBuilder("Onbase Entity: " + Environment.NewLine);
         LobjStringBuilder.Append("ID: " + this.ID + Environment.NewLine);
         LobjStringBuilder.Append("Name: " + this.Name + Environment.NewLine);
         return LobjStringBuilder.ToString();
      }


   }

   /// <summary>
   /// Onbase User
   /// </summary>
   public class OnBaseUser : OnBaseEntity, IComparable<OnBaseUser>
   { 
      public OnBaseUser ( int PiEntityId, string PstrEntityName, string PstrEntityDisplayName ,  string PstrGroupName ) 
         : base ( PiEntityId, PstrEntityName  )
      {
         MstrGroupName = PstrGroupName;
         MstrEntityDisplayName = PstrEntityDisplayName; 
      }

      string MstrGroupName;

      string MstrEntityDisplayName;

      public string GroupName
      {
         get { return MstrGroupName; }
         set { MstrGroupName = value; }
      }

      public string DisplayName
      {
         get { return MstrEntityDisplayName; }
         set { MstrEntityDisplayName = value; }
      }

      public int CompareTo (OnBaseUser PobjToCompare)
      {
         return this.DisplayName.CompareTo(PobjToCompare.DisplayName );
      }

      public bool Equals(OnBaseUser PobjToCompare)
      {

         OnBaseUser LobjToCompare = PobjToCompare as OnBaseUser;
         if (LobjToCompare == null)
         {
            throw new ArgumentException("Argument is of Invalid Type");
         }
         return this.ID.Equals(LobjToCompare.DisplayName);
      }

      public override string ToString()
      {
         System.Text.StringBuilder LobjStringBuilder = new System.Text.StringBuilder("Onbase User: " + Environment.NewLine);
         LobjStringBuilder.Append("ID: " + this.ID + Environment.NewLine);
         LobjStringBuilder.Append("Name: " + this.Name + Environment.NewLine);
         LobjStringBuilder.Append("DisplayName: " + this.DisplayName + Environment.NewLine);
         LobjStringBuilder.Append("GroupName: " + this.GroupName + Environment.NewLine);

         return LobjStringBuilder.ToString();
      }

   }

   /// <summary>
   /// Onbase User Group
   /// </summary>
   public class OnBaseUserGroup : OnBaseEntity
   {
      public OnBaseUserGroup(int PiEntityId, string PstrEntityName )
         : base(PiEntityId, PstrEntityName )
      {}


   }
}
