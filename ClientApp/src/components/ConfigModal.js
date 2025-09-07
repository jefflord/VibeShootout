import React, { useState, useEffect } from 'react';
import styled from 'styled-components';

const ModalOverlay = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 1rem;
`;

const ModalContent = styled.div`
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
`;

const ModalHeader = styled.div`
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
`;

const ModalTitle = styled.h2`
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
  font-weight: 600;
`;

const CloseButton = styled.button`
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #6b7280;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  
  &:hover {
    background: #f3f4f6;
    color: #374151;
  }
`;

const ModalBody = styled.div`
  padding: 1.5rem;
`;

const FormGroup = styled.div`
  margin-bottom: 1.5rem;
`;

const Label = styled.label`
  display: block;
  margin-bottom: 0.5rem;
  color: #374151;
  font-weight: 500;
  font-size: 0.9rem;
`;

const Input = styled.input`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  
  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`;

const Select = styled.select`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 1rem;
  background: white;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  
  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`;

const TextArea = styled.textarea`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 1rem;
  min-height: 120px;
  resize: vertical;
  font-family: inherit;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  
  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`;

const ButtonGroup = styled.div`
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  padding: 1.5rem;
  border-top: 1px solid #e5e7eb;
  
  @media (max-width: 480px) {
    flex-direction: column;
  }
`;

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid transparent;
  
  @media (max-width: 480px) {
    width: 100%;
  }
`;

const PrimaryButton = styled(Button)`
  background: #3b82f6;
  color: white;
  
  &:hover {
    background: #2563eb;
    transform: translateY(-1px);
  }
  
  &:active {
    transform: translateY(0);
  }
`;

const SecondaryButton = styled(Button)`
  background: white;
  color: #374151;
  border-color: #d1d5db;
  
  &:hover {
    background: #f9fafb;
    border-color: #9ca3af;
  }
`;

const HelperText = styled.div`
  margin-top: 0.25rem;
  font-size: 0.8rem;
  color: #6b7280;
`;

const FilePickerButton = styled.button`
  background: #f3f4f6;
  border: 1px solid #d1d5db;
  color: #374151;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.9rem;
  cursor: pointer;
  margin-top: 0.5rem;
  transition: background-color 0.2s ease;
  
  &:hover {
    background: #e5e7eb;
  }
`;

const SectionTitle = styled.h3`
  margin: 0 0 1rem 0;
  color: #1f2937;
  font-size: 1.1rem;
  font-weight: 600;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid #e5e7eb;
`;

const ProviderSection = styled.div`
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1.5rem;
`;

function ConfigModal({ config, onSave, onClose }) {
  const [formData, setFormData] = useState({
    provider: 'Ollama',
    ollamaUrl: 'http://10.0.0.90:11434',
    ollamaModel: 'gpt-oss:20b',
    openAIUrl: 'https://api.openai.com',
    openAIApiKey: '',
    openAIModel: 'gpt-4',
    reviewPrompt: 'Below is git diff, please create a code review using this information',
    repositoryPath: ''
  });

  useEffect(() => {
    if (config) {
      setFormData({
        provider: config.provider || 'Ollama',
        ollamaUrl: config.ollamaUrl || 'http://10.0.0.90:11434',
        ollamaModel: config.ollamaModel || 'gpt-oss:20b',
        openAIUrl: config.openAIUrl || 'https://api.openai.com',
        openAIApiKey: config.openAIApiKey || '',
        openAIModel: config.openAIModel || 'gpt-4',
        reviewPrompt: config.reviewPrompt || 'Below is git diff, please create a code review using this information',
        repositoryPath: config.repositoryPath || ''
      });
    }
  }, [config]);

  const handleChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSave(formData);
  };

  const handleBrowseFolder = async () => {
    // Note: In a real WebView2 environment, you might need to implement
    // a bridge to the .NET backend to show a folder dialog
    const path = prompt('Enter repository path:');
    if (path) {
      handleChange('repositoryPath', path);
    }
  };

  const currentProvider = formData.provider;

  return (
    <ModalOverlay onClick={(e) => e.target === e.currentTarget && onClose()}>
      <ModalContent>
        <form onSubmit={handleSubmit}>
          <ModalHeader>
            <ModalTitle>Configuration</ModalTitle>
            <CloseButton type="button" onClick={onClose}>×</CloseButton>
          </ModalHeader>
          
          <ModalBody>
            <FormGroup>
              <Label htmlFor="repositoryPath">Repository Path</Label>
              <Input
                id="repositoryPath"
                type="text"
                value={formData.repositoryPath}
                onChange={(e) => handleChange('repositoryPath', e.target.value)}
                placeholder="C:\path\to\your\git\repository"
              />
              <FilePickerButton type="button" onClick={handleBrowseFolder}>
                [FOLDER] Browse Folder
              </FilePickerButton>
              <HelperText>
                Path to the Git repository you want to monitor for changes.
              </HelperText>
            </FormGroup>

            <FormGroup>
              <Label htmlFor="provider">AI Provider</Label>
              <Select
                id="provider"
                value={formData.provider}
                onChange={(e) => handleChange('provider', e.target.value)}
                required
              >
                <option value="Ollama">Ollama (Local)</option>
                <option value="OpenAI">OpenAI (API)</option>
              </Select>
              <HelperText>
                Choose between local Ollama server or OpenAI API service.
              </HelperText>
            </FormGroup>

            {currentProvider === 'Ollama' && (
              <ProviderSection>
                <SectionTitle>Ollama Configuration</SectionTitle>
                <FormGroup>
                  <Label htmlFor="ollamaUrl">Ollama Server URL</Label>
                  <Input
                    id="ollamaUrl"
                    type="url"
                    value={formData.ollamaUrl}
                    onChange={(e) => handleChange('ollamaUrl', e.target.value)}
                    placeholder="http://10.0.0.90:11434"
                    required
                  />
                  <HelperText>
                    URL of your Ollama server. Default: http://10.0.0.90:11434
                  </HelperText>
                </FormGroup>

                <FormGroup>
                  <Label htmlFor="ollamaModel">Model</Label>
                  <Input
                    id="ollamaModel"
                    type="text"
                    value={formData.ollamaModel}
                    onChange={(e) => handleChange('ollamaModel', e.target.value)}
                    placeholder="gpt-oss:20b"
                    required
                  />
                  <HelperText>
                    Name of the Ollama model to use for code reviews.
                  </HelperText>
                </FormGroup>
              </ProviderSection>
            )}

            {currentProvider === 'OpenAI' && (
              <ProviderSection>
                <SectionTitle>OpenAI Configuration</SectionTitle>
                <FormGroup>
                  <Label htmlFor="openAIUrl">OpenAI-Compatible Endpoint URL</Label>
                  <Input
                    id="openAIUrl"
                    type="url"
                    value={formData.openAIUrl}
                    onChange={(e) => handleChange('openAIUrl', e.target.value)}
                    placeholder="https://api.openai.com"
                    required
                  />
                  <HelperText>
                    Base URL for OpenAI-compatible API. For OpenAI: https://api.openai.com. For other providers, use their base URL.
                  </HelperText>
                </FormGroup>

                <FormGroup>
                  <Label htmlFor="openAIApiKey">API Key (Optional)</Label>
                  <Input
                    id="openAIApiKey"
                    type="password"
                    value={formData.openAIApiKey}
                    onChange={(e) => handleChange('openAIApiKey', e.target.value)}
                    placeholder="sk-... (leave empty if not required)"
                  />
                  <HelperText>
                    Your OpenAI API key or API key for the OpenAI-compatible service. Leave empty if the endpoint doesn't require authentication.
                  </HelperText>
                </FormGroup>

                <FormGroup>
                  <Label htmlFor="openAIModel">Model</Label>
                  <Input
                    id="openAIModel"
                    type="text"
                    value={formData.openAIModel}
                    onChange={(e) => handleChange('openAIModel', e.target.value)}
                    placeholder="gpt-4"
                    required
                  />
                  <HelperText>
                    Model name to use (e.g., gpt-4, gpt-3.5-turbo, or custom model name for other providers).
                  </HelperText>
                </FormGroup>
              </ProviderSection>
            )}

            <FormGroup>
              <Label htmlFor="reviewPrompt">Code Review Prompt</Label>
              <TextArea
                id="reviewPrompt"
                value={formData.reviewPrompt}
                onChange={(e) => handleChange('reviewPrompt', e.target.value)}
                placeholder="Below is git diff, please create a code review using this information"
                required
              />
              <HelperText>
                This prompt will be sent to the AI along with the git diff to generate code reviews.
              </HelperText>
            </FormGroup>
          </ModalBody>

          <ButtonGroup>
            <SecondaryButton type="button" onClick={onClose}>
              Cancel
            </SecondaryButton>
            <PrimaryButton type="submit">
              Save Configuration
            </PrimaryButton>
          </ButtonGroup>
        </form>
      </ModalContent>
    </ModalOverlay>
  );
}

export default ConfigModal;