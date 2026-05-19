using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FreshCart.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [ValidateNever]
        public CartViewModel Cart { get; set; }
    }
}