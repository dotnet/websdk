using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.NET.Sdk.Web.ProjectSystem.Tasks
{
    sealed public class ValidateWebConfig : Task
    {
        [Required]
        public string ProjectFullPath { get; set; }

        [Required]
        public string ObsoleteWebConfigElements { get; set; }

        public override bool Execute()
        {
            var validator = new WebConfigValidator(ProjectFullPath, ObsoleteWebConfigElements, Log);

            return validator.Validate();
        }
    }
}
