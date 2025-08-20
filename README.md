# VibeShootout - AI Code Reviewer

A hybrid .NET 8 + React WebView2 desktop application that automatically performs AI-powered code reviews on local Git repositories using Ollama.

## Features

- **Automatic File Monitoring**: Watches your Git repository for changes using `FileSystemWatcher`
- **Git Integration**: Automatically generates diffs using `git diff` command
- **AI Code Reviews**: Sends diffs to Ollama for AI-powered code analysis
- **Modern UI**: React-based web interface with real-time updates via SignalR
- **WebView2 Integration**: Native desktop app hosting React web application
- **Configurable**: Easy configuration of Ollama server URL and review prompts

## Prerequisites

- .NET 8.0 or higher
- Node.js and npm (for React development)
- Git (must be accessible from command line)
- Ollama server running (default: http://10.0.0.90:11434)

## Getting Started

### 1. Build the Application

```bash
# Build the React frontend
cd VibeShootout/ClientApp
npm install
npm run build

# Build the .NET application
cd ..
dotnet build
```

### 2. Run the Application

```bash
dotnet run
```

The application will:
1. Start a web server on port 5632
2. Open a WPF window with WebView2 displaying the React UI
3. Load configuration from `config.json`

### 3. Configure the Application

1. Click "Edit Config" in the top-right corner
2. Set your Git repository path
3. Configure Ollama server URL (default: http://10.0.0.90:11434)
4. Customize the code review prompt
5. Save the configuration

### 4. Start Code Reviewing

- Make changes to files in your configured Git repository
- The application will automatically detect changes
- After 2 seconds of inactivity, it will generate a diff and send it to Ollama
- Code review results will appear in real-time in the UI

## Configuration

The application uses a `config.json` file with the following structure:

```json
{
  "OllamaUrl": "http://10.0.0.90:11434",
  "ReviewPrompt": "Below is git diff, please create a code review using this information",
  "RepositoryPath": "C:\\path\\to\\your\\git\\repository"
}
```

## Architecture

### Backend (.NET 8)
- **WPF Host**: Main desktop application window
- **WebView2**: Hosts the React application
- **ASP.NET Core Web Server**: Serves React app and provides API endpoints
- **SignalR Hub**: Real-time communication with frontend
- **File System Watcher**: Monitors repository for changes
- **Git Service**: Executes git commands to generate diffs
- **Ollama Service**: Communicates with Ollama API for code reviews

### Frontend (React)
- **Modern UI**: Clean, responsive design optimized for desktop and mobile
- **Real-time Updates**: SignalR integration for live code review results
- **Configuration Management**: UI for editing application settings
- **Code Display**: Syntax-highlighted diff and markdown-rendered reviews

## API Endpoints

- `GET /api/config` - Get current configuration
- `POST /api/config` - Save configuration
- `GET /` - Serve React application
- `/hubs/codereview` - SignalR hub for real-time updates

## Development

### Backend Development
```bash
cd VibeShootout
dotnet watch run
```

### Frontend Development
```bash
cd VibeShootout/ClientApp
npm start
```

Note: For frontend development, you may need to proxy API calls to `http://localhost:5632`

## Troubleshooting

### Common Issues

1. **"Git command not found"**
   - Ensure Git is installed and accessible from the command line
   - Check that `git` is in your system PATH

2. **"Cannot connect to Ollama"**
   - Verify Ollama server is running
   - Check the Ollama URL in configuration
   - Ensure network connectivity to Ollama server

3. **"Repository path invalid"**
   - Ensure the path points to a valid Git repository
   - Check that the `.git` folder exists in the specified directory

4. **React app not loading**
   - Ensure the React app has been built (`npm run build`)
   - Check that the `ClientApp/build` folder contains the built files

### Logs and Debugging

- The application logs errors to the console
- Check the browser developer tools in WebView2 for frontend issues
- SignalR connection status is displayed in the header

## License

This project is open source. See LICENSE file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request