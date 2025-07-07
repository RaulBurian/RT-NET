import { useState, useRef, useEffect } from 'react';
import './Chat.css';
import API_ROUTES from "../Common/Commons";

interface Message {
  id: number;
  text: string;
  name: string;
  owner: boolean;
}

function Chat({ userName, onLogout }) {
  const [messages, setMessages] = useState<Message[]>([]);
  const currentMessages = useRef<Message[]>(messages);
  const [newMessage, setNewMessage] = useState<string>('');
  const messagesEndRef = useRef<HTMLElement>(null);
  const eventSourceRef = useRef<EventSource | null>(null);

  useEffect(() => {
    const getData = async ()=>{
      const response = await fetch(API_ROUTES.MESSAGES.BASE);
      const data = await response.json() as Message[];
      data.forEach(mess=>{
        mess.owner = mess.name === userName;
      });
      setMessages(data);
    }

    getData();

    const sseUrl = `${API_ROUTES.MESSAGES.BASE}-sse`;
    eventSourceRef.current = new EventSource(sseUrl);

    eventSourceRef.current.onmessage = (event) => {
      const newMessage = JSON.parse(event.data) as Message;
      newMessage.owner = newMessage.name === userName;

      if (currentMessages.current.filter(mess => mess.id === newMessage.id).length > 0) {
        return;
      }

      setMessages(prevMessages => [...prevMessages, newMessage]);
    };

    return () => {
      if (eventSourceRef.current) {
        eventSourceRef.current.close();
      }
    }
  }, []);

  useEffect(() => {
    currentMessages.current = messages;
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async () => {
    if (newMessage.trim() === '') return;

    const response = await fetch(API_ROUTES.MESSAGES.BASE, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        text: newMessage,
        name: userName
      })
    });
    const responseJson = await response.json() as Message;

    setMessages([...messages, {
      id: responseJson.id,
      name: responseJson.name,
      text: responseJson.text,
      owner: true
    }]);
    setNewMessage('');
  };

  const handleKeyPress = async (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      await handleSendMessage();
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