﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://ws.njm.com/Imaging/1.0/ImagingServices", IsNullable = false)]
public partial class Document
{

   private DocumentKeyword[] keywordsField;

   private long documentTypeIDField;

   private long idField;

   private bool idFieldSpecified;

   private string typeField;

   private FileFormatType fileFormatField;

   private bool fileFormatFieldSpecified;

   public override string ToString()
   {
      string LstrReturn = "Document : " + System.Environment.NewLine;
      LstrReturn += "   Document Id " + idField.ToString() + System.Environment.NewLine;
      LstrReturn += "   Document Type Id " + documentTypeIDField.ToString() + System.Environment.NewLine;
      LstrReturn += "   Document Type " + typeField + System.Environment.NewLine;
      LstrReturn += "   File Format " + fileFormatField + System.Environment.NewLine;

      string LstrKeywords = "Keywords: " + System.Environment.NewLine;
      foreach (DocumentKeyword LobjKeyword in this.keywordsField)
      {
         LstrKeywords += LobjKeyword.ToString() + System.Environment.NewLine; 
      }
      LstrReturn += LstrKeywords;
      return LstrReturn; 
   }

   /// <remarks/>
   [System.Xml.Serialization.XmlArrayItemAttribute("Keyword", IsNullable = false)]
   public DocumentKeyword[] Keywords
   {
      get
      {
         return this.keywordsField;
      }
      set
      {
         this.keywordsField = value;
      }
   }

   /// <remarks/>
   public long DocumentTypeID
   {
      get
      {
         return this.documentTypeIDField;
      }
      set
      {
         this.documentTypeIDField = value;
      }
   }

   /// <remarks/>
   [System.Xml.Serialization.XmlAttributeAttribute()]
   public long ID
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
   [System.Xml.Serialization.XmlIgnoreAttribute()]
   public bool IDSpecified
   {
      get
      {
         return this.idFieldSpecified;
      }
      set
      {
         this.idFieldSpecified = value;
      }
   }

   /// <remarks/>
   [System.Xml.Serialization.XmlAttributeAttribute()]
   public string Type
   {
      get
      {
         return this.typeField;
      }
      set
      {
         this.typeField = value;
      }
   }

   /// <remarks/>
   [System.Xml.Serialization.XmlAttributeAttribute()]
   public FileFormatType FileFormat
   {
      get
      {
         return this.fileFormatField;
      }
      set
      {
         this.fileFormatField = value;
      }
   }

   /// <remarks/>
   [System.Xml.Serialization.XmlIgnoreAttribute()]
   public bool FileFormatSpecified
   {
      get
      {
         return this.fileFormatFieldSpecified;
      }
      set
      {
         this.fileFormatFieldSpecified = value;
      }
   }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class DocumentKeyword
{

   private string keyField;

   private string valueField;
   public override string ToString()
   {
      string LstrReturn = "Keyword : " + System.Environment.NewLine ;
      LstrReturn += "Key: " + keyField + " Value: " + valueField;
      return LstrReturn; 
   }

   /// <remarks/>
   public string Key
   {
      get
      {
         return this.keyField;
      }
      set
      {
         this.keyField = value;
      }
   }

   /// <remarks/>
   public string Value
   {
      get
      {
         return this.valueField;
      }
      set
      {
         this.valueField = value;
      }
   }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://ws.njm.com/Imaging/1.0/ImagingServices")]
public enum FileFormatType
{

   /// <remarks/>
   TextReportFormat,

   /// <remarks/>
   ImageFileFormat,

   /// <remarks/>
   ExportTransferDocument,

   /// <remarks/>
   DataMiningFormat,

   /// <remarks/>
   PCLDataStream,

   /// <remarks/>
   EmtexAFPMetacode,

   /// <remarks/>
   MSWordDocument,

   /// <remarks/>
   MSExcelSpreadsheet,

   /// <remarks/>
   MSPowerPoint,

   /// <remarks/>
   RichTextFormat,

   /// <remarks/>
   PDF,

   /// <remarks/>
   HTML,

   /// <remarks/>
   AVIMovie,

   /// <remarks/>
   QuickTimeMovie,

   /// <remarks/>
   WAVAudioFile,

   /// <remarks/>
   PCLFilter,

   /// <remarks/>
   RedactedImage,

   /// <remarks/>
   HitHighlights,

   /// <remarks/>
   ElectronicForm,

   /// <remarks/>
   ApproveITElectronicSignature,

   /// <remarks/>
   VirtualElectronicForm,

   /// <remarks/>
   DynamicDocument,

   /// <remarks/>
   AFPDocument,

   /// <remarks/>
   PCLDictionaryImport,

   /// <remarks/>
   DJDE,

   /// <remarks/>
   XML,

   /// <remarks/>
   PCLFullsize,

   /// <remarks/>
   VerityHitHighlights,

   /// <remarks/>
   MSOutlookMessage,

   /// <remarks/>
   PCLSelfContained,

   /// <remarks/>
   PCLOriginalImplementation,

   /// <remarks/>
   PhysicalRecord,

   /// <remarks/>
   LotusNotesDocument,

   /// <remarks/>
   InternalXML,

   /// <remarks/>
   PendingVerity,

   /// <remarks/>
   HTMLUnicode,

   /// <remarks/>
   X1,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://ws.njm.com/Imaging/1.0/ImagingServices", IsNullable = false)]
public partial class DocumentTypes
{

   private DocumentType[] documentTypeField;

   public override string ToString()
   {
      string LstrReturn = "DocumentTypes : " + System.Environment.NewLine;
      foreach (DocumentType LobjDocumentType in documentTypeField)
      {
         LstrReturn += LobjDocumentType.ToString() + System.Environment.NewLine ;
      }
      return LstrReturn; 
   }

   /// <remarks/>
   [System.Xml.Serialization.XmlElementAttribute("DocumentType")]
   public DocumentType[] DocumentType
   {
      get
      {
         return this.documentTypeField;
      }
      set
      {
         this.documentTypeField = value;
      }
   }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class DocumentType
{

   private string idField;

   private string valueField;

   public override string ToString()
   {
      string LstrReturn = "DocumentType : " + System.Environment.NewLine;
      LstrReturn += "ID: " + idField + " Value: " + valueField;
      return LstrReturn; 
   }


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
   public string Value
   {
      get
      {
         return this.valueField;
      }
      set
      {
         this.valueField = value;
      }
   }
}
