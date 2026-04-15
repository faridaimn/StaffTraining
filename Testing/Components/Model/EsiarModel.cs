using System.Text.Json.Serialization;

namespace Testing.Components.Model;

public class EsiarRequest
{
    public string token { get; set; } = "068ce1d5-e27e-4ad1-8336-7a5f4c3257d0";
    public int start { get; set; } = 0;
    public int length { get; set; } = 15;
    public int draw { get; set; } = 1;
    public string PubDept { get; set; } = "AT";
    public string SortBy { get; set; } = "FileDate";
    public string Dept { get; set; } = "";
    public string JenisDoc { get; set; } = "";
    public string KeyTitle { get; set; } = "";
    public string KeyDesc { get; set; } = "";
    public string DatePublishFrom { get; set; } = "2022-01-01";
    public string DatePublishTo { get; set; } = "2026-03-16";
}

public class EsiarResponse
{
    public List<EsiarDetail> Details { get; set; } = new();
    public int draw { get; set; }
    public int recordsFiltered { get; set; }
    public int recordsTotal { get; set; }
    public string status { get; set; } = "";
    public string statusmessage { get; set; } = "";
}

public class EsiarDetail
{
    public string DocID { get; set; } = "";
    public string Title { get; set; } = "";
    public string Desc { get; set; } = "";
    public string DeptName { get; set; } = "";
    public string PublishDate { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string Filename { get; set; } = "";
}

public class GetParameterRequest
{
    public string TableName { get; set; } = "";
    public string ColumnID { get; set; } = "";
    public string ColumnName { get; set; } = "";
}

public class GetParameterResponse
{
    public List<GetParameterDetail> Details { get; set; } = new();
    public string status { get; set; } = "";
    public string statusmessage { get; set; } = "";
}

public class GetParameterDetail
{
    public string ParameterID { get; set; } = "";
    public string ParameterName { get; set; } = "";
}