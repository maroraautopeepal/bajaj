using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

namespace Bajaj.Model
{
	[XmlRoot(ElementName = "DataIdentifier")]
	public class DataIdentifier
	{
		[XmlElement(ElementName = "Did__hex")]
		public string Did__hex { get; set; }
		[XmlElement(ElementName = "Conv_Rule")]
		public string Conv_Rule { get; set; }
		[XmlElement(ElementName = "size_Bits")]
		public string Size_Bits { get; set; }
		[XmlElement(ElementName = "Desc")]
		public string Desc { get; set; }
		[XmlElement(ElementName = "Unit")]
		public string Unit { get; set; }
		[XmlElement(ElementName = "scaling")]
		public string Scaling { get; set; }
		[XmlElement(ElementName = "Offset")]
		public string Offset { get; set; }
		[XmlElement(ElementName = "Access_Pvg")]
		public string Access_Pvg { get; set; }
		[XmlElement(ElementName = "Group_ID")]
		public string Group_ID { get; set; }
	}


	[XmlRoot(ElementName = "UDS_Diag_Measurements")]
	public class UDS_Diag_Measurements
	{
		[XmlElement(ElementName = "Version_Control")]
		public Version_Control Version_Control { get; set; }
		[XmlElement(ElementName = "Basic_ECU_Information")]
		public Basic_ECU_Information Basic_ECU_Information { get; set; }
		[XmlElement(ElementName = "DataIdentifier")]
		public List<DataIdentifier> DataIdentifier { get; set; }
	}

	[XmlRoot(ElementName = "Basic_ECU_Information")]
	public class Basic_ECU_Information
	{
		[XmlElement(ElementName = "Header")]
		public string Header { get; set; }
		[XmlElement(ElementName = "ECU_Name")]
		public string ECU_Name { get; set; }
		[XmlElement(ElementName = "RX_ID")]
		public string RX_ID { get; set; }
		[XmlElement(ElementName = "TX_ID")]
		public string TX_ID { get; set; }
		[XmlElement(ElementName = "BaudRate")]
		public string BaudRate { get; set; }
		[XmlElement(ElementName = "Security_Constant_L3")]
		public string Security_Constant_L3 { get; set; }
		[XmlElement(ElementName = "Security_Constant_L5")]
		public string Security_Constant_L5 { get; set; }
		[XmlElement(ElementName = "Protocol")]
		public string Protocol { get; set; }
		public ProtocolInfo protocolInfo;
	}


	[XmlRoot(ElementName = "Routine")]
	public class Routin
	{
		[XmlElement(ElementName = "Routine_ID")]
		public int Routine_ID { get; set; }
		[XmlElement(ElementName = "Routine_Name")]
		public string Routine_Name { get; set; }
		[XmlElement(ElementName = "Routine_Start")]
		public string Routine_Start { get; set; }
		[XmlElement(ElementName = "Routine_Stop")]
		public string Routine_Stop { get; set; }
		[XmlElement(ElementName = "Routine_Result")]
		public string Routine_Result { get; set; }
		[XmlElement(ElementName = "Routine")]
		public List<string> Routine { get; set; }
	}

	[XmlRoot(ElementName = "IOControl")]
	public class IOControl
	{
		[XmlElement(ElementName = "IO_ID")]
		public int IO_ID { get; set; }
		[XmlElement(ElementName = "IO_Name")]
		public string IO_Name { get; set; }
	}

	public class ProtocolInfo
    {
		public string protocol { get; set; }
		public string value { get; set; }
    }

}
