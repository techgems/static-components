using TechGems.StaticComponents;

namespace StaticComponents.Web.Views.StaticScriptTests;

/// <summary>
/// Live test component: a static-script with an inline script body and no "src" attribute.
/// This should render the script inline.
/// </summary>
public class StaticScriptWithBodyComponent : StaticComponent
{
    public StaticScriptWithBodyComponent()
    {
    }
}
