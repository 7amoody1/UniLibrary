namespace BulkyBook.Utility;

public class StripeSettings
{
	public const string SectionName = "Strip";
	public string SecretKey { get; set; }
	public string PublishableKey { get; set; }
}