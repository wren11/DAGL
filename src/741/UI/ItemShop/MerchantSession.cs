using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.ItemShop;

public class MerchantSession : ControlPane
{
    private ControlPane? _activeDialog;
    private byte _activeDialogType;
    private bool _isActive;
    private readonly List<ControlPane> _managedDialogs = [];

    public event EventHandler<ItemTransactionEventArgs> ItemTransactionRequested = delegate { };

    public MerchantSession(int merchantId, Dictionary<int, int> merchantItems, Dictionary<int, int> merchantPrices)
    {
        _activeDialogType = 0xFF;
        _isActive = false;
            
        Position = new System.Drawing.Point(0, 0);
        Size = new System.Drawing.Size(640, 480);
    }

    public void HandlePacket(byte[] packet)
    {
        if (packet == null || packet.Length < 2) return;

        if (_activeDialog != null)
        {
            RemoveChild(_activeDialog);
            _activeDialog = null;
        }

        var packetType = packet[1];
            
        try
        {
            switch (packetType)
            {
            case 0:
                _activeDialog = new TextMenuDialog(packet);
                break;
            case 1:
                _activeDialog = new ArgumentedTextMenuDialog(packet);
                break;
            case 2:
                _activeDialog = new ServerItemMenuDialog(packet);
                WireItemTransactionEvents(_activeDialog as ServerItemMenuDialog);
                break;
            case 3:
                _activeDialog = new ClientItemMenuDialog(packet);
                WireItemTransactionEvents(_activeDialog as ClientItemMenuDialog);
                break;
            case 4:
            case 10:
                _activeDialog = new TextInputMenuDialog(packet);
                break;
            case 5:
            case 11:
                _activeDialog = new ServerSpellMenuDialog(packet);
                break;
            case 6:
                _activeDialog = new ServerSkillMenuDialog(packet);
                break;
            case 7:
                _activeDialog = new FaceMenuDialog(packet);
                break;
            case 8:
                _activeDialog = new NPCServerItemMenuDialog(packet);
                WireItemTransactionEvents(_activeDialog as NPCServerItemMenuDialog);
                break;
            case 9:
                _activeDialog = new ServerItemMenuDialog2(packet);
                WireItemTransactionEvents(_activeDialog as ServerItemMenuDialog2);
                break;
            default:
                return;
            }

            if (_activeDialog != null)
            {
                _activeDialogType = packetType;
                _managedDialogs.Add(_activeDialog);
                AddChild(_activeDialog);
                _isActive = true;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error creating dialog for packet type {packetType}: {ex.Message}");
        }
    }

    private void WireItemTransactionEvents(ServerItemMenuDialog? dialog)
    {
        if (dialog != null)
        {
            dialog.ItemTransactionRequested += OnItemTransactionRequested;
        }
    }

    private void WireItemTransactionEvents(ClientItemMenuDialog? dialog)
    {
        if (dialog != null)
        {
            dialog.ItemTransactionRequested += OnItemTransactionRequested;
        }
    }

    private void WireItemTransactionEvents(NPCServerItemMenuDialog? dialog)
    {
        if (dialog != null)
        {
            dialog.ItemTransactionRequested += OnItemTransactionRequested;
        }
    }

    private void WireItemTransactionEvents(ServerItemMenuDialog2? dialog)
    {
        if (dialog != null)
        {
            dialog.ItemTransactionRequested += OnItemTransactionRequested;
        }
    }

    private void OnItemTransactionRequested(object? sender, ItemTransactionEventArgs e)
    {
        try
        {
            var packet = CreateTransactionPacket(e);
            SendTransactionPacket(packet);
            e.TransactionSuccessful = true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Transaction failed: {ex.Message}");
            e.TransactionSuccessful = false;
        }
    }

    private byte[] CreateTransactionPacket(ItemTransactionEventArgs e)
    {
        var packet = new List<byte>();
            
        packet.Add(0x2F);
        packet.Add(e.IsBuyTransaction ? (byte)0x02 : (byte)0x03);
        packet.AddRange(BitConverter.GetBytes(e.ItemId));
        packet.AddRange(BitConverter.GetBytes(e.Quantity));
        packet.AddRange(BitConverter.GetBytes(e.Price));
        packet.Add(e.MerchantId);
        packet.AddRange(BitConverter.GetBytes(e.MenuId));
            
        return packet.ToArray();
    }

    private void SendTransactionPacket(byte[] packet)
    {
        var networkManager = NetworkManager.Instance;
        if (networkManager != null)
        {
            networkManager.SendPacket(packet);
        }
    }

    public void CloseActiveDialog()
    {
        if (_activeDialog != null)
        {
            RemoveChild(_activeDialog);
            _activeDialog = null;
            _activeDialogType = 0xFF;
            _isActive = false;
        }
    }

    public override void Dispose()
    {
        if (_activeDialog != null)
        {
            CloseActiveDialog();
        }

        foreach (var dialog in _managedDialogs)
        {
            dialog?.Dispose();
        }
        _managedDialogs.Clear();

        base.Dispose();
    }

    public bool IsActive => _isActive;
    public byte ActiveDialogType => _activeDialogType;
}