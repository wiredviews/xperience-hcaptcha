using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Modules;
using CMS.SiteProvider;

namespace XperienceCommunity.HCaptcha.CMS
{
    public interface IHCaptchaSettingsInstaller
    {
        void Install();
    }

    /// <summary>
    /// Pattern sourced from https://github.com/heywills/xperience-content-reference-module-ui
    /// </summary>
    internal class HCaptchaSettingsInstaller : IHCaptchaSettingsInstaller
    {
        private readonly IEventLogService log;
        private readonly IResourceInfoProvider resourceProvider;
        private readonly ISettingsCategoryInfoProvider settingsCategoryProvider;
        private readonly IResourceSiteInfoProvider resourceSiteProvider;
        private readonly ISiteInfoProvider siteProvider;


        public HCaptchaSettingsInstaller(IEventLogService log,
                                           IResourceInfoProvider resourceProvider,
                                           ISettingsCategoryInfoProvider settingsCategoryProvider,
                                           IResourceSiteInfoProvider resourceSiteProvider,
                                           ISiteInfoProvider siteProvider)
        {
            this.log = log;
            this.resourceProvider = resourceProvider;
            this.settingsCategoryProvider = settingsCategoryProvider;
            this.resourceSiteProvider = resourceSiteProvider;
            this.siteProvider = siteProvider;
        }

        public void Install()
        {
            try
            {
                var resourceInfo = InstallResourceInfo();
                AssignModuleToSites(resourceInfo);
            }
            catch (Exception ex)
            {
                log.LogException(nameof(HCaptchaSettingsInstaller), "INSTALLATION_ERROR", ex);
            }
        }

        private void AssignModuleToSites(ResourceInfo resourceInfo)
        {
            using (new CMSActionContext
            {
                LogSynchronization = false,
                ContinuousIntegrationAllowObjectSerialization = false
            })
            {
                int[] sitesWithResourceInstalled = resourceSiteProvider
                    .Get()
                    .Column("SiteID")
                    .WhereEquals("ResourceID", resourceInfo.ResourceID)
                    .GetEnumerableTypedResult()
                    .Select(r => r.SiteID)
                    .ToArray();

                var unassignedSites = siteProvider
                    .Get()
                    .WhereNotIn("SiteID", sitesWithResourceInstalled)
                    .GetEnumerableTypedResult()
                    .Select(siteInfo => siteInfo.SiteID)
                    .ToList();

                unassignedSites.ForEach(siteId => resourceSiteProvider.Add(resourceInfo.ResourceID, siteId));

                int unassignedSiteCount = unassignedSites.Count;

                if (unassignedSiteCount > 0)
                {
                    LogInformation("ASSIGNED", $"Assigned the module '{ResourceConstants.ResourceDisplayName}' to {unassignedSiteCount} sites.");
                }
            }
        }

        private ResourceInfo InstallResourceInfo()
        {
            using (new CMSActionContext
            {
                LogSynchronization = false,
                ContinuousIntegrationAllowObjectSerialization = false
            })
            {
                var resourceInfo = resourceProvider.Get(ResourceConstants.ResourceName);

                if (InstalledModuleIsCurrent(resourceInfo))
                {
                    LogInformation("CURRENT", $"The '{ResourceConstants.ResourceDisplayName}' module is already installed and current.");
                    return resourceInfo;
                }

                LogInformation("START", $"{(resourceInfo == null ? "Installing" : "Updating")} the module '{ResourceConstants.ResourceDisplayName}'.");

                if (resourceInfo == null)
                {
                    resourceInfo = new ResourceInfo();
                }

                resourceInfo.ResourceDisplayName = ResourceConstants.ResourceDisplayName;
                resourceInfo.ResourceName = ResourceConstants.ResourceName;
                resourceInfo.ResourceDescription = ResourceConstants.ResourceDescription;
                resourceInfo.ResourceAuthor = ResourceConstants.ResourceAuthor;
                resourceInfo.ResourceIsInDevelopment = false;
                // Setting ResourceInstallationState to 'installed' will cause Kentico to uninstall related objects if it
                // finds a module meta file in ~\App_Data\CMSModules\CMSInstallation\Packages\Installed
                resourceInfo.ResourceInstallationState = "";
                resourceProvider.Set(resourceInfo);

                InstallSettings(resourceInfo);
                StoreInstalledVersion(resourceInfo);
                LogInformation("COMPLETE", $"{(resourceInfo == null ? "Install" : "Update")} of the module '{ResourceConstants.ResourceDisplayName}' version {resourceInfo.ResourceVersion} is complete.");
                return resourceInfo;
            }
        }

        /// <summary>
        /// Store the version number of the installed module. This should
        /// be done after the ResourceInfo and UiElementInfo are successfully
        /// updated and saved.
        /// </summary>
        /// <param name="resourceInfo"></param>
        private void StoreInstalledVersion(ResourceInfo resourceInfo)
        {
            using (new CMSActionContext
            {
                LogSynchronization = false,
                ContinuousIntegrationAllowObjectSerialization = false
            })
            {
                string newVersion = GetModuleVersionFromAssembly();
                resourceInfo.ResourceInstalledVersion = newVersion;
                resourceInfo.ResourceVersion = newVersion;
                resourceProvider.Set(resourceInfo);
            }
        }

        private void InstallSettings(ResourceInfo resourceInfo)
        {
            using (new CMSActionContext
            {
                LogSynchronization = false,
                ContinuousIntegrationAllowObjectSerialization = false
            })
            {
                var category = settingsCategoryProvider.Get(ResourceConstants.SettingsCategoryName);

                if (category is null)
                {
                    log.LogError(nameof(HCaptchaSettingsInstaller), "INSTALLATION_ERROR", $"Could not find Settings Category {ResourceConstants.SettingsCategoryName}");

                    return;
                }

                string publicKeyText = @"The public site API key for the site where you want to use hCaptcha.<br /><br /> You can get your API keys at <a href=""https://dashboard.hcaptcha.com/"">https://dashboard.hcaptcha.com/</a>.";
                string secretKeyText = @"The secret API key for the site where you want to use hCaptcha.<br /><br /> You can get your API keys at <a href=""https://dashboard.hcaptcha.com/"">https://dashboard.hcaptcha.com/</a>.";

                var keys = new SettingsKeyInfo[]
                {
                    new SettingsKeyInfo
                    {
                        KeyName = "CMSHCaptchaPublicKey",
                        KeyDisplayName = "hCaptcha Public Key",
                        KeyType = "string",
                        KeyCategoryID = category.CategoryID,
                        KeyDefaultValue = "",
                        KeyDescription = publicKeyText,
                        KeyExplanationText = publicKeyText,
                        KeyIsCustom = true,
                        KeyOrder = 10,
                    },
                    new SettingsKeyInfo
                    {
                        KeyName = "CMSHCaptchaSecretKey",
                        KeyDisplayName = "hCaptcha Secret Key",
                        KeyType = "string",
                        KeyCategoryID = category.CategoryID,
                        KeyDefaultValue = "",
                        KeyDescription = secretKeyText,
                        KeyExplanationText = secretKeyText,
                        KeyIsCustom = true,
                        KeyOrder = 11,
                    },
                };

                AddIfMissing(keys);
            }
        }

        private static void AddIfMissing(IEnumerable<SettingsKeyInfo> keys)
        {
            var existingKeys = SettingsKeyInfoProvider.GetSettingsKeys()
                .WhereIn(nameof(SettingsKeyInfo.KeyName), keys.Select(k => k.KeyName).ToArray())
                .TypedResult
                .ToList();

            foreach (var key in keys)
            {
                var existingKey = existingKeys.FirstOrDefault(k => string.Equals(k.KeyName, key.KeyName, StringComparison.OrdinalIgnoreCase));

                if (existingKey is null)
                {
                    key.Insert();

                    continue;
                }

                existingKey.KeyName = key.KeyName;
                existingKey.KeyDisplayName = key.KeyDisplayName;
                existingKey.KeyType = key.KeyType;
                existingKey.KeyCategoryID = key.KeyCategoryID;
                existingKey.KeyDefaultValue = key.KeyDefaultValue;
                existingKey.KeyDescription = key.KeyDescription;
                existingKey.KeyIsCustom = key.KeyIsCustom;
                existingKey.KeyOrder = key.KeyOrder;
                existingKey.KeyExplanationText = key.KeyExplanationText;

                existingKey.Update();
            }
        }

        private bool InstalledModuleIsCurrent(ResourceInfo resourceInfo) => (resourceInfo != null) &&
                   (GetModuleVersionFromAssembly() == resourceInfo.ResourceInstalledVersion);

        /// <summary>
        /// Create a Module version number from the assembly version.
        /// The module version must be in 3 parts (e.g. 1.0.13).
        /// </summary>
        /// <returns></returns>
        private string GetModuleVersionFromAssembly() => GetType().Assembly.GetName().Version.ToString(3);

        private void LogInformation(string eventCode, string eventMessage) => log.LogEvent(EventTypeEnum.Information,
                                      nameof(HCaptchaSettingsInstaller),
                                      eventCode,
                                      eventMessage);
    }

    internal static class ResourceConstants
    {
        public const string ResourceName = "XperienceCommunity.HCaptcha";
        public const string ResourceDisplayName = "XperienceCommunity HCaptcha";
        public const string ResourceDescription = "XperienceCommunity settings for hCaptcha Form Component";
        public const string ResourceAuthor = "WiredViews, Inc.";

        public const string SettingsCategoryName = "CMS.Security.Captcha";
    }
}
