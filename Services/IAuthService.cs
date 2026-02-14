using HospitalTriageAI.Models;

namespace HospitalTriageAI.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Get current logged in user
    /// </summary>
    User? CurrentUser { get; }
    
    /// <summary>
    /// Check if user is logged in
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Check if current user is admin
    /// </summary>
    bool IsAdmin { get; }
    
    /// <summary>
    /// Check if current user is patient
    /// </summary>
    bool IsPatient { get; }
    
    /// <summary>
    /// Check if current user is doctor
    /// </summary>
    bool IsDoctor { get; }
    
    /// <summary>
    /// Get current user's role
    /// </summary>
    UserRole? CurrentRole { get; }
    
    /// <summary>
    /// Check if current user has a specific role
    /// </summary>
    bool HasRole(UserRole role);
    
    /// <summary>
    /// Check if current user has any of the specified roles
    /// </summary>
    bool HasAnyRole(params UserRole[] roles);
    
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<(bool Success, string Message)> LoginAsync(string email, string password);
    
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string fullName, UserRole role = UserRole.Patient);
    
    /// <summary>
    /// Log out current user
    /// </summary>
    Task LogoutAsync();
    
    /// <summary>
    /// Try to restore session from stored credentials
    /// </summary>
    Task<bool> TryRestoreSessionAsync();
    
    /// <summary>
    /// Event fired when auth state changes
    /// </summary>
    event Action? OnAuthStateChanged;
}
