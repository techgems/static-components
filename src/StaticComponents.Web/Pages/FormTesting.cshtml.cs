using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StaticComponents.Web.Pages;

public class FormTestingModel : PageModel
{
    //  [StringLength(50, MinimumLength = 5)]
    //  [Range(1, 100)]
    //  [RegularExpression(@"^\d+$")]
    //  [EmailAddress]
    //  [Url]
    //  [Phone]
    //  [Remote(action: "VerifyEmail", controller: "Users")] --Not priority, but its important.

    [DisplayName("Testing String Value")]
    [RegularExpression(@"^\d+$", ErrorMessage = "The {0} field must be a valid number.")]
    [BindProperty]
    public string? TestStringValue { get; set; } = "My name is ned.";

    [DisplayName("Testing Number Value")]
    [Range(1, 100)]
    [BindProperty]
    public int TestNumberValue { get; set; }

    [DisplayName("Testing Checkbox Value")]
    [BindProperty]
    public bool TestCheckbox { get; set; }

    [DisplayName("Testing Radio Value")]
    [BindProperty]
    public string TestRadioValue { get; set; } = "Two";

    [DataType(DataType.Password)]
    [DisplayName("Testing Password Value")]
    [BindProperty]
    public string TestPasswordValue { get; set; } = "";

    [HiddenInput]
    [BindProperty]
    public string TestHiddenValue { get; set; } = "";

    [Required]
    [BindProperty]
    public int TestRequiredValue { get; set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {

    }
}
