using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Testing.Components.Data;
using Testing.Components.Model;

namespace Testing.Components.Pages
{
    public partial class Personlist
    {
        [Parameter] public string? Gender { get; set; }
        [Parameter] public string? NationalityName { get; set; }
        [Parameter] public string? MaritalStatus { get; set; }
        [Parameter] public string? BirthYear { get; set; }

        // Raw data loaded from DB (filtered by route params)
        private List<EmployeeProfile>? persons;

        // Vehicle popup
        private List<Vehicle> popupVehicles = new();
        private string popupPersonName = "";
        private bool showModal = false;

        // Nationality lookup
        private Dictionary<int, string> nationalityMap = new();

        #region Search fields
        private string searchIC = "";
        private string searchName = "";
        private string searchGender = "";
        private string searchMarital = "";
        #endregion

        #region Sorting
        private string sortColumn = "PersonName";
        private bool sortAscending = true;
        #endregion

        #region Pagination
        private int currentPage = 1;
        private int pageSize = 5;

        private int totalFilteredCount => filteredPersons.Count;
        private int totalPages => (int)Math.Ceiling((double)totalFilteredCount / pageSize);

        private List<EmployeeProfile> pagedPersons
        {
            get
            {
                return filteredPersons
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }
        #endregion

        #region Filtered & Sorted list
        private List<EmployeeProfile> filteredPersons
        {
            get
            {
                if (persons == null) return new List<EmployeeProfile>();

                var query = persons.AsEnumerable();

                // Apply inline search filters
                if (!string.IsNullOrWhiteSpace(searchIC))
                    query = query.Where(p => p.IC_No != null && p.IC_No.Contains(searchIC, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(searchName))
                    query = query.Where(p => p.PersonName != null && p.PersonName.Contains(searchName, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(searchGender))
                    query = query.Where(p => p.Gender == searchGender);

                if (!string.IsNullOrWhiteSpace(searchMarital))
                    query = query.Where(p => p.MaritalStatus == searchMarital);

                // Apply sorting
                query = sortColumn switch
                {
                    "IC_No" => sortAscending
                        ? query.OrderBy(p => p.IC_No)
                        : query.OrderByDescending(p => p.IC_No),

                    "PersonName" => sortAscending
                        ? query.OrderBy(p => p.PersonName)
                        : query.OrderByDescending(p => p.PersonName),

                    "Gender" => sortAscending
                        ? query.OrderBy(p => p.Gender)
                        : query.OrderByDescending(p => p.Gender),

                    "MaritalStatus" => sortAscending
                        ? query.OrderBy(p => p.MaritalStatus)
                        : query.OrderByDescending(p => p.MaritalStatus),

                    "BirthYear" => sortAscending
                        ? query.OrderBy(p => p.BirthDate?.Year ?? 0)
                        : query.OrderByDescending(p => p.BirthDate?.Year ?? 0),

                    "Nationality" => sortAscending
                        ? query.OrderBy(p => GetNationalityName(p.nationality_id))
                        : query.OrderByDescending(p => GetNationalityName(p.nationality_id)),

                    "VehicleCount" => sortAscending
                        ? query.OrderBy(p => p.Vehicles?.Count ?? 0)
                        : query.OrderByDescending(p => p.Vehicles?.Count ?? 0),

                    _ => query.OrderBy(p => p.PersonName)
                };

                return query.ToList();
            }
        }
        #endregion

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            await LoadNationalities();
        }

        protected override async Task OnParametersSetAsync()
        {
            await LoadNationalities();
            await LoadPersons();
        }
        #endregion

        #region Data Loading
        private async Task LoadNationalities()
        {
            var nationalities = await DbService.GetNationalitiesAsync();
            nationalityMap = nationalities.ToDictionary(n => n.Id, n => n.Name);
        }

        private async Task LoadPersons()
        {
            var allEmployees = await DbService.GetEmployeesAsync();

            // Apply route-based filter
            if (!string.IsNullOrEmpty(BirthYear))
            {
                persons = allEmployees.Where(e =>
                    e.BirthDate.HasValue &&
                    e.BirthDate.Value.Year.ToString() == BirthYear
                ).ToList();
            }
            else if (!string.IsNullOrEmpty(MaritalStatus))
            {
                persons = allEmployees.Where(e => e.MaritalStatus == MaritalStatus).ToList();
            }
            else if (!string.IsNullOrEmpty(NationalityName))
            {
                persons = allEmployees.Where(e =>
                    e.nationality_id.HasValue &&
                    nationalityMap.ContainsKey(e.nationality_id.Value) &&
                    nationalityMap[e.nationality_id.Value] == NationalityName
                ).ToList();
            }
            else if (Gender == "hasGender")
                persons = allEmployees.Where(e => !string.IsNullOrEmpty(e.Gender)).ToList();
            else if (Gender == "hasNationality")
                persons = allEmployees.Where(e => e.nationality_id.HasValue).ToList();
            else if (Gender == "hasMarital")
                persons = allEmployees.Where(e => !string.IsNullOrEmpty(e.MaritalStatus)).ToList();
            else if (Gender == "hasBirthYear")
                persons = allEmployees.Where(e => e.BirthDate.HasValue).ToList();
            else if (Gender == "all" || string.IsNullOrEmpty(Gender))
                persons = allEmployees;
            else
                persons = allEmployees.Where(e => e.Gender == Gender).ToList();

            // Reset to first page whenever data reloads
            currentPage = 1;
        }
        #endregion

        #region Search & Filter
        private async Task SearchDynamic(KeyboardEventArgs e)
        {
            currentPage = 1;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task ApplySearch()
        {
            currentPage = 1;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private void ClearSearch()
        {
            searchIC = "";
            searchName = "";
            searchGender = "";
            searchMarital = "";
            currentPage = 1;
            StateHasChanged();
        }
        #endregion

        #region Sorting
        private void SortBy(string column)
        {
            if (sortColumn == column)
                sortAscending = !sortAscending;
            else
            {
                sortColumn = column;
                sortAscending = true;
            }
            currentPage = 1;
        }
        #endregion

        #region Pagination
        private void ChangePageSize(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int newSize))
            {
                pageSize = newSize;
                currentPage = 1;
            }
        }
        #endregion

        #region Helpers
        private string GetNationalityName(int? id)
        {
            if (id.HasValue && nationalityMap.ContainsKey(id.Value))
                return nationalityMap[id.Value];
            return "-";
        }
        #endregion

        #region Navigation
        private void GoBack()
        {
            Navigation.NavigateTo("/reports");
        }

        private void ViewPersonDetail(string icNo)
        {
            Navigation.NavigateTo($"/users?ic={icNo}&source=personlist", forceLoad: true);
        }

        private void GoToVehicle(string icNo)
        {
            Navigation.NavigateTo($"/users?ic={icNo}&tab=vehicle&source=personlistpage");
        }
        #endregion

        #region Vehicle Popup
        private void ShowVehiclePopup(string icNo, string name)
        {
            var person = persons?.FirstOrDefault(p => p.IC_No == icNo);
            popupVehicles = person?.Vehicles ?? new List<Vehicle>();
            popupPersonName = name;
            showModal = true;
        }
        #endregion
    }
}