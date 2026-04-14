using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Testing.Components.Model;
using Testing.Components.Data;

namespace Testing.Components.Pages
{
    public partial class Vehicles
    {
        [Inject] private EmployeeService DbService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter] [SupplyParameterFromQuery(Name = "ic")] public string? Ic { get; set; }

        private List<Vehicle> vehicleList = new();
        private List<Vehicle> newVehicleList = new();
        private Vehicle newVehicle = new();

        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(Ic))
            {
                newVehicle.IC_No = Ic;
            }
        }
        private string? ErrorMessage = null;
        private bool isEditing = false;
        private bool onUpdate = false;
        private string? selectedVehicleNo = null;
        private string? vehicleNoDuplicateError;

        // Brand & Model dropdown data
        private List<string> filteredBrands = new();
        private List<string> filteredModels = new();
        private List<string> allBrands = new()
        {
            "Perodua", "Proton", "Toyota", "Mazda", "Honda"
        };
        private Dictionary<string, List<string>> brandModels = new()
        {
            { "Perodua", new() { "Bezza", "Myvi" } },
            { "Proton",  new() { "X50", "X70" } },
            { "Toyota",  new() { "Vios", "Hilux" } },
            { "Mazda",   new() { "Mazda 3", "CX-5" } },
            { "Honda",   new() { "Civic", "CR-V" } },
        };

        #region Search filters (listing section)
        private string searchVehicleNo = "";
        private string searchBrand = "";
        private string searchModel = "";
        private string searchIC = "";
        #endregion

        #region Table Pagination & Sorting (Main Listing)
        private int currentPage = 1;
        private int pageSize = 5;
        private string sortColumn = "VehicleNo";
        private bool sortAscending = true;

        private int totalFilteredCount => FilteredVehicleList.Count;
        private int totalPages => Math.Max(1, (int)Math.Ceiling((double)totalFilteredCount / pageSize));

        private List<Vehicle> FilteredVehicleList
        {
            get
            {
                if (vehicleList == null) return new List<Vehicle>();

                var filtered = vehicleList.AsEnumerable();

                // Apply Search Filters
                if (!string.IsNullOrWhiteSpace(searchVehicleNo))
                    filtered = filtered.Where(v => v.VehicleNo.Contains(searchVehicleNo, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(searchBrand))
                    filtered = filtered.Where(v => v.brand.Contains(searchBrand, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(searchModel))
                    filtered = filtered.Where(v => v.Model.Contains(searchModel, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(searchIC))
                    filtered = filtered.Where(v => v.IC_No.Contains(searchIC, StringComparison.OrdinalIgnoreCase));

                return filtered.ToList();
            }
        }

        private List<Vehicle> DisplayVehicles
        {
            get
            {
                var filtered = FilteredVehicleList;

                // Apply Sorting
                var sorted = sortColumn switch
                {
                    "IC_No" => sortAscending ? filtered.OrderBy(v => v.IC_No) : filtered.OrderByDescending(v => v.IC_No),
                    "VehicleNo" => sortAscending ? filtered.OrderBy(v => v.VehicleNo) : filtered.OrderByDescending(v => v.VehicleNo),
                    "Brand" => sortAscending ? filtered.OrderBy(v => v.brand) : filtered.OrderByDescending(v => v.brand),
                    "Model" => sortAscending ? filtered.OrderBy(v => v.Model) : filtered.OrderByDescending(v => v.Model),
                    "Color" => sortAscending ? filtered.OrderBy(v => v.Color) : filtered.OrderByDescending(v => v.Color),
                    _ => filtered.AsEnumerable()
                };

                // Apply Pagination
                return sorted.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        private void SortBy(string column)
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

        private void ChangePageSize(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int newSize))
            {
                pageSize = newSize;
                currentPage = 1;
            }
        }
        #endregion

        #region New Vehicle List logic (Form Section)
        // These are used for the table inside the "Add/Edit Form" section
        private string filterIC = "";
        private string filterVehicleNo = "";
        private string filterColor = "";
        private string filterBrandCol = "";
        private string filterModelCol = "";

        private List<Vehicle> pagedNewVehicles
        {
            get
            {
                var filtered = newVehicleList.Where(v =>
                    (string.IsNullOrEmpty(filterIC) || v.IC_No.Contains(filterIC, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(filterVehicleNo) || v.VehicleNo.Contains(filterVehicleNo, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(filterColor) || v.Color.Contains(filterColor, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(filterBrandCol) || v.brand.Contains(filterBrandCol, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(filterModelCol) || v.Model.Contains(filterModelCol, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                // Simple pagination for the temporary list in form
                return filtered.Take(100).ToList(); // No heavy paging needed here usually
            }
        }
        #endregion

        protected override async Task OnInitializedAsync()
        {
            await LoadVehicles();
        }

        private async Task LoadVehicles()
        {
            try
            {
                var employees = await DbService.GetEmployeesAsync();
                vehicleList = new List<Vehicle>();

                foreach (var emp in employees)
                {
                    if (emp.Vehicles != null && emp.Vehicles.Any())
                        vehicleList.AddRange(emp.Vehicles);
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        #region CRUD Operations
        private async Task AddVehicleToList()
        {
            if (string.IsNullOrEmpty(newVehicle.IC_No) ||
                string.IsNullOrEmpty(newVehicle.VehicleNo) ||
                string.IsNullOrEmpty(newVehicle.Color) ||
                string.IsNullOrEmpty(newVehicle.brand) ||
                string.IsNullOrEmpty(newVehicle.Model))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Please fill in all vehicle fields.");
                return;
            }

            if (!string.IsNullOrEmpty(vehicleNoDuplicateError))
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Cannot add vehicle: {vehicleNoDuplicateError}");
                return;
            }

            var cleanIC = newVehicle.IC_No.Replace("-", "").Trim();
            if (cleanIC.Length != 12 || !cleanIC.All(char.IsDigit))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Invalid IC No format. It must contain 12 digits (e.g. 950228-02-5678).");
                return;
            }

            if (selectedVehicleNo != null)
            {
                var target = newVehicleList.FirstOrDefault(v => v.VehicleNo == selectedVehicleNo);
                if (target != null)
                {
                    target.IC_No = newVehicle.IC_No;
                    target.VehicleNo = newVehicle.VehicleNo;
                    target.Color = newVehicle.Color;
                    target.brand = newVehicle.brand;
                    target.Model = newVehicle.Model;
                }
                selectedVehicleNo = null;
                onUpdate = false;
            }
            else
            {
                newVehicleList.Add(new Vehicle
                {
                    IC_No = newVehicle.IC_No,
                    VehicleNo = newVehicle.VehicleNo,
                    Color = newVehicle.Color,
                    brand = newVehicle.brand,
                    Model = newVehicle.Model,
                });
            }

            newVehicle = new Vehicle();
            vehicleNoDuplicateError = null;
        }

        private void EditVehicleInList(Vehicle v)
        {
            selectedVehicleNo = v.VehicleNo;
            newVehicle.IC_No = v.IC_No;
            newVehicle.VehicleNo = v.VehicleNo;
            newVehicle.brand = v.brand;
            newVehicle.Color = v.Color;
            newVehicle.Model = v.Model;
            onUpdate = true;
        }

        private async Task RemoveVehicleFromList(Vehicle v)
        {
            bool confirm = await JSRuntime.InvokeAsync<bool>("confirm", $"Remove vehicle {v.VehicleNo}?");
            if (!confirm) return;
            newVehicleList.Remove(v);
        }

        private async Task SaveAllVehicles()
        {
            try
            {
                var toInsert = newVehicleList.Where(v => v.Id == 0).ToList();
                var toUpdate = newVehicleList.Where(v => v.Id > 0).ToList();

                if (toInsert.Any()) await DbService.AddVehiclesAsync(toInsert);

                foreach (var vehicle in toUpdate)
                {
                    await DbService.UpdateVehicleAsync(vehicle);
                }

                await JSRuntime.InvokeVoidAsync("alert", "All vehicles saved successfully!");
                newVehicleList.Clear();
                await LoadVehicles();
                await HideFormClientSide();
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Save failed: {ex.Message}");
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private async Task EditVehicleFromList(Vehicle vehicle)
        {
            isEditing = true;
            newVehicleList = vehicleList.Where(v => v.IC_No == vehicle.IC_No).ToList();
            newVehicle = new Vehicle
            {
                IC_No = vehicle.IC_No,
                VehicleNo = vehicle.VehicleNo,
                brand = vehicle.brand,
                Model = vehicle.Model,
                Color = vehicle.Color,
            };
            selectedVehicleNo = vehicle.VehicleNo;
            onUpdate = true;
            await ShowFormClientSide();
        }

        private async Task DeleteVehicle(Vehicle vehicle)
        {
            bool confirm = await JSRuntime.InvokeAsync<bool>("confirm", $"Delete vehicle {vehicle.VehicleNo}?");
            if (confirm)
            {
                try
                {
                    await DbService.DeleteVehicleAsync(vehicle.Id);
                    vehicleList.RemoveAll(v => v.Id == vehicle.Id);
                    await JSRuntime.InvokeVoidAsync("alert", "Vehicle deleted!");
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    await JSRuntime.InvokeVoidAsync("alert", $"Error: {ex.Message}");
                }
            }
        }
        #endregion

        #region UI Handlers
        private async Task ShowFormClientSide()
        {
            await JSRuntime.InvokeVoidAsync("showVehicleFormClientSide");
        }

        private async Task PrepareNewVehicleForm()
        {
            isEditing = false;
            onUpdate = false;
            selectedVehicleNo = null;
            newVehicle = new Vehicle();
            newVehicleList.Clear();
            vehicleNoDuplicateError = null;
            filteredBrands.Clear();
            filteredModels.Clear();
            await ShowFormClientSide();
        }

        private async Task HideFormClientSide()
        {
            isEditing = false;
            onUpdate = false;
            selectedVehicleNo = null;
            newVehicle = new Vehicle();
            newVehicleList.Clear();
            vehicleNoDuplicateError = null;
            await JSRuntime.InvokeVoidAsync("hideVehicleFormClientSide");
        }

        private async Task ValidateVehicleNo(ChangeEventArgs e)
        {
            var val = e.Value?.ToString()?.Trim() ?? "";
            newVehicle.VehicleNo = val;
            vehicleNoDuplicateError = null;

            if (string.IsNullOrWhiteSpace(val)) return;

            // Check current unsaved list
            if (newVehicleList.Any(v => v.VehicleNo.Equals(val, StringComparison.OrdinalIgnoreCase) && v.VehicleNo != selectedVehicleNo))
            {
                vehicleNoDuplicateError = "This vehicle number is already in your unsaved list.";
                return;
            }

            // Check database
            int? excludeId = null;
            if (onUpdate && selectedVehicleNo != null)
            {
                excludeId = newVehicleList.FirstOrDefault(v => v.VehicleNo == selectedVehicleNo)?.Id;
                if (excludeId == 0 || excludeId == null) 
                    excludeId = vehicleList.FirstOrDefault(v => v.VehicleNo == selectedVehicleNo)?.Id;
                
                if (excludeId == 0) excludeId = null;
            }

            bool exists = await DbService.IsVehicleNoExistsAsync(val, excludeId);
            if (exists)
            {
                vehicleNoDuplicateError = "This vehicle number is already registered in the system.";
            }
        }

        private void FilterBrands(ChangeEventArgs e)
        {
            var input = e.Value?.ToString() ?? "";
            newVehicle.brand = input;
            newVehicle.Model = "";
            filteredModels.Clear();
            filteredBrands = string.IsNullOrEmpty(input)
                ? allBrands.ToList()
                : allBrands.Where(b => b.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void ShowAllBrands() => filteredBrands = allBrands.ToList();
        private void HideBrands() => filteredBrands.Clear();
        private void SelectBrand(string brand)
        {
            newVehicle.brand = brand;
            filteredBrands.Clear();
            newVehicle.Model = "";
            filteredModels.Clear();
        }

        private void ShowAllModels()
        {
            if (!string.IsNullOrEmpty(newVehicle.brand) && brandModels.ContainsKey(newVehicle.brand))
                filteredModels = brandModels[newVehicle.brand].ToList();
            else
                filteredModels.Clear();
        }

        private void HideModels() => filteredModels.Clear();

        private void FilterModels(ChangeEventArgs e)
        {
            var input = e.Value?.ToString() ?? "";
            newVehicle.Model = input;
            if (!string.IsNullOrEmpty(newVehicle.brand) && brandModels.ContainsKey(newVehicle.brand))
            {
                filteredModels = string.IsNullOrEmpty(input)
                    ? brandModels[newVehicle.brand].ToList()
                    : brandModels[newVehicle.brand].Where(m => m.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        private void SelectModel(string model)
        {
            newVehicle.Model = model;
            filteredModels.Clear();
        }
        #endregion

        #region Listing Search Handlers
        private void OnICSearchInput(ChangeEventArgs e) { filterIC = e.Value?.ToString() ?? ""; currentPage = 1; }
        private void OnVehicleSearchInput(ChangeEventArgs e) { filterVehicleNo = e.Value?.ToString() ?? ""; currentPage = 1; }
        private void OnColorSearchInput(ChangeEventArgs e) { filterColor = e.Value?.ToString() ?? ""; currentPage = 1; }
        private void OnBrandSearchInput(ChangeEventArgs e) { filterBrandCol = e.Value?.ToString() ?? ""; currentPage = 1; }
        private void OnModelSearchInput(ChangeEventArgs e) { filterModelCol = e.Value?.ToString() ?? ""; currentPage = 1; }
        #endregion

        private async Task PrepareNewVehicleForIC(string ic)
        {
            newVehicle = new Vehicle { IC_No = ic };
            ErrorMessage = null;
            vehicleNoDuplicateError = null;
            onUpdate = false;
            isEditing = false;
            
            // Clear list and dropdowns
            newVehicleList.Clear();
            filteredBrands.Clear();
            filteredModels.Clear();

            // Show the form and scroll to it
            await ShowFormClientSide();
            await JSRuntime.InvokeVoidAsync("scrollToElement", "formSection");
        }
    }
}
