# Xperience hCaptcha Form Components

[![GitHub Actions CI: Build](https://github.com/wiredviews/xperience-hcaptcha-form-components/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-hcaptcha-form-components/actions/workflows/ci.yml)

[![Publish Packages to NuGet](https://github.com/wiredviews/xperience-hcaptcha-form-components/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-hcaptcha-form-components/actions/workflows/publish.yml)

## Packages

### XperienceCommunity.HCaptchaFormComponents

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.HCaptchaFormComponents.svg)](https://www.nuget.org/packages/XperienceCommunity.HCaptchaFormComponents)

Kentico Xperience 13.0 ASP.NET Core Form Component that adds [hCaptcha](https://www.hcaptcha.com/) captcha validation to Form Builder forms.

### XperienceCommunity.HCaptchaModule

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.HCaptchaModule.svg)](https://www.nuget.org/packages/XperienceCommunity.HCaptchaModule)

Kentico Xperience 13.0 CMS installation module that adds [hCaptcha](https://www.hcaptcha.com/) settings keys to the Settings application.

## How to Use?

1. Install the `XperienceCommunity.HCaptchaFormComponents` NuGet package in your ASP.NET Core application:

   ```bash
   dotnet add package XperienceCommunity.HCaptchaFormComponents
   ```

1. Install the `XperienceCommunity.HCaptchaModule` NuGet package in your CMS application:

   ```bash
   dotnet add package XperienceCommunity.HCaptchaModule
   ```

1. Set the settings keys in the CMS Settings application with the values provided in your [hCaptcha Dashboard](https://dashboard.hcaptcha.com/).

1. Add the hCaptcha Form Component to a Form Builder form.

1. Load the form on your live site to see the captcha.

## Usage

## Contributions

If you discover a problem, please [open an issue](https://github.com/wiredviews/xperience-hcaptcha-form-components/issues/new).

If you would like contribute to the code or documentation, please [open a pull request](https://github.com/wiredviews/xperience-hcaptcha-form-components/compare).

## References

- [hCaptcha](https://www.hcaptcha.com/)

### Kentico Xperience

- [Form Components](https://docs.xperience.io/x/pQ2RBg)
