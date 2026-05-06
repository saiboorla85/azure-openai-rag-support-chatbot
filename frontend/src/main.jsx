import React, { useRef, useState, useEffect } from "react";
import { createRoot } from "react-dom/client";
import { Send, Bot, User, FileText, RefreshCcw, AlertCircle } from "lucide-react";
import "./styles.css";
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider, useMsal } from "@azure/msal-react";
import { msalConfig, loginRequest } from "./authConfig";

const API_URL = "https://localhost:7288/api/chat";

const initialMessages = [
  {
    role: "assistant",
    content:
      "Hello! I can help with property search questions, validation errors, and application usage guidance. Try asking: “I get a postcode validation error. How do I fix it?”",
    sources: []
  }
];

function App() {
  const [messages, setMessages] = useState(initialMessages);
  const [input, setInput] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState("");
  const bottomRef = useRef(null);

  const { instance, accounts } = useMsal();
const isAuthenticated = accounts.length > 0;

async function signIn() {
  await instance.loginRedirect(loginRequest);
}

async function signOut() {
  await instance.logoutRedirect({
    postLogoutRedirectUri: "http://localhost:5173"
  });
}

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isSending]);

  async function sendMessage(event) {
    event.preventDefault();

    const trimmed = input.trim();
    if (!trimmed || isSending) return;

    setError("");

    const userMessage = {
      role: "user",
      content: trimmed,
      sources: []
    };

    const updatedMessages = [...messages, userMessage];
    setMessages(updatedMessages);
    setInput("");
    setIsSending(true);

    try {
      const history = updatedMessages
        .filter((message) => message.role === "user" || message.role === "assistant")
        .slice(-8)
        .map((message) => ({
          role: message.role,
          content: message.content
        }));

    if (!isAuthenticated) {
  sessionStorage.setItem("pendingQuestion", trimmed);
  await signIn();
  return;
}

const tokenResponse = await instance.acquireTokenSilent({
  ...loginRequest,
  account: accounts[0]
});

const response = await fetch(API_URL, {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    Authorization: `Bearer ${tokenResponse.accessToken}`
  },
  body: JSON.stringify({
    message: trimmed,
    history
  })
});

      if (!response.ok) {
        throw new Error(`API returned ${response.status}`);
      }

      const data = await response.json();

      const assistantMessage = {
        role: "assistant",
        content: data.reply || "Sorry, I could not generate a response.",
        sources: data.sources || []
      };

      setMessages((current) => [...current, assistantMessage]);
    } catch (err) {
      console.error(err);

      setError(
        "Could not connect to the chatbot API. Please check that the .NET API is running on https://localhost:7288."
      );

      setMessages((current) => [
        ...current,
        {
          role: "assistant",
          content:
            "Sorry, I could not connect to the backend API. Please make sure the .NET project is running and Swagger works.",
          sources: []
        }
      ]);
    } finally {
      setIsSending(false);
    }
  }

  function clearChat() {
    setMessages(initialMessages);
    setInput("");
    setError("");
  }

  function insertExample(text) {
    setInput(text);
  }

  return (
    <main className="app-page">
      <section className="chat-layout">
        <aside className="side-panel">
          <div className="brand-block">
            <div className="brand-icon">
              <Bot size={28} />
            </div>
            <div>
              <h1>Property Support Assistant</h1>
              <p>React UI for your Azure OpenAI RAG API</p>
            </div>
          </div>

          <div className="info-card">
            <h2>Try these questions</h2>
            <button
              type="button"
              onClick={() =>
                insertExample("I get a validation error when searching by postcode. How do I fix it?")
              }
            >
              Postcode validation error
            </button>
            <button
              type="button"
              onClick={() => insertExample("Why am I getting no results when searching for a property?")}
            >
              No results found
            </button>
            <button
              type="button"
              onClick={() => insertExample("How should I search for a property using postcode?")}
            >
              How to search
            </button>
          </div>

          <div className="info-card muted">
            <h2>Backend API</h2>
            <p>{API_URL}</p>
            <p>Keep your .NET API running before sending messages from this UI.</p>
          </div>
        </aside>

        <section className="chat-card">
          <header className="chat-header">
            <div>
              <h2>Customer Chat</h2>
              <p>Answers are generated from your PDF knowledge base.</p>
            </div>
           <div className="header-actions">
  {isAuthenticated ? (
    <button className="clear-button" type="button" onClick={signOut}>
      Sign out
    </button>
  ) : (
    <button className="clear-button" type="button" onClick={signIn}>
      Sign in
    </button>
  )}

  <button className="clear-button" type="button" onClick={clearChat}>
    <RefreshCcw size={16} />
    Clear
  </button>
</div>
          </header>

          {error && (
            <div className="error-banner">
              <AlertCircle size={18} />
              <span>{error}</span>
            </div>
          )}

          <div className="messages">
            {messages.map((message, index) => (
              <MessageBubble key={index} message={message} />
            ))}

            {isSending && (
              <div className="message-row assistant">
                <div className="avatar">
                  <Bot size={18} />
                </div>
                <div className="bubble typing">
                  <span></span>
                  <span></span>
                  <span></span>
                </div>
              </div>
            )}

            <div ref={bottomRef} />
          </div>

          <form className="input-area" onSubmit={sendMessage}>
            <input
              value={input}
              onChange={(event) => setInput(event.target.value)}
              placeholder="Ask about property search errors or how to use the application..."
              disabled={isSending}
            />
            <button type="submit" disabled={isSending || !input.trim()}>
              <Send size={18} />
              Send
            </button>
          </form>
        </section>
      </section>
    </main>
  );
}

function MessageBubble({ message }) {
  const isUser = message.role === "user";

  return (
    <div className={`message-row ${isUser ? "user" : "assistant"}`}>
      <div className="avatar">
        {isUser ? <User size={18} /> : <Bot size={18} />}
      </div>

      <div className="message-content">
        <div className="bubble">{formatMessage(message.content)}</div>

        {!isUser && message.sources?.length > 0 && (
          <div className="sources">
            <div className="sources-title">
              <FileText size={15} />
              Sources
            </div>
            {message.sources.slice(0, 3).map((source, index) => (
              <div className="source-item" key={index}>
                <strong>{source.title || "Support document"}</strong>
                {source.score && <span>Score: {source.score.toFixed(2)}</span>}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function formatMessage(text) {
  return text.split("\n").map((line, index, lines) => (
    <React.Fragment key={index}>
      {line}
      {index < lines.length - 1 && <br />}
    </React.Fragment>
  ));
}

const msalInstance = new PublicClientApplication(msalConfig);

createRoot(document.getElementById("root")).render(
  <MsalProvider instance={msalInstance}>
    <App />
  </MsalProvider>
);
