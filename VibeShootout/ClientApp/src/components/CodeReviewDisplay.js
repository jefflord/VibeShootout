import React from 'react';
import styled from 'styled-components';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

const Container = styled.div`
  margin-top: 2rem;
`;

const ReviewCard = styled.div`
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  margin-bottom: 1.5rem;
  overflow: hidden;
  transition: transform 0.2s ease, box-shadow 0.2s ease;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
  }
`;

const ReviewHeader = styled.div`
  background: ${props => props.isError ? '#fee2e2' : props.isSkipped ? '#fef3c7' : '#f0f9ff'};
  border-bottom: 1px solid ${props => props.isError ? '#fecaca' : props.isSkipped ? '#fde68a' : '#e0f2fe'};
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
`;

const ReviewTitle = styled.h3`
  margin: 0;
  color: ${props => props.isError ? '#dc2626' : props.isSkipped ? '#d97706' : '#0369a1'};
  font-size: 1.1rem;
  font-weight: 600;
`;

const ReviewMeta = styled.div`
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
`;

const MetricsRow = styled.div`
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  align-items: center;
`;

const Timestamp = styled.span`
  color: #6b7280;
  font-size: 0.85rem;
`;

const Duration = styled.span`
  color: #059669;
  font-size: 0.8rem;
  font-weight: 500;
  background: rgba(5, 150, 105, 0.1);
  padding: 0.2rem 0.5rem;
  border-radius: 12px;
`;

const OllamaMetric = styled.span`
  color: #7c2d12;
  font-size: 0.75rem;
  font-weight: 500;
  background: rgba(124, 45, 18, 0.1);
  padding: 0.15rem 0.4rem;
  border-radius: 8px;
  font-family: monospace;
`;

const TokenMetric = styled.span`
  color: #0369a1;
  font-size: 0.75rem;
  font-weight: 500;
  background: rgba(3, 105, 161, 0.1);
  padding: 0.15rem 0.4rem;
  border-radius: 8px;
  font-family: monospace;
`;

const Checksum = styled.span`
  color: #7c3aed;
  font-size: 0.75rem;
  font-family: monospace;
  background: rgba(124, 58, 237, 0.1);
  padding: 0.15rem 0.4rem;
  border-radius: 8px;
`;

const GitBadge = styled.span`
  color: #16a34a;
  font-size: 0.75rem;
  font-weight: 500;
  background: rgba(22, 163, 74, 0.1);
  padding: 0.15rem 0.4rem;
  border-radius: 8px;
  border: 1px solid rgba(22, 163, 74, 0.2);
`;

const ReviewContent = styled.div`
  padding: 1.5rem;
`;

const MetricsSection = styled.div`
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  margin-bottom: 1rem;
  overflow: hidden;
`;

const MetricsHeader = styled.div`
  background: #e2e8f0;
  padding: 0.5rem 1rem;
  font-weight: 600;
  font-size: 0.9rem;
  color: #475569;
  display: flex;
  justify-content: space-between;
  align-items: center;
`;

const MetricsGrid = styled.div`
  padding: 1rem;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
`;

const MetricItem = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
`;

const MetricLabel = styled.span`
  font-size: 0.8rem;
  color: #64748b;
  font-weight: 500;
`;

const MetricValue = styled.span`
  font-size: 1rem;
  color: #1f2937;
  font-weight: 600;
  font-family: monospace;
`;

const DiffSection = styled.div`
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  margin-bottom: 1rem;
  overflow: hidden;
`;

const DiffHeader = styled.div`
  background: #e2e8f0;
  padding: 0.5rem 1rem;
  font-weight: 600;
  font-size: 0.9rem;
  color: #475569;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
`;

const DiffStats = styled.span`
  font-size: 0.8rem;
  color: #64748b;
  font-weight: normal;
`;

const DiffContent = styled.pre`
  margin: 0;
  padding: 1rem;
  overflow-x: auto;
  font-family: 'Courier New', monospace;
  font-size: 0.85rem;
  line-height: 1.4;
  white-space: pre-wrap;
  word-break: break-word;
`;

const ReviewText = styled.div`
  line-height: 1.6;
  
  h1, h2, h3, h4, h5, h6 {
    color: #1f2937;
    margin-top: 1.5rem;
    margin-bottom: 0.5rem;
  }
  
  p {
    margin-bottom: 1rem;
    color: #374151;
  }
  
  ul, ol {
    margin-bottom: 1rem;
    padding-left: 1.5rem;
  }
  
  li {
    margin-bottom: 0.25rem;
    color: #374151;
  }
  
  code {
    background: #f3f4f6;
    padding: 0.2rem 0.4rem;
    border-radius: 3px;
    font-family: 'Courier New', monospace;
    font-size: 0.9em;
    color: #1f2937;
  }
  
  pre {
    background: #f3f4f6;
    padding: 1rem;
    border-radius: 6px;
    overflow-x: auto;
    margin: 1rem 0;
    border: 1px solid #e5e7eb;
  }

  blockquote {
    border-left: 4px solid #d1d5db;
    padding-left: 1rem;
    margin: 1rem 0;
    font-style: italic;
    color: #6b7280;
    background: #f9fafb;
    padding: 1rem;
    border-radius: 6px;
  }

  /* Table styling for markdown tables */
  table {
    width: 100%;
    border-collapse: collapse;
    margin: 1rem 0;
    font-size: 0.9rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    border-radius: 6px;
    overflow: hidden;
  }

  thead {
    background: #f8fafc;
  }

  th {
    background: #e2e8f0;
    color: #374151;
    font-weight: 600;
    padding: 0.75rem 1rem;
    text-align: left;
    border-bottom: 2px solid #d1d5db;
  }

  td {
    padding: 0.75rem 1rem;
    border-bottom: 1px solid #e5e7eb;
    color: #374151;
    vertical-align: top;
  }

  tbody tr:nth-child(even) {
    background: #f9fafb;
  }

  tbody tr:hover {
    background: #f3f4f6;
  }

  /* Code blocks within table cells */
  td code, th code {
    background: #e5e7eb;
    padding: 0.15rem 0.3rem;
    border-radius: 3px;
    font-size: 0.85em;
  }

  /* Strikethrough text */
  del {
    color: #9ca3af;
    text-decoration: line-through;
  }

  /* Task lists */
  input[type="checkbox"] {
    margin-right: 0.5rem;
  }

  /* Horizontal rule */
  hr {
    border: none;
    border-top: 2px solid #e5e7eb;
    margin: 2rem 0;
  }

  /* Links */
  a {
    color: #3b82f6;
    text-decoration: none;
  }

  a:hover {
    text-decoration: underline;
  }

  /* Strong/bold text */
  strong {
    font-weight: 600;
    color: #1f2937;
  }

  /* Emphasis/italic text */
  em {
    font-style: italic;
    color: #4b5563;
  }
`;

const ErrorMessage = styled.div`
  color: #dc2626;
  background: #fee2e2;
  padding: 1rem;
  border-radius: 6px;
  border: 1px solid #fecaca;
`;

const SkippedMessage = styled.div`
  color: #d97706;
  background: #fef3c7;
  padding: 1rem;
  border-radius: 6px;
  border: 1px solid #fde68a;
  font-style: italic;
`;

const EmptyState = styled.div`
  text-align: center;
  padding: 4rem 2rem;
  color: rgba(255, 255, 255, 0.8);
`;

const EmptyStateIcon = styled.div`
  font-size: 4rem;
  margin-bottom: 1rem;
`;

const EmptyStateText = styled.h2`
  margin: 0 0 0.5rem 0;
  font-weight: 300;
  font-size: 1.5rem;
`;

const EmptyStateSubtext = styled.p`
  margin: 0;
  font-size: 1rem;
  opacity: 0.7;
`;

function CodeReviewDisplay({ reviews }) {
  const formatTimestamp = (timestamp) => {
    return new Date(timestamp).toLocaleString();
  };

  const formatDuration = (durationMs) => {
    if (durationMs == null || isNaN(durationMs)) return 'N/A';
    
    if (durationMs < 1000) {
      return `${Math.round(durationMs)}ms`;
    } else if (durationMs < 60000) {
      return `${(durationMs / 1000).toFixed(1)}s`;
    } else {
      const minutes = Math.floor(durationMs / 60000);
      const seconds = ((durationMs % 60000) / 1000).toFixed(0);
      return `${minutes}m ${seconds}s`;
    }
  };

  const formatSeconds = (seconds) => {
    if (seconds == null || isNaN(seconds)) return 'N/A';
    return `${seconds.toFixed(2)}s`;
  };

  const formatTokensPerSecond = (tokensPerSecond) => {
    if (tokensPerSecond == null || isNaN(tokensPerSecond)) return 'N/A';
    return `${tokensPerSecond.toFixed(1)} tok/s`;
  };

  const getDiffStats = (diff) => {
    if (!diff) return '';
    const lines = diff.split('\n');
    const additions = lines.filter(line => line.startsWith('+')).length;
    const deletions = lines.filter(line => line.startsWith('-')).length;
    return `+${additions} -${deletions} (${lines.length} lines)`;
  };

  const getDiffType = (diff) => {
    if (!diff) return '';
    
    if (diff.includes('# Staged Changes:') && diff.includes('# Unstaged Changes:')) {
      return 'Staged + Unstaged';
    } else if (diff.includes('# Staged Changes:')) {
      return 'Staged Changes';
    } else if (diff.includes('# Unstaged Changes:')) {
      return 'Unstaged Changes';
    } else {
      return 'Tracked Files';
    }
  };

  const isSkipped = (review) => {
    return !review.isSuccess && review.errorMessage && review.errorMessage.includes('Duplicate diff');
  };

  // Debug logging to see what data we're receiving
  if (reviews.length > 0) {
    console.log('CodeReviewDisplay - First review data:', reviews[0]);
    if (reviews[0].ollamaMetrics) {
      console.log('OllamaMetrics:', reviews[0].ollamaMetrics);
    }
  }

  if (reviews.length === 0) {
    return (
      <Container>
        <EmptyState>
          <EmptyStateIcon>??</EmptyStateIcon>
          <EmptyStateText>Waiting for code changes...</EmptyStateText>
          <EmptyStateSubtext>
            Configure a repository path and start making changes to tracked files to see AI-powered code reviews here.
          </EmptyStateSubtext>
        </EmptyState>
      </Container>
    );
  }

  return (
    <Container>
      {reviews.map((review) => {
        const isError = !review.isSuccess && !isSkipped(review);
        const skipped = isSkipped(review);
        const diffType = getDiffType(review.diff);
        
        // More robust metrics checking with fallbacks
        const hasMetrics = review.ollamaMetrics || review.OllamaMetrics;
        const metrics = hasMetrics;
        
        return (
          <ReviewCard key={review.id}>
            <ReviewHeader isError={isError} isSkipped={skipped}>
              <ReviewTitle isError={isError} isSkipped={skipped}>
                {review.isSuccess 
                  ? '? Code Review Completed' 
                  : skipped 
                    ? '?? Review Skipped' 
                    : '? Review Failed'}
              </ReviewTitle>
              
              <ReviewMeta>
                <Timestamp>{formatTimestamp(review.timestamp)}</Timestamp>
                
                <MetricsRow>
                  {review.durationMs !== undefined && review.durationMs !== null && (
                    <Duration>?? {formatDuration(review.durationMs)}</Duration>
                  )}
                  {diffType && (
                    <GitBadge title="Git diff type">?? {diffType}</GitBadge>
                  )}
                </MetricsRow>
                
                {metrics && (
                  <MetricsRow>
                    {metrics.promptTokensPerSecond != null && (
                      <TokenMetric title="Input tokens per second">
                        ?? {formatTokensPerSecond(metrics.promptTokensPerSecond)}
                      </TokenMetric>
                    )}
                    {metrics.outputTokensPerSecond != null && (
                      <TokenMetric title="Output tokens per second">
                        ?? {formatTokensPerSecond(metrics.outputTokensPerSecond)}
                      </TokenMetric>
                    )}
                    {metrics.totalDurationSeconds != null && (
                      <OllamaMetric title="Total Ollama processing time">
                        ?? {formatSeconds(metrics.totalDurationSeconds)}
                      </OllamaMetric>
                    )}
                  </MetricsRow>
                )}
                
                <MetricsRow>
                  {review.diffChecksum && (
                    <Checksum title="Diff checksum for duplicate detection">
                      #{review.diffChecksum.substring(0, 8)}
                    </Checksum>
                  )}
                </MetricsRow>
              </ReviewMeta>
            </ReviewHeader>
            
            <ReviewContent>
              {review.isSuccess ? (
                <>
                  {metrics && (
                    <MetricsSection>
                      <MetricsHeader>
                        ?? Ollama Performance Metrics
                      </MetricsHeader>
                      <MetricsGrid>
                        <MetricItem>
                          <MetricLabel>Total Processing Time</MetricLabel>
                          <MetricValue>{formatSeconds(metrics.totalDurationSeconds)}</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Model Load Time</MetricLabel>
                          <MetricValue>{formatSeconds(metrics.loadDurationSeconds)}</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Input Tokens</MetricLabel>
                          <MetricValue>{metrics.promptEvalCount || 0} tokens</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Input Speed</MetricLabel>
                          <MetricValue>{formatTokensPerSecond(metrics.promptTokensPerSecond)}</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Output Tokens</MetricLabel>
                          <MetricValue>{metrics.evalCount || 0} tokens</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Output Speed</MetricLabel>
                          <MetricValue>{formatTokensPerSecond(metrics.outputTokensPerSecond)}</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Prompt Processing Time</MetricLabel>
                          <MetricValue>{formatSeconds(metrics.promptEvalDurationSeconds)}</MetricValue>
                        </MetricItem>
                        <MetricItem>
                          <MetricLabel>Generation Time</MetricLabel>
                          <MetricValue>{formatSeconds(metrics.evalDurationSeconds)}</MetricValue>
                        </MetricItem>
                      </MetricsGrid>
                    </MetricsSection>
                  )}

                  {review.diff && (
                    <DiffSection>
                      <DiffHeader>
                        ?? Git Diff - Tracked Files Only
                        <DiffStats>{getDiffStats(review.diff)}</DiffStats>
                      </DiffHeader>
                      <DiffContent>{review.diff}</DiffContent>
                    </DiffSection>
                  )}
                  
                  <ReviewText>
                    <ReactMarkdown 
                      remarkPlugins={[remarkGfm]}
                      components={{
                        // Custom table rendering with proper styling
                        table: ({ children }) => (
                          <table style={{ width: '100%', borderCollapse: 'collapse', margin: '1rem 0' }}>
                            {children}
                          </table>
                        ),
                        // Ensure code blocks in tables render properly
                        code: ({ inline, children, ...props }) => (
                          inline ? (
                            <code style={{ 
                              background: '#f3f4f6', 
                              padding: '0.2rem 0.4rem', 
                              borderRadius: '3px',
                              fontSize: '0.9em'
                            }} {...props}>
                              {children}
                            </code>
                          ) : (
                            <pre style={{
                              background: '#f3f4f6',
                              padding: '1rem',
                              borderRadius: '6px',
                              overflow: 'auto',
                              border: '1px solid #e5e7eb'
                            }}>
                              <code {...props}>{children}</code>
                            </pre>
                          )
                        )
                      }}
                    >
                      {review.review || 'No review content available'}
                    </ReactMarkdown>
                  </ReviewText>
                </>
              ) : skipped ? (
                <>
                  {review.diff && (
                    <DiffSection>
                      <DiffHeader>
                        ?? Git Diff - Tracked Files Only (Duplicate)
                        <DiffStats>{getDiffStats(review.diff)}</DiffStats>
                      </DiffHeader>
                      <DiffContent>{review.diff}</DiffContent>
                    </DiffSection>
                  )}
                  
                  <SkippedMessage>
                    <strong>Review Skipped:</strong> {review.errorMessage || 'Unknown reason'}
                  </SkippedMessage>
                </>
              ) : (
                <ErrorMessage>
                  <strong>Error:</strong> {review.errorMessage || 'Unknown error occurred'}
                </ErrorMessage>
              )}
            </ReviewContent>
          </ReviewCard>
        );
      })}
    </Container>
  );
}

export default CodeReviewDisplay;