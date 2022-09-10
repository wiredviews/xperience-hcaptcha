using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

namespace XperienceCommunity.HCaptchaModule
{
    public class HCaptchaInstallerModule : Module
    {
        public HCaptchaInstallerModule() : base(nameof(HCaptchaInstallerModule)) { }

        protected override void OnPreInit()
        {
            base.OnPreInit();

            Service.Use<IHCaptchaSettingsInstaller, HCaptchaSettingsInstaller>();
        }

        protected override void OnInit()
        {
            if (IsRunningInCmsApp())
            {
                var installer = Service.Resolve<IHCaptchaSettingsInstaller>();

                installer.Install();
            }

            base.OnInit();
        }

        private static bool IsRunningInCmsApp() => SystemContext.IsCMSRunningAsMainApplication && SystemContext.IsWebSite;
    }
}
