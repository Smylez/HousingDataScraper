using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WebScraper
{
    public partial class WebScraperGUI : Form
    {
        public string locationInput;
        public string locationKey;
        public string roomsFilter;
        public string filePath = "C:\\Users\\Flip\\Downloads";
        public WebScraperGUI()
        {
            InitializeComponent();
            locationCombobox.Validating += locationCombobox_Validating;
            goButton.Enabled = false;
        }

        private void GUI_Load(object sender, EventArgs e)
        {
            var resourceItems = new List<Location>();
            ResourceManager resourceManager = new ResourceManager("Webscraper.Locations", typeof(WebScraperGUI).Assembly);
            var resourceSet = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);

            // Add a default item at the beginning of the list
            resourceItems.Add(new Location
            {
                DisplayText = "Select a location...",
                Value = string.Empty
            });

            if (resourceSet != null)
            {
                foreach (DictionaryEntry entry in resourceSet)
                {
                    var resourceItem = new Location
                    {
                        Value = entry.Value.ToString(),
                        DisplayText = entry.Key.ToString()
                    };
                    resourceItems.Add(resourceItem);
                }
            }
            // Bind the ComboBox to the list of ResourceItems
            locationCombobox.DataSource = resourceItems;
            locationCombobox.DisplayMember = "DisplayText";
            locationCombobox.ValueMember = "Value";

            roomsFilterCombobox.Items.AddRange(new string[] { "N/A", "Bachelor", "1", "2", "3", "4" });

            loadingGif.Visible = false;
            loadingLabel.Visible = false;
        }

        private void locationCombobox_Validating(object sender, CancelEventArgs e)
        {
            // Check if the default item is selected
            if (locationCombobox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a location.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Set e.Cancel to true to prevent focus from leaving the ComboBox
                e.Cancel = true;
            }
            else
            {     
                goButton.Enabled = true;
            }
        }

        private void locationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            locationInput = locationCombobox.Text.ToString();
            locationKey = locationCombobox.SelectedValue.ToString();
        }

        private void roomsFilterCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            roomsFilter = roomsFilterCombobox.SelectedItem.ToString();
        }

        private async void goButton_Click(object sender, EventArgs e)
        {
            {
                try
                {

                    loadingLabel.Text = "Processing...";
                    loadingGif.Visible = true;
                    loadingLabel.Visible = true;
                    // Disable the Go button to prevent multiple clicks during processing
                    goButton.Enabled = false;

                    // Call the web scraping method and pass any necessary parameters
                    await Task.Run(() => Program.RunWebScraper(new string[] { locationInput, locationKey, roomsFilter, filePath }));
                    //await Program.RunWebScraper(new string[] { locationInput, locationKey, roomsFilter, filePath });

                    loadingGif.Visible = false;
                    loadingLabel.Text = "Scraping completed!";
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    goButton.Enabled = true;
                }
            }
        }

        private void loadingGif_Click(object sender, EventArgs e)
        {

        }
    }
}
