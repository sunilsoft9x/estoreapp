using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models;
namespace MyEstore.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage ="Shipping address is required")]
        [MaxLength(200, ErrorMessage = "Shipping address cannot exceed 200 characters")]
        public string ShippingAddress { get; set;} = string.Empty;
        public string City { get; set;} = string.Empty;
        public string State { get; set;} = string.Empty;
        public string PINCode { get; set;} = string.Empty;
        public string Country { get; set;} = string.Empty;
        //Billing Address - optional
        public bool IsBillingAddressSameAsShipping { get; set;} = true; // Optional field to indicate if billing address is same as shipping address, providing flexibility for customers who may have different billing and shipping addresses.
        public string? BillingAddress { get; set;} // Optional field for billing address, allowing
        public string? BillingCity { get; set;}
        public string? BillingState { get; set;}
        public string? BillingPINCode { get; set;}
        public string? BillingCountry { get; set;}
//Payment Details

        [Required(ErrorMessage = "Payment method is required")]
        [MaxLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string PaymentMethod { get; set;} = string.Empty;
        //UPI, Credit Card, Debit Card, Net Banking, Cash on Delivery
        //Coupon Code - optional
        public string? CouponCode { get; set;} // Optional field for coupon code, allowing customers to apply discounts or promotions to their order, enhancing the shopping experience and encouraging repeat purchases.
        //Delivery Slot
        public DateTime? PrefferedDeliveryDate { get; set;} // Optional field for preferred delivery slot, providing customers with the flexibility to choose a convenient time for their order delivery, improving customer satisfaction and enhancing the overall shopping experience.
        public string? DeliveryTimeSlot { get; set;} // Optional field for delivery time slot, allowing customers to specify a preferred time range for their order delivery, enhancing convenience and improving customer satisfaction.
        //Gift Information
        public bool IsGift { get; set;} = false; // Optional field to indicate if
        public string? GiftMessage { get; set;} // Optional field for gift message, allowing customers to include a personalized message when sending an order as a gift, enhancing the gifting experience and adding a personal touch to the order.
        public string? Instructions { get; set;} // Optional field for additional instructions or notes related to the order, providing flexibility for customers to communicate specific requirements or preferences regarding their order.
    }

}
// {
//   "shippingAddress": "123 Sector 22",
//   "city": "Chandigarh",
//   "state": "Punjab",
//   "postalCode": "160022",
//   "country": "India",

//   "isBillingSameAsShipping": false,
//   "billingAddress": "456 Sector 17",
//   "billingCity": "Chandigarh",
//   "billingState": "Punjab",
//   "billingPostalCode": "160017",
//   "billingCountry": "India",

//   "paymentMethod": "UPI",
//   "couponCode": "NEWUSER10",

//   "preferredDeliveryDate": "2026-04-20",
//   "deliveryTimeSlot": "10AM-1PM",

//   "isGift": true,
//   "giftMessage": "Happy Birthday!",

//   "instructions": "Call before delivery"
// }