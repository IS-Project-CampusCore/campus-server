 // Abstract record - cannot be instantiated directly public abstract record User
public abstract record User
{
  
  public string Id { get; init; } // init-only property (immutable after creation)
  public string Name { get; init; }
  public string Email { get; init; }
  public string PasswordHash { get; init; }
  public string Role { get; init; }
  public bool IsVerified { get; init; }
   
}