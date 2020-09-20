using System.Collections.Generic;
using System.Linq;

namespace XTI_WebApp.Api
{
    public sealed class AppApiActionTemplate
    {
        public AppApiActionTemplate
        (
            string name,
            string friendlyName,
            ResourceAccess access,
            ValueTemplate modelTemplate,
            ValueTemplate resultTemplate
        )
        {
            Name = name;
            FriendlyName = friendlyName;
            Access = access;
            ModelTemplate = modelTemplate;
            ResultTemplate = resultTemplate;
        }

        public string Name { get; }
        public string FriendlyName { get; }
        public ResourceAccess Access { get; }
        public ValueTemplate ModelTemplate { get; }
        public ValueTemplate ResultTemplate { get; }

        public bool IsView() => ResultTemplate.DataType == typeof(AppActionViewResult);
        public bool IsRedirect() => ResultTemplate.DataType == typeof(AppActionRedirectResult);
        public bool HasEmptyModel() => ModelTemplate.DataType == typeof(EmptyRequest);

        public IEnumerable<ObjectValueTemplate> ObjectTemplates() =>
            ModelTemplate.ObjectTemplates()
                .Union
                (
                    ResultTemplate.ObjectTemplates()
                )
                .Distinct();
    }
}
