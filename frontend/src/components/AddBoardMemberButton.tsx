import React, { useState, useEffect } from 'react';

interface Role
{
    key: string;
    displayName: string;
}

interface AddBoardMemberButtonProps
{
    onAdd: (emailOrUsername: string, role: string) => void;
    roles: Role[];
    isLoading?: boolean;
}

const AddBoardMemberButton: React.FC<AddBoardMemberButtonProps> = ({
    onAdd,
    roles,
    isLoading = false,
}) =>
{
    const [input, setInput] = useState('');
    const [role, setRole] = useState(roles[0]?.key ?? '');

    useEffect(() =>
    {
        if (roles.length > 0) setRole(roles[0].key);
    }, [roles]);

    const handleAdd = (e?: React.FormEvent) =>
    {
        e?.preventDefault();
        onAdd(input.trim(), role);
        setInput('');
        setRole(roles[0]?.key ?? '');
    };

    return (
        <form className="flex gap-2 mb-4" onSubmit={handleAdd}>
            <input
                type="text"
                className="flex-1 border rounded px-3 py-2"
                placeholder="Email or username"
                required
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
                type="submit"
                className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
                disabled={isLoading}
            >
                Add
            </button>
        </form>
    );
};

export default AddBoardMemberButton;