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

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        string filePath = "C:\\Users\\Flip\\Downloads";
        string locationInput;
        string locationKey;
        string categoryInput;
        string categoryKey;
        string roomsInput;
        int numberOfRooms;
        string baseURL = "https://www.kijiji.ca";
        //Filtering to long term rentals only for now
        string filteredUrl = baseURL + "/b-apartments-condos/";
        string cardUrl;
        bool isFiltered;
        string html;
        string detailsHTML;
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        string price;
        string location;
        string postingDate;
        string address;
        string listingDetailsURL;
        string listingContainerNodeString = "//ul[@data-testid='srp-search-list']";
        string listingCardNodeString = ".//li[starts-with(@data-testid, 'listing-card-list-item-')]";
        //string listingCardLinkString = ".//a[@data-testid='listing-link']";
        string priceNodeString = ".//p[@data-testid='listing-price']";
        string locationNodeString = ".//p[@data-testid='listing-location']";
        string postingDateNodeString = ".//p[@data-testid='listing-date']";
        string listingDetailsURLNodeString = ".//a[@data-testid='listing-link']";
        string addressNodeString = ".//span[@itemprop='address']";
        int pageScrapeDelaytimer = 1000;
        int adDetailsScrapeDelaytimer = 500;
        string startMarker = "datetime=\"";
        string endMarker = "Z\" title";
        int startIndex;
        int endIndex;
        int datetimeLength;

        Console.WriteLine("Please enter the city:");
        locationInput = Console.ReadLine();
        locationKey = Locations.ResourceManager.GetString(locationInput.ToUpper());
        ////Hardcoding these values for now, not sure how Kijiji generates them but they're location specific
        //if (locationInput.ToUpper() == "FREDERICTON")
        //{
        //    locationKey = "c37l1700018";
        //}
        //if (locationInput.ToUpper() == "MONCTON")
        //{
        //    locationKey = "c37l1700001";
        //}
        //else
        //{
        //    locationKey = "";
        //}

        //TODO - put all this in GUI 
        //keep in mind some locations are displayed oddly (ie: New Brunswick is new-brunswick)

        Console.WriteLine("Please the number of bedrooms between 1 & 4, enter 0 for bachelor/studio:");
        roomsInput = Console.ReadLine();
        if(!int.TryParse(roomsInput, out numberOfRooms))
        {
            Console.WriteLine("Input is not valid. Please enter a number.");
            throw new Exception("Input is not valid. Please enter a number.");
        }
        if (numberOfRooms > 4 || numberOfRooms < 0)
        {
            Console.WriteLine("Input is not valid. Please choose a number between 0 & 4");
            throw new Exception("Input is not valid. Please choose a number between 0 & 4");
        }

        categoryInput = "RealEstate";
        categoryKey = Categories.ResourceManager.GetString(categoryInput.ToUpper());

        //TODO - this needs update since the location key changes
        //don't need a27949001 unless filtering
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
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
 
                //Get the html
                //only gets first page, need to find a way to know how many pages of results are there in advance
                html = await client.GetStringAsync(filteredUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                //Find the main container that holds all the listing cards.
                var listingContainer = doc.DocumentNode.SelectSingleNode(listingContainerNodeString);
                if (listingContainer != null)
                {
                    //Find all the listing cards within the container.
                    var listingCards = listingContainer.SelectNodes(listingCardNodeString);

                    if (listingCards != null)
                    {
                        //Create a list to store the scraped data.
                        var scrapedData = new List<(string price, string location, string postingDate, string listingDetailsURL, string address)>();

                        foreach (var card in listingCards)
                        {
                            //Extract listing price and location.
                            var priceNode = card.SelectSingleNode(priceNodeString);
                            var locationNode = card.SelectSingleNode(locationNodeString);
                            price = priceNode?.InnerText.Trim() ?? "N/A";
                            location = locationNode?.InnerText.Trim() ?? "N/A";

                            //Link to details page
                            var listingDetailsURLNode = card.SelectSingleNode(listingDetailsURLNodeString);
                            listingDetailsURL = baseURL + listingDetailsURLNode?.GetAttributeValue("href", string.Empty);
                                                       
                            //TODO - navigate to each card to get address info (do we need to rate limit?)
                            //var cardURL = card.SelectSingleNode(listingCardLinkString);
                            //add delay here
                            detailsHTML = await client.GetStringAsync(listingDetailsURL);
                            var detailsDoc = new HtmlDocument();
                            detailsDoc.LoadHtml(detailsHTML);

                            //Get address and posting date
                            var detailsContainer = detailsDoc.DocumentNode.SelectSingleNode("//div[@id='mainPageContent']");
                            var addressNode = detailsContainer.SelectSingleNode("//div[contains(@class, 'locationContainer-')]//span[@itemprop='address']");
                            var postingDateNode = detailsContainer.SelectSingleNode("//div[contains(@class, 'datePosted-')]");
                            address = addressNode?.InnerText.Trim() ?? "N/A";

                            //Posting date substring manipulation
                            startIndex = postingDateNode.InnerHtml.IndexOf(startMarker);
                            //Get endof index position to start
                            startIndex = startIndex + startMarker.Length;
                            //Get positon to end the substring
                            endIndex = postingDateNode.InnerHtml.IndexOf(endMarker, startIndex);
                            // Calculate the length of the datetime string to return
                            datetimeLength = endIndex - startIndex;
                            // Extract the datetime value
                            postingDate = postingDateNode?.InnerHtml.Substring(startIndex, datetimeLength).Trim() ?? "N/A";

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
            worksheet.Cells["B2"].Value = "Price";
            worksheet.Cells["C2"].Value = "Posting Date";
            worksheet.Cells["D2"].Value = "Address";

            int row = 3;
            
            //Style the title cell with a background color.
            var titleCell = worksheet.Cells["A1"];
            titleCell.Value = roomsInput + " Bedroom rentals";
            titleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //titleCell.Style.Fill.BackgroundColor.SetColor(blue);

            //TODO: should I filter out sponsored ads?
            // const filteredAds = allAds.filter(ad => ad.adSource === "ORGANIC");
            // Filter out rows where the price is "Please Contact"
            data.RemoveAll(item => item.price.Equals("Please Contact", StringComparison.OrdinalIgnoreCase));
            
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.location;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                if (decimal.TryParse(item.price, System.Globalization.NumberStyles.Currency, null, out decimal parsedPrice))
                {
                    worksheet.Cells[row, 2].Value = parsedPrice;
                }
                //worksheet.Cells[row, 2].Value = decimal.TryParse(item.Price, System.Globalization.NumberStyles.Currency, null, out decimal result) ? result : item.Price;
                worksheet.Cells[row, 3].Value = item.postingDate;
                worksheet.Cells[row, 3].Value = item.address;

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
            var tableRange = worksheet.Cells["A2:B" + row];
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
