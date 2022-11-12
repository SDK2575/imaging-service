using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for User
/// </summary>
public class Users
   {

      private User[] userTypeField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute("User")]
      public User[] User
      {
         get
         {
            return this.userTypeField;
         }
         set
         {
            this.userTypeField = value;
         }
      }
   }


public class User
   {

      private string idField;

      private int onbaseIdField;

      private string displayNameField;

      private string groupNameField;
      /// <remarks/>
      public string ID
      {
         get
         {
            return this.idField;
         }
         set
         {
            this.idField = value;
         }
      }

      /// <remarks/>
      public string DisplayName
      {
         get
         {
            return this.displayNameField;
         }
         set
         {
            this.displayNameField = value;
         }
      }

      /// <remarks/>
      public string GroupName
      {
         get
         {
            return this.groupNameField;
         }
         set
         {
            this.groupNameField = value;
         }
      }

      /// <remarks/>
      public int OnbaseId
      {
         get
         {
            return this.onbaseIdField;
         }
         set
         {
            this.onbaseIdField = value;
         }
      }


   }


