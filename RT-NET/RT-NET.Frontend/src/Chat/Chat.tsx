import { useState, useRef, useEffect } from 'react';
import './Chat.css';

function Chat({ userName, onLogout }) {
  const [messages, setMessages] = useState([
    { id: 1, text: 'Hello!', name: 'Assistant', owner: false },
    { id: 2, text: `Hi ${userName}! How can I help you today?`, name: 'Assistant', owner: false },
  ]);
  const [newMessage, setNewMessage] = useState('');
  const messagesEndRef = useRef(null);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = () => {
    if (newMessage.trim() === '') return;

    // Add user message
    const userMessage = {
      id: messages.length + 1,
      text: newMessage,
      name: userName,
      owner: true
    };

    setMessages([...messages, userMessage]);
    setNewMessage('');

    // Simulate a response after a short delay
    setTimeout(() => {
      const botMessage = {
        id: messages.length + 2,
        text: `I received your message: "${newMessage}"`,
        name: 'Assistant',
        owner: false
      };
      setMessages(prevMessages => [...prevMessages, botMessage]);
    }, 1000);
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  return (
      <div className="chat-container">
        <div className="chat-header">
          <div className="user-info">
            <span className="user-name">{userName}</span>
          </div>
          <h2>RT Chat</h2>
          <button className="logout-button" onClick={onLogout}>
            Logout
          </button>
        </div>

        <div className="messages-container">
          {messages.map((message) => (
              <div
                  key={message.id}
                  className={`message-wrapper ${message.owner ? 'owner-wrapper' : 'other-wrapper'}`}
              >
                <div className="message-name">{message.name}</div>
                <div className={`message ${message.owner ? 'owner-message' : 'other-message'}`}>
                  {message.text}
                </div>
              </div>
          ))}
          <div ref={messagesEndRef} />
        </div>

        <div className="input-container">
        <textarea
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            onKeyDown={handleKeyPress}
            placeholder="Type a message..."
        />
          <button
              className="send-button"
              onClick={handleSendMessage}
              disabled={!newMessage.trim()}
          >
            Send
          </button>
        </div>
      </div>
  );
}

export default Chat;