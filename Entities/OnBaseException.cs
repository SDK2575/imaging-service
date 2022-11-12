using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseImaging.ImagingServices.Entities
{
   public class ImagingServicesException : Exception 
   {
      public ImagingServicesException(string PstrMessage)
         : base(PstrMessage)
      { }
   }

   public class CoreAPIException : Exception
   {
      public CoreAPIException(string PstrMessage)
         : base(PstrMessage)
      { }
   

    public CoreAPIException(string PstrMessage, Exception PobjInnerException)
         : base(PstrMessage, PobjInnerException)
      {
      }
  }
}
