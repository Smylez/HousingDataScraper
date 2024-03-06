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
using System.IO;

namespace WebScraper
{
    public partial class WebScraperGUI : Form
    {
        public string locationInput;
        public string locationKey;
        public string roomsFilter;
        public string filePath;
        public WebScraperGUI()
        {
            InitializeComponent();
            locationCombobox.Validating += locationCombobox_Validating;
            goButton.Enabled = false;
        }

        private string GetFilePath()
        {
            string customFolderName = "ScrapedData";

            // Get the path to the user's Documents directory
            string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Combine the Documents directory and custom folder name to create the full path
            string customFolderPath = Path.Combine(documentsDirectory, customFolderName);

            // Ensure the custom folder exists; if not, create it
            if (!Directory.Exists(customFolderPath))
            {
                Directory.CreateDirectory(customFolderPath);
            }

            // Specify the desired file name
            string fileName = "ScrapedData" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

            // Combine the custom folder path and file name to create the full path for saving the file
            string fullPath = Path.Combine(customFolderPath, fileName);

            return fullPath;
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

            roomsFilterCombobox.Items.AddRange(new string[] { "All", "Bachelor", "1", "2", "3", "4" });

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
                    filePath = GetFilePath();
                    loadingLabel.Text = "Processing, Please Wait";
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
