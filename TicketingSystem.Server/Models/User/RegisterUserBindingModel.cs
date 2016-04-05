namespace TicketingSystem.Server.Models.User
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Model used for serialization of registration data.
    /// </summary>
    [JsonObject]
    public class RegisterUserBindingModel
    {
        /// <summary>
        /// The username of the user.
        /// It is required.
        /// </summary>
        [Required(ErrorMessage = "The username is required")]
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// The email of the user.
        /// It is required.
        /// </summary>
        [JsonProperty("email")]
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }


        /// <summary>
        /// The phone number of the user.
        /// It is optional.
        /// </summary>
        [JsonProperty("phoneNumber")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The first name of the user.
        /// The minimum length of the first name is 3 characters
        /// </summary>
        [JsonProperty("firstName")]
        [Required(ErrorMessage = "First name is required")]
        [MinLength(3, ErrorMessage = "The minimal length for first name is {0}")]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user.
        /// The minimum length of the last name is 3 characters
        /// </summary>
        [JsonProperty("lastName")]
        [MinLength(3, ErrorMessage = "The minimal length for last name is {0}")]
        public string LastName { get; set; }


        /// <summary>
        /// The password of the user.
        /// It is required and should have length between 2 and 100 characters
        /// </summary>
        [JsonProperty("password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "The password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Password { get; set; }

        /// <summary>
        /// The confirmation password.
        /// It should match the password.
        /// </summary>
        [JsonProperty("passwordConfirm")]
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string PasswordConfirm { get; set; }
    }
}