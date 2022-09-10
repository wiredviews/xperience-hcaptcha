using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using CMS.Base.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using Kentico.Forms.Web.Mvc;
using Newtonsoft.Json;
using XperienceCommunity.HCaptcha.Components.FormComponents;

[assembly: RegisterFormComponent(
    HCaptchaFormComponent.IDENTIFIER,
    typeof(HCaptchaFormComponent),
    "{$xperiencecommunity.formbuilder.component.hcaptcha.name$}",
    Description = "{$xperiencecommunity.formbuilder.component.hcaptcha.description$}", IconClass = "icon-recaptcha", ViewName = "~/Components/FormComponents/HCaptchaFormComponent.cshtml")]

namespace XperienceCommunity.HCaptcha.Components.FormComponents;

/// <summary>
/// HCaptcha captcha form component.
/// </summary>
/// <remarks>
/// Large portions of functionality sourced from Kentico Xperience 13.0 source RecaptchaComponent
/// </remarks>
public class HCaptchaFormComponent : FormComponent<HCaptchaFormComponentProperties, string>
{
    /// <summary>
    /// <see cref="HCaptchaFormComponent"/> component identifier.
    /// </summary>
    public const string IDENTIFIER = "xperiencecommunity.formcomponent.hcaptcha";

    private static readonly Lazy<HashSet<string>> mFullLanguageCodes = new(() =>
    {
        string[] codes = new[] { "zh-HK", "zh-CN", "zh-TW", "en-GB", "fr-CA", "de-AT", "de-CH", "pt-BR", "pt-PT" };

        return new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
    });

    private string mLanguage = "";
    private bool? mSkipHCaptcha;


    /// <summary>
    /// Holds nothing and is here just because it is required.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// HCaptcha site key from https://dashboard.hcaptcha.com.
    /// </summary>
    public string PublicKey => SettingsKeyInfoProvider.GetValue("CMSHCaptchaPublicKey", SiteContext.CurrentSiteID);


    /// <summary>
    /// HCaptcha private key from https://dashboard.hcaptcha.com.
    /// </summary>
    public string SecretKey => SettingsKeyInfoProvider.GetValue("CMSHCaptchaSecretKey", SiteContext.CurrentSiteID);

    /// <summary>
    /// Optional. Forces the HCaptcha to render in a specific language.
    /// Auto-detects the user's language if unspecified.
    /// Currently supported values are listed at https://docs.hcaptcha.com/languages.
    /// </summary>
    public string Language
    {
        get
        {
            if (string.IsNullOrEmpty(mLanguage))
            {
                var currentCulture = CultureInfo.CurrentCulture;

                mLanguage = mFullLanguageCodes.Value.Contains(currentCulture.Name) ? currentCulture.Name : currentCulture.TwoLetterISOLanguageName;
            }

            return mLanguage;
        }
        set => mLanguage = value;
    }

    /// <summary>
    /// Determines whether the component is configured and allowed to be displayed.
    /// </summary>
    public bool IsConfigured => AreKeysConfigured && !SkipHCaptcha;

    /// <summary>
    /// Indicates whether to skip the HCaptcha validation.
    /// Useful for testing platform. Can be set using HCaptchaSkipValidation in AppSettings.
    /// </summary>
    private bool SkipHCaptcha
    {
        get
        {
            if (!mSkipHCaptcha.HasValue)
            {
                mSkipHCaptcha = ValidationHelper.GetBoolean(Service.Resolve<IAppSettingsService>()["HCaptchaSkipValidation"], false);
            }

            return mSkipHCaptcha.Value;
        }
    }

    /// <summary>
    /// Indicates whether both required keys are configured in the Settings application.
    /// </summary>
    private bool AreKeysConfigured => !string.IsNullOrEmpty(PublicKey) && !string.IsNullOrEmpty(SecretKey);

    /// <summary>
    /// Label "for" cannot be used for this component. 
    /// </summary>
    public override string LabelForPropertyName => "";

    /// <summary>
    /// Returns empty string since the <see cref="Value"/> does not hold anything.
    /// </summary>
    /// <returns>Returns the value of the form component.</returns>
    public override string GetValue() => string.Empty;

    /// <summary>
    /// Does nothing since the <see cref="Value"/> does not need to hold anything.
    /// </summary>
    /// <param name="value">Value to be set.</param>
    public override void SetValue(string value)
    {
        // the Value does not need to hold anything
    }

    /// <summary>
    /// Performs validation of the HCaptcha component.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection that holds failed-validation information.</returns>
    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();
        errors.AddRange(base.Validate(validationContext));

        bool isRenderedInAdminUI = VirtualContext.IsInitialized;

        if (!IsConfigured || isRenderedInAdminUI)
        {
            return errors;
        }

        var httpContext = Service.Resolve<IHttpContextRetriever>().GetContext();
        string response = httpContext.Request.Form.TryGetValue("h-captcha-response", out var value)
            ? value.ToString()
            : string.Empty;

        var validator = new HCaptchaValidator(PublicKey, SecretKey, RequestContext.UserHostAddress, response);

        var validationResult = validator.Validate();

        if (validationResult is not null)
        {
            if (!string.IsNullOrEmpty(validationResult.ErrorMessage))
            {
                errors.Add(new ValidationResult(validationResult.ErrorMessage));
            }
        }
        else
        {
            errors.Add(new ValidationResult(ResHelper.GetString("HCaptcha.error.serverunavailable")));
        }

        return errors;
    }
}

public class HCaptchaFormComponentProperties : FormComponentProperties<string>
{
    //
    // Summary:
    //     Gets or sets the default value of the form component and underlying field.
    public override string DefaultValue { get; set; } = "";

    //
    // Summary:
    //     Gets or sets value indicating whether the underlying field is required. False
    //     by default. If false, the form component's implementation must accept nullable
    //     input.
    public override bool Required { get; set; }

    //
    // Summary:
    //     Represents the color theme of the component (light or dark).
    // See: https://docs.hcaptcha.com/configuration#hcaptcha-container-configuration
    [EditingComponent("Kentico.DropDown", Label = "{$xperiencecommunity.formbuilder.component.hcaptcha.properties.theme$}", Order = 1)]
    [EditingComponentProperty("DataSource", "light;{$xperiencecommunity.formbuilder.component.hcaptcha.properties.theme.light$}\r\ndark;{$xperiencecommunity.formbuilder.component.hcaptcha.properties.theme.dark$}")]
    public string Theme { get; set; } = "light";

    //
    // Summary:
    //     Represents the layout of the component (normal or compact).
    // See: https://docs.hcaptcha.com/configuration#hcaptcha-container-configuration
    [EditingComponent("Kentico.DropDown", Label = "{$xperiencecommunity.formbuilder.component.hcaptcha.properties.layout$}", Order = 2)]
    [EditingComponentProperty("DataSource", "normal;{$xperiencecommunity.formbuilder.component.hcaptcha.properties.layout.normal$}\r\ncompact;{$xperiencecommunity.formbuilder.component.hcaptcha.properties.layout.compact$}")]
    public string Layout { get; set; } = "normal";

    //
    // Summary:
    //     Initializes a new instance of the HCaptchaProperties class.
    //
    // Remarks:
    //     The constructor initializes the base class to data type CMS.DataEngine.FieldDataType.Text
    //     and size 1.
    public HCaptchaFormComponentProperties()
        : base("text", 1, -1)
    {
    }
}

/// <summary>
/// Calls the HCaptcha server to validate the answer to a HCaptcha challenge.
/// </summary>
public class HCaptchaValidator
{
    private const string VERIFYURL = "https://hcaptcha.com/siteverify";

    /// <summary>
    /// The shared key between the site and HCaptcha.
    /// </summary>
    private readonly string secretKey;
    private readonly string publicKey;
    /// <summary>
    /// The user's IP address.
    /// </summary>
    private readonly string remoteIP;
    /// <summary>
    /// The user response token provided by HCaptcha, verifying the user on your site.
    /// </summary>
    private readonly string response;

    public HCaptchaValidator(string publicKey, string secretKey, string remoteIP, string formResponseValue)
    {
        this.publicKey = publicKey;
        this.secretKey = secretKey;

        var ip = IPAddress.Parse(remoteIP);

        if (ip == null ||
            (ip.AddressFamily != AddressFamily.InterNetwork &&
            ip.AddressFamily != AddressFamily.InterNetworkV6))
        {
            throw new ArgumentException("Expecting an IP address, got " + ip);
        }

        this.remoteIP = ip.ToString();
        response = formResponseValue;
    }

    /// <summary>
    /// Validate HCaptcha response
    /// </summary>
    public HCaptchaResponse? Validate()
    {
        var log = Service.Resolve<IEventLogService>();

        // Prepare web request
        var content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            ["secret"] = HttpUtility.UrlEncode(secretKey),
            ["remoteip"] = HttpUtility.UrlEncode(remoteIP),
            ["response"] = HttpUtility.UrlEncode(response),
            ["sitekey"] = HttpUtility.UrlEncode(publicKey)
        });

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(VERIFYURL),
            Method = HttpMethod.Post,
            Version = new Version(1, 0),
            Content = content
        };
        request.Headers.Add("User-Agent", "HCaptcha/ASP.NET");

        // Get validation response
        try
        {

            using var client = new HttpClient();
            var response = client.Send(request);
            string jsonResult = response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            return JsonConvert.DeserializeObject<HCaptchaResponse>(jsonResult);
        }
        catch (WebException ex)
        {
            log.LogException(
                nameof(HCaptchaFormComponent),
                "VALIDATE",
                ex);

            return null;
        }
    }
}

/// <summary>
/// Encapsulates a response from hCaptcha web service.
/// </summary>
public class HCaptchaResponse
{
    /// <summary>
    /// Indicates whether the hCaptcha validation was successful.
    /// </summary>
    [JsonProperty("success")]
    public bool IsValid { get; set; }


    /// <summary>
    /// The hostname of the site where the hCaptcha was solved
    /// </summary>
    [JsonProperty("hostname")]
    public string HostName { get; set; } = "";


    /// <summary>
    /// Timestamp of the challenge load.
    /// </summary>
    [JsonProperty("challenge_ts")]
    public DateTime TimeStamp { get; set; }


    /// <summary>
    /// Error codes explaining why hCaptcha validation failed.
    /// </summary>
    [JsonProperty("error-codes")]
    public IEnumerable<string> ErrorCodes { get; set; } = Enumerable.Empty<string>();


    /// <summary>
    /// Aggregated error message from all the error codes.
    /// </summary>
    [JsonIgnore]
    public string ErrorMessage => string.Join(" ", ErrorCodes.Select(x => ResHelper.GetString("recaptcha.error." + x)));
}
