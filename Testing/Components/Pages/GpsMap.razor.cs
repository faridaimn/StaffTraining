using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Testing.Components.Data;
using Testing.Components.Model;

namespace Testing.Components.Pages;

public partial class GpsMap : IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private EmployeeService DbService { get; set; } = default!;

    private string? errorMessage = null;
    private List<GpsLocation> savedLocations = new();

    // Map click tracking
    private bool isFormVisible = false;
    private double newLat = 0;
    private double newLng = 0;
    private string newTitle = "";
    private string newDesc = "";

    // DotNetObjectReference so Javascript can talk to C#
    private DotNetObjectReference<GpsMap>? dotNetHelper;

    protected override async Task OnInitializedAsync()
    {
        await LoadLocationsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize JS Interop Helper
            dotNetHelper = DotNetObjectReference.Create(this);

            try
            {
                // Kuala Lumpur approximate default coordinates
                double defaultLat = 3.140853; 
                double defaultLng = 101.693207;
                int defaultZoomLevel = 8;

                // Load OpenStreetMap through our map.js
                await JSRuntime.InvokeVoidAsync("initializeMap", "osmMap", defaultLat, defaultLng, defaultZoomLevel, dotNetHelper);

                // Plot all locations currently fetched from the database
                await ReplotAllPinsAsync();
            }
            catch (Exception ex)
            {
                errorMessage = "Map integration failed: " + ex.Message;
                StateHasChanged();
            }
        }
    }

    private async Task LoadLocationsAsync()
    {
        try
        {
            savedLocations = await DbService.GetGpsLocationsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = "Failed to load GPS history: " + ex.Message;
        }
    }

    private async Task ReplotAllPinsAsync()
    {
        await JSRuntime.InvokeVoidAsync("clearAllMarkers");
        
        foreach (var loc in savedLocations)
        {
            await JSRuntime.InvokeVoidAsync("addMarker", loc.Latitude, loc.Longitude, loc.Title);
        }
    }

    // Called automatically by Leaflet click event via our map.js
    [JSInvokable]
    public void OnMapClicked(double lat, double lng)
    {
        newLat = lat;
        newLng = lng;
        newTitle = "";
        newDesc = "";
        isFormVisible = true;
        errorMessage = null;

        StateHasChanged();
    }

    private void CancelPlot()
    {
        isFormVisible = false;
    }

    private async Task SaveCoordinate()
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            errorMessage = "Please enter a title for the location first.";
            return;
        }

        try
        {
            var newLoc = new GpsLocation
            {
                Latitude = newLat,
                Longitude = newLng,
                Title = newTitle,
                Description = newDesc,
                CreatedAt = DateTime.Now
            };

            await DbService.AddGpsLocationAsync(newLoc);
            
            // Immediately drop pin on map
            await JSRuntime.InvokeVoidAsync("addMarker", newLoc.Latitude, newLoc.Longitude, newLoc.Title);

            // Hide form and refresh list
            isFormVisible = false;
            await LoadLocationsAsync();
            errorMessage = null;
        }
        catch (Exception ex)
        {
            errorMessage = "Failed to save coordinate: " + ex.Message;
        }
    }

    private async Task GoToLocation(GpsLocation loc)
    {
        // Cancel plotting if any
        isFormVisible = false;
        
        // Let the map fly to the selected target smoothly
        await JSRuntime.InvokeVoidAsync("flyToLocation", loc.Latitude, loc.Longitude, 16);
    }

    public async ValueTask DisposeAsync()
    {
        dotNetHelper?.Dispose();
    }
}
