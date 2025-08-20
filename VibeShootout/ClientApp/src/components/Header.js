import React from 'react';
import styled from 'styled-components';

const HeaderContainer = styled.header`
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
`;

const Title = styled.h1`
  color: white;
  margin: 0;
  font-size: 1.8rem;
  font-weight: 300;
  
  @media (max-width: 768px) {
    font-size: 1.4rem;
  }
`;

const StatusInfo = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
`;

const StatusIndicator = styled.div`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: white;
  font-size: 0.9rem;
`;

const StatusDot = styled.div`
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: ${props => props.connected ? '#4ade80' : '#ef4444'};
`;

const ProviderBadge = styled.div`
  background: ${props => props.provider === 'Ollama' ? 'rgba(124, 45, 18, 0.3)' : 'rgba(5, 150, 105, 0.3)'};
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 500;
  border: 1px solid ${props => props.provider === 'Ollama' ? 'rgba(124, 45, 18, 0.5)' : 'rgba(5, 150, 105, 0.5)'};
  display: flex;
  align-items: center;
  gap: 0.25rem;
`;

const RepositoryPath = styled.div`
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.8rem;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  
  @media (max-width: 768px) {
    max-width: 200px;
  }
`;

const ConfigButton = styled.button`
  background: rgba(255, 255, 255, 0.2);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s ease;

  &:hover {
    background: rgba(255, 255, 255, 0.3);
    transform: translateY(-1px);
  }

  &:active {
    transform: translateY(0);
  }
`;

function Header({ onEditConfig, isConnected, repositoryPath, config }) {
  const provider = config?.provider || 'Unknown';
  
  return (
    <HeaderContainer>
      <Title>VibeShootout AI Code Reviewer</Title>
      
      <StatusInfo>
        <StatusIndicator>
          <StatusDot connected={isConnected} />
          {isConnected ? 'Connected' : 'Disconnected'}
        </StatusIndicator>
        
        {provider !== 'Unknown' && (
          <ProviderBadge provider={provider}>
            {provider === 'Ollama' ? '[AI]' : '[API]'} {provider}
          </ProviderBadge>
        )}
        
        {repositoryPath && (
          <RepositoryPath title={repositoryPath}>
            [REPO] {repositoryPath}
          </RepositoryPath>
        )}
        
        <ConfigButton onClick={onEditConfig}>
          Edit Config
        </ConfigButton>
      </StatusInfo>
    </HeaderContainer>
  );
}

export default Header;