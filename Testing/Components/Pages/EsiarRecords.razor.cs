using System.Data.Common;
using Microsoft.AspNetCore.Components;
using Testing.Components.Data;
using Testing.Components.Model;

namespace Testing.Components.Pages;

public partial class EsiarRecords
{
    [Inject] private EsiarService EsiarService { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;

    private bool isLoading = true;
    private string? errorMessage = null;
    private List<EsiarDetail> records = new();
    protected int totalRecords = 0;
    private EsiarDetail? selectedRecord = null;

    private string? pdfUrl = null;

    // PDF viewer state
    private bool pdfLoadError = false;
    private bool pdfIsLoading = false;

    // Filter & pagination
    protected string selectedDept = "";
    protected string selectedJenisDoc = "";
    protected string keyTitle = "";
    protected string keyDesc = "";
    protected DateTime? dateFrom = null;
    protected DateTime? dateTo = null;
    protected int currentPage = 1;
    protected int pageSize = 15;
    protected int draw = 1;

    //dropdown data
    protected List<SelectOption> deptList = new();
    protected List<SelectOption> docTypeList = new();

    //pdf viewer
    protected EsiarDetail? selectRecord = null;


    protected override async Task OnInitializedAsync()
    {
        await LoadDropdowns();
        await LoadRecords();
    }

    #region LoadDropDown data

    /// Memuatkan senarai jabatan (Dept) dan jenis dokumen (DocType) dari webservice GetParameter.
    /// Jika gagal, guna fallback hardcode dari dokumen.
    protected async Task LoadDropdowns()
    {
        try
        {
            var deptRequest = new GetParameterRequest
            {
                TableName = "TBL_DEPT",
                ColumnID = "DEPT",
                ColumnName = "NAME"
            };
            var deptResponse = await EsiarService.GetParameterAsync(deptRequest);

            if (deptResponse != null && deptResponse.status == "1" && deptResponse.Details.Any())
            {
                deptList = deptResponse.Details.Select(d => new SelectOption
                {
                    Value = d.ParameterID,
                    Text = d.ParameterName
                }).ToList();
            }
            else
            {
                deptList = new List<SelectOption>
            {
                new() { Value = "", Text = "-- SEMUA --" },
                new() { Value = "APKP", Text = "ARAHAN PERTUKARAN PESURUHJAYA POLIS NEGERI/KETUA POLIS NEGERI" },
                new() { Value = "CK", Text = "CAWANGAN KHAS" },
                new() { Value = "JIPS", Text = "JABATAN INTEGRITI DAN PEMATUHAN STANDARD" },
                new() { Value = "JL", Text = "JABATAN LOGISTIK" },
                new() { Value = "JP", Text = "JABATAN PENGURUSAN" },
                new() { Value = "JSJ", Text = "JABATAN SIASATAN JENAYAH" },
                new() { Value = "JSJK", Text = "JABATAN SIASATAN JENAYAH KOMERSIL" },
                new() { Value = "JSJN", Text = "JABATAN SIASATAN JENAYAH NARKOTIK" },
                new() { Value = "KDN/KA", Text = "KESELAMATAN DALAM NEGERI/KETENTERAMAN AWAM" }
            };
            }

            var docTypeRequest = new GetParameterRequest
            {
                TableName = "Tbl_DocType",
                ColumnID = "TypeID",
                ColumnName = "Type"
            };
            var docTypeResponse = await EsiarService.GetParameterAsync(docTypeRequest);

            if (docTypeResponse != null && docTypeResponse.status == "1" && docTypeResponse.Details.Any())
            {
                docTypeList = docTypeResponse.Details.Select(d => new SelectOption
                {
                    Value = d.ParameterID,
                    Text = d.ParameterName
                }).ToList();
            }
            else
            {
                docTypeList = new List<SelectOption>
            {
                new() { Value = "", Text = "-- SEMUA --" },
                new() { Value = "5", Text = "Arahan Pentadbiran" },
                new() { Value = "11", Text = "Lain-Lain Arahan & Perintah" }
            };
            }

            if (!deptList.Any(x => x.Value == ""))
                deptList.Insert(0, new SelectOption { Value = "", Text = "-- SEMUA --" });
            if (!docTypeList.Any(x => x.Value == ""))
                docTypeList.Insert(0, new SelectOption { Value = "", Text = "-- SEMUA --" });
        }
        catch (Exception ex)
        {
            // Jika sebarang exception (network, json, dll), guna fallback sepenuhnya
            errorMessage = $"Gagal memuatkan senarai dari service: {ex.Message}. Menggunakan data setempat.";

            deptList = new List<SelectOption>
        {
            new() { Value = "", Text = "-- SEMUA --" },
            new() { Value = "APKP", Text = "APKP" },
            new() { Value = "CK", Text = "CK" },
            new() { Value = "JIPS", Text = "JIPS" },
            new() { Value = "JL", Text = "JL" },
            new() { Value = "JP", Text = "JP" },
            new() { Value = "JSJ", Text = "JSJ" },
            new() { Value = "JSJK", Text = "JSJK" },
            new() { Value = "JSJN", Text = "JSJN" },
            new() { Value = "KDN/KA", Text = "KDN/KA" }
        };

            docTypeList = new List<SelectOption>
        {
            new() { Value = "", Text = "-- SEMUA --" },
            new() { Value = "5", Text = "Arahan Pentadbiran" },
            new() { Value = "11", Text = "Lain-Lain Arahan" }
        };
        }
    }

    #endregion

    #region LoadRecords

    private async Task LoadRecords()
    {
        isLoading = true;
        errorMessage = null;
        try
        {
            // Kira start berdasarkan currentPage dan pageSize
            int start = (currentPage - 1) * pageSize;
            draw++; // increment setiap kali request baru

            var request = new EsiarRequest
            {
                token = "068ce1d5-e27e-4ad1-8336-7a5f4c3257d0",
                start = start,
                length = pageSize,
                draw = draw,
                PubDept = "AT",
                SortBy = "FileDate",
                Dept = selectedDept ?? "",
                JenisDoc = selectedJenisDoc ?? "",
                KeyTitle = keyTitle ?? "",
                KeyDesc = keyDesc ?? "",
                DatePublishFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "",
                DatePublishTo = dateTo?.ToString("yyyy-MM-dd") ?? ""
            };

            var response = await EsiarService.GetTopRecordsAsync(request);

            if (response != null && response.status == "1")
            {
                records = response.Details ?? new List<EsiarDetail>();
                totalRecords = response.recordsTotal;
            }
            else
            {
                errorMessage = response?.statusmessage ?? "Tiada rekod ditemui.";
                records = new List<EsiarDetail>();
                totalRecords = 0;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to retrieve e-SIAR records: {ex.Message}";
            records = new List<EsiarDetail>();
            totalRecords = 0;
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region search Record
    protected async Task SearchRecords()
    {
        currentPage = 1;
        draw = 1;
        await LoadRecords();
    }
    #endregion

    #region pagination 
    protected async Task PrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            await LoadRecords();
        }
    }

    protected async Task NextPage()
    {
        if (currentPage * pageSize < totalRecords)
        {
            currentPage++;
            await LoadRecords();
        }
    }

    #endregion


    private async Task ViewDocument(EsiarDetail record)
    {
        selectedRecord = record;
        pdfLoadError = false;
        pdfIsLoading = true;
        pdfUrl = null;
        StateHasChanged();

        try
        {
            var relativeUrl = GetPdfUrl(record.FilePath);
            var absoluteUrl = NavManager.ToAbsoluteUri(relativeUrl).ToString();

            var response = await EsiarService.CheckPdfExistsAsync(absoluteUrl);
            if (response)
            {
                pdfUrl = relativeUrl;
                pdfLoadError = false;
            }
            else
            {
                pdfLoadError = true;
            }
        }
        catch
        {
            pdfLoadError = true;
        }
        finally
        {
            pdfIsLoading = false;
            StateHasChanged();
        }
    }

    private void OnIframeLoad()
    {
        pdfIsLoading = false;
        StateHasChanged();
    }

    private void OnIframeError()
    {
        pdfIsLoading = false;
        pdfLoadError = true;
        StateHasChanged();
    }

    private async Task RetryLoadPdf()
    {
        if (selectedRecord != null)
            await ViewDocument(selectedRecord);
    }

    private void CloseViewer()
    {
        selectedRecord = null;
        pdfUrl = null;
        pdfLoadError = false;
        pdfIsLoading = false;
    }

    private string GetPdfUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return "";

        // Encode filePath supaya special characters (space, dll) tidak break URL
        var encodedPath = System.Net.WebUtility.UrlEncode(filePath);
        return $"/api/pdf/viewpdf?path={encodedPath}";
    }

    #region Helper class untuk dropdown
    public class SelectOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    // Response dari GetParameter
    public class EsiarParameterResponse
    {
        public string status { get; set; } = "";
        public string statusmessage { get; set; } = "";
        public List<EsiarParameterDetail> Details { get; set; } = new();
    }

    public class EsiarParameterDetail
    {
        public string ParameterID { get; set; } = "";
        public string ParameterName { get; set; } = "";
    }
    #endregion
}
