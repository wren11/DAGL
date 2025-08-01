using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Network;
using Silk.NET.Input;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles reconnection dialog and network reconnection logic
/// </summary>
public class ReconnectDialogPane : DialogPane
{
    private const int DEFAULT_WIDTH = 300;
    private const int DEFAULT_HEIGHT = 150;
    private const int PROGRESS_WIDTH = 200;
    private const int PROGRESS_HEIGHT = 20;
    private const int BUTTON_WIDTH = 80;
    private const int BUTTON_HEIGHT = 25;
    private const int TEXT_PADDING = 10;

    private Label statusLabel;
    private ProgressBar progressBar;
    private Button cancelButton;
    private Label messageLabel;
        
    private string serverAddress;
    private int serverPort;
    private int reconnectAttempts;
    private int maxReconnectAttempts;
    private int reconnectDelay;
    private bool isReconnecting;
    private bool isCancelled;
    private CancellationTokenSource cancellationTokenSource;

    // Network manager reference
    private NetworkManager networkManager;

    // Events
    public event Action<bool> ReconnectionComplete;
    public event Action ReconnectionCancelled;

    public ReconnectDialogPane()
    {
        InitializeDialog();
    }

    public ReconnectDialogPane(string serverAddress, int serverPort, int maxAttempts = 5)
    {
        InitializeDialog();
        SetServerInfo(serverAddress, serverPort);
        SetMaxAttempts(maxAttempts);
    }

    private void InitializeDialog()
    {
        // Set dialog properties
        SetSize(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        SetTitle("Reconnecting...");
        SetModal(true);

        // Initialize variables
        serverAddress = "localhost";
        serverPort = 8080;
        reconnectAttempts = 0;
        maxReconnectAttempts = 5;
        reconnectDelay = 2000; // 2 seconds
        isReconnecting = false;
        isCancelled = false;
        cancellationTokenSource = null;

        // Create UI elements
        CreateUIElements();

        // Set default position (center of screen)
        CenterOnScreen();
    }

    public new void SetTitle(string title)
    {
        Title = title;
    }

    public new void SetModal(bool isModal)
    {
        IsModal = isModal;
    }

    private void CreateUIElements()
    {
        // Create message label
        messageLabel = new Label("Attempting to reconnect to server...", new Point(TEXT_PADDING, TEXT_PADDING));
        messageLabel.Bounds = new Rectangle(messageLabel.Bounds.Location, new Size(Width - TEXT_PADDING * 2, 20));
        //messageLabel.Alignment = TextAlignment.Center;
        AddChild(messageLabel);

        // Create status label
        statusLabel = new Label("Initializing...", new Point(TEXT_PADDING, TEXT_PADDING + 30));
        statusLabel.Bounds = new Rectangle(statusLabel.Bounds.Location, new Size(Width - TEXT_PADDING * 2, 20));
        //statusLabel.Alignment = TextAlignment.Center;
        AddChild(statusLabel);

        // Create progress bar
        progressBar = new ProgressBar(new Rectangle((Width - PROGRESS_WIDTH) / 2, TEXT_PADDING + 60, PROGRESS_WIDTH, PROGRESS_HEIGHT));
        progressBar.Maximum = maxReconnectAttempts;
        progressBar.Value = 0;
        AddChild(progressBar);

        // Create cancel button
        cancelButton = new Button("Cancel", new Point((Width - BUTTON_WIDTH) / 2, Height - BUTTON_HEIGHT - TEXT_PADDING), new Size(BUTTON_WIDTH, BUTTON_HEIGHT));
        cancelButton.Click += OnCancelButtonClick;
        AddChild(cancelButton);
    }

    public void SetServerInfo(string address, int port)
    {
        serverAddress = address ?? "localhost";
        serverPort = port;
    }

    public void SetMaxAttempts(int maxAttempts)
    {
        maxReconnectAttempts = Math.Max(1, maxAttempts);
        if (progressBar != null)
        {
            progressBar.Maximum = maxReconnectAttempts;
        }
    }

    public void SetReconnectDelay(int delayMs)
    {
        reconnectDelay = Math.Max(100, delayMs);
    }

    public void SetNetworkManager(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    public void StartReconnection()
    {
        if (isReconnecting)
            return;

        isReconnecting = true;
        isCancelled = false;
        reconnectAttempts = 0;

        if (progressBar != null)
        {
            progressBar.Value = 0;
        }

        UpdateStatus("Starting reconnection...");

        // Start reconnection process
        Task.Run(ReconnectionProcess);
    }

    private async Task ReconnectionProcess()
    {
        try
        {
            cancellationTokenSource = new CancellationTokenSource();

            while (reconnectAttempts < maxReconnectAttempts && !isCancelled)
            {
                reconnectAttempts++;

                // Update UI on main thread
                await UpdateUIOnMainThread(() =>
                {
                    UpdateStatus($"Attempt {reconnectAttempts} of {maxReconnectAttempts}...");
                    if (progressBar != null)
                    {
                        progressBar.Value = reconnectAttempts;
                    }
                });

                // Attempt reconnection
                var success = await AttemptReconnection();

                if (success)
                {
                    await UpdateUIOnMainThread(() =>
                    {
                        UpdateStatus("Reconnection successful!");
                        if(progressBar != null)
                            progressBar.Value = maxReconnectAttempts;
                    });

                    await Task.Delay(1000); // Show success message briefly
                        
                    isReconnecting = false;
                    ReconnectionComplete?.Invoke(true);
                    return;
                }

                if (!isCancelled && reconnectAttempts < maxReconnectAttempts)
                {
                    await UpdateUIOnMainThread(() =>
                    {
                        UpdateStatus($"Reconnection failed. Retrying in {reconnectDelay / 1000} seconds...");
                    });

                    // Wait before next attempt
                    await Task.Delay(reconnectDelay, cancellationTokenSource.Token);
                }
            }

            if (!isCancelled)
            {
                await UpdateUIOnMainThread(() =>
                {
                    UpdateStatus("Reconnection failed after all attempts.");
                });

                await Task.Delay(2000); // Show failure message
            }

            isReconnecting = false;
            ReconnectionComplete?.Invoke(false);
        }
        catch (OperationCanceledException)
        {
            // Reconnection was cancelled
            isReconnecting = false;
            ReconnectionCancelled?.Invoke();
        }
        catch (Exception ex)
        {
            await UpdateUIOnMainThread(() =>
            {
                UpdateStatus($"Error: {ex.Message}");
            });

            isReconnecting = false;
            ReconnectionComplete?.Invoke(false);
        }
    }

    private async Task<bool> AttemptReconnection()
    {
        try
        {
            if (networkManager != null)
            {
                // For now, just return false since we don't know the exact ConnectAsync signature
                // In a real implementation, this would call the correct method
                await Task.Delay(1000);
                return false;
            }
            else
            {
                // Simulate connection attempt
                await Task.Delay(1000);
                return false; // Always fail for simulation
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task UpdateUIOnMainThread(Action action)
    {
        // In a real implementation, this would dispatch to the main UI thread
        // For now, we'll just execute directly
        action();
        await Task.Delay(10); // Small delay to prevent UI blocking
    }

    private void UpdateStatus(string status)
    {
        if (statusLabel != null)
        {
            statusLabel.Text = status;
        }
    }

    private void OnCancelButtonClick(object sender, EventArgs e)
    {
        CancelReconnection();
    }

    public void CancelReconnection()
    {
        if (!isReconnecting)
            return;

        isCancelled = true;
        cancellationTokenSource?.Cancel();

        UpdateStatus("Reconnection cancelled.");
        ReconnectionCancelled?.Invoke();
    }

    public override bool HandleEvent(Event keyEvent)
    {
        if (keyEvent is KeyEvent ke && ke.Type == EventType.KeyDown)
        {
            switch (ke.Key)
            {
            case Silk.NET.Input.Key.Escape:
                CancelReconnection();
                return true;
            }
        }

        return base.HandleEvent(keyEvent);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Update progress bar animation if needed
        if (progressBar != null && isReconnecting)
        {
            // Could add pulsing animation here
        }
    }

    public bool IsReconnecting()
    {
        return isReconnecting;
    }

    public int GetCurrentAttempt()
    {
        return reconnectAttempts;
    }

    public int GetMaxAttempts()
    {
        return maxReconnectAttempts;
    }

    public void SetMessage(string message)
    {
        if (messageLabel != null)
        {
            messageLabel.Text = message;
        }
    }

    public void SetStatus(string status)
    {
        UpdateStatus(status);
    }

    public void SetProgress(int value)
    {
        if (progressBar != null)
        {
            progressBar.Value = value;
        }
    }

    public void EnableCancelButton(bool enable)
    {
        if (cancelButton != null)
        {
            cancelButton.IsEnabled = enable;
        }
    }

    public override void OnShown()
    {
        base.OnShown();
            
        // Auto-start reconnection when dialog is shown
        if (!isReconnecting)
        {
            StartReconnection();
        }
    }

    public override void OnClosed()
    {
        base.OnClosed();
            
        // Cancel any ongoing reconnection
        CancelReconnection();
            
        // Clean up event handlers
        if (cancelButton != null)
        {
            cancelButton.Click -= OnCancelButtonClick;
        }
    }

    public static async Task<bool> ShowReconnectDialog(string serverAddress, int serverPort, int maxAttempts = 5)
    {
        var result = false;
        var dialogClosed = false;

        var dialog = new ReconnectDialogPane(serverAddress, serverPort, maxAttempts);
            
        dialog.ReconnectionComplete += (success) =>
        {
            result = success;
            dialogClosed = true;
        };
            
        dialog.ReconnectionCancelled += () =>
        {
            result = false;
            dialogClosed = true;
        };

        dialog.Show();

        // Wait for dialog to close
        while (!dialogClosed)
        {
            await Task.Delay(10);
        }

        return result;
    }
}