using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;

namespace XperienceCommunity.HCaptchaFormComponents
{
    public class HCaptchaFormComponent : FormComponent<HCaptchaFormComponentProperties, string>
    {
        public override string GetValue() => throw new System.NotImplementedException();
        public override void SetValue(string value) => throw new System.NotImplementedException();
    }

    public class HCaptchaFormComponentProperties : FormComponentProperties<string>
    {
        public HCaptchaFormComponentProperties() : base(FieldDataType.Text, 1)
        {
        }

        public override string DefaultValue { get; set; } = "";
    }
}
