namespace QuickRotate;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Console.WriteLine("Starting");
        Application.Run(new TrayContext());
    }
}

class TrayContext : ApplicationContext
{
    List<uint> displayIDs;
    ContextMenuStrip contextMenu;
    NotifyIcon trayIcon;

    public TrayContext()
    {
        displayIDs = new();

        contextMenu = new();
        contextMenu.Opened += RefreshDisplays;
        contextMenu.Closing += OnClosing;
        contextMenu.Items.AddRange(new ToolStripItem[]{
            new ToolStripMenuItem("Refresh Displays", null, RefreshDisplays),
            new ToolStripMenuItem("Select/Deselect All", null, ToggleAll),
            new ToolStripSeparator(),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Landscape", null, Landscape),
            new ToolStripMenuItem("Portrait", null, Portrait),
            new ToolStripMenuItem("Landscape (Flipped)", null, LandscapeFlipped),
            new ToolStripMenuItem("Portrait (Flipped)", null, PortraitFlipped),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Exit", null, Exit)
        });

        trayIcon = new();
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Text = "QuickRotate";
        trayIcon.Visible = true;
        trayIcon.ContextMenuStrip = contextMenu;
    }

    void Refresh()
    {
        var newDisplayIDs = DisplayUtils.GetDisplayIDs();

        if (displayIDs.SequenceEqual(newDisplayIDs))
            return;

        var items = contextMenu.Items;

        for (int i = 0; i < displayIDs.Count; ++i)
            items.RemoveAt(3);

        displayIDs = newDisplayIDs;

        for (int i = 0; i < displayIDs.Count; ++i)
        {
            var item = new ToolStripMenuItem($"Display {displayIDs[i]}");
            item.CheckOnClick = true;


            items.Insert(3 + i, item);
        }
    }

    void ApplyRotation(DisplayUtils.Orientation orientation)
    {
        for (int i = 0; i < displayIDs.Count; ++i) {
            var item = contextMenu.Items[3 + i] as ToolStripMenuItem;
            
            if (!item!.Checked)
                continue;

            var id = displayIDs[i];

            if (!DisplayUtils.Rotate(id, orientation)) {
                MessageBox.Show($"Couldn't rotate display {id}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    void RefreshDisplays(object? sender, EventArgs e)
    {
        Refresh();
    }

    // Of course the simple act of not closing the whole form when a checkbox
    // is clicked must have 10 different solutions each with their own UI or
    // behavioral problems. This one requires buttons to call .Close()
    // explicitly, but at least it does not require checking cursor bounds or
    // setting tags on everything.
    void OnClosing(object? sender, ToolStripDropDownClosingEventArgs e)
    {
        e.Cancel = e.CloseReason != ToolStripDropDownCloseReason.AppFocusChange
            && e.CloseReason != ToolStripDropDownCloseReason.CloseCalled;
    }

    void ToggleAll(object? sender, EventArgs e)
    {
        var items = contextMenu.Items;
        var checkAll = false;

        for (int i = 0; i < displayIDs.Count; ++i) {
            var item = items[3 + i] as ToolStripMenuItem;

            if (!item!.Checked)
            {
                checkAll = true;
                break;
            }
        }

        for (int i = 0; i < displayIDs.Count; ++i) {
            var item = items[3 + i] as ToolStripMenuItem;

            item!.Checked = checkAll;
        }
    }

    void Landscape(object? sender, EventArgs e)
    {
        ApplyRotation(DisplayUtils.Orientation.LANDSCAPE);
        contextMenu.Close();
    }

    void Portrait(object? sender, EventArgs e)
    {
        ApplyRotation(DisplayUtils.Orientation.PORTRAIT);
        contextMenu.Close();
    }

    void LandscapeFlipped(object? sender, EventArgs e)
    {
        ApplyRotation(DisplayUtils.Orientation.LANDSCAPE_FLIPPED);
        contextMenu.Close();
    }

    void PortraitFlipped(object? sender, EventArgs e)
    {
        ApplyRotation(DisplayUtils.Orientation.PORTRAIT_FLIPPED);
        contextMenu.Close();
    }

    void Exit(object? sender, EventArgs e)
    {
        // Icon lingers after closing if this is not set explicitly
        trayIcon.Visible = false;
        Application.Exit();
    }
}