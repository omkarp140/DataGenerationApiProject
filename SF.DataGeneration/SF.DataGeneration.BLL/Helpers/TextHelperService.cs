using SF.DataGeneration.Models.Enum;
using System.Text;

namespace SF.DataGeneration.BLL.Helpers
{
    //public interface ITextHelperService
    //{
    //    string CleanTextExtractedFromPdf(string text);

    //}

    public static class TextHelperService
    {
        public static string CleanTextExtractedFromPdf(string text)
        {
            text = text.Replace("\r\n", "</br>");
            text = text.Replace("\n", "</br>");
            return text;
        }

        public static string GenerateRandomString(string inputString)
        {
            const string alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string alphabeticChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string numericChars = "0123456789";

            bool containsAlphabets = inputString.Any(char.IsLetter);
            bool containsNumbers = inputString.Any(char.IsDigit);

            if (containsAlphabets && !containsNumbers)
            {
                return GenerateRandomStringOfType(alphabeticChars, inputString.Length);
            }
            else if (!containsAlphabets && containsNumbers)
            {
                return GenerateRandomStringOfType(numericChars, inputString.Length);
            }
            else
            {
                return GenerateRandomStringOfType(alphanumericChars, inputString.Length);
            }
        }

        private static string GenerateRandomStringOfType(string chars, int length)
        {
            var random = new Random();
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }

        private static DateTime RandomDateTime(DateTime startDate, DateTime endDate)
        {
            Random random = new Random();
            int range = (endDate - startDate).Days;
            return startDate.AddDays(random.Next(range));
        }

        public static string GenerateReadbleRandomData(string randomDataType)
        {
            var _random = new Random();
            var nameOptions = new string[] { "Aarav", "Aanya", "Gagan", "Diya", "Aryan", "Mira", "Devan", "Elina", "Gauti", "Janvi",
                                            "Kunal", "Ishna", "Preet", "Trish", "Mohit", "Nehal", "Mukul", "Pooja", "Kiran", "Priya",
                                            "Rakes", "Shana", "Rahul", "Sakhi", "Alok", "Simmi", "Vikas", "Tanvi", "Sagar", "Tina",
                                            "Samir", "Tisha", "Amol", "Yammi", "Sunny", "Zara", "Siddh", "Anika", "Yuvan", "Ishaa",
                                            "Aashi", "Rishi", "Drishti", "Lucky", "Roopa", "Sahil", "Juhi", "Vishu", "Naina", "Aarti",
                                            "Rahil", "Mansi", "Shiva", "Anaya", "Rishi", "Pari", "Jaiya", "Nikku", "Samar", "Aisha",
                                            "Ankit", "Aaroh", "Faiza", "Jyoti", "Vedha", "Lisha", "Rohit", "Mithu", "Raman", "Ishra",
                                            "Manny", "Dinky", "Suman", "Swati", "Preet", "Anuva", "Rohan", "Hansa", "Pinky", "Misha",
                                            "Varun", "Yashi", "Saara", "Kavya", "Zaina", "Rishu", "Nihar", "Hemal", "Jatin", "Sange",
                                            "Shalu", "Vicky", "Bhavy", "Deepa", "Rahul", "Mansi", "Vinit", "Nidhi", "Ashna", "Vikku",
                                            "Aaran", "Aaren", "Aarez", "Aarman", "Aaron", "Aarron", "Aaryan", "Aaryn", "Aayan", "Aazaan", "Abaan", "Abbas"};

            var emailHelperOptions = new string[] { ".", "_" };

            var emailExt = new string[] { "@gmail.com", "@outlook.com", "@icloud.com", "@yahoo.com", "@simplifai.ai", "@hotmail.com",
                                          "@aol.com", "@protonmail.com", "@mail.com", "@zoho.com", "@yandex.com", "@fastmail.com",
                                          "@rediffmail.com", "@gmx.com", "@mailinator.com", "@inbox.com", "@rocketmail.com", "@yahoo.co.uk",
                                          "@outlook.in", "@live.com", "@yopmail.com", "@tutanota.com", "@mail.ru" };

            var prefixes = new string[] { "Evo", "Lunar", "Zephyr", "Chrono", "Sol", "Inferno", "Cryo", "Seren", "Vortex", "Aurora",
                                               "Blaze", "Volt", "Celestial", "Tempest", "Sapphire", "Radiant", "Dynamo", "Ember", "Thunder",
                                               "Galaxy", "Mystic", "Nebula", "Astral", "Haze", "Crimson", "Zodiac", "Orbit", "Luminous",
                                               "Eclipse", "Quasar", "Whirlwind", "Pulsar", "Spectral", "Enigma", "Frost", "Stellar", "Solar",
                                               "Lithium", "Plasma", "Aether", "Comet", "Electron", "Eternal", "Fusion", "Nuclear", "Synth",
                                               "Umbra", "Venom", "Zenith" };

            var buildingNames = new string[] { "Sky Tower", "City Center", "Liberty Plaza", "Tech Park", "Crystal Heights", "Summit Place",
                                               "Harmony Gardens", "Emerald View", "Innovation Hub", "Ocean View", "Horizon Square", "Sunrise Court",
                                               "Evergreen Estate", "Golden Gate", "Diamond Ridge", "Sunlight Gardens", "Lakeside Manor",
                                               "Crown Plaza", "Maple Grove", "Silver Oaks", "Riverfront Court", "Palm Paradise", "Bayside Residences",
                                               "Grand Boulevard", "Harbor Point", "Whispering Pines", "Vista Valley", "Meadowbrook Village",
                                               "Parkside Terrace", "Azure Heights", "Serenity Springs", "Marina Shores", "Mountain View", "Twin Oaks",
                                               "Majestic Gardens", "Birchwood Place", "Regal Ridge", "Sapphire Meadows", "Willowbrook Estate", "Tranquil Haven",
                                               "Amber Ridge", "Hillcrest Court", "Ocean Breeze", "Harvest Fields", "Royal Reserve", "Greenfield Heights", "Breezy Heights" };

            var areaNames = new string[] { "Bandra", "Juhu", "Koramangala", "Malad", "Connaught", "Andheri", "Saket", "Alipore", "Banjara", "Fort",
                                           "Chelsea", "Venice", "Copacabana", "Camden", "Montmartre", "Malabar", "Marais", "Pudong", "Darlinghurst",
                                           "Georgetown", "Wynwood", "Montecito", "Shinjuku", "Gracia", "Thamel", "Vesterbro", "Fitzroy", "Kreuzberg",
                                           "Shoreditch", "Södermalm", "Gastown", "Palermo", "Kiyosumi", "Frelard", "Grünerløkka", "Xuhui", "Cihangir",
                                           "Haga", "Kallio", "Miraflores", "Sheung", "Gion", "Kypseli", "Thonglor", "Kalamaja", "Salthill", "Greenpoint" };

            var cityNames = new string[] { "Delhi", "Mumbai", "Pune", "Salem", "Indore", "Agra", "Patna", "Ranchi", "Bhopal", "Surat",
                                           "Ajmer", "Latur", "Sagar", "Bikaner", "Kolar", "Hinda", "Adoni", "Baram", "Hosur", "Kapur",
                                           "Gonda", "Lanka", "Anand", "Sindh", "Hansi", "Rajah", "Ropar", "Nizam", "Mauni", "Deesa",
                                           "Mundi", "Chitt", "Basti", "Mandi", "Kadap", "Pilib", "Raiga", "Dharm", "Mursh", "Jhars",
                                           "Tiruv", "Bhadr", "Girid", "Nager", "Katra", "Kasau", "Sawai", "Hosur", "Bhadr", "Nawas",
                                           "Delhi", "Mumbai", "Pune", "Salem", "Indore", "Agra", "Patna", "Ranchi", "Bhopal", "Surat",
                                           "Ajmer", "Latur", "Sagar", "Bikaner", "Kolar", "Hinda", "Adoni", "Baram", "Hosur", "Kapur",
                                           "Gonda", "Lanka", "Anand", "Sindh", "Hansi", "Rajah", "Ropar", "Nizam", "Mauni", "Deesa",
                                           "Mundi", "Chitt", "Basti", "Mandi", "Kadap", "Pilib", "Raiga", "Dharm", "Mursh", "Jhars",
                                           "Tiruv", "Bhadr", "Girid", "Nager", "Katra", "Kasau", "Sawai", "Hosur", "Bhadr", "Nawas",
                                           "Hangzhou", "Kingston", "Valletta", "Salvador", "Surabaya", "Adelaide", "Hamilton", "Lausanne",
                                           "Brisbane", "Beijing", "Sofia", "Managua", "Belgrade", "Tbilisi", "Bogota", "Marrakech", "Marbella",
                                           "Florence", "Pretoria", "Canberra", "Helsinki", "Detroit", "Colombo", "Houston", "Nairobi", "Medellin",
                                           "Casablanca", "Hangzhou", "Curitiba", "Santiago", "NewYork","London","Tokyo","Paris","Mumbai",
                                           "Sydney","Shanghai","CapeTown","Rio","Barcelona","LosAngeles","HongKong","Berlin","Amsterdam",
                                           "Singapore","Rome","Toronto","Dubai","Seoul","Vienna","Stockholm","SanFrancisco","Prague",
                                           "Budapest","Bangkok","Copenhagen","Moscow","Dublin","Zurich","Wellington","Amman","Helsinki",
                                           "Reykjavik","Oslo","Jerusalem","KualaLumpur","Lisbon","Montreal","Warsaw","Athens","Nairobi",
                                           "Auckland","Cairo","BuenosAires","Delhi","Jakarta","Johannesburg","MexicoCity","Riyadh"};


            var designations = new string[] { "Data Engineer", "Software Developer", "Project Manager", "Sales Executive",
                                              "Marketing Specialist", "HR Manager", "Financial Analyst", "Product Manager",
                                              "Customer Support", "Business Analyst", "Graphic Designer", "Quality Assurance",
                                              "Operations Manager", "Content Writer", "IT Technician", "Research Analyst",
                                              "Executive Assistant", "Social Media","Web Designer", "System Administrator",
                                              "Security Analyst", "Technical Writer", "Network Engineer", "Data Scientist",
                                              "Machine Learning", "Product Owner", "Scrum Master", "Mobile Developer",
                                              "Java Developer", "Python Developer", "Ruby Developer", "C Developer",
                                              "PHP Developer", "iOS Developer", "Android Developer", "Game Developer",
                                              "UI Developer", "System Architect", "Software Architect", "Solution Architect",
                                              "Mobile Developer", "Java Developer", "Python Developer", "Ruby Developer",
                                              "Network Specialist", "C Developer", "PHP Developer", "iOS Developer",
                                              "Android Developer", "Game Developer", "UI Developer", "Network Security",
                                              "Cloud Architect", "Software Engineer", "Test Engineer", "Quality Engineer",
                                              "Network Analyst", "Quality Engineer", "Network Analyst", "Test Engineer" };


            if (Enum.TryParse(randomDataType, out RandomDataType dataType))
            {
                switch (dataType)
                {
                    case RandomDataType.Name:
                        return nameOptions[_random.Next(0, nameOptions.Length)];

                    case RandomDataType.EmailId:
                        return $"{nameOptions[_random.Next(0, nameOptions.Length)].ToLower()}{emailHelperOptions[_random.Next(0, emailHelperOptions.Length)]}{prefixes[_random.Next(0, prefixes.Length)].ToLower()}{_random.Next(0, 999)}{emailExt[_random.Next(0, emailExt.Length)]}";

                    case RandomDataType.Address:
                        return $"{_random.Next(100, 999)}, {buildingNames[_random.Next(0, buildingNames.Length)]}, {areaNames[_random.Next(0, areaNames.Length)]}, {cityNames[_random.Next(0, cityNames.Length)]} - {_random.Next(100000, 999999)}.";

                    case RandomDataType.Date:
                        var randomDate = RandomDateTime(new DateTime(1990, 1, 1), DateTime.Now).AddDays(-1);
                        string[] dateFormats = new string[]
                        {
                            "dd/MM/yyyy",
                            "MM/dd/yyyy",
                            "yyyy-MM-dd",
                            "dd-MMM-yyyy",
                            "yyyy/MM/dd",
                            "dd-MMMM-yyyy",
                            "MMMM-dd, yyyy"
                        };
                        return randomDate.ToString(dateFormats[_random.Next(0, dateFormats.Length)]);

                    case RandomDataType.UserId:
                        return $"{prefixes[_random.Next(0, prefixes.Length)]}{_random.Next(0, 1000)}";

                    case RandomDataType.MobileNo:
                        return $"+{_random.Next(0, 99)} {GenerateRandomStringOfType("0123456789", 10)}";

                    case RandomDataType.Designation:
                        return designations[_random.Next(0, designations.Length)];

                    default:
                        return "Unknown Data Type";
                }
            }
            else
            {
                return "Invalid Data Type";
            }
        }
    }
}
