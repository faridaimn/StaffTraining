using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Testing.Components.Model;

namespace Testing.Components.Pages
{
    public partial class User
    {
        [Parameter]
        [SupplyParameterFromQuery(Name = "ic")]
        public string? Ic { get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "tab")]
        public string? Tab { get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "source")]
        public string? Source { get; set; }


        private List<EmployeeProfile>? EmployeesList;
        private List<Vehicle> vehicleList = new();
        private List<String> filteredModels = new();
        private List<string> filteredBrands = new();
        private List<string> allBrands = new()
    {
        "Perodua", "Proton", "Toyota", "Mazda", "Honda"
    };

        private EmployeeProfile editingEmployee = new();
        private Vehicle editingVehicle = new();
        private byte[]? pendingPortraitBytes;
        private string? pendingPortraitExtension;
        private byte[]? pendingDocumentBytes;
        private string? pendingDocumentExtension;

        private string activeTab = "user";

        private bool isFormVisible = false;
        private bool isEditing = false;
        private bool isViewing = false;
        private bool showDropdown = false;
        private bool onUpdate = false;

        private const string Reading = "Reading";
        private const string Hiking = "Hiking";
        private string displayPortrait => portraitPreviewUrl ?? editingEmployee.PersonPortraitPath ?? "";
        private List<(int Id, string code, string name)> NationalityList = new();
        private string? selectedVehicleNo = null;

        // Search filters
        private string searchName = "";
        private string searchIC = "";
        private string? vehicleNoDuplicateError;
        private string? ErrorMessage = null;
        private string? portraitPreviewUrl;

        //Dictionary car model based on model
        private Dictionary<String, List<string>> brandModels = new()
    {
        { "Perodua",        new() { "Bezza", "Myvi" } },
        { "Proton",         new() { "X50", "X70" } },
        { "Toyota",         new() { "Vios", "Hilux" } },
        { "Mazda",          new() { "Mazda 3", "CX-5" } },
        { "Honda",          new() { "Civic", "CR-V" } },
    };

        #region employee pagination and sorting
        private string empSortColumn = "PersonName";
        private bool empSortAscending = true;
        private int empCurrentPage = 1;
        private int empPageSize = 5;

        private int empTotalFilteredCount => EmployeesList?.Count ?? 0;
        private int empTotalPages => (int)Math.Ceiling((double)empTotalFilteredCount / empPageSize);

        private List<EmployeeProfile> pagedEmployees
        {
            get
            {
                if (EmployeesList == null) return new List<EmployeeProfile>();

                var sorted = empSortColumn switch
                {
                    "IC_No" => empSortAscending
                        ? EmployeesList.OrderBy(e => e.IC_No).ToList()
                        : EmployeesList.OrderByDescending(e => e.IC_No).ToList(),
                    "PersonName" => empSortAscending
                        ? EmployeesList.OrderBy(e => e.PersonName).ToList()
                        : EmployeesList.OrderByDescending(e => e.PersonName).ToList(),
                    "Description" => empSortAscending
                        ? EmployeesList.OrderBy(e => e.DescriptionOfPerson).ToList()
                        : EmployeesList.OrderByDescending(e => e.DescriptionOfPerson).ToList(),
                    _ => EmployeesList
                };

                return sorted
                    .Skip((empCurrentPage - 1) * empPageSize)
                    .Take(empPageSize)
                    .ToList();
            }
        }

        private void SortEmpBy(string column)
        {
            if (empSortColumn == column)
            {
                empSortAscending = !empSortAscending;
            }
            else
            {
                empSortColumn = column;
                empSortAscending = true;
            }
        }

        private void ChangePageSize(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int newSize))
            {
                empPageSize = newSize;
                empCurrentPage = 1;
            }
        }
        #endregion

        #region table vehicle variable

        #region pagination variable

        private int currentPage = 1;
        private int pageSize = 5;
        private int totalFilteredCount => vehicleList
            .Where(v => (string.IsNullOrWhiteSpace(filterVehicleNo) || v.VehicleNo.Contains(filterVehicleNo, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(filterColor) || v.Color.Contains(filterColor, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(filterBrand) || v.brand.Contains(filterBrand, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(filterModel) || v.Model.Contains(filterModel, StringComparison.OrdinalIgnoreCase))
                        ).Count();
        private int totalPages => (int)Math.Ceiling((double)totalFilteredCount / pageSize);

        private List<Vehicle> pagedVehicles
        {
            get
            {
                var filtered = vehicleList
                    .Where(v =>
                        (string.IsNullOrEmpty(filterVehicleNo) || v.VehicleNo.Contains(filterVehicleNo, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(filterColor) || v.Color.Contains(filterColor, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(filterBrand) || v.brand.Contains(filterBrand, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(filterModel) || v.Model.Contains(filterModel, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                var sorted = sortColumn switch
                {
                    "VehicleNo" => sortAscending
                        ? filtered.OrderBy(v => v.VehicleNo).ToList()
                        : filtered.OrderByDescending(v => v.VehicleNo).ToList(),
                    "Color" => sortAscending
                        ? filtered.OrderBy(v => v.Color).ToList()
                        : filtered.OrderByDescending(v => v.Color).ToList(),
                    "Brand" => sortAscending
                        ? filtered.OrderBy(v => v.brand).ToList()
                        : filtered.OrderByDescending(v => v.brand).ToList(),
                    "Model" => sortAscending
                        ? filtered.OrderBy(v => v.Model).ToList()
                        : filtered.OrderByDescending(v => v.Model).ToList(),
                    _ => filtered
                };

                return sorted
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }
        #endregion

        #region sorting variable

        private string sortColumn = "";
        private bool sortAscending = true;

        #endregion

        #region filtering

        private string filterVehicleNo = "";
        private string filterColor = "";
        private string filterBrand = "";
        private string filterModel = "";

        #endregion

        #endregion

        #region active Tab

        private void SetActiveTab(string tab)
        {
            activeTab = tab;
        }

        #endregion

        #region OnInitializedAsync
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await LoadNationalities();
        }
        #endregion

        #region onParameterAsync
        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(Ic))
            {
                await ViewRecord(Ic);
                if (Tab == "vehicle")
                    activeTab = "vehicle";

                StateHasChanged();
            }
        }

        #endregion

        #region Load Data

        private async Task LoadData()
        {
            EmployeesList = await DbService.GetEmployeesAsync(searchName, searchIC);
            empCurrentPage = 1; // Reset to first page when data is loaded/searched
        }

        private async Task LoadNationalities()
        {
            NationalityList = await DbService.GetNationalitiesAsync();
        }
        #endregion

        #region searching function

        private async Task SearchDynamic(KeyboardEventArgs e)
        {
            await LoadData();
        }

        private async Task ClearSearch()
        {
            searchName = "";
            searchIC = "";
            await LoadData();
        }

        #endregion

        #region show Form
        private void ShowCreateForm()
        {
            editingEmployee = new EmployeeProfile();
            isEditing = false;
            isViewing = false;
            isFormVisible = true;
        }

        #endregion

        #region Name UpperCase
        private void HandleNameInput(ChangeEventArgs e)
        {
            if (e.Value is string val)
            {
                var upper = val.ToUpperInvariant();
                editingEmployee.PersonName = upper.Length > 100 ? upper.Substring(0, 100) : upper;
            }
        }
        #endregion

        #region saveRecord
        private async Task SaveRecord()
        {
            try
            {
                ErrorMessage = null;

                if (pendingDocumentBytes != null)
                {
                    var trustedFileName = $"{Guid.NewGuid()}{pendingDocumentExtension}";
                    var uploadsFolder = Path.Combine(Env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    await File.WriteAllBytesAsync(Path.Combine(uploadsFolder, trustedFileName), pendingDocumentBytes);
                    editingEmployee.SupportingDocumentPath = $"/uploads/{trustedFileName}";
                    pendingDocumentBytes = null;
                }

                if (pendingPortraitBytes != null)
                {
                    var trustedFileName = $"{Guid.NewGuid()}{pendingPortraitExtension}";
                    var uploadsFolder = Path.Combine(Env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, trustedFileName);

                    await File.WriteAllBytesAsync(filePath, pendingPortraitBytes);

                    editingEmployee.PersonPortraitPath = $"/uploads/{trustedFileName}";
                    pendingPortraitBytes = null;
                }


                if (isEditing)
                {
                    await DbService.UpdateEmployeeAsync(editingEmployee, vehicleList);
                    await JSRuntime.InvokeVoidAsync("alert", "Record Updated Successfully!");
                }
                else
                {
                    var existing = await DbService.GetEmployeesAsync(searchIC: editingEmployee.IC_No);
                    if (existing.Any(x => x.IC_No == editingEmployee.IC_No))
                    {
                        await JSRuntime.InvokeVoidAsync("alert", "Record Creation Failed: IC No already exists.");
                        return;
                    }

                    await DbService.CreateEmployeeAsync(editingEmployee, vehicleList);
                    await JSRuntime.InvokeVoidAsync("alert", "Record Created Successfully!");
                }

                ResetForm();
                await LoadData();
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Save failed: {ex.Message}");
                ErrorMessage = $"System Error: {ex.Message}";
            }
        }
        #endregion

        #region viewRecord
        public async Task ViewRecord(string icNo)
        {
            var records = await DbService.GetEmployeesAsync(searchIC: icNo);
            var rec = records.FirstOrDefault(r => r.IC_No == icNo);
            if (rec != null)
            {
                editingEmployee = rec;
                vehicleList = rec.Vehicles ?? new List<Vehicle>();
                isViewing = true;
                isEditing = false;
                isFormVisible = true;
                activeTab = "user";
            }
        }
        #endregion

        #region EditRecord
        private async Task EditRecord(string icNo)
        {
            var records = await DbService.GetEmployeesAsync(searchIC: icNo);
            var rec = records.FirstOrDefault(r => r.IC_No == icNo);
            if (rec != null)
            {
                editingEmployee = rec;
                vehicleList = rec.Vehicles ?? new List<Vehicle>();
                isEditing = true;
                isViewing = false;
                isFormVisible = true;
                activeTab = "user";
            }
        }
        #endregion

        #region DeleteRecord
        private async Task DeleteRecord(string icNo)
        {
            bool confirm = await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete record IC No: {icNo}?");
            if (confirm)
            {
                try
                {
                    await DbService.DeleteEmployeeAsync(icNo);
                    await JSRuntime.InvokeVoidAsync("alert", "Record Deleted Successfully!");
                    ResetForm();
                    await LoadData();
                }
                catch (Exception ex)
                {
                    await JSRuntime.InvokeVoidAsync("alert", $"Delete failed: {ex.Message}");
                }
            }
        }
        #endregion

        #region ResetForm
        private void ResetForm()
        {
            editingVehicle = new Vehicle();
            vehicleList = new List<Vehicle>();
            editingEmployee = new EmployeeProfile();
            pendingDocumentBytes = null;      
            pendingDocumentExtension = null;   
            pendingPortraitBytes = null;       
            pendingPortraitExtension = null;  
            portraitPreviewUrl = null;
            isEditing = false;
            isViewing = false;
            isFormVisible = false;
            activeTab = "user";   
            Source = null;        
            Tab = null;           
        }
        #endregion

        #region ToggleHobby
        void ToggleHobby(String hobby, bool isChecked)
        {
            if (editingEmployee.Hobby == null)
                editingEmployee.Hobby = new List<string>();

            if (isChecked)
            {
                if (!editingEmployee.Hobby.Contains(hobby))
                    editingEmployee.Hobby.Add(hobby);
            }
            else
            {
                editingEmployee.Hobby.Remove(hobby);
            }
        }

        #endregion

        #region HandleFileSelected
        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            var buffer = new byte[e.File.Size];
            await e.File.OpenReadStream(5 * 1024 * 1024).ReadAsync(buffer);
            pendingDocumentBytes = buffer;
            pendingDocumentExtension = Path.GetExtension(e.File.Name);
        }

        #endregion

        #region HandlePortraitSelected
        private async Task HandlePortraitSelected(InputFileChangeEventArgs e)
        {
            pendingPortraitExtension = Path.GetExtension(e.File.Name);

            var buffer = new byte[e.File.Size];
            await e.File.OpenReadStream(2 * 1024 * 1024).ReadAsync(buffer);
            pendingPortraitBytes = buffer;

            // preview
            var base64 = Convert.ToBase64String(buffer);
            var mimeType = e.File.ContentType;
            portraitPreviewUrl = $"data:{mimeType};base64,{base64}";
        }
        #endregion

        #region TriggerPortraitUpload
        private async Task TriggerPortraitUpload()
        {
            if (!isViewing)
                await JSRuntime.InvokeVoidAsync("eval", "document.getElementById('portraitFileInput').click()");
        }
        #endregion

        #region RemovePotrait

        private void RemovePotrait()
        {
            editingEmployee.PersonPortraitPath = null;
            portraitPreviewUrl = null;
            pendingPortraitBytes = null;       
            pendingPortraitExtension = null;   
        }

        #endregion

        #region PrintRecordList

        private async Task PrintRecordList()
        {
            var header = @"<h2 style='text-align:center; font-weight:bold;'>EMPLOYEE PROFILES REPORT</h2>
            <table style='width:100%; border-collapse:collapse; table-layout:fixed;'>
            <thead><tr>
            <th style='border:1px solid black; padding:8px; width:18%;'>IC No</th>
            <th style='border:1px solid black; padding:8px; width:20%;'>Name</th>
            <th style='border:1px solid black; padding:8px; width:22%;'>Description</th>
            <th style='border:1px solid black; padding:8px; width:15%;'>Date of Birth</th>
            <th style='border:1px solid black; padding:8px; width:15%;'>Nationality</th>
            <th style='border:1px solid black; padding:8px; width:10%;'>Age</th>
            </tr></thead><tbody>";


            var sbScreen = new System.Text.StringBuilder();
            var sbPrint = new System.Text.StringBuilder();

            sbScreen.Append(header);
            sbPrint.Append(header);

            if (EmployeesList != null)
            {
                foreach (var emp in EmployeesList)
                {
                    var nationality = NationalityList.FirstOrDefault(n => n.Id == emp.nationality_id).name ?? "-";
                    var desc = emp.DescriptionOfPerson ?? "-";
                    var shortDesc = desc.Length > 20 ? desc.Substring(0, 20) + "..." : desc;

                    //screen print preview
                    sbScreen.Append("<tr>");
                    sbScreen.Append($"<td style='border:1px solid black; padding:8px;'>{emp.IC_No}</td>");
                    sbScreen.Append($"<td style='border:1px solid black; padding:8px;'>{emp.PersonName}</td>");
                    sbScreen.Append($"<td style='border:1px solid black; padding:8px;'>{shortDesc}</td>");
                    sbScreen.Append($"<td style='border:1px solid black; padding:8px;'>{emp.BirthDate?.ToString("dd MMM yyyy")}</td>");
                    sbScreen.Append($"<td style='border:1px solid black; padding:8px;'>{nationality}</td>");
                    sbScreen.Append($"<td style='border:1px solid black; padding:8px;'>{emp.CurrentAge}</td>");
                    sbScreen.Append("</tr>");

                    //print screen
                    sbPrint.Append("<tr>");
                    sbPrint.Append($"<td style='border:1px solid black; padding:8px;'>{emp.IC_No}</td>");
                    sbPrint.Append($"<td style='border:1px solid black; padding:8px;'>{emp.PersonName}</td>");
                    sbPrint.Append($"<td style='border:1px solid black; padding:8px; word-wrap:break-word;'>{desc}</td>");
                    sbPrint.Append($"<td style='border:1px solid black; padding:8px;'>{emp.BirthDate?.ToString("dd MMM yyyy")}</td>");
                    sbPrint.Append($"<td style='border:1px solid black; padding:8px;'>{nationality}</td>");
                    sbPrint.Append($"<td style='border:1px solid black; padding:8px;'>{emp.CurrentAge}</td>");
                    sbPrint.Append("</tr>");
                }
            }

            sbScreen.Append("</tbody></table>");
            sbPrint.Append("</tbody></table>");

            await JSRuntime.InvokeVoidAsync("printSection", sbScreen.ToString(), sbPrint.ToString());
        }

        #endregion

        #region autosearching brand

        private void FilterBrands(ChangeEventArgs e)
        {
            var input = e.Value?.ToString() ?? "";
            editingVehicle.brand = input;
            editingVehicle.Model = ""; // Reset model
            filteredModels.Clear();

            filteredBrands = string.IsNullOrEmpty(input)
                ? allBrands.ToList()
                : allBrands.Where(b => b.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void showAllBrands()
        {
            filteredBrands = allBrands.ToList();
        }

        private void hideBrands()
        {
            filteredBrands.Clear();
        }


        private void selectBrand(String brand)
        {
            editingVehicle.brand = brand;
            editingVehicle.Model = ""; // Reset model
            filteredModels.Clear();
            filteredBrands.Clear();
        }

        #endregion

        #region car model

        private void showAllModels()
        {
            if (!string.IsNullOrEmpty(editingVehicle.brand) && brandModels.ContainsKey(editingVehicle.brand))
                filteredModels = brandModels[editingVehicle.brand].ToList();
            else
                filteredModels.Clear();
        }

        private void HideModels()
        {
            filteredModels.Clear();
        }

        private void FilterModels(ChangeEventArgs e)
        {
            var input = e.Value?.ToString() ?? "";
            editingVehicle.Model = input;

            if (!string.IsNullOrEmpty(editingVehicle.brand) && brandModels.ContainsKey(editingVehicle.brand))
            {
                filteredModels = string.IsNullOrEmpty(input)
                    ? brandModels[editingVehicle.brand].ToList()
                    : brandModels[editingVehicle.brand].Where(m => m.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        private void selectModel(String model)
        {
            editingVehicle.Model = model;
            filteredModels.Clear();
        }

        #endregion

        #region ADD/REMOVE Vehicle

        private async Task AddVehicle()
        {
            if (string.IsNullOrEmpty(editingVehicle.VehicleNo) ||
            string.IsNullOrEmpty(editingVehicle.Color) ||
            string.IsNullOrEmpty(editingVehicle.brand) ||
            string.IsNullOrEmpty(editingVehicle.Model))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Please fill in all vehicle fields.");
                return;
            }

            if (!string.IsNullOrEmpty(vehicleNoDuplicateError))
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Cannot add vehicle: {vehicleNoDuplicateError}");
                return;
            }

            if(selectedVehicleNo != null)
            {

                var target = vehicleList.FirstOrDefault(v=>v.VehicleNo == selectedVehicleNo);
                if (target != null)
                {
                    target.Color = editingVehicle.Color;
                    target.brand = editingVehicle.brand;
                    target.Model = editingVehicle.Model;
                }
                selectedVehicleNo = null;
                onUpdate = false;
            }
            else
            {
                vehicleList.Add(new Vehicle
                {
                    VehicleNo = editingVehicle.VehicleNo,
                    Color = editingVehicle.Color,
                    brand = editingVehicle.brand,
                    Model = editingVehicle.Model,
                    IC_No = editingEmployee.IC_No
                });
            }
            editingVehicle = new Vehicle();
            vehicleNoDuplicateError = null;
        }

        private async Task RemoveVehicle(Vehicle v)
        {
            bool confirm = await JSRuntime.InvokeAsync<bool>("confirm", $"Remove vehicle {v.VehicleNo}?");

            if (!confirm) return;

            if (v.Id > 0)
            {
                await DbService.DeleteVehicleAsync(v.Id);
            }

            vehicleList.Remove(v);
        }

        private void viewVehicle(Vehicle v)
        {
            selectedVehicleNo = v.VehicleNo;
            editingVehicle.VehicleNo = v.VehicleNo;
            editingVehicle.brand = v.brand;
            editingVehicle.Color = v.Color;
            editingVehicle.Model = v.Model;

            onUpdate = true;
            vehicleNoDuplicateError = null;
        }

        private async Task ValidateVehicleNo(ChangeEventArgs e)
        {
            var val = e.Value?.ToString()?.Trim() ?? "";
            editingVehicle.VehicleNo = val;
            vehicleNoDuplicateError = null;

            if (string.IsNullOrWhiteSpace(val)) return;

            // Check local list first
            if (vehicleList.Any(v => v.VehicleNo.Equals(val, StringComparison.OrdinalIgnoreCase) && v.VehicleNo != selectedVehicleNo))
            {
                vehicleNoDuplicateError = "This vehicle number is already in your list.";
                return;
            }

            // Check database
            int? excludeId = null;
            if (onUpdate && selectedVehicleNo != null)
            {
                excludeId = vehicleList.FirstOrDefault(v => v.VehicleNo == selectedVehicleNo)?.Id;
                if (excludeId == 0) excludeId = null;
            }

            bool exists = await DbService.IsVehicleNoExistsAsync(val, excludeId);
            if (exists)
            {
                vehicleNoDuplicateError = "This vehicle number is already registered in the system.";
            }
        }

        #endregion

        #region sorting

        private void SortBy(String column)
        {
            if (sortColumn == column)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                sortColumn = column;
                sortAscending = true;
            }
        }

        #endregion

        #region OnSearchInput

        private void OnVehicleSearchInput(ChangeEventArgs e)
        {
            filterVehicleNo = e.Value?.ToString() ?? "";
            currentPage = 1;
        }

        private void OnColorSearchInput(ChangeEventArgs e)
        {
            filterColor = e.Value?.ToString() ?? "";
            currentPage = 1;
        }

        private void OnBrandSearchInput(ChangeEventArgs e)
        {
            filterBrand = e.Value?.ToString() ?? "";
            currentPage = 1;
        }



        private void OnModelSearchInput(ChangeEventArgs e)
        {
            filterModel = e.Value?.ToString() ?? "";
            currentPage = 1;
        }

        #endregion

        #region HandleIcInput
        private void HandleIcInput(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (val != null)
            {
                HandleIcInput(val);
            }
        }

        private void HandleIcInput(string val)
        {
            editingEmployee.IC_No = val;

            if (!string.IsNullOrEmpty(val))
            {
                var cleanIc = val.Replace("-", "").Trim();
                if (cleanIc.Length >= 6)
                {
                    var dobString = cleanIc.Substring(0, 6);
                    if (DateTime.TryParseExact(dobString, "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        if (parsedDate > DateTime.Today)
                        {
                            parsedDate = parsedDate.AddYears(-100);
                        }
                        editingEmployee.BirthDate = parsedDate;
                    }
                }
            }
        }
        #endregion

        private bool scrolledToDocs = false;

        private async Task HandleNextStep()
        {
            if (!scrolledToDocs)
            {
                await ScrollToElement("documentSection");
                scrolledToDocs = true;
            }
            else
            {
                await GoToVehicleTab();
                scrolledToDocs = false;
                vehicleNoDuplicateError = null;
            }
        }

        private async Task ScrollToElement(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("scrollToElement", elementId);
        }

        private async Task GoToVehicleTab()
        {
            SetActiveTab("vehicle");
            // Wait for Blazor to render the new tab
            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("scrollToElement", "vehicleSection");
        }
    }
}
