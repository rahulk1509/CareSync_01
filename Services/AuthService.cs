using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using HospitalTriageAI.Data;
using HospitalTriageAI.Models;
using System.Security.Cryptography;
using System.Text;

namespace HospitalTriageAI.Services;

/// <summary>
/// Authentication service implementation with session persistence
/// </summary>
public class AuthService : IAuthService
{
    private readonly IServiceProvider _serviceProvider;
    private User? _currentUser;
    private const string SessionKey = "triageai_session";
    
    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Role == UserRole.Admin;
    
    public event Action? OnAuthStateChanged;
    
    public AuthService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    private IJSRuntime? GetJSRuntime()
    {
        try
        {
            return _serviceProvider.GetService<IJSRuntime>();
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var passwordHash = HashPassword(password);
            var user = await context.Users
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.PasswordHash == passwordHash);
            
            if (user == null)
            {
                return (false, "Invalid email or password.");
            }
            
            if (!user.IsActive)
            {
                return (false, "Account is deactivated.");
            }
            
            user.LastLoginAt = DateTime.Now;
            await context.SaveChangesAsync();
            
            _currentUser = user;
            
            // Save session to localStorage
            try
            {
                var js = GetJSRuntime();
                if (js != null)
                {
                    await js.InvokeVoidAsync("localStorage.setItem", SessionKey, $"{user.Id}|{passwordHash}");
                }
            }
            catch { /* Ignore JS errors */ }
            
            OnAuthStateChanged?.Invoke();
            
            return (true, "Login successful.");
        }
        catch (Exception ex)
        {
            return (false, $"Login error: {ex.Message}");
        }
    }
    
    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var js = GetJSRuntime();
            if (js == null) return false;
            
            var sessionData = await js.InvokeAsync<string?>("localStorage.getItem", SessionKey);
            if (string.IsNullOrEmpty(sessionData)) return false;
            
            var parts = sessionData.Split('|');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var userId)) return false;
            
            var storedHash = parts[1];
            
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var user = await context.Users
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId && u.PasswordHash == storedHash && u.IsActive);
            
            if (user == null) return false;
            
            _currentUser = user;
            OnAuthStateChanged?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string fullName, UserRole role = UserRole.Patient)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Check if email already exists
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (existingUser != null)
            {
                return (false, "Email already registered.", null);
            }
            
            var user = new User
            {
                Email = email.ToLower(),
                PasswordHash = HashPassword(password),
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            return (true, "Registration successful.", user);
        }
        catch (Exception ex)
        {
            return (false, $"Registration error: {ex.Message}", null);
        }
    }
    
    public async Task LogoutAsync()
    {
        _currentUser = null;
        try
        {
            var js = GetJSRuntime();
            if (js != null)
            {
                await js.InvokeVoidAsync("localStorage.removeItem", SessionKey);
            }
        }
        catch { /* Ignore JS errors */ }
        OnAuthStateChanged?.Invoke();
    }
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "TriageAI_Salt_2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
