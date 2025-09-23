import { useState, useEffect } from 'react';
import { ChatBubbleLeftRightIcon, PaperAirplaneIcon, XMarkIcon } from '@heroicons/react/24/solid';
import Spinner from './Spinner';
import api from '../api';
import ReactMarkdown from 'react-markdown';

interface ChatWithAIButtonProps
{
    boardId?: string;
}

type ChatMessage = {
    role: 'user' | 'ai';
    text: string;
};

function ChatWithAIButton({ boardId }: ChatWithAIButtonProps)
{
    const [expanded, setExpanded] = useState(false);
    const [message, setMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [animatedText, setAnimatedText] = useState('');
    const [animatingIdx, setAnimatingIdx] = useState<number | null>(null);

    useEffect(() =>
    {
        // Find the last AI message
        if (messages.length === 0) return;
        const lastIdx = messages.length - 1;
        const lastMsg = messages[lastIdx];
        if (lastMsg.role === 'ai')
        {
            setAnimatingIdx(lastIdx);
            setAnimatedText('');
            let i = 0;
            const text = lastMsg.text;
            const interval = setInterval(() =>
            {
                setAnimatedText(text.slice(0, i + 1));
                i++;
                if (i >= text.length)
                {
                    clearInterval(interval);
                    setAnimatingIdx(null);
                }
            }, 16); // ~60fps
            return () => clearInterval(interval);
        }
        // Only animate when a new AI message is added
    }, [messages]);

    const handleSend = async (e: React.FormEvent) =>
    {
        e.preventDefault();
        if (!boardId || !message.trim()) return;
        const userMsg = message.trim();
        setMessages(msgs => [...msgs, { role: 'user', text: userMsg }]);
        setLoading(true);
        setMessage('');
        try
        {
            const res = await api.post(
                '/api/ollama/chat',
                {
                    boardId,
                    message: userMsg
                }
            );
            // Assume response is { message: string } or similar
            const aiText = res.data?.responseMessage ?? 'No response';
            setMessages(msgs => [...msgs, { role: 'ai', text: aiText }]);
        }
        catch
        {
            setMessages(msgs => [...msgs, { role: 'ai', text: 'Error: Could not get response.' }]);
        }
        finally
        {
            setLoading(false);
        }
    };

    return (
        <div className="relative">
            {!expanded ? (
                <button
                    className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 transition"
                    onClick={() => setExpanded(true)}
                >
                    <ChatBubbleLeftRightIcon className="w-5 h-5" />
                    Chat with AI
                </button>
            ) : (
                <div className="absolute z-20 left-0 mt-2  bg-white rounded shadow-lg border border-gray-200 p-4" style={{ width: '36rem' }}>
                    <div className="flex items-center justify-between mb-2">
                        <span className="flex items-center gap-2 font-semibold text-gray-800">
                            <ChatBubbleLeftRightIcon className="w-5 h-5" />
                            Chat with AI
                        </span>
                        <button
                            className="p-1 rounded hover:bg-gray-100"
                            onClick={() => setExpanded(false)}
                            aria-label="Close chat"
                        >
                            <XMarkIcon className="w-5 h-5 text-gray-500" />
                        </button>
                    </div>
                    <div className="flex flex-col gap-2 mb-2 max-h-96 overflow-y-auto">
                        {messages.map((msg, idx) => (
                            msg.role === 'user' ? (
                                <span
                                    key={idx}
                                    className="self-end bg-green-100 text-gray-900 px-3 py-2 rounded-lg max-w-[90%]"
                                >
                                    {msg.text}
                                </span>
                            ) : (
                                <div
                                    key={idx}
                                    className="self-start bg-gray-100 text-gray-900 px-3 py-2 rounded-lg max-w-[90%] prose prose-sm"
                                >
                                    {animatingIdx === idx
                                        ? <ReactMarkdown>{animatedText}</ReactMarkdown>
                                        : <ReactMarkdown>{msg.text}</ReactMarkdown>
                                    }
                                </div>
                            )
                        ))}
                        {loading && (
                            <span className="self-start bg-gray-100 text-gray-500 px-3 py-2 rounded-lg max-w-[90%] flex items-center gap-2">
                                <Spinner className="w-4 h-4" /> AI is typing...
                            </span>
                        )}
                    </div>
                    <form onSubmit={handleSend}>
                        <textarea
                            className="w-full min-h-[10vh] p-2 border border-gray-300 rounded resize-none focus:outline-none focus:ring-2 focus:ring-green-400"
                            placeholder="Ask the AI anything about your board..."
                            value={message}
                            onChange={e => setMessage(e.target.value)}
                            disabled={loading}
                        />
                        <button
                            type="submit"
                            className="mt-2 flex items-center gap-2 px-3 py-1 bg-green-600 text-white rounded hover:bg-green-700 transition disabled:opacity-50"
                            disabled={!message.trim() || loading}
                        >
                            {loading ? <Spinner className="w-4 h-4" /> : <PaperAirplaneIcon className="w-4 h-4" />}
                            Send
                        </button>
                    </form>
                </div>
            )}
        </div>
    );
}

export default ChatWithAIButton;
