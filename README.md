# Xperience hCaptcha Form Components

[![GitHub Actions CI: Build](https://github.com/wiredviews/xperience-hcaptcha/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-hcaptcha/actions/workflows/ci.yml)

[![Publish Packages to NuGet](https://github.com/wiredviews/xperience-hcaptcha/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-hcaptcha/actions/workflows/publish.yml)

## Packages

### XperienceCommunity.HCaptcha

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.HCaptcha.svg)](https://www.nuget.org/packages/XperienceCommunity.HCaptcha)

Kentico Xperience 13.0.66 (or higher) ASP.NET Core 6.0 Form Component that adds [hCaptcha](https://www.hcaptcha.com/) captcha validation to Form Builder forms.

### XperienceCommunity.HCaptcha.CMS

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.HCaptcha.CMS.svg)](https://www.nuget.org/packages/XperienceCommunity.HCaptcha.CMS)

Kentico Xperience 13.0.66 (or higher) CMS installation module that adds [hCaptcha](https://www.hcaptcha.com/) settings keys to the Settings application.

## How to Use?

1. Install the `XperienceCommunity.HCaptcha` NuGet package in your ASP.NET Core application:

   ```bash
   dotnet add package XperienceCommunity.HCaptcha
   ```

1. Install the `XperienceCommunity.HCaptcha.CMS` NuGet package in your CMS application:

   ```bash
   dotnet add package XperienceCommunity.HCaptcha.CMS
   ```

1. Add a registration for the `HCaptchaInstallerModule` in your `CMSApp` project:

   ```csharp
   using XperienceCommunity.HCaptcha.CMS

   [assembly: RegisterModule(typeof(HCaptchaInstallerModule))]

   namespace CMSApp.Configuration
   {
      public class DependencyRegistrations
      {

      }
   }
   ```

1. Set the settings keys in the CMS Settings application with the values provided in your [hCaptcha Dashboard](https://dashboard.hcaptcha.com/).

1. Add the hCaptcha Form Component to a Form Builder form.

1. Load the form on your live site to see the captcha.

## Usage

## Contributions

If you discover a problem, please [open an issue](https://github.com/wiredviews/xperience-hcaptcha/issues/new).

If you would like contribute to the code or documentation, please [open a pull request](https://github.com/wiredviews/xperience-hcaptcha/compare).

## References

- [hCaptcha](https://www.hcaptcha.com/)

### Kentico Xperience

- [Form Components](https://docs.xperience.io/x/pQ2RBg)
