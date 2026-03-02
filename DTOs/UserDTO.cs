namespace DTOs
{
    public record UserDTO(
     int Id ,

     string Email ,

     string FirstName ,

     string LastName ,

     string Address ,

     string PhoneNumber ,

     bool IsAdmin,

     ICollection<OrderDTO> Orders 
    );
    

    
}
