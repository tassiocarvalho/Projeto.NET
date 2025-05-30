// MainPage.xaml.cs
using System.Text.Json;
using Microsoft.Maui.Controls;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace DynamicMauiApp
{
    public partial class MainPage : TabbedPage // Changed from ContentPage to TabbedPage
    {
        private List<View> mandatoryFields = new List<View>();
        private Dictionary<string, object> controlValues = new Dictionary<string, object>(); // To store values if needed

        public MainPage()
        {
            InitializeComponent();
            LoadDynamicLayout();
        }

        private async void LoadDynamicLayout()
        {
            try
            {
                var jsonString = await ReadJsonFile("DynamicMauiApp.Resources.Raw.layout.json");
                if (string.IsNullOrEmpty(jsonString))
                {
                    await DisplayAlert("Error", "Could not read layout JSON file.", "OK");
                    return;
                }

                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    if (document.RootElement.TryGetProperty("items", out JsonElement itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var itemElement in itemsElement.EnumerateArray())
                        {
                            var page = CreatePageFromJson(itemElement);
                            if (page != null)
                            {
                                this.Children.Add(page);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load dynamic layout: {ex.Message}", "OK");
            }
        }

        private ContentPage CreatePageFromJson(JsonElement itemElement)
        {
            if (itemElement.TryGetProperty("type", out JsonElement typeElement) && typeElement.GetString() == "tabpage")
            {
                var page = new ContentPage();
                page.Title = itemElement.TryGetProperty("text", out JsonElement textElement) ? textElement.GetString() : "Tab";

                var scrollView = new ScrollView();
                var stackLayout = new VerticalStackLayout { Padding = new Thickness(10), Spacing = 10 };

                if (itemElement.TryGetProperty("items", out JsonElement contentItemsElement) && contentItemsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var contentItem in contentItemsElement.EnumerateArray())
                    {
                        var control = CreateControlFromJson(contentItem);
                        if (control != null)
                        {
                            stackLayout.Children.Add(control);
                        }
                    }
                }

                // Add a validation button at the end of each tab page
                var validateButton = new Button
                {
                    Text = "Validate Page",
                    Margin = new Thickness(0, 20, 0, 0)
                };
                validateButton.Clicked += OnValidateButtonClicked;
                stackLayout.Children.Add(validateButton);

                scrollView.Content = stackLayout;
                page.Content = scrollView;
                return page;
            }
            return null;
        }

        private View CreateControlFromJson(JsonElement itemElement)
        {
            string type = itemElement.TryGetProperty("type", out var typeProp) ? typeProp.GetString()?.ToLower() : null;
            string text = itemElement.TryGetProperty("text", out var textProp) ? textProp.GetString() : "";
            string name = itemElement.TryGetProperty("nome", out var nameProp) ? nameProp.GetString() : text; // Use text as fallback name
            bool isMandatory = itemElement.TryGetProperty("ismandatory", out var mandatoryProp) && mandatoryProp.ValueKind == JsonValueKind.True;
            string defaultValue = itemElement.TryGetProperty("valorPadrao", out var defaultProp) ? defaultProp.GetString() : null;
            defaultValue ??= itemElement.TryGetProperty("initialvalue", out var initialProp) ? initialProp.GetString() : null;

            View control = null;
            VerticalStackLayout container = new VerticalStackLayout { Spacing = 2 };

            // Add Label for the control
            container.Children.Add(new Label { Text = text, FontAttributes = FontAttributes.Bold });

            switch (type)
            {
                case "textbox":
                case "multilinetextbox":
                    var entry = new Entry
                    {
                        Placeholder = $"Enter {text}",
                        Text = defaultValue,
                        AutomationId = name // Use name for identification
                    };
                    if (type == "multilinetextbox")
                    {
                        entry.HeightRequest = 100; // Basic multi-line support
                        // MAUI Entry doesn't have a built-in multi-line mode like WPF/WinForms.
                        // Editor control should be used for true multi-line, but let's stick to Entry for simplicity based on JSON.
                    }
                    control = entry;
                    break;

                case "checkbox":
                    var checkBox = new CheckBox
                    {
                        IsChecked = defaultValue?.ToLower() == "true",
                        AutomationId = name
                    };
                    // MAUI CheckBox doesn't have text property, use the label above.
                    control = checkBox;
                    break;

                case "dropdown":
                    var picker = new Picker
                    {
                        Title = $"Select {text}",
                        AutomationId = name
                    };
                    // Populate options if available in JSON (not present in the sample, add dummy data)
                    if (itemElement.TryGetProperty("opcoes", out var optionsProp) && optionsProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var option in optionsProp.EnumerateArray())
                        {
                            picker.Items.Add(option.GetString());
                        }
                    }
                    else
                    {
                        // Add dummy options if none provided
                        picker.Items.Add("Option 1");
                        picker.Items.Add("Option 2");
                        picker.Items.Add("Option 3");
                    }
                    if (!string.IsNullOrEmpty(defaultValue) && picker.Items.Contains(defaultValue))
                    {
                        picker.SelectedItem = defaultValue;
                    }
                    control = picker;
                    break;

                case "datepicker": // Not in JSON, but required by PDF
                    var datePicker = new DatePicker
                    {
                        AutomationId = name
                    };
                    if (DateTime.TryParse(defaultValue, out DateTime date))
                    {
                        datePicker.Date = date;
                    }
                    control = datePicker;
                    break;

                case "label": // Not in JSON, but required by PDF
                     // The label is already added above the control. If it's meant to be a standalone label:
                     var labelOnly = new Label { Text = defaultValue ?? text, AutomationId = name };
                     container.Children.Clear(); // Remove the bold label added earlier
                     container.Children.Add(labelOnly);
                     control = null; // The container itself holds the label
                     break;

                case "photo": // Placeholder for photo functionality
                    var photoButton = new Button { Text = $"Add {text}", AutomationId = name };
                    // Add logic for photo capture/selection here if needed
                    control = photoButton;
                    break;

                case "gps": // Placeholder for GPS functionality
                    var gpsButton = new Button { Text = $"Get {text}", AutomationId = name };
                    // Add logic for GPS capture here if needed
                    control = gpsButton;
                    break;

                case "itemscontainermaster": // Basic handling for container
                    var containerMasterLayout = new VerticalStackLayout { Margin = new Thickness(10, 5, 0, 5) };
                    containerMasterLayout.Children.Add(new Label { Text = text, FontAttributes = FontAttributes.Italic });
                    if (itemElement.TryGetProperty("items", out var subItemsProp) && subItemsProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var subItem in subItemsProp.EnumerateArray())
                        {
                            var subControl = CreateControlFromJson(subItem);
                            if (subControl != null)
                            {
                                containerMasterLayout.Children.Add(subControl);
                            }
                        }
                    }
                    // Add 'Add/Clone' button if specified
                    if (itemElement.TryGetProperty("addclonebutton", out var cloneProp) && cloneProp.GetString()?.ToLower() == "true")
                    {
                        containerMasterLayout.Children.Add(new Button { Text = "Add Item" }); // Placeholder
                    }
                    control = containerMasterLayout;
                    container.Children.Clear(); // Use the specific container layout
                    container.Children.Add(control);
                    control = null; // The container holds the layout
                    break;

                 case "radiobuttonlist": // Placeholder - MAUI doesn't have a direct RadioButtonList
                    var radioLayout = new VerticalStackLayout();
                    radioLayout.Children.Add(new Label { Text = "(Radio Button List - Placeholder)" });
                    // Add dummy radio buttons
                    radioLayout.Children.Add(new RadioButton { Content = "Option A", GroupName = name });
                    radioLayout.Children.Add(new RadioButton { Content = "Option B", GroupName = name });
                    control = radioLayout;
                    break;

                // Add cases for other types if needed

                default:
                    // Maybe just display the text if type is unknown
                    container.Children.Add(new Label { Text = $"(Unsupported Type: {type})", FontAttributes = FontAttributes.Italic });
                    control = null;
                    break;
            }

            if (control != null)
            {
                container.Children.Add(control);
                if (isMandatory)
                {
                    // Add visual indicator (e.g., asterisk) to the label
                    if (container.Children[0] is Label fieldLabel)
                    {
                        fieldLabel.Text += " *";
                    }
                    mandatoryFields.Add(control); // Add the actual input control for validation
                }
            }

            return container; // Return the container holding the label and the control
        }

        private async Task<string> ReadJsonFile(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Resource not found: {resourceName}");
                    return null;
                }
                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private async void OnValidateButtonClicked(object sender, EventArgs e)
        {
            bool allValid = true;
            List<string> errorMessages = new List<string>();

            // Find the current page's controls
            var button = sender as Button;
            var currentPage = button?.Parent?.Parent?.Parent as ContentPage; // Adjust based on actual hierarchy
            if (currentPage == null) return;

            var controlsToCheck = FindControlsRecursive(currentPage.Content);

            foreach (var control in controlsToCheck)
            {
                if (!mandatoryFields.Contains(control)) continue; // Only check mandatory fields on this page

                bool fieldValid = true;
                string controlText = ""; // Get the label text associated with the control
                if (control.Parent is Layout layout && layout.Children.Count > 0 && layout.Children[0] is Label label)
                {
                    controlText = label.Text.Replace(" *", "");
                }

                if (control is Entry entry)
                {
                    if (string.IsNullOrWhiteSpace(entry.Text))
                    {
                        fieldValid = false;
                    }
                }
                else if (control is Picker picker)
                {
                    if (picker.SelectedIndex == -1)
                    {
                        fieldValid = false;
                    }
                }
                else if (control is DatePicker datePicker)
                {
                    // Basic check, might need more specific validation
                    if (datePicker.Date == datePicker.MinimumDate) // Or check against a specific default/null value if applicable
                    {
                       // fieldValid = false; // DatePicker always has a value, validation might differ
                    }
                }
                else if (control is CheckBox checkBox)
                {
                    // Checkboxes might not need validation unless specifically required to be checked
                }
                else if (control is Button buttonControl) // For placeholder controls like Photo/GPS
                {
                    // Add specific validation logic if these become actual inputs
                    // For now, assume they are valid if present, or add logic based on actual implementation
                }
                // Add checks for other mandatory control types here

                if (!fieldValid)
                {
                    allValid = false;
                    errorMessages.Add($"Field 	'{controlText}' is required.");
                }
            }

            if (!allValid)
            {
                await DisplayAlert("Validation Failed", string.Join("\n", errorMessages), "OK");
            }
            else
            {
                await DisplayAlert("Validation Successful", "All mandatory fields on this page are filled.", "OK");
            }
        }

        // Helper to find all relevant controls within a page/layout
        private List<View> FindControlsRecursive(Element element)
        {
            var controls = new List<View>();
            if (element is View view && (view is Entry || view is Picker || view is DatePicker || view is CheckBox || view is Button /* Add other types */))
            {
                controls.Add(view);
            }

            if (element is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    controls.AddRange(FindControlsRecursive(child));
                }
            }
            else if (element is ContentPage contentPage)
            {
                 if (contentPage.Content != null)
                    controls.AddRange(FindControlsRecursive(contentPage.Content));
            }
            else if (element is ScrollView scrollView)
            {
                 if (scrollView.Content != null)
                    controls.AddRange(FindControlsRecursive(scrollView.Content));
            }
            // Add checks for other container types if necessary

            return controls;
        }
    }
}

