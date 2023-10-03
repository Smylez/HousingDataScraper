namespace WebScraper
{
    partial class WebScraperGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebScraperGUI));
            this.goButton = new System.Windows.Forms.Button();
            this.roomsFilterCombobox = new System.Windows.Forms.ComboBox();
            this.locationCombobox = new System.Windows.Forms.ComboBox();
            this.Disclaimer = new System.Windows.Forms.Label();
            this.loadingGif = new System.Windows.Forms.PictureBox();
            this.loadingLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.loadingGif)).BeginInit();
            this.SuspendLayout();
            // 
            // goButton
            // 
            this.goButton.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.goButton.Location = new System.Drawing.Point(100, 283);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(400, 66);
            this.goButton.TabIndex = 0;
            this.goButton.Text = "GO";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // roomsFilterCombobox
            // 
            this.roomsFilterCombobox.FormattingEnabled = true;
            this.roomsFilterCombobox.Location = new System.Drawing.Point(200, 245);
            this.roomsFilterCombobox.Name = "roomsFilterCombobox";
            this.roomsFilterCombobox.Size = new System.Drawing.Size(200, 21);
            this.roomsFilterCombobox.TabIndex = 1;
            this.roomsFilterCombobox.Text = "Number Of Rooms";
            this.roomsFilterCombobox.SelectedIndexChanged += new System.EventHandler(this.roomsFilterCombobox_SelectedIndexChanged);
            // 
            // locationCombobox
            // 
            this.locationCombobox.FormattingEnabled = true;
            this.locationCombobox.Location = new System.Drawing.Point(200, 208);
            this.locationCombobox.Name = "locationCombobox";
            this.locationCombobox.Size = new System.Drawing.Size(200, 21);
            this.locationCombobox.TabIndex = 3;
            this.locationCombobox.Text = "Location";
            this.locationCombobox.SelectedIndexChanged += new System.EventHandler(this.locationComboBox_SelectedIndexChanged);
            // 
            // Disclaimer
            // 
            this.Disclaimer.AutoSize = true;
            this.Disclaimer.Font = new System.Drawing.Font("Segoe UI", 12.25F);
            this.Disclaimer.Location = new System.Drawing.Point(27, 9);
            this.Disclaimer.Name = "Disclaimer";
            this.Disclaimer.Size = new System.Drawing.Size(533, 115);
            this.Disclaimer.TabIndex = 5;
            this.Disclaimer.Text = resources.GetString("Disclaimer.Text");
            // 
            // loadingGif
            // 
            this.loadingGif.Image = ((System.Drawing.Image)(resources.GetObject("loadingGif.Image")));
            this.loadingGif.Location = new System.Drawing.Point(178, 152);
            this.loadingGif.Name = "loadingGif";
            this.loadingGif.Size = new System.Drawing.Size(222, 203);
            this.loadingGif.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.loadingGif.TabIndex = 6;
            this.loadingGif.TabStop = false;
            this.loadingGif.Click += new System.EventHandler(this.loadingGif_Click);
            // 
            // loadingLabel
            // 
            this.loadingLabel.AutoSize = true;
            this.loadingLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadingLabel.Location = new System.Drawing.Point(204, 124);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new System.Drawing.Size(186, 25);
            this.loadingLabel.TabIndex = 7;
            this.loadingLabel.Text = "Loading Please Wait.";
            // 
            // WebScraperGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.loadingGif);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.roomsFilterCombobox);
            this.Controls.Add(this.locationCombobox);
            this.Controls.Add(this.Disclaimer);
            this.Controls.Add(this.loadingLabel);
            this.Name = "WebScraperGUI";
            this.Text = "Housing Data Scraper";
            this.Load += new System.EventHandler(this.GUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.loadingGif)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.ComboBox roomsFilterCombobox;
        private System.Windows.Forms.ComboBox locationCombobox;
        private System.Windows.Forms.Label Disclaimer;
        private System.Windows.Forms.PictureBox loadingGif;
        private System.Windows.Forms.Label loadingLabel;
    }
}