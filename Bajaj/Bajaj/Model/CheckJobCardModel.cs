namespace Bajaj.Model
{
    public class CheckJobCardModel
    {
        public D d { get; set; }
    }

    public class Metadata
    {
        public string id { get; set; }
        public string uri { get; set; }
        public string type { get; set; }
    }

    public class Deferred
    {
        public string uri { get; set; }
    }

    public class NPHEADERJOB
    {
        public Deferred __deferred { get; set; }
    }

    public class D
    {
        public Metadata __metadata { get; set; }
        public string BodyType { get; set; }
        public string Jobnr { get; set; }
        public string PrdSegemnt { get; set; }
        public string VehGrp { get; set; }
        public string StearGr { get; set; }
        public string VehModDes { get; set; }
        public string DelName { get; set; }
        public string FrntAxle { get; set; }
        public string DeCode { get; set; }
        public string RearTyreNoInnerLhs2 { get; set; }
        public string CustName { get; set; }
        public string RearTyreNoOuterLhs2 { get; set; }
        public string Loc { get; set; }
        public string RearTyreNoInnerRhs2 { get; set; }
        public string RearTyreNoOuterRhs2 { get; set; }
        public string VehMod { get; set; }
        public string FrontTyreNoLhs2 { get; set; }
        public string VehTyp { get; set; }
        public string DelLoc { get; set; }
        public string FrontTyreNoRhs2 { get; set; }
        public string GearBox { get; set; }
        public string Vhvin { get; set; }
        public string EngNo { get; set; }
        public string TurboChrg { get; set; }
        public string InstDate { get; set; }
        public string RearAxle { get; set; }
        public string BsNorm { get; set; }
        public string RegNo { get; set; }
        public string VehStatus { get; set; }
        public string JobCrd { get; set; }
        public string Permit { get; set; }
        public string FailDate { get; set; }
        public string KmsCover { get; set; }
        public string EngHr { get; set; }
        public string DrvName { get; set; }
        public string ContNo { get; set; }
        public string PreStat { get; set; }
        public string VehRoad { get; set; }
        public string TechName { get; set; }
        public string MechNo { get; set; }
        public NPHEADERJOB NP_HEADER_JOB { get; set; }
    }

    public class JobcardModelSecond
    {
        public string AssignedTo { get; set; }
        public string DealerOrServiceEnggContactNumber { get; set; }
        public string OpenCloseStatus { get; set; }
        public string TicketIdAlias { get; set; }
        public string BreakdownLocation { get; set; }
        public string VehicleRegisterNumber { get; set; }
        public string CustomerContactNo { get; set; }
        public string Dealerdealer_name { get; set; }
        public string DealerContactNumber1 { get; set; }
        public string ChassisNo { get; set; }
        public double KmCovered { get; set; }
        public string CustomerCustomerName { get; set; }
        public string ModelNumber { get; set; }
        public string ProductVarient { get; set; }
        public string TripStart { get; set; }
        public string TripEnd { get; set; }
        public string DealerCode { get; set; }
    }
}
