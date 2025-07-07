import { useState, useRef, useEffect } from 'react';
import './Chat.css';
import API_ROUTES, {API_BASE_URL} from "../Common/Commons";
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';

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
  const signalRRef = useRef<HubConnection>(null);

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

    const connection = new HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/messagesHub`)
        .build();

    signalRRef.current = connection;

    connection.start()
        .then(() => console.log('Connected to SignalR hub'))
        .catch(err => console.error('Error connecting to hub:', err));

    connection.on('ReceiveMessage', (id, name, text) => {

      setMessages(prev => [...prev, {id, name, text, owner: name === userName }]);
    });
  }, [userName]);

  useEffect(() => {
    currentMessages.current = messages;
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async () => {
    if (newMessage.trim() === '') return;

    await signalRRef.current.invoke('SendMessage', userName, newMessage)
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