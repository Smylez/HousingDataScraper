using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Drawing;
using HtmlAgilityPack;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using static System.Net.WebRequestMethods;
using System.Resources;
using WebScraper;
using System.Threading.Tasks;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        //TODO - move these to where they're actually used instead of all at the top
        string filePath = "C:\\Users\\Flip\\Downloads";
        string baseURL = "https://www.kijiji.ca";
        string cardUrl;
        bool isFiltered;
        int pageScrapeDelaytimer = 1000;

        Console.WriteLine("Please enter the city:");
        string locationInput = Console.ReadLine();
        string locationKey = Locations.ResourceManager.GetString(locationInput.ToUpper());

        //TODO - put all this in GUI 
        //keep in mind some locations are displayed oddly (ie: New Brunswick is new-brunswick)

        Console.WriteLine("Please the number of bedrooms between 1 & 4, enter 0 for bachelor/studio:");
        string roomsInput = Console.ReadLine();
        if(!int.TryParse(roomsInput, out int numberOfRooms))
        {
            Console.WriteLine("Input is not valid. Please enter a number.");
            throw new Exception("Input is not valid. Please enter a number.");
        }
        if (numberOfRooms > 4 || numberOfRooms < 0)
        {
            Console.WriteLine("Input is not valid. Please choose a number between 0 & 4");
            throw new Exception("Input is not valid. Please choose a number between 0 & 4");
        }

        string categoryInput = "RealEstate";
        string categoryKey = Categories.ResourceManager.GetString(categoryInput.ToUpper());

        //TODO - this needs update since the location key changes
        //don't need a27949001 unless filtering
        //Filtering to long term rentals only for now
        string filteredUrl = baseURL + "/b-apartments-condos/";

        if (numberOfRooms == 0) {
            roomsInput = "bachelor+studio";
            filteredUrl = filteredUrl + locationInput + "/" + roomsInput + "/c" + categoryKey + "l" + locationKey + "a27949001?ad=offer";//"a27949001";//"/c37l1700018a27949001";
        }else if(numberOfRooms == 1){
            filteredUrl = filteredUrl + locationInput + "/" + numberOfRooms + "+bedroom" + "/c" + categoryKey + "l" + locationKey + "a27949001?ad=offer";//"a27949001";//c37l1700018a27949001";
        }
        else
        {
            filteredUrl = filteredUrl + locationInput + "/" + numberOfRooms + "+bedrooms" + "/c" + categoryKey + "l" + locationKey + "a27949001?ad=offer";//"a27949001";//c37l1700018a27949001";
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                //Fake the user agent
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";          
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                
                ///pagination start
                //Get the html
                //only gets first page, need to find a way to know how many pages of results are there in advance
                string html = await client.GetStringAsync(filteredUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                //Find the main container that holds all the listing cards.
                string listingContainerNodeString = "//ul[@data-testid='srp-search-list']";              
                var listingContainer = doc.DocumentNode.SelectSingleNode(listingContainerNodeString);
                if (listingContainer != null)
                {
                    //Find all the listing cards within the container.
                    string listingCardNodeString = ".//li[starts-with(@data-testid, 'listing-card-list-item-')]";
                    var listingCards = listingContainer.SelectNodes(listingCardNodeString);

                    if (listingCards != null)
                    {
                        //Create a list to store the scraped data.
                        var scrapedData = new List<(string price, string location, string postingDate, string listingDetailsURL, string address)>();

                        foreach (var card in listingCards)
                        {
                            //Extract listing price and location.
                            string priceNodeString = ".//p[@data-testid='listing-price']";
                            var priceNode = card.SelectSingleNode(priceNodeString);
                            string locationNodeString = ".//p[@data-testid='listing-location']";
                            var locationNode = card.SelectSingleNode(locationNodeString);
                            string price = priceNode?.InnerText.Trim() ?? "N/A";
                            string location = locationNode?.InnerText.Trim() ?? "N/A";

                            //Link to details page
                            string listingDetailsURLNodeString = ".//a[@data-testid='listing-link']";
                            var listingDetailsURLNode = card.SelectSingleNode(listingDetailsURLNodeString);
                            string listingDetailsURL = baseURL + listingDetailsURLNode?.GetAttributeValue("href", string.Empty);

                            //Navigate to each card to get address info
                            //Add 500 ms delay here to prevent ban detection
                            await Task.Delay(500);
                            string detailsHTML = await client.GetStringAsync(listingDetailsURL);
                            var detailsDoc = new HtmlDocument();
                            detailsDoc.LoadHtml(detailsHTML);

                            //Get address and posting date
                            var detailsContainer = detailsDoc.DocumentNode.SelectSingleNode("//div[@id='mainPageContent']");
                            string addressNodeString = "//div[contains(@class, 'locationContainer-')]//span[@itemprop='address']";
                            var addressNode = detailsContainer.SelectSingleNode(addressNodeString);
                            string postingDateNodeString = "//div[contains(@class, 'datePosted-')]";
                            var postingDateNode = detailsContainer.SelectSingleNode(postingDateNodeString);
                            string address = addressNode?.InnerText.Trim() ?? "N/A";

                            //Alternate markers
                            //string startMarker = "datetime=\"";
                            //string endMarker = "Z\" title";

                            //Posting date substring manipulation
                            string startMarker = "title=\"";
                            string endMarker = "\">";
                            string postingDate = "N/A";
                            //detects ads that have been taken down recently, this can cause an exception
                            if (postingDateNode != null)
                            {
                                int startIndex = postingDateNode.InnerHtml.IndexOf(startMarker);
                                //Get endof index position to start
                                startIndex = startIndex + startMarker.Length;
                                //Get positon to end the substring
                                int endIndex = postingDateNode.InnerHtml.IndexOf(endMarker, startIndex);
                                // Calculate the length of the datetime string to return
                                int datetimeLength = endIndex - startIndex;
                                
                                if (datetimeLength > 0)
                                {
                                    // Extract the datetime value
                                    postingDate = postingDateNode?.InnerHtml.Substring(startIndex, datetimeLength).Trim() ?? "N/A";
                                }
                            }
                            else
                            {
                                postingDate = "Taken Down";
                            }
                            scrapedData.Add((price, location, postingDate, listingDetailsURL, address));
                        }
                        //TODO - add pagination before export call, add 1000ms delay before each page scrape to mimic reasonable user input
                        /*  const firstResultPageURL = await this.getFirstResultPageURL(params);

                        // Specify page number. It must be the last path component of the URL
                        const url = firstResultPageURL.replace(LOCATION_REGEX, `$1/page-${pageNum}$2`);
                        
                         isLastPage: body.indexOf("pagination-next-link") === -1*/

                        //Export the scraped data to an Excel file.
                        ExportToExcel(scrapedData, filePath+"\\ScrapedData.xlsx", roomsInput);
                    }
                    else
                    {
                        Console.WriteLine("No listing cards found on the page.");
                    }
                }
                else
                {
                    Console.WriteLine("No listing container found on the page.");
                }
                ///pagination end
                ///put export here instead after pagination working. That will collect all the records before exporting
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error: {ex.Message}");
            }
        }
    }

    static void ExportToExcel(List<(string price, string location, string postingDate, string listingDetailsURL, string address)> data, string filePath, string roomsInput)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Scraped Data");
            worksheet.Cells["A2"].Value = "Location";
            worksheet.Cells["B2"].Value = "Address";
            worksheet.Cells["C2"].Value = "Price";
            worksheet.Cells["D2"].Value = "Posting Date";

            int row = 3;
            
            //Style the title cell with a background color.
            var titleCell = worksheet.Cells["A1"];
            titleCell.Value = roomsInput + " Bedroom rentals";
            titleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //titleCell.Style.Fill.BackgroundColor.SetColor(blue);

            //TODO: should I filter out sponsored ads?
            //const filteredAds = allAds.filter(ad => ad.adSource === "ORGANIC");
            //Filter out rows where the price is "Please Contact"
            data.RemoveAll(item => item.price.Equals("Please Contact", StringComparison.OrdinalIgnoreCase));
            //Filter out useless records
            data.RemoveAll(item => item.address.Equals("N/A", StringComparison.OrdinalIgnoreCase));

            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.location;
                worksheet.Cells[row, 2].Value = item.address;
               
                if (decimal.TryParse(item.price, System.Globalization.NumberStyles.Currency, null, out decimal parsedPrice))
                {
                    worksheet.Cells[row, 3].Value = parsedPrice;
                }
                worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";

                if (DateTime.TryParse(item.postingDate, out DateTime parsedDate))
                {
                    worksheet.Cells[row, 4].Value = parsedDate;
                }
                worksheet.Cells[row, 4].Style.Numberformat.Format = "yyyy-MM-dd";

                row++;
            }
            //Calculate the average price
            decimal averagePrice = CalculateAveragePrice(data);
            //Insert the average price in the last cell
            worksheet.Cells[row, 1].Value = "Average Price";
            worksheet.Cells[row, 2].Value = averagePrice;
            //Apply currency format
            worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";

            //Format the table.
            var tableRange = worksheet.Cells["A2:D" + row];
            var table = worksheet.Tables.Add(tableRange, "ScrapedDataTable");
            table.TableStyle = TableStyles.Light16;

            package.SaveAs(new FileInfo(filePath));
            Console.WriteLine($"Data exported to {filePath}");
        }
    }
    //Calculate the average price
    static decimal CalculateAveragePrice(List<(string Price, string Location, string postingDate, string listingDetailsURL, string address)> data)
    {
        decimal total = 0;
        int count = 0;

        foreach (var item in data)
        {
            if (decimal.TryParse(item.Price.Replace("$", "").Replace(",", ""), out decimal price))
            {
                total += price;
                count++;
            }
        }
        return count > 0 ? total / count : 0;
    }
}
