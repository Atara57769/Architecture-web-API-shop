# Controllers & DTOs Layer Guide

## Overview
Controllers expose API endpoints and handle HTTP requests/responses. They delegate business logic to services and perform response mapping via AutoMapper. DTOs (Data Transfer Objects) define request/response contracts and keep the API decoupled from entity models.

## File Structure

### DTOs (`../DTOs/`)
- Defined as C# records (immutable, lightweight)
- Separate DTO types for requests (PostUserDTO) vs. responses (UserDTO)
- Never include sensitive fields like internal IDs or computed data

```csharp
public record UserDTO(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string Address,
    string PhoneNumber,
    bool IsAdmin,
    ICollection<OrderDTO> Orders
);

public record PostUserDTO(
    string Email,
    string FirstName,
    string LastName,
    string Address,
    string PhoneNumber,
    string Password,
    bool IsAdmin
);
```

### Controllers (`WebApiShop/Controllers/`)

#### Basic REST Pattern
```csharp
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET all
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> Get()
    {
        IEnumerable<UserDTO> users = await _userService.GetUsers();
        return users.Count() > 0 ? Ok(users) : NoContent();
    }

    // GET by id
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> Get(int id)
    {
        UserDTO user = await _userService.GetUserById(id);
        return user == null ? NotFound() : Ok(user);
    }

    // POST create
    [HttpPost]
    public async Task<ActionResult<UserDTO>> Post([FromBody] PostUserDTO newUser)
    {
        if (!await _userService.UserWithSameEmail(newUser.Email))
            return BadRequest("The email already exists. Please try again.");
        if (!_userService.IsPasswordStrong(newUser.Password))
            return BadRequest("The password is too weak. Please try again.");
        
        UserDTO returnedUser = await _userService.AddUser(newUser);
        return returnedUser == null 
            ? BadRequest() 
            : CreatedAtAction(nameof(Get), new { id = returnedUser.Id }, returnedUser);
    }

    // PUT update
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] PostUserDTO updateUser)
    {
        if (!await _userService.UserWithSameEmail(updateUser.Email, updateUser.Id))
            return BadRequest("The email already exists. Please try again.");
        if (!_userService.IsPasswordStrong(updateUser.Password))
            return BadRequest("The password is too weak. Please try again.");
        
        await _userService.UpdateUser(id, updateUser);
        return NoContent();
    }
}
```

## Service Layer Integration

### Services (`../Services/`)
Services contain business logic and handle:
- Input validation (email duplicates, password strength)
- DTO to Entity mapping (via AutoMapper)
- Entity to DTO mapping (responses)
- Orchestration across repositories

```csharp
public class UserService : IUserService
{
    private const int MinimumPasswordScore = 2;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IPasswordService passwordService, IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDTO>> GetUsers()
    {
        return _mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(
            await _userRepository.GetUsers()
        );
    }

    public async Task<UserDTO> AddUser(PostUserDTO user)
    {
        return _mapper.Map<User, UserDTO>(
            await _userRepository.AddUser(_mapper.Map<PostUserDTO, User>(user))
        );
    }

    public bool IsPasswordStrong(string password)
    {
        int score = _passwordService.GetPasswordScore(password);
        return score >= MinimumPasswordScore;
    }
}
```

## AutoMapper Configuration

### Location
`WebApiShop/AutoMapper.cs` - Centralized mapping configuration

### Setup Pattern
```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Bidirectional mappings
        CreateMap<User, UserDTO>().ReverseMap();
        CreateMap<User, PostUserDTO>().ReverseMap();
        CreateMap<Order, OrderDTO>().ReverseMap();
        CreateMap<OrderItem, OrderItemDTO>().ReverseMap();
        CreateMap<Category, CategoryDTO>().ReverseMap();
        CreateMap<Product, ProductDTO>().ReverseMap();
    }
}
```

### Important Notes
- Use `.ReverseMap()` for bidirectional mappings
- Configure all mappings BEFORE runtime
- Maps are injected via `IMapper` interface
- **Never** use `.Map()` in repositories; keep them entity-only

## HTTP Response Patterns

### Response Status Codes
```csharp
// 200 OK - Data returned
return Ok(user);

// 201 Created - Resource created
return CreatedAtAction(nameof(Get), new { id = user.Id }, user);

// 204 No Content - Success, no data
return NoContent();

// 400 Bad Request - Validation error
return BadRequest("Email already exists");

// 401 Unauthorized - Authentication failed
return Unauthorized();

// 404 Not Found - Resource not found
return NotFound();

// 500 Server Error - Handled by ErrorHandlingMiddleware
```

## Validation in Controllers

### Email Uniqueness
```csharp
// userService.UserWithSameEmail(email, id) returns TRUE if VALID (no duplicate)
// For new users: UserWithSameEmail(email, -1 or omit id)
// For updates: UserWithSameEmail(email, userId)

if (!await _userService.UserWithSameEmail(newUser.Email))
    return BadRequest("Email already exists");
```

### Password Strength
```csharp
// isPasswordStrong() returns TRUE if score >= 2
if (!_userService.IsPasswordStrong(newUser.Password))
    return BadRequest("Password is too weak");
```

### Order Validation
```csharp
// Check order belongs to user before returning
var orders = await _userService.GetUsersOrders(id);
if (orders.Count() > 0)
    return Ok(orders);
return NoContent();
```

## Logging in Controllers

Log important actions at the service layer interaction point:

```csharp
[HttpPost("login")]
public async Task<ActionResult<UserDTO>> Login([FromBody] LoginUserDTO loginUser)
{
    UserDTO user = await _userService.Login(loginUser);
    if (user == null)
        return Unauthorized();
    
    _logger.LogInformation(
        $"Login attempted - Id:{user.Id} Email:{user.Email} Name:{user.FirstName} {user.LastName}"
    );
    return Ok(user);
}
```

## Controllers in This Project

| Controller | Endpoints | Purpose |
|-----------|-----------|---------|
| **UsersController** | POST /api/users, GET /users, POST /users/login, PUT /users/{id}, GET /users/{id}/orders | User CRUD & authentication |
| **ProductsController** | GET| Product management |
| **OrdersController** | GET, POST | Order management |
| **PasswordsController** | POST /api/passwords/PasswordScore | Password strength validation |

## Middleware Integration

### Error Handling
`ErrorHandlingMiddleware` in `Program.cs`:
```csharp
app.UseErrorHandling(); // Catches unhandled exceptions, logs, returns 500
```
Any unhandled exception is caught, logged with full stack trace, and returns HTTP 500.

### Request Rating
`RatingMiddleware` in `Program.cs`:
```csharp
app.UseRating(); // Logs all HTTP requests
```
Every request is logged to the Rating table with host, method, path, headers, and timestamp.

## Guidelines

- **Thin Controllers:** Business logic goes in services, not controllers
- **Validate Early:** Check inputs before calling services
- **Consistent Responses:** Use patterns from examples above
- **Appropriate Status Codes:** Follow HTTP conventions (201 for POST, 204 for successful PUT)
- **Log Critical Events:** Login attempts, data modifications
- **DTOs First:** Define DTOs before writing controller logic
- **Dependency Inject:** Never instantiate services manually
