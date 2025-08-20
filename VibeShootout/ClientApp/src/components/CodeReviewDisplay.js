import React from 'react';
import styled from 'styled-components';
import ReactMarkdown from 'react-markdown';

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
  background: ${props => props.isError ? '#fee2e2' : '#f0f9ff'};
  border-bottom: 1px solid ${props => props.isError ? '#fecaca' : '#e0f2fe'};
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
`;

const ReviewTitle = styled.h3`
  margin: 0;
  color: ${props => props.isError ? '#dc2626' : '#0369a1'};
  font-size: 1.1rem;
  font-weight: 600;
`;

const Timestamp = styled.span`
  color: #6b7280;
  font-size: 0.85rem;
`;

const ReviewContent = styled.div`
  padding: 1.5rem;
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
  }
  
  pre {
    background: #f3f4f6;
    padding: 1rem;
    border-radius: 6px;
    overflow-x: auto;
    margin: 1rem 0;
  }

  blockquote {
    border-left: 4px solid #d1d5db;
    padding-left: 1rem;
    margin: 1rem 0;
    font-style: italic;
    color: #6b7280;
  }
`;

const ErrorMessage = styled.div`
  color: #dc2626;
  background: #fee2e2;
  padding: 1rem;
  border-radius: 6px;
  border: 1px solid #fecaca;
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

  if (reviews.length === 0) {
    return (
      <Container>
        <EmptyState>
          <EmptyStateIcon>??</EmptyStateIcon>
          <EmptyStateText>Waiting for code changes...</EmptyStateText>
          <EmptyStateSubtext>
            Configure a repository path and start making changes to see AI-powered code reviews here.
          </EmptyStateSubtext>
        </EmptyState>
      </Container>
    );
  }

  return (
    <Container>
      {reviews.map((review) => (
        <ReviewCard key={review.id}>
          <ReviewHeader isError={!review.isSuccess}>
            <ReviewTitle isError={!review.isSuccess}>
              {review.isSuccess ? '? Code Review Completed' : '? Review Failed'}
            </ReviewTitle>
            <Timestamp>{formatTimestamp(review.timestamp)}</Timestamp>
          </ReviewHeader>
          
          <ReviewContent>
            {review.isSuccess ? (
              <>
                {review.diff && (
                  <DiffSection>
                    <DiffHeader>?? Git Diff</DiffHeader>
                    <DiffContent>{review.diff}</DiffContent>
                  </DiffSection>
                )}
                
                <ReviewText>
                  <ReactMarkdown>{review.review}</ReactMarkdown>
                </ReviewText>
              </>
            ) : (
              <ErrorMessage>
                <strong>Error:</strong> {review.errorMessage}
              </ErrorMessage>
            )}
          </ReviewContent>
        </ReviewCard>
      ))}
    </Container>
  );
}

export default CodeReviewDisplay;