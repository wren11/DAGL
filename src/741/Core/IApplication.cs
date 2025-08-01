using System;

namespace DarkAges.Library.Core;

public interface IApplication
{
    void Initialize();
    void Run();
    void Shutdown();
    bool IsRunning { get; }
        
    // UI Dialog methods
    void ShowCreateUserDialog();
    void ShowLoginDialog();
    void ShowPasswordDialog();
    void ShowCreditPane();
    void OpenHomepage();
    void Exit();
}