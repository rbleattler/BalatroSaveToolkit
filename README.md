# Balatro JKR File Viewer

## Overview

This is a simple .NET 9 WPF application built with C# 13 that loads and parses .jkr files containing Lua tables. The application uses the existing `LuaTableConverter` class to convert Lua tables to C# Dictionary objects and displays them in a browsable tree format.

## Features

### Core Functionality

- **Load JKR Files**: Click "Load JKR File" button to open and parse .jkr files
- **Tree View Display**: Browse nested dictionary data in an expandable tree structure
- **Logging Panel**: Toggle-able logging panel with timestamp logging
- **Status Updates**: Real-time status updates showing current operations

### Technical Features

- **File Operations**: Handles file loading with proper error handling
- **Lua Parsing**: Uses the existing LuaTableConverter to parse Lua table syntax
- **Data Visualization**: Displays nested objects and arrays in a clear hierarchy
- **Persistent Logging**: Logs are saved to temp directory and cleared on startup

## Project Structure

### Core Files

- `MainWindow.xaml` - Main UI layout with TreeView, logging panel, and controls
- `MainWindow.xaml.cs` - Main window logic and event handling
- `LuaTableConverter.cs` - Existing Lua table parser (enhanced)
- `Logger.cs` - Logging system with file output
- `TreeNodeViewModel.cs` - View model for TreeView data binding

### Key Components

#### MainWindow

- Load File button with OpenFileDialog
- TreeView for browsing parsed data
- Collapsible logging panel
- Status label for operation feedback

#### Logger Class

- Timestamped log entries
- Event-based logging for UI updates
- Automatic file logging to temp directory
- Log clearing functionality

#### TreeNodeViewModel

- Data binding for TreeView
- Hierarchical data representation
- Display name and value formatting
- ObservableCollection for child nodes

## Usage

### Loading Files

1. Click "Load JKR File" button
2. Select a .jkr file from the file dialog
3. The application will parse the Lua table and display it in the tree

### Viewing Data

- Expand/collapse tree nodes to navigate the data structure
- Each node shows the key name and value/type information
- Nested objects and arrays are clearly indicated

### Logging

- Check "Show Logs" to display the logging panel
- Logs show file operations, parsing results, and any errors
- Click "Clear" to clear the current log display
- Logs are automatically saved to the temp directory

## Sample JKR File Format (after decompression)

```lua
{
  ["name"] = "Test Game",
  ["level"] = 5,
  ["active"] = true,
  ["player"] = {
    ["name"] = "John",
    ["score"] = 1250,
    ["inventory"] = {
      ["item1"] = "sword",
      ["item2"] = "shield"
    }
  },
  ["enemies"] = {
    [1] = { ["name"] = "Goblin", ["hp"] = 30 },
    [2] = { ["name"] = "Orc", ["hp"] = 50 }
  }
}
```

## Error Handling

- File loading errors are displayed in message boxes and logged
- Parsing errors show the specific line and character position
- Invalid file formats are handled gracefully
- Log file write errors are silently ignored to prevent crashes

## Technical Requirements Met

- ✅ .NET 9 and C# 13 compatible
- ✅ WPF with XAML UI
- ✅ Simple architecture without over-engineering
- ✅ Built-in controls (TreeView, TextBox, etc.)
- ✅ Log file in app temp directory
- ✅ Proper error handling throughout

## Building and Running

```bash
cd BalatroSaveExplorer
dotnet build
dotnet run
```

The application will launch and be ready to load .jkr files immediately.
