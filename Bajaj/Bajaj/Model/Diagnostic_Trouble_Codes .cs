using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Bajaj.Model
{
	[XmlRoot(ElementName = "Version_Control")]
	public class Version_Control
	{
		[XmlElement(ElementName = "Version")]
		public string Version { get; set; }
		[XmlElement(ElementName = "Date_Time")]
		public string Date_Time { get; set; }
		[XmlElement(ElementName = "Createdby")]
		public string Createdby { get; set; }
	}

	[XmlRoot(ElementName = "DTC_Info")]
	public class DTC_Info
	{
		[XmlElement(ElementName = "DTC_Mask")]
		public string DTC_Mask { get; set; }
		[XmlElement(ElementName = "DTC_Status_Active")]
		public string DTC_Status_Active { get; set; }
		[XmlElement(ElementName = "DTC_Status_History")]
		public string DTC_Status_History { get; set; }
	}

	[XmlRoot(ElementName = "DTC_Identifier")]
	public class DTC_Identifier
	{
		[XmlElement(ElementName = "DTC_Code")]
		public string DTC_Code { get; set; }
		[XmlElement(ElementName = "HEXCODE")]
		public string HEXCODE { get; set; }
		[XmlElement(ElementName = "BitPos")]
		public string BitPos { get; set; }
		[XmlElement(ElementName = "FaultName")]
		public string FaultName { get; set; }
		[XmlElement(ElementName = "Desc")]
		public string Desc { get; set; }
		[XmlElement(ElementName = "Remedy")]
		public string Remedy { get; set; }
	}

	[XmlRoot(ElementName = "Diagnostics_Trouble_Codes")]
	public class Diagnostics_Trouble_Codes
	{
		[XmlElement(ElementName = "Version_Control")]
		public Version_Control Version_Control { get; set; }
		[XmlElement(ElementName = "DTC_Info")]
		public DTC_Info DTC_Info { get; set; }
		[XmlElement(ElementName = "DTC_Identifier")]
		public List<DTC_Identifier> DTC_Identifier { get; set; }
	}



}
