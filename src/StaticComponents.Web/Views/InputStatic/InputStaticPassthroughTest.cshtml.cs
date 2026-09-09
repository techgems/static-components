using TechGems.StaticComponents.Headless;

namespace StaticComponents.Web.Views.InputStatic;

/// <summary>
/// Exercises attribute passthrough. Everything the consumer writes that isn't bound to a property
/// of StaticInputBase lands in AdditionalAttributes, with the Alpine bindings routed to the wrapper
/// and the rest applied to the input itself.
/// </summary>
public class InputStaticPassthroughTest : StaticInput
{
}
