import React, { useEffect, useRef, useState } from 'react';
import Spinner from './Spinner';

interface Member
{
    id: string;
    username: string;
    email: string;
    role: string;
}

interface Role
{
    key: string;
    displayName: string;
}

interface BoardAddMemberModalProps
{
    onClose: () => void;
    members: Member[];
    onAdd: (emailOrUsername: string, role: string) => void;
    onRemove: (memberId: string) => void;
    roles: Role[];
    isLoading?: boolean;
}


const BoardAddMemberModal: React.FC<BoardAddMemberModalProps> = ({
    onClose,
    members,
    onAdd,
    onRemove,
    roles,
    isLoading = false,
}) =>
{
    const [show, setShow] = useState(false);
    const [input, setInput] = useState('');
    const [role, setRole] = useState(roles[0]?.key ?? '');
    const [error, setError] = useState('');
    const panelRef = useRef<HTMLDivElement>(null);

    useEffect(() => setShow(true), []);
    useEffect(() =>
    {
        if (roles.length > 0) setRole(roles[0].key);
    }, [roles]);
    // Close on click outside
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
            {
                onClose();
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [onClose]);

    const handleAdd = (e?: React.FormEvent) =>
    {
        e?.preventDefault();
        setError('');
        onAdd(input.trim(), role);
        setInput('');
        setRole(roles[0]?.key ?? '');
    };
    return (
        <div className="fixed inset-0 z-50">
            {/* Overlay */}
            <div className="absolute inset-0 bg-black opacity-40" />
            {/* Modal */}
            <div
                ref={panelRef}
                className={`
                    absolute top-40 left-1/2 -translate-x-1/2 w-full max-w-2xl
                    bg-white rounded-xl shadow-2xl border border-gray-200 p-6
                    transition-all duration-300 ease-out
                    ${show ? 'opacity-100 translate-y-0 scale-100' : 'opacity-0 -translate-y-4 scale-95'}
                `}
                style={{ willChange: 'opacity, transform' }}
            >
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold">Manage Members</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={onClose}
                        aria-label="Close"
                    >
                        ×
                    </button>
                </div>
                <div className="mb-4 flex gap-2">
                    <input
                        type="text"
                        className="flex-1 border rounded px-3 py-2"
                        placeholder="Email or username" required
                        value={input}
                        onChange={e => setInput(e.target.value)}
                        disabled={isLoading}
                    />
                    <select
                        className="border rounded px-2 py-2"
                        value={role}
                        onChange={e => setRole(e.target.value)}
                        disabled={isLoading}
                    >
                        {roles.map(r => (
                            <option key={r.key} value={r.key}>{r.displayName}</option>
                        ))}
                    </select>
                    <button
                        type="button"
                        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
                        onClick={handleAdd}
                        disabled={isLoading}
                    >
                        Add
                        {isLoading && <span className="ml-2"><Spinner /></span>}
                    </button>
                </div>
                {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
                <div>
                    <h3 className="font-semibold mb-2">Current Members</h3>
                    {isLoading ? (
                        <div className="flex justify-center py-8">
                            <Spinner />
                        </div>
                    ) : (
                        <ul className="space-y-2 max-h-90 overflow-y-auto">
                            {members.map(member => (
                                <li key={member.id} className="flex items-center gap-3 p-2 rounded hover:bg-gray-50">
                                    {/* Avatar icon (simple circle with initials) */}
                                    <div className="w-9 h-9 rounded-full bg-blue-200 flex items-center justify-center text-blue-800 font-bold text-lg">
                                        {member.username.slice(0, 2).toUpperCase()}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2">
                                            <span className="font-medium">{member.username}</span>
                                            <span className="text-xs bg-gray-200 px-2 py-0.5 rounded">{member.role}</span>
                                        </div>
                                        <div className="text-xs text-gray-500">{member.email}</div>
                                    </div>
                                    <button
                                        className="ml-2 text-red-500 hover:text-red-700 text-sm px-2 py-1 rounded transition"
                                        onClick={() => onRemove(member.id)}
                                        title="Remove from board"
                                    >
                                        Remove
                                    </button>
                                </li>
                            ))}
                            {members.length === 0 && (
                                <li className="text-gray-400 text-sm">No members yet.</li>
                            )}
                        </ul>
                    )}
                </div>
            </div>
        </div>
    );
};

export default BoardAddMemberModal;