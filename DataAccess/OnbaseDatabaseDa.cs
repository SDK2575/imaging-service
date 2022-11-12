using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseImaging.ImagingServices.Entities;
using System.Data.SqlClient; 

namespace EnterpriseImaging.ImagingServices.DataAccess
{
//   select username, realname, usergroupname  from  hsi.userxusergroup ugx inner join
//hsi.useraccount u on ugx.usernum = u.usernum
//inner join hsi.usergroup ug on ug.usergroupnum = ugx.usergroupnum
   public class OnbaseDatabaseDa : IDisposable 
   {
      /// <summary>
      /// direct access to Onbase database
      /// implemented due to the fact that Onbase APIs are very slow inn Users/UsersGroup processing
      /// </summary>

      SqlConnection MobjConnection = new SqlConnection() ;
      SqlDataReader MobjReader  ; 


      /// <summary>
      /// Constructor
      /// establishes db connection
      /// </summary>
      /// <param name="LstrConnectionString"></param>
      public OnbaseDatabaseDa(string LstrConnectionString)
      {
         MobjConnection.ConnectionString = LstrConnectionString;
         MobjConnection.Open(); 
      }



      /// <summary>
      /// get user for group by group name
      /// </summary>
      /// <param name="PstrGroup"></param>
      /// <returns></returns>
      public List<OnBaseUser> GetUsersForGroups(string PstrGroup )
      {
         List<OnBaseUser> LobjUsers = new List<OnBaseUser>();

         string LstrQuery = string.Format(@"select u.usernum, u.username, u.realname, ug.usergroupname  from  hsi.userxusergroup ugx 
                              inner join hsi.useraccount u on ugx.usernum = u.usernum
                              inner join hsi.usergroup ug  on ug.usergroupnum = ugx.usergroupnum  where ug.usergroupname ='{0}' ", PstrGroup );

         GetData( LstrQuery );

         while (MobjReader.Read())
         {
            OnBaseUser LobjUser = new OnBaseUser((int)MobjReader["usernum"], 
                                                   MobjReader["username"].ToString(), 
                                                   ( MobjReader["realname"].ToString().Trim() == "")? MobjReader["username"].ToString(): MobjReader["realname"].ToString().Trim(), 
                                                   MobjReader["usergroupname"].ToString());
            LobjUsers.Add(LobjUser); 
         }
         LobjUsers.Sort(); 
         return LobjUsers; 
      }

      /// <summary>
      /// get users for group by group id
      /// </summary>
      /// <param name="PiGroupId"></param>
      /// <returns></returns>
      public List<OnBaseUser> GetUsersForGroups(int PiGroupId)
      {
         List<OnBaseUser> LobjUsers = new List<OnBaseUser>();

         string LstrQuery = string.Format(@"select u.usernum, u.username, u.realname, ug.usergroupname  from  hsi.userxusergroup ugx 
                              inner join hsi.useraccount u on ugx.usernum = u.usernum
                              inner join hsi.usergroup ug  on ug.usergroupnum = ugx.usergroupnum  where ug.usergroupnum ={0} ", PiGroupId);

         GetData(LstrQuery);

         while (MobjReader.Read())
         {
            OnBaseUser LobjUser = new OnBaseUser((int)MobjReader["usernum"], 
                                                   MobjReader["username"].ToString(), 
                                                   ( MobjReader["realname"].ToString().Trim() == "")? MobjReader["username"].ToString(): MobjReader["realname"].ToString().Trim(), 
                                                   MobjReader["usergroupname"].ToString());
            LobjUsers.Add(LobjUser);
         }
         LobjUsers.Sort(); 
         return LobjUsers;
      }

      /// <summary>
      /// get all user groups
      /// </summary>
      /// <returns></returns>
      public List<OnBaseUserGroup> GetUserGroups()
      {
         List<OnBaseUserGroup> LobjGroups = new List<OnBaseUserGroup>();

         string LstrQuery = @"select usergroupnum, usergroupname  from  hsi.usergroup";
         GetData(LstrQuery);

         while (MobjReader.Read())
         {
            OnBaseUserGroup LobjGroup = new OnBaseUserGroup((int)MobjReader["usergroupnum"], MobjReader["usergroupname"].ToString() );
            LobjGroups.Add(LobjGroup);
         }

         return LobjGroups; 
      }


      /// <summary>
      /// reads data from DB
      /// </summary>
      /// <param name="PstrQuery"></param>
      private void GetData(string PstrQuery)
      {
         SqlCommand LobjCommand = new SqlCommand(PstrQuery, MobjConnection);
         LobjCommand.CommandType = System.Data.CommandType.Text;
         MobjReader = LobjCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection); 

      }

      /// <summary>
      /// disposing connection
      /// </summary>
      public void Dispose ()
      {
         MobjReader.Close();
         if ( MobjConnection.State == System.Data.ConnectionState.Open )   MobjConnection.Close(); 
      }


   }
}
