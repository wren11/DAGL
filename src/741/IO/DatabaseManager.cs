using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DarkAges.Library.IO;

public static class DatabaseManager
{
    private static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkAges", "Data");
    private static readonly string UsersDirectory = Path.Combine(DataDirectory, "Users");
    private static readonly string CharactersDirectory = Path.Combine(DataDirectory, "Characters");
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
        
    static DatabaseManager()
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(UsersDirectory);
            Directory.CreateDirectory(CharactersDirectory);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to create database directories: {ex.Message}", ex);
        }
    }

    public static async Task SaveUserDataAsync<T>(string identifier, T data)
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));
            
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        try
        {
            var filePath = Path.Combine(UsersDirectory, $"{identifier}.json");
            var jsonString = JsonSerializer.Serialize(data, DefaultSerializerOptions);
            await File.WriteAllTextAsync(filePath, jsonString);
        }
        catch (Exception ex)
        {
            throw new IOException($"Error saving user data for {identifier}: {ex.Message}", ex);
        }
    }

    public static async Task<T?> LoadUserDataAsync<T>(string identifier) where T : class
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

        try
        {
            var filePath = Path.Combine(UsersDirectory, $"{identifier}.json");
            if (!File.Exists(filePath))
                return null;

            var jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonString, DefaultSerializerOptions);
        }
        catch (Exception ex)
        {
            throw new IOException($"Error loading user data for {identifier}: {ex.Message}", ex);
        }
    }

    public static async Task DeleteUserDataAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

        try
        {
            var filePath = Path.Combine(UsersDirectory, $"{identifier}.json");
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Error deleting user data for {identifier}: {ex.Message}", ex);
        }
    }

    public static async Task<bool> UserDataExistsAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

        try
        {
            var filePath = Path.Combine(UsersDirectory, $"{identifier}.json");
            return await Task.Run(() => File.Exists(filePath));
        }
        catch (Exception ex)
        {
            throw new IOException($"Error checking user data existence for {identifier}: {ex.Message}", ex);
        }
    }

    public static async Task SaveCharacterDataAsync<T>(string characterName, T data)
    {
        try
        {
            var filePath = Path.Combine(CharactersDirectory, $"{characterName}.json");
            var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await File.WriteAllTextAsync(filePath, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving character data: {ex.Message}");
        }
    }

    public static async Task<T?> LoadCharacterDataAsync<T>(string characterName) where T : class
    {
        try
        {
            var filePath = Path.Combine(CharactersDirectory, $"{characterName}.json");
            if (!File.Exists(filePath))
                return null;

            var jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading character data: {ex.Message}");
            return null;
        }
    }

    public static void SaveUserData<T>(string identifier, T data)
    {
        SaveUserDataAsync(identifier, data).Wait();
    }

    public static T? LoadUserData<T>(string identifier) where T : class
    {
        return LoadUserDataAsync<T>(identifier).Result;
    }

    public static void SaveCharacterData<T>(string characterName, T data)
    {
        SaveCharacterDataAsync(characterName, data).Wait();
    }

    public static T? LoadCharacterData<T>(string characterName) where T : class
    {
        return LoadCharacterDataAsync<T>(characterName).Result;
    }

    public static bool UserExists(string identifier)
    {
        var filePath = Path.Combine(UsersDirectory, $"{identifier}.json");
        return File.Exists(filePath);
    }

    public static bool CharacterExists(string characterName)
    {
        var filePath = Path.Combine(CharactersDirectory, $"{characterName}.json");
        return File.Exists(filePath);
    }

    public static List<string> GetAllCharacters()
    {
        var characters = new List<string>();
        try
        {
            var files = Directory.GetFiles(CharactersDirectory, "*.json");
            foreach (var file in files)
            {
                characters.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting character list: {ex.Message}");
        }
        return characters;
    }

    public static List<string> GetAllUsers()
    {
        var users = new List<string>();
        try
        {
            var files = Directory.GetFiles(UsersDirectory, "*.json");
            foreach (var file in files)
            {
                users.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user list: {ex.Message}");
        }
        return users;
    }

    public static bool DeleteUser(string identifier)
    {
        try
        {
            var filePath = Path.Combine(UsersDirectory, $"{identifier}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
        }
        return false;
    }

    public static bool DeleteCharacter(string characterName)
    {
        try
        {
            var filePath = Path.Combine(CharactersDirectory, $"{characterName}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting character: {ex.Message}");
        }
        return false;
    }
}