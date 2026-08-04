using TechGems.StaticComponents;

namespace StaticComponents.Web.Views.StaticScriptTests;

/// <summary>
/// Live test component: a static-script that only declares a "src" attribute and has no script body.
/// This should render successfully because static-script allows a body OR a src.
/// </summary>
public class StaticScriptWithSrcComponent : StaticComponent
{
    public StaticScriptWithSrcComponent()
    {
    }
}
