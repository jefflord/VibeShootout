import React, { useState, useEffect } from 'react';
import styled from 'styled-components';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import CodeReviewDisplay from './components/CodeReviewDisplay';
import ConfigModal from './components/ConfigModal';
import Header from './components/Header';
import { getConfig, saveConfig } from './services/api';

const AppContainer = styled.div`
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
`;

const MainContent = styled.div`
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
`;

function App() {
  const [codeReviews, setCodeReviews] = useState([]);
  const [config, setConfig] = useState(null);
  const [showConfigModal, setShowConfigModal] = useState(false);
  const [connection, setConnection] = useState(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    console.log('App component mounting...');
    loadConfig();
    setupSignalRConnection();
  }, []);

  const loadConfig = async () => {
    try {
      console.log('Loading config from API...');
      const configData = await getConfig();
      console.log('Config loaded:', configData);
      setConfig(configData);
    } catch (error) {
      console.error('Failed to load config:', error);
    }
  };

  const setupSignalRConnection = async () => {
    console.log('Setting up SignalR connection...');
    
    const newConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5632/hubs/codereview')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    newConnection.on('CodeReviewCompleted', (result) => {
      console.log('Received CodeReviewCompleted:', result);
      setCodeReviews(prev => {
        const updated = [result, ...prev.slice(0, 9)];
        console.log('Updated code reviews:', updated);
        return updated;
      });
    });

    newConnection.onreconnecting(() => {
      console.log('SignalR: Reconnecting...');
      setIsConnected(false);
    });

    newConnection.onreconnected(() => {
      console.log('SignalR: Reconnected');
      setIsConnected(true);
    });

    newConnection.onclose(() => {
      console.log('SignalR: Connection closed');
      setIsConnected(false);
    });

    try {
      console.log('Starting SignalR connection...');
      await newConnection.start();
      setConnection(newConnection);
      setIsConnected(true);
      console.log('SignalR Connected successfully');
    } catch (error) {
      console.error('SignalR Connection Error:', error);
      setIsConnected(false);
    }
  };

  const handleSaveConfig = async (newConfig) => {
    try {
      console.log('Saving config:', newConfig);
      await saveConfig(newConfig);
      setConfig(newConfig);
      setShowConfigModal(false);
      console.log('Config saved successfully');
    } catch (error) {
      console.error('Failed to save config:', error);
      alert('Failed to save configuration: ' + error.message);
    }
  };

  console.log('App render - codeReviews count:', codeReviews.length, 'isConnected:', isConnected);

  return (
    <AppContainer>
      <Header 
        onEditConfig={() => setShowConfigModal(true)}
        isConnected={isConnected}
        repositoryPath={config?.repositoryPath}
      />
      
      <MainContent>
        <CodeReviewDisplay reviews={codeReviews} />
        
        {showConfigModal && (
          <ConfigModal
            config={config}
            onSave={handleSaveConfig}
            onClose={() => setShowConfigModal(false)}
          />
        )}
      </MainContent>
    </AppContainer>
  );
}

export default App;