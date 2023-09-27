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

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        string filePath = "C:\\Users\\Flip\\Downloads";
        string locationInput;
        string locationKey;
        string roomsInput;
        string url;
        string html;

        Console.WriteLine("Please enter the city:");
        locationInput = Console.ReadLine();

        //Hardcoding these values for now, not sure how Kijiji generates them but they're location specific
        if (locationInput.ToUpper() == "FREDERICTON")
        {
            locationKey = "c37l1700018";
        }
        if (locationInput.ToUpper() == "MONCTON")
        {
            locationKey = "c37l1700001";
        }
        else
        {
            locationKey = "";
        }

        Console.WriteLine("Please the number of bedrooms between 1 & 4, enter 0 for bachelor/studio:");
        roomsInput = Console.ReadLine();
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

        if (numberOfRooms == 0) {
            roomsInput = "bachelor+studio";
            url = "https://www.kijiji.ca/b-apartments-condos/" + locationInput + "/" + roomsInput + "/" + locationKey;//"/c37l1700018a27949001";
        }
        else{
            url = "https://www.kijiji.ca/b-apartments-condos/" + locationInput + "/" + numberOfRooms + "+bedroom/" + locationKey;//c37l1700018a27949001";
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                //Fake the user agent
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
 
                //Get the html
                //only gets first page, need to find a way to know how many pages of results are there in advance
                html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                //Find the main container that holds all the listing cards.
                var listingContainer = doc.DocumentNode.SelectSingleNode("//ul[@data-testid='srp-search-list']");
                if (listingContainer != null)
                {
                    //Find all the listing cards within the container.
                    var listingContainers = listingContainer.SelectNodes(".//li[starts-with(@data-testid, 'listing-card-list-item-')]");

                    if (listingContainers != null)
                    {
                        //Create a list to store the scraped data.
                        var scrapedData = new List<(string Price, string Location)>();

                        foreach (var card in listingContainers)
                        {
                            //Extract listing price and location.
                            var priceNode = card.SelectSingleNode(".//p[@data-testid='listing-price']");
                            var locationNode = card.SelectSingleNode(".//p[@data-testid='listing-location']");

                            string price = priceNode?.InnerText.Trim() ?? "N/A";
                            string location = locationNode?.InnerText.Trim() ?? "N/A";

                            scrapedData.Add((price, location));
                        }

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

    static void ExportToExcel(List<(string Price, string Location)> data, string filePath, string roomsInput)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Scraped Data");
            worksheet.Cells["A2"].Value = "Location";
            worksheet.Cells["B2"].Value = "Price";

            int row = 3;
            
            //Style the title cell with a background color.
            var titleCell = worksheet.Cells["A1"];
            titleCell.Value = roomsInput + " Bedroom rentals";
            titleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //titleCell.Style.Fill.BackgroundColor.SetColor(blue);

            // Filter out rows where the price is "Please Contact"
            data.RemoveAll(item => item.Price.Equals("Please Contact", StringComparison.OrdinalIgnoreCase));
            
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.Location;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                if (decimal.TryParse(item.Price, System.Globalization.NumberStyles.Currency, null, out decimal parsedPrice))
                {
                    worksheet.Cells[row, 2].Value = parsedPrice;
                }
                //worksheet.Cells[row, 2].Value = decimal.TryParse(item.Price, System.Globalization.NumberStyles.Currency, null, out decimal result) ? result : item.Price;

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
    static decimal CalculateAveragePrice(List<(string Price, string Location)> data)
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
