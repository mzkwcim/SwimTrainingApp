using SwimTrainingApp.Data;
using System.Data;

namespace SwimTrainingApp.Services
{
    public class Scraper
    {
        public static async Task ScrapeAndSaveData(string url, ClubRecordsDbContext dbContext)
        {
            var htmlDocument = Loader(url);

            string distance = DataConversion.StrokeTranslation(
                DataChecker.LapChecker(htmlDocument.DocumentNode.SelectSingleNode("//td[@class='titleCenter']").InnerText));

            if (!string.IsNullOrEmpty(distance))
            {
                var record = new ClubRecord
                {
                    Distance = distance,
                    AthleteName = htmlDocument.DocumentNode.SelectSingleNode("//td[@class='fullname']").InnerText,
                    ReadableTime = DataConversion.TextSanitizer(htmlDocument.DocumentNode.SelectSingleNode("//td[@class='time']").InnerText),
                    Date = DataConversion.DateTranslation(htmlDocument.DocumentNode.SelectSingleNode("//td[@class='date']").InnerText),
                    City = htmlDocument.DocumentNode.SelectSingleNode("//td[@class='city']").InnerText.Replace("&nbsp;", " "),
                    Time = DataConversion.ConvertToDouble(
                        DataConversion.TextSanitizer(htmlDocument.DocumentNode.SelectSingleNode("//td[@class='time']").InnerText))
                };

                await dbContext.ClubRecords.AddAsync(record);
                await dbContext.SaveChangesAsync();
            }
        }

    }
}
