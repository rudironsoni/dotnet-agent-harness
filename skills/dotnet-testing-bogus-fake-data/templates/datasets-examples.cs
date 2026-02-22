// =============================================================================
// Bogus DataSet Usage Examples
// Demonstrates complete usage of various built-in DataSets
// =============================================================================

using Bogus;
using FluentAssertions;
using Xunit;

namespace BogusDataSets.Templates;

// =============================================================================
// Complete DataSet Examples
// =============================================================================

public class DataSetExamples
{
    private readonly Faker _faker = new();

    #region Person DataSet

    /// <summary>
    /// Person DataSet - Personal Information
    /// </summary>
    [Fact]
    public void PersonDataSet_PersonalInformation()
    {
        // Person is pre-generated complete personal data
        var person = _faker.Person;

        // Basic information
        var fullName = person.FullName;        // Full name
        var firstName = person.FirstName;      // First name
        var lastName = person.LastName;        // Last name
        var userName = person.UserName;        // Username

        // Contact information
        var email = person.Email;              // Email
        var phone = person.Phone;              // Phone number
        var website = person.Website;          // Website

        // Personal attributes
        var gender = person.Gender;            // Gender (Male/Female)
        var dateOfBirth = person.DateOfBirth;  // Date of birth

        // Company information
        var company = person.Company;          // Company info object

        // Address information
        var address = person.Address;          // Address object

        // Validation
        fullName.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
        dateOfBirth.Should().BeBefore(DateTime.Now);
    }

    /// <summary>
    /// Name DataSet - Name related
    /// </summary>
    [Fact]
    public void NameDataSet_NameRelated()
    {
        var firstName = _faker.Name.FirstName();          // First name
        var lastName = _faker.Name.LastName();            // Last name
        var fullName = _faker.Name.FullName();            // Full name
        var prefix = _faker.Name.Prefix();                // Prefix (Mr., Ms., Dr.)
        var suffix = _faker.Name.Suffix();                // Suffix (Jr., Sr., III)
        var jobTitle = _faker.Name.JobTitle();            // Job title
        var jobDescriptor = _faker.Name.JobDescriptor();  // Job descriptor
        var jobArea = _faker.Name.JobArea();              // Job area

        firstName.Should().NotBeNullOrEmpty();
        jobTitle.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Address DataSet

    /// <summary>
    /// Address DataSet - Address Information
    /// </summary>
    [Fact]
    public void AddressDataSet_AddressInformation()
    {
        // Full address
        var fullAddress = _faker.Address.FullAddress();

        // Address components
        var streetAddress = _faker.Address.StreetAddress();    // Street address
        var secondaryAddress = _faker.Address.SecondaryAddress(); // Secondary address (apartment)
        var city = _faker.Address.City();                       // City
        var cityPrefix = _faker.Address.CityPrefix();           // City prefix
        var citySuffix = _faker.Address.CitySuffix();           // City suffix
        var state = _faker.Address.State();                     // State/Province
        var stateAbbr = _faker.Address.StateAbbr();             // State/Province abbreviation
        var zipCode = _faker.Address.ZipCode();                 // ZIP code
        var buildingNumber = _faker.Address.BuildingNumber();   // Building number
        var streetName = _faker.Address.StreetName();           // Street name
        var streetSuffix = _faker.Address.StreetSuffix();       // Street suffix

        // Country related
        var country = _faker.Address.Country();                 // Country
        var countryCode = _faker.Address.CountryCode();         // Country code

        // Geographic coordinates
        var latitude = _faker.Address.Latitude();               // Latitude
        var longitude = _faker.Address.Longitude();             // Longitude
        var direction = _faker.Address.Direction();             // Direction (North, South)
        var cardinalDirection = _faker.Address.CardinalDirection(); // Cardinal direction

        fullAddress.Should().NotBeNullOrEmpty();
        latitude.Should().BeInRange(-90, 90);
        longitude.Should().BeInRange(-180, 180);
    }

    #endregion

    #region Company DataSet

    /// <summary>
    /// Company DataSet - Company Information
    /// </summary>
    [Fact]
    public void CompanyDataSet_CompanyInformation()
    {
        var companyName = _faker.Company.CompanyName();        // Company name
        var companySuffix = _faker.Company.CompanySuffix();    // Company suffix (Inc., LLC)
        var catchPhrase = _faker.Company.CatchPhrase();        // Catch phrase
        var bs = _faker.Company.Bs();                          // Business jargon

        companyName.Should().NotBeNullOrEmpty();
        catchPhrase.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Commerce DataSet

    /// <summary>
    /// Commerce DataSet - Commerce Information
    /// </summary>
    [Fact]
    public void CommerceDataSet_CommerceInformation()
    {
        // Product information
        var productName = _faker.Commerce.ProductName();           // Product name
        var productAdjective = _faker.Commerce.ProductAdjective(); // Product adjective
        var productMaterial = _faker.Commerce.ProductMaterial();   // Product material
        var product = _faker.Commerce.Product();                   // Product type

        // Department and category
        var department = _faker.Commerce.Department();             // Department
        var categories = _faker.Commerce.Categories(3);            // Multiple categories

        // Price (returns string format)
        var price = _faker.Commerce.Price(1, 1000, 2);             // Price string
        var priceDecimal = _faker.Commerce.Price(1, 1000, 2, "$"); // With symbol

        // Barcode
        var ean8 = _faker.Commerce.Ean8();                         // EAN-8 barcode
        var ean13 = _faker.Commerce.Ean13();                       // EAN-13 barcode

        // Color
        var color = _faker.Commerce.Color();                       // Color name

        productName.Should().NotBeNullOrEmpty();
        department.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Internet DataSet

    /// <summary>
    /// Internet DataSet - Internet Information
    /// </summary>
    [Fact]
    public void InternetDataSet_InternetInformation()
    {
        // Email
        var email = _faker.Internet.Email();                          // Random email
        var emailWithName = _faker.Internet.Email("john", "doe");     // With specified name
        var exampleEmail = _faker.Internet.ExampleEmail();            // example.com email

        // Username and password
        var userName = _faker.Internet.UserName();                    // Username
        var userNameWithName = _faker.Internet.UserName("john", "doe"); // With specified name
        var password = _faker.Internet.Password();                    // Password
        var passwordLength = _faker.Internet.Password(16, false, "", "!@#"); // Custom password

        // URL
        var url = _faker.Internet.Url();                              // URL
        var urlWithProtocol = _faker.Internet.UrlWithPath();          // URL with path
        var domainName = _faker.Internet.DomainName();                // Domain name
        var domainWord = _faker.Internet.DomainWord();                // Domain word
        var domainSuffix = _faker.Internet.DomainSuffix();            // Domain suffix

        // IP addresses
        var ip = _faker.Internet.Ip();                                // IPv4
        var ipv6 = _faker.Internet.Ipv6();                            // IPv6
        var mac = _faker.Internet.Mac();                              // MAC address

        // Others
        var userAgent = _faker.Internet.UserAgent();                  // User Agent
        var protocol = _faker.Internet.Protocol();                    // Protocol (http/https)
        var port = _faker.Internet.Port();                            // Port

        // Avatar
        var avatar = _faker.Internet.Avatar();                        // Avatar URL

        email.Should().Contain("@");
        ip.Should().MatchRegex(@"^\d+\.\d+\.\d+\.\d+$");
    }

    #endregion

    #region Finance DataSet

    /// <summary>
    /// Finance DataSet - Financial Information
    /// </summary>
    [Fact]
    public void FinanceDataSet_FinancialInformation()
    {
        // Credit card
        var creditCardNumber = _faker.Finance.CreditCardNumber();     // Credit card number
        var creditCardCvv = _faker.Finance.CreditCardCvv();           // CVV

        // Account
        var account = _faker.Finance.Account();                       // Account number
        var accountName = _faker.Finance.AccountName();               // Account name
        var routingNumber = _faker.Finance.RoutingNumber();           // Routing number

        // Amount
        var amount = _faker.Finance.Amount(100, 10000, 2);            // Amount

        // Currency
        var currency = _faker.Finance.Currency();                     // Currency object

        // International bank
        var iban = _faker.Finance.Iban();                             // IBAN
        var bic = _faker.Finance.Bic();                               // BIC/SWIFT

        // Cryptocurrency
        var bitcoinAddress = _faker.Finance.BitcoinAddress();         // Bitcoin address
        var ethereumAddress = _faker.Finance.EthereumAddress();       // Ethereum address

        creditCardNumber.Should().NotBeNullOrEmpty();
        amount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Date DataSet

    /// <summary>
    /// Date DataSet - Date and Time
    /// </summary>
    [Fact]
    public void DateDataSet_DateAndTime()
    {
        // Past and future
        var past = _faker.Date.Past();                               // Within past year
        var pastYears = _faker.Date.Past(5);                         // Within past 5 years
        var future = _faker.Date.Future();                           // Within future year
        var futureYears = _faker.Date.Future(3);                     // Within future 3 years

        // Recent and soon
        var recent = _faker.Date.Recent();                           // Recent days
        var recentDays = _faker.Date.Recent(7);                      // Recent 7 days
        var soon = _faker.Date.Soon();                               // Soon
        var soonDays = _faker.Date.Soon(14);                         // Within 14 days

        // Range
        var between = _faker.Date.Between(
            DateTime.Now.AddYears(-1),
            DateTime.Now);                                           // Date within range

        // Birthday (specific age range)
        var birthday = _faker.Date.Past(50, DateTime.Now.AddYears(-18)); // 18-68 years old

        // Time related
        var timespan = _faker.Date.Timespan();                       // TimeSpan
        var weekday = _faker.Date.Weekday();                         // Day of week
        var month = _faker.Date.Month();                             // Month name

        // DateTimeOffset
        var pastOffset = _faker.Date.PastOffset();                   // DateTimeOffset
        var futureOffset = _faker.Date.FutureOffset();

        past.Should().BeBefore(DateTime.Now);
        future.Should().BeAfter(DateTime.Now);
    }

    #endregion

    #region Lorem DataSet

    /// <summary>
    /// Lorem DataSet - Text Content
    /// </summary>
    [Fact]
    public void LoremDataSet_TextContent()
    {
        // Word
        var word = _faker.Lorem.Word();                              // Word
        var words = _faker.Lorem.Words(5);                           // Multiple words

        // Sentence
        var sentence = _faker.Lorem.Sentence();                      // Sentence
        var sentenceWords = _faker.Lorem.Sentence(10);               // 10 word sentence
        var sentences = _faker.Lorem.Sentences(3);                   // Multiple sentences

        // Paragraph
        var paragraph = _faker.Lorem.Paragraph();                    // Paragraph
        var paragraphSentences = _faker.Lorem.Paragraph(5);          // 5 sentence paragraph
        var paragraphs = _faker.Lorem.Paragraphs(2);                 // Multiple paragraphs

        // Text block
        var text = _faker.Lorem.Text();                              // Text block
        var lines = _faker.Lorem.Lines();                            // Multi-line text

        // Slug
        var slug = _faker.Lorem.Slug();                              // URL-friendly string

        // Letters and characters
        var letter = _faker.Lorem.Letter();                          // Single letter
        var letters = _faker.Lorem.Letter(10);                       // Multiple letters

        word.Should().NotBeNullOrEmpty();
        sentence.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Phone DataSet

    /// <summary>
    /// Phone DataSet - Phone Numbers
    /// </summary>
    [Fact]
    public void PhoneDataSet_PhoneNumbers()
    {
        var phoneNumber = _faker.Phone.PhoneNumber();                // Phone number
        var phoneNumberFormat = _faker.Phone.PhoneNumberFormat();    // Formatted phone

        phoneNumber.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region System DataSet

    /// <summary>
    /// System DataSet - System Related
    /// </summary>
    [Fact]
    public void SystemDataSet_SystemRelated()
    {
        // File related
        var fileName = _faker.System.FileName();                     // File name
        var commonFileName = _faker.System.CommonFileName();         // Common file name
        var fileExt = _faker.System.FileExt();                       // Extension
        var commonFileExt = _faker.System.CommonFileExt();           // Common extension

        // MIME type
        var mimeType = _faker.System.MimeType();                     // MIME type
        var commonFileType = _faker.System.CommonFileType();         // Common file type

        // Path
        var filePath = _faker.System.FilePath();                     // File path
        var directoryPath = _faker.System.DirectoryPath();           // Directory path

        // Version
        var version = _faker.System.Version();                       // Version number
        var semver = _faker.System.Semver();                         // Semantic version

        // Android
        var androidId = _faker.System.AndroidId();                   // Android ID

        // Apple
        var applePushToken = _faker.System.ApplePushToken();         // Apple Push Token

        fileName.Should().NotBeNullOrEmpty();
        mimeType.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Random DataSet

    /// <summary>
    /// Random DataSet - Random Data
    /// </summary>
    [Fact]
    public void RandomDataSet_RandomData()
    {
        // Numbers
        var randomInt = _faker.Random.Int(1, 100);                   // Integer
        var randomLong = _faker.Random.Long(1, 1000000);             // Long
        var randomDecimal = _faker.Random.Decimal(0, 1000);          // Decimal
        var randomDouble = _faker.Random.Double(0, 100);             // Double
        var randomFloat = _faker.Random.Float(0, 10);                // Float
        var randomByte = _faker.Random.Byte();                       // Byte
        var randomShort = _faker.Random.Short(0, 1000);              // Short

        // Characters and strings
        var randomChar = _faker.Random.Char('a', 'z');               // Character
        var randomString = _faker.Random.String(10);                 // String
        var randomString2 = _faker.Random.String2(10, "abc123");     // With character set
        var alphanumeric = _faker.Random.AlphaNumeric(8);            // Alphanumeric

        // Boolean
        var randomBool = _faker.Random.Bool();                       // Boolean
        var weightedBool = _faker.Random.Bool(0.8f);                 // 80% true

        // GUID and Hash
        var randomGuid = _faker.Random.Guid();                       // GUID
        var randomUuid = _faker.Random.Uuid();                       // UUID
        var randomHash = _faker.Random.Hash();                       // Hash value
        var randomHexadecimal = _faker.Random.Hexadecimal(16);       // Hexadecimal

        // Enum
        var randomEnum = _faker.Random.Enum<DayOfWeek>();            // Random enum

        // Collection operations
        var array = new[] { "A", "B", "C", "D", "E" };
        var randomElement = _faker.Random.ArrayElement(array);       // Random array element
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var randomListItem = _faker.Random.ListItem(list);           // Random list item
        var shuffled = _faker.Random.Shuffle(array);                 // Shuffle

        // Take multiple
        var randomElements = _faker.Random.ArrayElements(array, 3);  // Take 3

        randomInt.Should().BeInRange(1, 100);
        randomGuid.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Vehicle DataSet

    /// <summary>
    /// Vehicle DataSet - Vehicle Information
    /// </summary>
    [Fact]
    public void VehicleDataSet_VehicleInformation()
    {
        var manufacturer = _faker.Vehicle.Manufacturer();            // Manufacturer
        var model = _faker.Vehicle.Model();                          // Model
        var type = _faker.Vehicle.Type();                            // Type
        var fuel = _faker.Vehicle.Fuel();                            // Fuel type
        var vin = _faker.Vehicle.Vin();                              // Vehicle identification number

        manufacturer.Should().NotBeNullOrEmpty();
        vin.Should().HaveLength(17);
    }

    #endregion

    #region Image DataSet

    /// <summary>
    /// Image DataSet - Image URLs
    /// </summary>
    [Fact]
    public void ImageDataSet_ImageUrls()
    {
        var imageUrl = _faker.Image.PicsumUrl();                     // Picsum image
        var loremFlickr = _faker.Image.LoremFlickrUrl();             // LoremFlickr
        var placeholder = _faker.Image.PlaceholderUrl();             // Placeholder

        // Specific category images
        var abstract_ = _faker.Image.Abstract();
        var animals = _faker.Image.Animals();
        var business = _faker.Image.Business();
        var cats = _faker.Image.Cats();
        var city = _faker.Image.City();
        var food = _faker.Image.Food();
        var nightlife = _faker.Image.Nightlife();
        var fashion = _faker.Image.Fashion();
        var people = _faker.Image.People();
        var nature = _faker.Image.Nature();
        var sports = _faker.Image.Sports();
        var technics = _faker.Image.Technics();
        var transport = _faker.Image.Transport();

        imageUrl.Should().StartWith("http");
    }

    #endregion

    #region Rant DataSet

    /// <summary>
    /// Rant DataSet - Reviews and Rants
    /// </summary>
    [Fact]
    public void RantDataSet_Reviews()
    {
        var review = _faker.Rant.Review();                           // Product review
        var reviews = _faker.Rant.Reviews(3);                        // Multiple reviews

        review.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Hacker DataSet

    /// <summary>
    /// Hacker DataSet - Technical Terms
    /// </summary>
    [Fact]
    public void HackerDataSet_TechnicalTerms()
    {
        var abbreviation = _faker.Hacker.Abbreviation();             // Abbreviation (TCP, HTTP)
        var adjective = _faker.Hacker.Adjective();                   // Adjective
        var noun = _faker.Hacker.Noun();                             // Noun
        var verb = _faker.Hacker.Verb();                             // Verb
        var ingverb = _faker.Hacker.IngVerb();                       // -ing verb
        var phrase = _faker.Hacker.Phrase();                         // Technical phrase

        phrase.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Database DataSet

    /// <summary>
    /// Database DataSet - Database Related
    /// </summary>
    [Fact]
    public void DatabaseDataSet_Database()
    {
        var column = _faker.Database.Column();                       // Column name
        var type = _faker.Database.Type();                           // Data type
        var collation = _faker.Database.Collation();                 // Collation
        var engine = _faker.Database.Engine();                       // Database engine

        column.Should().NotBeNullOrEmpty();
    }

    #endregion
}
