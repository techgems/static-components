using TechGems.StaticComponents;

namespace StaticComponents.Web.Views.StaticScriptTests;

/// <summary>
/// Live test component: a static-script with neither an inline body nor a "src" attribute.
/// This is the invalid case that static-script guards against and is expected to fail.
/// </summary>
public class StaticScriptFailingComponent : StaticComponent
{
    public StaticScriptFailingComponent()
    {
    }
}
